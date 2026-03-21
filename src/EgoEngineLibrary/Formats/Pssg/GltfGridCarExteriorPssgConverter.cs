using System.Numerics;
using System.Text.RegularExpressions;
using EgoEngineLibrary.Graphics.Pssg;
using EgoEngineLibrary.Graphics.Pssg.Elements;
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;

namespace EgoEngineLibrary.Formats.Pssg
{
    public partial class GltfGridCarExteriorPssgConverter
    {
        private partial class ImportState : PssgModelWriterState
        {
            public int LodNumber { get; set; }

            public int MpjriCount { get; set; }

            public int RenderDataSourceCount { get; set; }

            public uint JointId { get; set; }

            public PssgElement RdsLib { get; }

            public PssgElement RibLib { get; }

            public Dictionary<string, ShaderInputInfo> ShaderGroupMap { get; }

            public Dictionary<int, ShaderInstanceData> MatShaderMapping { get; }

            [GeneratedRegex("^LOD([0-9]+)_$", RegexOptions.CultureInvariant)]
            public partial Regex LodMatcher { get; }

            public ImportState(PssgElement rdsLib, PssgElement ribLib, Dictionary<string, ShaderInputInfo> shaderGroupMap)
            {
                RdsLib = rdsLib;
                RibLib = ribLib;
                ShaderGroupMap = shaderGroupMap;
                MatShaderMapping = new Dictionary<int, ShaderInstanceData>();
            }
        }
        private class ShaderInstanceData
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
            var rsiNodes = pssg.Elements<PssgRenderStreamInstance>()
                .Where(x => x.IsExactType<PssgRenderStreamInstance>());
            return rsiNodes.Any() && (rsiNodes.First().ParentElement?.IsExactType<PssgMatrixPaletteNode>() ?? false);
        }

        public void Convert(ModelRoot gltf, PssgFile pssg)
        {
            // Get a list of nodes in the default scene as a flat list
            var rootNode = gltf.DefaultScene.FindNode(n => n.Name.StartsWith("Scene Root"));
            if (rootNode is null)
                throw new InvalidDataException("The default scene must have node name starting with `Scene Root`.");

            // Determine libraries in which to store data
            var nodeLib = pssg.Elements<PssgLibrary>().First(x => x.Type == "NODE");
            var rdsLib = pssg.Elements<PssgLibrary>().First(x => x.Type == "RENDERDATASOURCE");
            var ribLib = pssg.Elements<PssgLibrary>().First(x => x.Type == "RENDERINTERFACEBOUND");

            var state = new ImportState(rdsLib, ribLib, ShaderInputInfo.CreateFromPssg(pssg).ToDictionary(si => si.ShaderGroupId));

            // Clear out the libraries
            nodeLib.RemoveChildElements(nodeLib.ChildElements.Where(n => n.IsExactType<PssgRootNode>()));
            rdsLib.RemoveChildElements(rdsLib.ChildElements.Where(n => n.IsExactType<PssgRenderDataSource>()));
            ribLib.RemoveChildElements(ribLib.ChildElements.Where(n => n.IsExactType<PssgDataBlock>()));

            // Write the scene graph, and collect mesh data
            ConvertSceneNodes(pssg, nodeLib, rootNode, state);
        }

