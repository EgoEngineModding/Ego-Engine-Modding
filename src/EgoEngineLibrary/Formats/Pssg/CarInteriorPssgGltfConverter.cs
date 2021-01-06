using EgoEngineLibrary.Graphics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace EgoEngineLibrary.Formats.Pssg
{
	public class CarInteriorPssgGltfConverter : PssgGltfConverter
	{
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
			return pssg.FindNodes("RENDERNODE").Any();
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
			else if (node.Name == "RENDERNODE")
			{
				gltfNode = CreateMeshNode(sceneBuilder, node, parent, state);
			}
			else if (node.Name == "NODE")
			{
				string name = (string)node.Attributes["id"].Value;
				gltfNode = parent.CreateNode(name);
				gltfNode.LocalTransform = getTransform((byte[])node.ChildNodes[0].Value);
			}
			else
			{
				throw new NotImplementedException($"Support for node {node.Name} not implemented.");
			}

			foreach (var child in node.ChildNodes)
			{
				CreateNode(sceneBuilder, child, gltfNode, state);
			}
		}

		private static NodeBuilder CreateMeshNode(SceneBuilder sceneBuilder, PssgNode renderNode, NodeBuilder parent, ExportState state)
		{
			string name = (string)renderNode.Attributes["id"].Value;
			NodeBuilder node = parent.CreateNode(name);
			node.LocalTransform = getTransform((byte[])renderNode.ChildNodes[0].Value);

			var mesh = ConvertMesh(renderNode, state);
			sceneBuilder.AddRigidMesh(mesh, node);

			return node;
		}

		private static MeshBuilder<VertexPositionNormal, VertexColor1Texture4, VertexEmpty> ConvertMesh(PssgNode renderNode, ExportState state)
		{
			string name = (string)renderNode.Attributes["id"].Value;
			var mb = new MeshBuilder<VertexPositionNormal, VertexColor1Texture4, VertexEmpty>(name);
			IEnumerable<PssgNode> primitives = renderNode.FindNodes("RENDERSTREAMINSTANCE");

			foreach (var prim in primitives)
			{
				var shaderName = ((string)prim.Attributes["shader"].Value).Substring(1);
				var material = state.ShaderMaterialMap[shaderName];
				var pb = mb.UsePrimitive(material);

				string rdsId = ((string)prim.Attributes["indices"].Value).Substring(1);
				var rdsNode = prim.File.FindNodes("RENDERDATASOURCE", "id", rdsId).First();

				var rds = new RenderDataSourceReader(rdsNode);
				var triangles = rds.GetTriangles();
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

		private static VertexBuilder<VertexPositionNormal, VertexColor1Texture4, VertexEmpty> GetVertexBuilder(RenderDataSourceReader rds, uint index, ExportState state)
		{
			var vb = new VertexBuilder<VertexPositionNormal, VertexColor1Texture4, VertexEmpty>();
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
			vb.Material.TexCoord2 = rds.GetTexCoord(index, 2);
			vb.Material.TexCoord3 = rds.GetTexCoord(index, 3);

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
				var shaderGroupId = shader.Attributes["shaderGroup"].GetValue<string>().Substring(1);
				var sgNode = shader.File.FindNodes("SHADERGROUP", "id", shaderGroupId).FirstOrDefault();
				var textureInputs = shader.FindNodes("SHADERINPUT", "type", "texture");

				var id = shader.Attributes["id"].GetValue<string>();
				var mat = new MaterialBuilder(id);

				mat.WithMetallicRoughnessShader()
					.WithMetallicRoughness(0.1f, 0.5f)
					.WithBaseColor(new Vector4(1, 1, 1, 1));

				mat.UseChannel(KnownChannel.BaseColor).UseTexture()
					.WithPrimaryImage(new MemoryImage(GetDiffuseTexture(sgNode, textureInputs)))
					.WithCoordinateSet(0);
				mat.UseChannel(KnownChannel.Occlusion).UseTexture()
					.WithPrimaryImage(new MemoryImage(GetOcclusionTexture(sgNode, textureInputs)))
					.WithCoordinateSet(1);
				mat.UseChannel(KnownChannel.Emissive).UseTexture()
					.WithPrimaryImage(new MemoryImage(GetEmissiveTexture(sgNode, textureInputs)))
					.WithCoordinateSet(2);
				mat.UseChannel(KnownChannel.Normal).UseTexture()
					.WithPrimaryImage(new MemoryImage(GetNormalTexture(sgNode, textureInputs)))
					.WithCoordinateSet(3);

				state.ShaderMaterialMap.Add(id, mat);
			}
		}
	}
}
