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
using EgoEngineLibrary.Graphics.Pssg;

namespace EgoEngineLibrary.Formats.Pssg
{
    public class CarExteriorPssgGltfConverter : PssgGltfConverter
	{
		private class ExportState : PssgModelReaderState
		{
            public ExportState()
            {
			}
		}

		public static bool SupportsPssg(PssgFile pssg)
		{
			return pssg.FindElements("MATRIXPALETTERENDERINSTANCE").Any() ||
				pssg.FindElements("MATRIXPALETTEJOINTRENDERINSTANCE").Any();
		}

		public ModelRoot Convert(PssgFile pssg)
		{
			var sceneBuilder = new SceneBuilder();

			var state = new ExportState();

			// F1 games use lib YYY
			var parent = pssg.FindElements("LIBRARY", "type", "NODE").FirstOrDefault();
			if (parent is null)
			{
				parent = pssg.FindElements("LIBRARY", "type", "YYY").FirstOrDefault();
				state.IsF1 = true;
			}
			if (parent is null)
				throw new InvalidDataException("Could not find library with scene nodes.");

			foreach (var child in parent.ChildElements)
			{
				CreateNode(sceneBuilder, child, null, state);
			}

			return sceneBuilder.ToGltf2();
		}

		private static void CreateNode(SceneBuilder sceneBuilder, PssgElement element, NodeBuilder? parent, ExportState state)
		{
			// only consider a scene node if it has a transform child node
			if (!element.ChildElements.Any(c => c.Name == "TRANSFORM")) return;

			NodeBuilder gltfNode;
			if (parent is null)
			{
				string name = (string)element.Attributes["id"].Value;
				gltfNode = new NodeBuilder(name);
				gltfNode.LocalTransform = GetTransform(element);
			}
			else if (element.Name == "MATRIXPALETTEJOINTNODE")
			{
				gltfNode = CreateMeshNode(sceneBuilder, element, parent, state);
			}
			else if (element.Name == "MATRIXPALETTENODE")
			{
				// do nothing for this node
				return;
			}
			else if (element.Name == "NODE" || element.Name == "MATRIXPALETTEBUNDLENODE")
			{
				string name = (string)element.Attributes["id"].Value;
				gltfNode = parent.CreateNode(name);
				gltfNode.LocalTransform = GetTransform(element);
			}
			else
			{
				throw new NotImplementedException($"Support for node {element.Name} not implemented.");
			}

			foreach (var child in element.ChildElements)
			{
				CreateNode(sceneBuilder, child, gltfNode, state);
			}
		}

		private static NodeBuilder CreateMeshNode(SceneBuilder sceneBuilder, PssgElement mpjnElement, NodeBuilder parent, ExportState state)
		{
			string name = (string)mpjnElement.Attributes["id"].Value;
			NodeBuilder node = parent.CreateNode(name);
			node.LocalTransform = GetTransform(mpjnElement);

			var mesh = ConvertMesh(mpjnElement, state);
			sceneBuilder.AddRigidMesh(mesh, node);

			return node;
		}

        private static IMeshBuilder<MaterialBuilder> ConvertMesh(PssgElement mpjnElement, ExportState state)
		{
			IEnumerable<PssgElement> primitives = mpjnElement.FindElements("MATRIXPALETTERENDERINSTANCE"); // RD: Grid
			primitives = primitives.Concat(mpjnElement.FindElements("MATRIXPALETTEJOINTRENDERINSTANCE")); // Dirt 2 and beyond

			var primitiveDatas = new List<PrimitiveData>();
			var texCoordSets = 0;
			foreach (var prim in primitives)
			{
				var shaderName = ((string)prim.Attributes["shader"].Value).Substring(1);
				var material = CreateMaterialBuilder(shaderName, state, out var createdNew);

				string rdsId = ((string)prim.Attributes["indices"].Value).Substring(1);
				var rdsNode = prim.File.FindElements("RENDERDATASOURCE", "id", rdsId).First();

				var rds = new RenderDataSourceReader(rdsNode);
				texCoordSets = Math.Max(texCoordSets, rds.TexCoordSetCount);

				primitiveDatas.Add(new PrimitiveData(prim, material, createdNew, rds));
			}

			string name = (string)mpjnElement.Attributes["id"].Value;
			var mb = CreateMeshBuilder(name, texCoordSets);
			foreach (var prim in primitiveDatas)
			{
				if (prim.CreatedNewMaterial)
					ConvertMaterial(prim.Element.File, prim.Material, prim.Rds.TexCoordSetCount);

				var pb = mb.UsePrimitive(prim.Material);
				var rds = prim.Rds;

				var indexOffset = prim.Element.Attributes["indexOffset"].GetValue<uint>();
				var indexCount = prim.Element.Attributes["indicesCountFromOffset"].GetValue<uint>();
				var triangles = rds.GetTriangles((int)indexOffset, (int)indexCount);
				foreach (var tri in triangles)
				{
					pb.AddTriangle(
						CreateVertexBuilder(rds, tri.A, state),
						CreateVertexBuilder(rds, tri.B, state),
						CreateVertexBuilder(rds, tri.C, state));
				}
			}

			return mb;
		}

		private static void ConvertMaterial(PssgFile pssg, MaterialBuilder mat, int texCoordSets)
		{
			var shader = pssg.FindElements("SHADERINSTANCE", "id", mat.Name).FirstOrDefault();
			if (shader is null)
				throw new InvalidDataException($"Could not find shader instance {mat.Name} referenced by the model.");

			mat.WithMetallicRoughnessShader()
				.WithMetallicRoughness(0.1f, 0.5f)
				.WithBaseColor(new Vector4(1, 1, 1, 1));

			if (texCoordSets > 0)
			{
				mat.UseChannel(KnownChannel.BaseColor).UseTexture()
					.WithPrimaryImage(new MemoryImage(grayImageBytes))
					.WithCoordinateSet(0);
			}

			if (texCoordSets > 1)
			{
				mat.UseChannel(KnownChannel.Occlusion).UseTexture()
					.WithPrimaryImage(new MemoryImage(whiteImageBytes))
					.WithCoordinateSet(1);
			}

			if (texCoordSets > 2)
			{
				mat.UseChannel(KnownChannel.Emissive).UseTexture()
					.WithPrimaryImage(new MemoryImage(blackImageBytes))
					.WithCoordinateSet(2);
			}

			if (texCoordSets > 3)
			{
				mat.UseChannel(KnownChannel.Normal).UseTexture()
					.WithPrimaryImage(new MemoryImage(blackImageBytes))
					.WithCoordinateSet(3);
			}
		}
	}
}
