using System.Numerics;
using EgoEngineLibrary.Graphics.Pssg;
using EgoEngineLibrary.Graphics.Pssg.Elements;
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;

namespace EgoEngineLibrary.Formats.Pssg
{
    public class GltfCarInteriorPssgConverter
    {
        protected class ImportState : PssgModelWriterState
        {
            public int RenderStreamInstanceCount { get; set; }

            public int SegmentSetCount { get; set; }

            public int RenderDataSourceCount { get; set; }

            public PssgElement RdsLib { get; }

            public PssgElement RibLib { get; }

            public bool IsF1 { get; }

            public Dictionary<string, ShaderInputInfo> ShaderGroupMap { get; }

            public Dictionary<int, ShaderInstanceData> MatShaderMapping { get; }

            public ImportState(PssgElement rdsLib, PssgElement ribLib, Dictionary<string, ShaderInputInfo> shaderGroupMap)
            {
                RdsLib = rdsLib;
                RibLib = ribLib;
                ShaderGroupMap = shaderGroupMap;
                MatShaderMapping = new Dictionary<int, ShaderInstanceData>();

                if (rdsLib == ribLib)
                    IsF1 = true;
            }

            public virtual PssgNode CreateRenderNode(PssgFile pssg, PssgElement? parent)
            {
                return new PssgRenderNode(pssg, parent);
            }
        }
        protected class ShaderInstanceData
        {
            public string ShaderInstanceName { get; }

            public string ShaderGroupName { get; }

            public RenderDataSourceWriter Rds { get; }

            public ShaderInstanceData(string shaderInstanceName, string shaderGroupName, RenderDataSourceWriter rds)
            {
                ShaderInstanceName = shaderInstanceName;
                ShaderGroupName = shaderGroupName;
                Rds = rds;
            }
        }

        public static bool SupportsPssg(PssgFile pssg)
        {
            return pssg.Elements<PssgRenderNode>().Any(x => x.IsExactType<PssgRenderNode>());
        }

        protected virtual ImportState CreateState(PssgElement rdsLib, PssgElement ribLib, Dictionary<string, ShaderInputInfo> shaderGroupMap)
        {
            return new ImportState(rdsLib, ribLib, shaderGroupMap);
        }

        public void Convert(ModelRoot gltf, PssgFile pssg)
        {
            // Get a list of nodes in the default scene as a flat list
            var rootNode = gltf.DefaultScene.FindNode(n => n.Name.StartsWith("Scene Root", PssgStringHelper.StringComparison));
            if (rootNode is null)
                throw new InvalidDataException("The default scene must have node name starting with `Scene Root`.");

            // Determine libraries in which to store data
            var nodeLib = pssg.Elements<PssgLibrary>().FirstOrDefault(x => x.Type == "NODE");
            PssgElement rdsLib; PssgElement ribLib;
            if (nodeLib is not null)
            {
                rdsLib = pssg.Elements<PssgLibrary>().First(x => x.Type == "SEGMENTSET");
                ribLib = pssg.Elements<PssgLibrary>().First(x => x.Type == "RENDERINTERFACEBOUND");
            }
            else
            {
                // F1 games use YYY, and put almost everything in this lib
                nodeLib = pssg.Elements<PssgLibrary>().FirstOrDefault(x => x.Type == "YYY");
                if (nodeLib is null)
                    throw new InvalidDataException("Could not find library with scene nodes.");

                rdsLib = nodeLib;
                ribLib = nodeLib;
            }

            var state = CreateState(rdsLib, ribLib, ShaderInputInfo.CreateFromPssg(pssg).ToDictionary(si => si.ShaderGroupId));

            // Clear out the libraries
            nodeLib.RemoveChildElements(nodeLib.ChildElements.Where(n => n.IsExactType<PssgRootNode>()));
            rdsLib.RemoveChildElements(rdsLib.ChildElements.Where(n => n.IsExactType<PssgSegmentSet>()));
            ribLib.RemoveChildElements(ribLib.ChildElements.Where(n => n.IsExactType<PssgDataBlock>()));

            // Write the scene graph, and collect mesh data
            ConvertSceneNodes(pssg, nodeLib, rootNode, state);
        }

