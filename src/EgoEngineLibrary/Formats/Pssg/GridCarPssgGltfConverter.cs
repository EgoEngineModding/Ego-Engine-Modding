using EgoEngineLibrary.Graphics;
using MiscUtil.Conversion;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace EgoEngineLibrary.Formats.Pssg
{
    public class GridCarPssgGltfConverter
    {
        public ModelRoot Convert(PssgFile pssg)
		{
			var sceneBuilder = new SceneBuilder();

			var matBuilderMap = ConvertMaterials(pssg);

			var parent = pssg.FindNodes("LIBRARY", "type", "NODE").First();
			foreach (var child in parent.ChildNodes)
			{
				CreateNode(sceneBuilder, child, null, matBuilderMap);
			}

			List<PssgNode> mpbnNodes = pssg.FindNodes("MATRIXPALETTEBUNDLENODE");
			foreach (PssgNode mpbnNode in mpbnNodes)
			{
				String lod = (string)mpbnNode.Attributes["id"].Value;
				List<PssgNode> mpjnNodes = mpbnNode.FindNodes("MATRIXPALETTEJOINTNODE");
				foreach (PssgNode mpjnNode in mpjnNodes)
				{
					//CreateMeshNode(sceneBuilder, pssg, mpjnNode, matBuilderMap);
				}
			}

			return sceneBuilder.ToGltf2();
		}

		private static void CreateNode(SceneBuilder sceneBuilder, PssgNode node, NodeBuilder? parent, Dictionary<string, MaterialBuilder> matBuilderMap)
		{
			// only consider a scene node if it has a transform child node
			if (!node.ChildNodes.Any(c => c.Name == "TRANSFORM")) return;

			NodeBuilder gltfNode;
			if (parent is null)
			{
				string name = (string)node.Attributes["id"].Value;
				gltfNode = new NodeBuilder(name);
				gltfNode.LocalTransform = getTransform((byte[])node.ChildNodes[0].Value);
			}
			else if (node.Name == "MATRIXPALETTEJOINTNODE")
			{
				gltfNode = CreateMeshNode(sceneBuilder, node, parent, matBuilderMap);
			}
			else if (node.Name == "MATRIXPALETTENODE")
			{
				// do nothing for this node
				return;
			}
			else
			{
				string name = (string)node.Attributes["id"].Value;
				gltfNode = parent.CreateNode(name);
				gltfNode.LocalTransform = getTransform((byte[])node.ChildNodes[0].Value);
			}

			foreach (var child in node.ChildNodes)
			{
				CreateNode(sceneBuilder, child, gltfNode, matBuilderMap);
			}
		}

		private static NodeBuilder CreateMeshNode(SceneBuilder sceneBuilder, PssgNode mpjnNode, NodeBuilder parent, Dictionary<string, MaterialBuilder> matBuilderMap)
		{
			string name = (string)mpjnNode.Attributes["id"].Value;
			NodeBuilder node = parent.CreateNode(name);
			node.LocalTransform = getTransform((byte[])mpjnNode.ChildNodes[0].Value);

			var mesh = ConvertMesh(mpjnNode, matBuilderMap);
			sceneBuilder.AddRigidMesh(mesh, node);

			return node;
		}

        private static MeshBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty> ConvertMesh(PssgNode mpjnNode, Dictionary<string, MaterialBuilder> matBuilderMap)
		{
			string name = (string)mpjnNode.Attributes["id"].Value;
			var mb = new MeshBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty>(name);
			var primitives = mpjnNode.FindNodes("MATRIXPALETTERENDERINSTANCE");

			foreach (var prim in primitives)
			{
				var shaderName = ((string)prim.Attributes["shader"].Value).Substring(1);
				var material = matBuilderMap[shaderName];
				var pb = mb.UsePrimitive(material);

				string rdsId = ((string)prim.Attributes["indices"].Value).Substring(1);
				var rdsNode = prim.File.FindNodes("RENDERDATASOURCE", "id", rdsId).First();

				var rds = new RenderDataSource(rdsNode);
				var positions = rds.GetPositions();
				var colors = rds.GetColors();
				var normals = rds.GetNormals();
				var tangents = rds.GetTangents();
				var texCoords = rds.GetTexCoords();

				var indexOffset = prim.Attributes["indexOffset"].GetValue<uint>();
				var indexCount = prim.Attributes["indicesCountFromOffset"].GetValue<uint>();
				var indices = rds.GetIndices().AsSpan().Slice((int)indexOffset, (int)indexCount);
				for (int i = 0; i < indexCount; i += 3)
				{
					var a = indices[i + 0];
					var b = indices[i + 1];
					var c = indices[i + 2];

					pb.AddTriangle(
						GetVertexBuilder(positions[a], normals[a], tangents[a], texCoords[a], colors[a]),
						GetVertexBuilder(positions[b], normals[b], tangents[b], texCoords[b], colors[b]),
						GetVertexBuilder(positions[c], normals[c], tangents[c], texCoords[c], colors[c]));
				}
			}

			return mb;
		}

		private static VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty> GetVertexBuilder(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texCoord, uint color)
		{
			var vb = new VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty>();
			vb.Geometry.Position = position;
			vb.Geometry.Normal = normal;
			//vb.Geometry.Tangent = new Vector4(tangent, 1);

			vb.Material.TexCoord = texCoord;
            vb.Material.Color = new Vector4(
                ((color >> 8) & 0xFF) / (float)byte.MaxValue,
                ((color >> 16) & 0xFF) / (float)byte.MaxValue,
                ((color >> 24) & 0xFF) / (float)byte.MaxValue,
                ((color >> 0) & 0xFF) / (float)byte.MaxValue);

            //var vertWeight = mesh.VertexWeights[face.Indices[index]];
            //var vws = vertWeight.BoneIndices.Zip(vertWeight.Weights, (First, Second) => (First, Second)).Where(vw => vw.Second > 0).ToArray();
            //if (vws.Length > 4) throw new NotSupportedException("A vertex cannot be bound to more than 4 bones.");
            //vb.Skinning.SetWeights(SparseWeight8.Create(vws));

            return vb;
		}

		private Dictionary<string, MaterialBuilder> ConvertMaterials(PssgFile pssg)
		{
			var shaders = pssg.FindNodes("SHADERINSTANCE");

			var matBuilderMap = new Dictionary<string, MaterialBuilder>();
			foreach (var shader in shaders)
			{
				var id = shader.Attributes["id"].GetValue<string>();
				var mat = new MaterialBuilder(id);

				mat.WithMetallicRoughnessShader();
				var cb = mat.UseChannel(KnownChannel.MetallicRoughness);
				cb.Parameter = new Vector4(0.1f, 0.5f, 0, 0);
				cb = mat.UseChannel(KnownChannel.BaseColor);
				cb.Parameter = new Vector4(0.5f, 0.5f, 0.5f, 1);

				matBuilderMap.Add(id, mat);
			}

			return matBuilderMap;
		}

		private static ReadOnlySpan<(ushort A, ushort B, ushort C)> GetTriangles(ReadOnlySpan<ushort> indices)
		{
			return MemoryMarshal.Cast<ushort, (ushort A, ushort B, ushort C)>(indices);
		}

		private static Matrix4x4 getTransform(byte[] buffer)
		{
			Matrix4x4 t = new Matrix4x4();
			MiscUtil.Conversion.BigEndianBitConverter bc = new MiscUtil.Conversion.BigEndianBitConverter();

			// Surely i've missed something and there's a way to loop through this? Please?
			int i = 0;
			t.M11 = bc.ToSingle(buffer, i); i += 4;
			t.M12 = bc.ToSingle(buffer, i); i += 4;
			t.M13 = bc.ToSingle(buffer, i); i += 4;
			t.M14 = bc.ToSingle(buffer, i); i += 4;

			t.M21 = bc.ToSingle(buffer, i); i += 4;
			t.M22 = bc.ToSingle(buffer, i); i += 4;
			t.M23 = bc.ToSingle(buffer, i); i += 4;
			t.M24 = bc.ToSingle(buffer, i); i += 4;

			t.M31 = bc.ToSingle(buffer, i); i += 4;
			t.M32 = bc.ToSingle(buffer, i); i += 4;
			t.M33 = bc.ToSingle(buffer, i); i += 4;
			t.M34 = bc.ToSingle(buffer, i); i += 4;

			t.M41 = bc.ToSingle(buffer, i); i += 4;
			t.M42 = bc.ToSingle(buffer, i); i += 4;
			t.M43 = bc.ToSingle(buffer, i); i += 4;
			t.M44 = bc.ToSingle(buffer, i); i += 4;

			return t;
		}

		private class RenderDataSource
		{
			private readonly PssgNode _rdsNode;
			private readonly PssgNode _risNode;
			private readonly PssgNode _isdNode;

			private readonly List<PssgNode> _renderStreamNodes;
			private readonly Dictionary<string, PssgNode> _dataBlockNodes;

			public RenderDataSource(PssgNode rdsNode)
			{
				_rdsNode = rdsNode;
				_risNode = rdsNode.ChildNodes.FirstOrDefault(n => n.Name == "RENDERINDEXSOURCE") ??
					throw new InvalidDataException($"RDS node {(string)_rdsNode.Attributes["id"].Value} must have RENDERINDEXSOURCE as its first child.");

				_isdNode = _risNode.ChildNodes.FirstOrDefault(n => n.Name == "INDEXSOURCEDATA") ??
					throw new InvalidDataException($"RENDERINDEXSOURCE node {(string)_risNode.Attributes["id"].Value} must have INDEXSOURCEDATA as its first child.");

				_renderStreamNodes = _rdsNode.FindNodes("RENDERSTREAM").ToList();
				var dbNodes = _renderStreamNodes
					.SelectMany(r => r.File.FindNodes("DATABLOCK", "id", r.Attributes["dataBlock"].GetValue<string>().Substring(1)));
				_dataBlockNodes = new Dictionary<string, PssgNode>();
				foreach (var db in dbNodes)
				{
					var id = db.Attributes["id"].GetValue<string>();
					_dataBlockNodes[id] = db;
				}
			}

			public ushort[] GetIndices()
			{
				var format = (string)_risNode.Attributes["format"].Value;
				if (format != "ushort")
					throw new NotImplementedException($"Support for {format} face index format not implemented.");

				var primitive = (string)_risNode.Attributes["primitive"].Value;
				if (primitive != "triangles")
					throw new NotImplementedException($"Support for {primitive} primitives not implemented.");

				var count = _risNode.Attributes["count"].GetValue<uint>();
				var data = ((byte[])_isdNode.Value).AsSpan();
				var indices = new ushort[count];
				for (int i = 0; i < count; ++i)
				{
					indices[i] = BinaryPrimitives.ReadUInt16BigEndian(data);
					data = data.Slice(2); // 2 bytes per ushort
				}

				return indices;
			}

			public Vector3[] GetVector3(string renderType)
			{
				foreach (var stream in _renderStreamNodes)
				{
					var dbId = stream.Attributes["dataBlock"].GetValue<string>().Substring(1);
					var subStream = stream.Attributes["subStream"].GetValue<uint>();
					var db = _dataBlockNodes[dbId];

					var dbStream = db.ChildNodes[(int)subStream];
					if (dbStream.Attributes["renderType"].GetValue<string>() != renderType)
						continue;

					var dataType = dbStream.Attributes["dataType"].GetValue<string>();
					if (dataType != "float3")
						throw new NotImplementedException($"Support for {renderType} data type {dataType} is not implemented.");

					var size = db.Attributes["size"].GetValue<uint>();
					var elemCount = db.Attributes["elementCount"].GetValue<uint>();
					var dataBlockData = db.FindNodes("DATABLOCKDATA").First();
					var data = (byte[])dataBlockData.Value;

					var offset = dbStream.Attributes["offset"].GetValue<uint>();
					var stride = dbStream.Attributes["stride"].GetValue<uint>();
					var positions = new Vector3[elemCount];
					for (uint i = 0, e = 0; i < size; i += stride, ++e)
					{
						var start = (int)(i + offset);
						Vector3 pos = new Vector3();
						pos.X = EndianBitConverter.Big.ToSingle(data, start);
						pos.Y = EndianBitConverter.Big.ToSingle(data, start + 4);
						pos.Z = EndianBitConverter.Big.ToSingle(data, start + 8);
						positions[e] = pos;
					}

					return positions;
				}

				throw new InvalidDataException($"Could not find {renderType} data for RENDERDATASOURCE {_rdsNode.Attributes["id"].GetValue<string>()}.");
			}

			public Vector2[] GetVector2(string renderType)
			{
				foreach (var stream in _renderStreamNodes)
				{
					var dbId = stream.Attributes["dataBlock"].GetValue<string>().Substring(1);
					var subStream = stream.Attributes["subStream"].GetValue<uint>();
					var db = _dataBlockNodes[dbId];

					var dbStream = db.ChildNodes[(int)subStream];
					if (dbStream.Attributes["renderType"].GetValue<string>() != renderType)
						continue;

					var dataType = dbStream.Attributes["dataType"].GetValue<string>();
					if (dataType != "float2")
						throw new NotImplementedException($"Support for {renderType} data type {dataType} is not implemented.");

					var size = db.Attributes["size"].GetValue<uint>();
					var elemCount = db.Attributes["elementCount"].GetValue<uint>();
					var dataBlockData = db.FindNodes("DATABLOCKDATA").First();
					var data = (byte[])dataBlockData.Value;

					var offset = dbStream.Attributes["offset"].GetValue<uint>();
					var stride = dbStream.Attributes["stride"].GetValue<uint>();
					var vecs = new Vector2[elemCount];
					for (uint i = 0, e = 0; i < size; i += stride, ++e)
					{
						var start = (int)(i + offset);
						var vec = new Vector2();
						vec.X = EndianBitConverter.Big.ToSingle(data, start);
						vec.Y = EndianBitConverter.Big.ToSingle(data, start + 4);
						vecs[e] = vec;
					}

					return vecs;
				}

				throw new InvalidDataException($"Could not find {renderType} data for RENDERDATASOURCE {_rdsNode.Attributes["id"].GetValue<string>()}.");
			}

			public uint[] GetUInt32Color(string renderType)
			{
				foreach (var stream in _renderStreamNodes)
				{
					var dbId = stream.Attributes["dataBlock"].GetValue<string>().Substring(1);
					var subStream = stream.Attributes["subStream"].GetValue<uint>();
					var db = _dataBlockNodes[dbId];

					var dbStream = db.ChildNodes[(int)subStream];
					if (dbStream.Attributes["renderType"].GetValue<string>() != renderType)
						continue;

					var dataType = dbStream.Attributes["dataType"].GetValue<string>();
					if (dataType != "uint_color_argb")
						throw new NotImplementedException($"Support for {renderType} data type {dataType} is not implemented.");

					var size = db.Attributes["size"].GetValue<uint>();
					var elemCount = db.Attributes["elementCount"].GetValue<uint>();
					var dataBlockData = db.FindNodes("DATABLOCKDATA").First();
					var data = (byte[])dataBlockData.Value;

					var offset = dbStream.Attributes["offset"].GetValue<uint>();
					var stride = dbStream.Attributes["stride"].GetValue<uint>();
					var vecs = new uint[elemCount];
					for (uint i = 0, e = 0; i < size; i += stride, ++e)
					{
						var start = (int)(i + offset);
						vecs[e] = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan().Slice(start));
					}

					return vecs;
				}

				throw new InvalidDataException($"Could not find {renderType} data for RENDERDATASOURCE {_rdsNode.Attributes["id"].GetValue<string>()}.");
			}

			public Vector3[] GetPositions()
			{
				return GetVector3("Vertex");
			}

			public Vector3[] GetNormals()
			{
				return GetVector3("Normal");
			}

			public Vector3[] GetTangents()
			{
				return GetVector3("Tangent");
			}

			public Vector2[] GetTexCoords()
			{
				return GetVector2("ST");
			}

			public uint[] GetColors()
			{
				return GetUInt32Color("Color");
			}
		}
	}
}