        private static void ConvertSceneNodes(PssgFile pssg, PssgElement parent, Node gltfNode, ImportState state)
        {
            PssgNode element;
            Match lodMatch;
            if (gltfNode.Name.StartsWith("Scene Root"))
            {
                element = new PssgRootNode(parent.File, parent);
                element.StopTraversal = false;
                element.Nickname = "Scene Root";
                element.Id = "Scene Root";
                parent.ChildElements.Add(element);
            }
            else if ((lodMatch = state.LodMatcher.Match(gltfNode.Name)).Success)
            {
                var lodNumber = int.Parse(lodMatch.Groups[1].Value);
                element = CreateMatrixPaletteBundleNode(parent, gltfNode, lodNumber, state);
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

        private static PssgNode CreateMatrixPaletteBundleNode(PssgElement parent, Node gltfNode, int lodNumber, ImportState state)
        {
            var element = new PssgMatrixPaletteBundleNode(parent.File, parent);
            element.StopTraversal = false;
            element.Nickname = $"LOD{lodNumber}_";
            element.Id = $"LOD{lodNumber}_";
            parent.ChildElements.Add(element);

            element.Initialize();
            element.Transform.Transform = gltfNode.LocalMatrix;
            element.BoundingBox.BoundsMin = Vector3.Zero;
            element.BoundingBox.BoundsMax = Vector3.Zero;

            state.LodNumber = lodNumber;
            state.JointId = 0;
            state.MatShaderMapping.Clear();

            List<string> jointNames = new List<string>();
            foreach (var child in gltfNode.VisualChildren)
            {
                if (child.Mesh is null) continue;
                if (child.Mesh.Primitives.Count == 0) continue;

                var mpjn = CreateMatrixPaletteJointNode(element, child, state);
                jointNames.Add(mpjn.Id);
            }

            CreateMatrixPaletteNode(element, jointNames, state);

            // Write the mesh data
            WriteMeshData(state);

            return element;
        }

        private static void CreateMatrixPaletteNode(PssgElement parent, List<string> jointNames, ImportState state)
        {
            var node = new PssgMatrixPaletteNode(parent.File, parent);
            node.JointCount = state.JointId;
            node.StopTraversal = false;
            node.Id = $"x{state.LodNumber}_MPN";
            parent.ChildElements.Add(node);

            node.Initialize();
            node.Transform.Transform = Matrix4x4.Identity;
            node.BoundingBox.BoundsMin = Vector3.Zero;
            node.BoundingBox.BoundsMax = Vector3.Zero;

            foreach (var shader in state.MatShaderMapping.Values)
            {
                var rsiNode = new PssgRenderStreamInstance(node.File, node);
                rsiNode.SourceCount = 1u;
                rsiNode.Indices = $"#{shader.Rds.Name}";
                rsiNode.StreamCount = 0;
                rsiNode.Shader = $"#{shader.ShaderInstanceName}";
                rsiNode.Id = shader.Rds.Name.Replace("RDS", "RSI");
                node.ChildElements.Add(rsiNode);

                var risNode = new PssgRenderInstanceSource(rsiNode.File, rsiNode);
                risNode.Source = $"#{shader.Rds.Name}";
                rsiNode.ChildElements.Add(risNode);
            }

            foreach (var jointName in jointNames)
            {
                var mpsjNode = new PssgMatrixPaletteSkinJoint(node.File, node);
                mpsjNode.Joint = $"#{jointName}";
                node.ChildElements.Add(mpsjNode);
            }
        }

        private static PssgMatrixPaletteJointNode CreateMatrixPaletteJointNode(PssgElement parent, Node gltfNode, ImportState state)
        {
            var node = new PssgMatrixPaletteJointNode(parent.File, parent);
            node.StopTraversal = false;
            node.Nickname = gltfNode.Name;
            node.Id = gltfNode.Name;
            node.JointId = state.JointId;
            node.MatrixPalette = $"#x{state.LodNumber}_MPN";
            parent.ChildElements.Add(node);

            // Now add a new mesh from mesh builder
            ConvertMesh(node, gltfNode, state);

            state.JointId++;
            return node;
        }

        private static void ConvertMesh(PssgMatrixPaletteJointNode mpjnElement, Node gltfNode, ImportState state)
        {
            var mesh = gltfNode.Mesh;
            if (mesh.Primitives.Any(p => p.Material == null))
                throw new NotImplementedException(
                    $"The converter does not support primitives ({mesh.Name}) with a null material.");

            mpjnElement.Initialize();
            mpjnElement.Transform.Transform = gltfNode.LocalMatrix;

            // Add to the material shader mapping
            var gltfMats = mesh.Primitives.Select(p => p.Material);
            ConvertMaterials(gltfMats, mpjnElement.File, state);

            // Export Vertices, Normals, TexCoords, VertexWeights and Faces
            Mesh gltfMesh = gltfNode.Mesh;
            var meshDecoder = gltfMesh.Decode();
            Vector3 minExtent = new Vector3(float.MaxValue);
            Vector3 maxExtent = new Vector3(float.MinValue);
            foreach (var p in meshDecoder.Primitives)
            {
                // skip primitives that aren't tris
                if (!p.TriangleIndices.Any())
                    continue;

                // Get the new material index in grn
                int faceMatId = p.Material.LogicalIndex;
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
                var tris = p.TriangleIndices.ToArray();
                var baseVertexIndex = rds.Positions.Count;

                var mpriNode = new PssgMatrixPaletteRenderInstance(mpjnElement.File, mpjnElement);
                mpriNode.StreamOffset = (uint)(rds.Positions.Count);
                mpriNode.ElementCountFromOffset = (uint)(p.VertexCount);
                mpriNode.IndexOffset = (uint)(rds.Indices.Count);
                mpriNode.IndicesCountFromOffset = (uint)(tris.Length * 3);
                mpriNode.SourceCount = 1u;
                mpriNode.Indices = $"#{rds.Name}";
                mpriNode.StreamCount = 0;
                mpriNode.Shader = $"#{shaderData.ShaderInstanceName}";
                mpriNode.Id = $"MPJRI{state.MpjriCount}"; state.MpjriCount++;
                mpjnElement.ChildElements.Add(mpriNode);

                var risNode = new PssgRenderInstanceSource(mpriNode.File, mpriNode);
                risNode.Source = $"#{shaderData.Rds.Name}";
                mpriNode.ChildElements.Add(risNode);

                var texCoordSet = GetDiffuseBaseColorTexCoord(p.Material);

                // Make sure we have all the necessary data
                if (p.VertexCount < 3) throw new InvalidDataException($"Mesh ({gltfMesh.Name}) must have at least 3 positions.");

                if (p.TexCoordsCount <= texCoordSet) throw new InvalidDataException($"Mesh ({gltfMesh.Name}) must have tex coord set {texCoordSet}.");

                // Grab the data
                for (int i = 0; i < p.VertexCount; ++i)
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
                    rds.TexCoords0.Add(p.GetTextureCoord(i, texCoordSet));
                    rds.Colors.Add(color);
                    rds.SkinIndices.Add(state.JointId);
                }

                foreach (var tri in p.TriangleIndices)
                {
                    var a = tri.A + baseVertexIndex;
                    var b = tri.B + baseVertexIndex;
                    var c = tri.C + baseVertexIndex;

                    rds.Indices.Add((uint)a);
                    rds.Indices.Add((uint)b);
                    rds.Indices.Add((uint)c);
                }
            }

            mpjnElement.BoundingBox.BoundsMin = minExtent;
            mpjnElement.BoundingBox.BoundsMax = maxExtent;
            return;

            static int GetDiffuseBaseColorTexCoord(Material srcMaterial)
            {
                var channel = srcMaterial.FindChannel("Diffuse");
                if (channel.HasValue) return channel.Value.TextureCoordinate;

                channel = srcMaterial.FindChannel("BaseColor");
                if (channel.HasValue) return channel.Value.TextureCoordinate;

                return 0;
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
                        new RenderDataSourceWriter($"x{state.LodNumber}_RDS{state.RenderDataSourceCount}")));
                state.RenderDataSourceCount++;
            }
        }

        private static void WriteMeshData(ImportState state)
        {
            foreach (var shader in state.MatShaderMapping.Values)
            {
                var rds = shader.Rds;
                if (!state.ShaderGroupMap.TryGetValue(shader.ShaderGroupName, out var shaderInput))
                    throw new InvalidDataException($"The pssg does not have existing data blocks to model the layout of the input for shader {shader.ShaderGroupName}.");

                rds.Write(shaderInput, state.RdsLib, state.RibLib, state);
            }
        }
    }
}