        private static void ConvertSceneNodes(PssgFile pssg, PssgElement parent, Node gltfNode, ImportState state)
        {
            PssgNode element;
            if (gltfNode.Name.StartsWith("Scene Root", PssgStringHelper.StringComparison))
            {
                element = new PssgRootNode(parent.File, parent);
                element.StopTraversal = false;
                element.Nickname = "Scene Root";
                element.Id = "Scene Root";
                parent.ChildElements.Add(element);
            }
            else if (gltfNode.Mesh is not null)
            {
                _ = CreateRenderNode(parent, gltfNode, state);
                return;
            }
            else
            {
                element = new PssgNode(parent.File, parent);
                element.StopTraversal = false;
                element.Nickname = gltfNode.Name;
                element.Id = gltfNode.Name;
                parent.ChildElements.Add(element);
            }

            element.Initialize();
            element.Transform.Transform = gltfNode.LocalMatrix;
            element.BoundingBox.BoundsMin = Vector3.Zero;
            element.BoundingBox.BoundsMax = Vector3.Zero;

            foreach (var child in gltfNode.VisualChildren)
            {
                ConvertSceneNodes(pssg, element, child, state);
            }
        }

        private static PssgElement CreateRenderNode(PssgElement parent, Node gltfNode, ImportState state)
        {
            var node = state.CreateRenderNode(parent.File, parent);
            node.StopTraversal = false;
            node.Nickname = gltfNode.Name;
            node.Id = gltfNode.Name;
            parent.ChildElements.Add(node);

            state.MatShaderMapping.Clear();

            // Now add a new mesh from mesh builder
            ConvertMesh(node, gltfNode, state);

            // Write the mesh data
            WriteMeshData(state);

            return node;
        }

        private static void ConvertMesh(PssgNode renderElement, Node gltfNode, ImportState state)
        {
            var mesh = gltfNode.Mesh;
            if (mesh.Primitives.Any(p => p.Material == null))
                throw new NotImplementedException($"The converter does not support primitives ({mesh.Name}) with a null material.");

            renderElement.Initialize();
            renderElement.Transform.Transform = gltfNode.LocalMatrix;

            // Add to the material shader mapping
            var gltfMats = mesh.Primitives.Select(p => p.Material);
            ConvertMaterials(gltfMats, renderElement.File, state);

            // Export Vertices, Normals, TexCoords, VertexWeights and Faces
            var gltfMesh = gltfNode.Mesh;
            var meshDecoder = gltfMesh.Decode();
            var minExtent = new Vector3(float.MaxValue);
            var maxExtent = new Vector3(float.MinValue);
            foreach (var p in meshDecoder.Primitives)
            {
                // skip primitives that aren't tris
                if (!p.TriangleIndices.Any())
                    continue;

                // Get the new material index in grn
                var faceMatId = p.Material.LogicalIndex;
                ShaderInstanceData shaderData;
                if (state.MatShaderMapping.ContainsKey(faceMatId))
                {
                    shaderData = state.MatShaderMapping[faceMatId];
                }
                else
                {
                    throw new InvalidDataException($"Mesh ({gltfMesh.Name}) has an invalid material id " + faceMatId + ".");
                }

                var rds = shaderData.Rds;
                var baseVertexIndex = rds.Positions.Count;

                var rsiNode = new PssgRenderStreamInstance(renderElement.File, renderElement);
                rsiNode.SourceCount = 1u;
                rsiNode.Indices = $"#{rds.Name}";
                rsiNode.StreamCount = 0;
                rsiNode.Shader = $"#{shaderData.ShaderInstanceName}";
                rsiNode.Id = $"!RSI{state.RenderStreamInstanceCount}"; state.RenderStreamInstanceCount++;
                renderElement.ChildElements.Add(rsiNode);

                var risNode = new PssgRenderInstanceSource(rsiNode.File, rsiNode);
                risNode.Source = $"#{shaderData.Rds.Name}";
                rsiNode.ChildElements.Add(risNode);

                var texCoordSet0 = GetDiffuseBaseColorTexCoord(p.Material);
                var texCoordSet1 = GetOcclusionTexCoord(p.Material);
                var texCoordSet2 = GetEmissiveTexCoord(p.Material);
                var texCoordSet3 = GetNormalTexCoord(p.Material);

                if (state.IsF1)
                {
                    // F1 stores spec occ first, then diffuse
                    var temp = texCoordSet0;
                    texCoordSet0 = texCoordSet1;
                    texCoordSet1 = temp;
                }

                // Make sure we have all the necessary data
                if (p.VertexCount < 3) throw new InvalidDataException($"Mesh ({gltfMesh.Name}) must have at least 3 positions.");

                // Grab the data
                for (var i = 0; i < p.VertexCount; ++i)
                {
                    var pos = p.GetPosition(i);
                    var color = p.GetColor(i, 0);

                    // Compute extents for bounding box
                    minExtent.X = Math.Min(pos.X, minExtent.X);
                    minExtent.Y = Math.Min(pos.Y, minExtent.Y);
                    minExtent.Z = Math.Min(pos.Z, minExtent.Z);
                    maxExtent.X = Math.Max(pos.X, maxExtent.X);
                    maxExtent.Y = Math.Max(pos.Y, maxExtent.Y);
                    maxExtent.Z = Math.Max(pos.Z, maxExtent.Z);

                    rds.Positions.Add(pos);
                    rds.Normals.Add(p.GetNormal(i));
                    rds.Tangents.Add(p.GetTangent(i));
                    rds.TexCoords0.Add(p.GetTextureCoord(i, texCoordSet0));
                    rds.TexCoords1.Add(p.GetTextureCoord(i, texCoordSet1));
                    rds.TexCoords2.Add(p.GetTextureCoord(i, texCoordSet2));
                    rds.TexCoords3.Add(p.GetTextureCoord(i, texCoordSet3));
                    rds.Colors.Add(color);
                    rds.SkinIndices.Add(0);
                }

                foreach (var (A, B, C) in p.TriangleIndices)
                {
                    var a = A + baseVertexIndex;
                    var b = B + baseVertexIndex;
                    var c = C + baseVertexIndex;

                    rds.Indices.Add((uint)a);
                    rds.Indices.Add((uint)b);
                    rds.Indices.Add((uint)c);
                }
            }

            renderElement.BoundingBox.BoundsMin = minExtent;
            renderElement.BoundingBox.BoundsMax = maxExtent;
            return;

            static int GetDiffuseBaseColorTexCoord(Material srcMaterial)
            {
                var channel = srcMaterial.FindChannel("Diffuse");
                if (channel.HasValue) return channel.Value.TextureCoordinate;

                channel = srcMaterial.FindChannel("BaseColor");
                if (channel.HasValue) return channel.Value.TextureCoordinate;

                return 0;
            }
            static int GetOcclusionTexCoord(Material srcMaterial)
            {
                var channel = srcMaterial.FindChannel("Occlusion");
                if (channel.HasValue) return channel.Value.TextureCoordinate;

                return GetDiffuseBaseColorTexCoord(srcMaterial);
            }
            static int GetEmissiveTexCoord(Material srcMaterial)
            {
                var channel = srcMaterial.FindChannel("Emissive");
                if (channel.HasValue) return channel.Value.TextureCoordinate;

                return GetDiffuseBaseColorTexCoord(srcMaterial);
            }
            static int GetNormalTexCoord(Material srcMaterial)
            {
                var channel = srcMaterial.FindChannel("Normal");
                if (channel.HasValue) return channel.Value.TextureCoordinate;

                return GetDiffuseBaseColorTexCoord(srcMaterial);
            }
        }

