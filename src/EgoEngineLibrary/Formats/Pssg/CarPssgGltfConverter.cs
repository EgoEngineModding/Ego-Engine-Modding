using EgoEngineLibrary.Graphics;
using MiscUtil.Conversion;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace EgoEngineLibrary.Formats.Pssg
{
    public class CarPssgGltfConverter
    {
		private static readonly byte[] defaultImageBytes;

		static CarPssgGltfConverter()
		{
			using (var image = new Image<Rgba32>(1, 1))
			using (var ms = new MemoryStream())
			{
				image[0, 0] = new Rgba32(64, 64, 64);
				image.SaveAsPng(ms);
				defaultImageBytes = ms.ToArray();
			}
		}

		private class ExportState
		{
			public bool IsF1 { get; set; }

			public Dictionary<string, MaterialBuilder> ShaderMaterialMap { get; }

            public ExportState()
            {
				ShaderMaterialMap = new Dictionary<string, MaterialBuilder>();
			}
		}

		public static bool SupportsPssg(PssgFile pssg)
		{
			return pssg.FindNodes("MATRIXPALETTERENDERINSTANCE").Any() ||
				pssg.FindNodes("MATRIXPALETTEJOINTRENDERINSTANCE").Any();
		}

		public ModelRoot Convert(PssgFile pssg)
		{
			var sceneBuilder = new SceneBuilder();

			var state = new ExportState();
			ConvertMaterials(pssg, state);

			// F1 games use lib YYY
			var parent = pssg.FindNodes("LIBRARY", "type", "NODE").FirstOrDefault();
			if (parent is null)
			{
				parent = pssg.FindNodes("LIBRARY", "type", "YYY").FirstOrDefault();
				state.IsF1 = true;
			}
			if (parent is null)
				throw new InvalidDataException("Could not find library with scene nodes.");

			foreach (var child in parent.ChildNodes)
			{
				CreateNode(sceneBuilder, child, null, state);
			}

			return sceneBuilder.ToGltf2();
		}

		private static void CreateNode(SceneBuilder sceneBuilder, PssgNode node, NodeBuilder? parent, ExportState state)
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
				gltfNode = CreateMeshNode(sceneBuilder, node, parent, state);
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
				CreateNode(sceneBuilder, child, gltfNode, state);
			}
		}

		private static NodeBuilder CreateMeshNode(SceneBuilder sceneBuilder, PssgNode mpjnNode, NodeBuilder parent, ExportState state)
		{
			string name = (string)mpjnNode.Attributes["id"].Value;
			NodeBuilder node = parent.CreateNode(name);
			node.LocalTransform = getTransform((byte[])mpjnNode.ChildNodes[0].Value);

			var mesh = ConvertMesh(mpjnNode, state);
			sceneBuilder.AddRigidMesh(mesh, node);

			return node;
		}

        private static MeshBuilder<VertexPositionNormal, VertexColor1Texture2, VertexEmpty> ConvertMesh(PssgNode mpjnNode, ExportState state)
		{
			string name = (string)mpjnNode.Attributes["id"].Value;
			var mb = new MeshBuilder<VertexPositionNormal, VertexColor1Texture2, VertexEmpty>(name);
			IEnumerable<PssgNode> primitives = mpjnNode.FindNodes("MATRIXPALETTERENDERINSTANCE"); // RD: Grid
			primitives = primitives.Concat(mpjnNode.FindNodes("MATRIXPALETTEJOINTRENDERINSTANCE")); // Dirt 2 and beyond

			foreach (var prim in primitives)
			{
				var shaderName = ((string)prim.Attributes["shader"].Value).Substring(1);
				var material = state.ShaderMaterialMap[shaderName];
				var pb = mb.UsePrimitive(material);

				string rdsId = ((string)prim.Attributes["indices"].Value).Substring(1);
				var rdsNode = prim.File.FindNodes("RENDERDATASOURCE", "id", rdsId).First();

				var rds = new RenderDataSourceReader(rdsNode);
				var indexOffset = prim.Attributes["indexOffset"].GetValue<uint>();
				var indexCount = prim.Attributes["indicesCountFromOffset"].GetValue<uint>();
				var triangles = rds.GetTriangles((int)indexOffset, (int)indexCount);
				foreach (var tri in triangles)
				{
					pb.AddTriangle(
						GetVertexBuilder(rds, tri.A, state),
						GetVertexBuilder(rds, tri.B, state),
						GetVertexBuilder(rds, tri.C, state));
				}
			}

			return mb;
		}

		private static VertexBuilder<VertexPositionNormal, VertexColor1Texture2, VertexEmpty> GetVertexBuilder(RenderDataSourceReader rds, uint index, ExportState state)
		{
			var vb = new VertexBuilder<VertexPositionNormal, VertexColor1Texture2, VertexEmpty>();
			vb.Geometry.Position = rds.GetPosition(index);
			vb.Geometry.Normal = rds.GetNormal(index);
			// Sometimes the normal would be NaNs, and the tangent/binormal zeros
			// not sure what to do in this case so I just leave out the tangent
			//vb.Geometry.Tangent = rds.GetTangent(index);

			if (state.IsF1)
			{
				// F1 puts diffuse in 1 and spec occ in 0, swap it
				vb.Material.TexCoord0 = rds.GetTexCoord(index, 1);
				vb.Material.TexCoord1 = rds.GetTexCoord(index, 0);
			}
			else
			{
				vb.Material.TexCoord0 = rds.GetTexCoord(index, 0);
				vb.Material.TexCoord1 = rds.GetTexCoord(index, 1);
			}

			var color = rds.GetColor(index);
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

		private void ConvertMaterials(PssgFile pssg, ExportState state)
		{
			var shaders = pssg.FindNodes("SHADERINSTANCE");

			foreach (var shader in shaders)
			{
				var id = shader.Attributes["id"].GetValue<string>();
				var mat = new MaterialBuilder(id);

				mat.WithMetallicRoughnessShader()
				    .WithMetallicRoughness(0.1f, 0.5f)
					.WithBaseColor(new Vector4(0.5f, 0.5f, 0.5f, 1));

				mat.UseChannel(KnownChannel.BaseColor).UseTexture()
					.WithPrimaryImage(new MemoryImage(defaultImageBytes))
					.WithCoordinateSet(0);
				mat.UseChannel(KnownChannel.Occlusion).UseTexture()
					.WithPrimaryImage(new MemoryImage(defaultImageBytes))
					.WithCoordinateSet(1);

				state.ShaderMaterialMap.Add(id, mat);
			}
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
	}
}
