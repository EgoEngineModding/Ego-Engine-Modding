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
	public class CarInteriorPssgGltfConverter : PssgGltfConverter
	{
		protected class ExportState : PssgModelReaderState
		{
            public virtual bool IsRenderNode(PssgNode element)
            {
                return element.IsExactType<PssgRenderNode>();
            }
		}

		public static bool SupportsPssg(PssgFile pssg)
		{
            return pssg.Elements<PssgRenderNode>().Any(x => x.IsExactType<PssgRenderNode>());
		}

		protected virtual ExportState CreateState()
		{
			return new ExportState();
		}

		public ModelRoot Convert(PssgFile pssg)
		{
			var sceneBuilder = new SceneBuilder();

			var state = CreateState();

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
                gltfNode = new NodeBuilder(name)
                {
                    LocalTransform = element.Transform.Transform
                };
            }
			else if (state.IsRenderNode(element))
			{
				gltfNode = CreateMeshNode(sceneBuilder, (PssgVisibleRenderNode)element, parent, state);
			}
			else if (element.IsExactType<PssgNode>())
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

		private static NodeBuilder CreateMeshNode(SceneBuilder sceneBuilder, PssgVisibleRenderNode renderElement, NodeBuilder parent, ExportState state)
		{
			string name = renderElement.Id;
			NodeBuilder node = parent.CreateNode(name);
			node.LocalTransform = renderElement.Transform.Transform;

			var mesh = ConvertMesh(renderElement, state);
			sceneBuilder.AddRigidMesh(mesh, node);

			return node;
		}

		private static IMeshBuilder<MaterialBuilder> ConvertMesh(PssgVisibleRenderNode renderElement, ExportState state)
		{
			var primitives = renderElement.RenderInstances;

			var primitiveDatas = new List<PrimitiveData>();
			var texCoordSets = 0;
			foreach (var prim in primitives)
			{
                if (prim is not PssgRenderStreamInstance streamInstance)
                {
                    throw new NotImplementedException($"Support for '{prim.Name}' in node '{renderElement.Name}' is not implemented.");
                }
                
				var shader = prim.GetShaderInstance();
				var material = CreateMaterialBuilder(shader, state, out var createdNew);

				var rdsNode = streamInstance.GetRenderDataSource();
				var rds = new RenderDataSourceReader(rdsNode);
				texCoordSets = Math.Max(texCoordSets, rds.TexCoordSetCount);

				primitiveDatas.Add(new PrimitiveData(streamInstance, shader, material, createdNew, rds));
			}

			string name = renderElement.Id;
			var mb = CreateMeshBuilder(name, texCoordSets);
			foreach (var prim in primitiveDatas)
			{
				if (prim.CreatedNewMaterial)
					ConvertMaterial(prim.ShaderInstance, prim.Material, prim.Rds.TexCoordSetCount);

				var pb = mb.UsePrimitive(prim.Material);
				var rds = prim.Rds;

				var triangles = rds.GetTriangles();
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
			var sgNode = shader.GetShaderGroup();
			var textureInputs = shader.Inputs.Where(x => x.Type == "texture");

			mat.WithMetallicRoughnessShader()
				.WithMetallicRoughness(0.1f, 0.5f)
				.WithBaseColor(new Vector4(1, 1, 1, 1));

			if (texCoordSets > 0)
			{
				mat.UseChannel(KnownChannel.BaseColor).UseTexture()
					.WithPrimaryImage(new MemoryImage(GetDiffuseTexture(sgNode, textureInputs)))
					.WithCoordinateSet(0);
			}

			if (texCoordSets > 1)
			{
				mat.UseChannel(KnownChannel.Occlusion).UseTexture()
					.WithPrimaryImage(new MemoryImage(GetOcclusionTexture(sgNode, textureInputs)))
					.WithCoordinateSet(1);
			}

			if (texCoordSets > 2)
			{
				mat.UseChannel(KnownChannel.Emissive).UseTexture()
					.WithPrimaryImage(new MemoryImage(GetEmissiveTexture(sgNode, textureInputs)))
					.WithCoordinateSet(2);
			}

			if (texCoordSets > 3)
			{
				mat.UseChannel(KnownChannel.Normal).UseTexture()
					.WithPrimaryImage(new MemoryImage(GetNormalTexture(sgNode, textureInputs)))
					.WithCoordinateSet(3);
			}
		}
	}
}