        private static void ConvertMaterials(IEnumerable<Material> gltfMats, PssgFile pssg, ImportState state)
        {
            foreach (var gltfMat in gltfMats)
            {
                if (state.MatShaderMapping.ContainsKey(gltfMat.LogicalIndex))
                    continue;

                // Find shader instance with same name as mat
                var shader = pssg.TryGetObject<PssgShaderInstance>(gltfMat.Name.AsMemory());
                if (shader is null)
                    throw new InvalidDataException($"The pssg must already contain a shader instance with name {gltfMat.Name}.");

                state.MatShaderMapping.Add(gltfMat.LogicalIndex,
                    new ShaderInstanceData(
                        shader.Id,
                        shader.ShaderGroup[1..],
                        new RenderDataSourceWriter($"!RDS{state.RenderDataSourceCount}")));
                state.RenderDataSourceCount++;
            }
        }

        private static void WriteMeshData(ImportState state)
        {
            var ssNode = new PssgSegmentSet(state.RdsLib.File, state.RdsLib);
            ssNode.SegmentCount = (uint)state.MatShaderMapping.Count;
            ssNode.Id = $"!SS{state.SegmentSetCount}";
            state.SegmentSetCount++;
            state.RdsLib.ChildElements.Add(ssNode);

            foreach (var shader in state.MatShaderMapping.Values)
            {
                var rds = shader.Rds;
                if (!state.ShaderGroupMap.TryGetValue(shader.ShaderGroupName, out var shaderInputInfo))
                    throw new InvalidDataException($"The pssg does not have existing data blocks to model the layout of the input for shader {shader.ShaderGroupName}.");

                rds.Write(shaderInputInfo, ssNode, state.RibLib, state);
            }
        }
    }
}
