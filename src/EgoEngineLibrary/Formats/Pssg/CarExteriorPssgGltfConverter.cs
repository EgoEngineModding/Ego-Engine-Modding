using System.Numerics;
using EgoEngineLibrary.Graphics.Pssg;
using EgoEngineLibrary.Graphics.Pssg.Elements;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;

namespace EgoEngineLibrary.Formats.Pssg
{
    public class CarExteriorPssgGltfConverter : PssgGltfConverter
	{
		private class ExportState : PssgModelReaderState;

		public static bool SupportsPssg(PssgFile pssg)
		{
			return pssg.Elements<PssgMatrixPaletteRenderStreamInstance>().Any();
		}

		public ModelRoot Convert(PssgFile pssg)
		{
			var sceneBuilder = new SceneBuilder();

			var state = new ExportState();

			// F1 games use lib YYY
            var parent = pssg.Elements<PssgLibrary>().FirstOrDefault(x => x.Type == "NODE");
			if (parent is null)
			{
                parent = pssg.Elements<PssgLibrary>().FirstOrDefault(x => x.Type == "YYY");
				state.IsF1 = true;
			}
			if (parent is null)
				throw new InvalidDataException("Could not find library with scene nodes.");

			foreach (var child in parent.ChildElements.OfType<PssgNode>())
			{
				CreateNode(sceneBuilder, child, null, state);
			}

			return sceneBuilder.ToGltf2();
		}

		private static void CreateNode(SceneBuilder sceneBuilder, PssgNode element, NodeBuilder? parent, ExportState state)
		{
			NodeBuilder gltfNode;
			if (parent is null)
			{
				string name = element.Id;
				gltfNode = new NodeBuilder(name);
				gltfNode.LocalTransform = element.Transform.Transform;
			}
			else if (element.IsExactType<PssgMatrixPaletteJointNode>())
			{
				gltfNode = CreateMeshNode(sceneBuilder, (PssgMatrixPaletteJointNode)element, parent, state);
			}
			else if (element.IsExactType<PssgMatrixPaletteNode>())
			{
				// do nothing for this node
				return;
			}
			else if (element.IsExactType<PssgNode>() || element.IsExactType<PssgMatrixPaletteBundleNode>())
			{
				string name = element.Id;
				gltfNode = parent.CreateNode(name);
				gltfNode.LocalTransform = element.Transform.Transform;
			}
			else
			{
				throw new NotImplementedException($"Support for node {element.Name} not implemented.");
			}

			foreach (var child in element.ChildElements.OfType<PssgNode>())
			{
				CreateNode(sceneBuilder, child, gltfNode, state);
			}
		}

		private static NodeBuilder CreateMeshNode(SceneBuilder sceneBuilder, PssgMatrixPaletteJointNode mpjnElement, NodeBuilder parent, ExportState state)
		{
			string name = mpjnElement.Id;
			NodeBuilder node = parent.CreateNode(name);
			node.LocalTransform = mpjnElement.Transform.Transform;

			var mesh = ConvertMesh(mpjnElement, state);
			sceneBuilder.AddRigidMesh(mesh, node);

			return node;
		}

        private static IMeshBuilder<MaterialBuilder> ConvertMesh(PssgMatrixPaletteJointNode mpjnElement, ExportState state)
		{
			IEnumerable<PssgMatrixPaletteRenderStreamInstance> primitives = mpjnElement.RenderInstances; // RD: Grid
			primitives = primitives.Concat(mpjnElement.JointRenderInstances); // Dirt 2 and beyond

			var primitiveDatas = new List<PrimitiveData>();
			var texCoordSets = 0;
			foreach (var prim in primitives)
			{
				var shader = prim.GetShaderInstance();
				var material = CreateMaterialBuilder(shader, state, out var createdNew);

				var rdsNode = prim.GetRenderDataSource();
				var rds = new RenderDataSourceReader(rdsNode);
				texCoordSets = Math.Max(texCoordSets, rds.TexCoordSetCount);

				primitiveDatas.Add(new PrimitiveData(prim, shader, material, createdNew, rds));
			}

			string name = mpjnElement.Id;
			var mb = CreateMeshBuilder(name, texCoordSets);
			foreach (var prim in primitiveDatas)
			{
				if (prim.CreatedNewMaterial)
					ConvertMaterial(prim.ShaderInstance, prim.Material, prim.Rds.TexCoordSetCount);

				var pb = mb.UsePrimitive(prim.Material);
				var rds = prim.Rds;

                var primElement = (PssgMatrixPaletteRenderStreamInstance)prim.Element;
				var indexOffset = primElement.IndexOffset;
				var indexCount = primElement.IndicesCountFromOffset;
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

		private static void ConvertMaterial(PssgShaderInstance shader, MaterialBuilder mat, int texCoordSets)
		{
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
