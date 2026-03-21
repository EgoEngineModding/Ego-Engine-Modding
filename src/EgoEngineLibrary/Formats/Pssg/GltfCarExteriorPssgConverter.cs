using System.Numerics;
using System.Text.RegularExpressions;
using EgoEngineLibrary.Graphics.Pssg;
using EgoEngineLibrary.Graphics.Pssg.Elements;
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;

namespace EgoEngineLibrary.Formats.Pssg
{
    // Starting with Dirt 2 and F1 2010:
    // - the MatrixPaletteNode has MatrixPaletteRenderInstance instead of RenderStreamInstance
    // - MatrixPaletteRenderInstance now holds the jointCount and MatrixPaletteSkinJoint instead
    // - the MatrixPaletteJoinNode has MatrixPaletteJointRenderInstance instead of MatrixPaletteRenderInstance
    // - MatrixPaletteJointRenderInstance now holds the jointId instead (jointId is now unique per shader instead of per MatrixPaletteBundleNode)
    // - The DataBlocks are rearranged in how they hold the data, and ST is float4 instead of float2 (assuming this means 2 sets of tex coords)
    // Starting with Dirt 3:
    // - The DataBlocks with ST/Tangent/Binormal now use half4/half4/half4 instead of float4/float3/float3
    public partial class GltfCarExteriorPssgConverter
    {
        private partial class ImportState : PssgModelWriterState
        {
            public int LodNumber { get; set; }

            public int MpjriCount { get; set; }

            public int RenderDataSourceCount { get; set; }

            public PssgElement RdsLib { get; }

            public PssgElement RibLib { get; }

            public bool IsF1 { get; }

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

                if (rdsLib == ribLib)
                    IsF1 = true;
            }
        }
        private class ShaderInstanceData
        {
            public string ShaderInstanceName { get; }

            public string ShaderGroupName { get; }

            public RenderDataSourceWriter Rds { get; }

            public List<string> JointNames { get; }

            public ShaderInstanceData(string shaderInstanceName, string shaderGroupName, RenderDataSourceWriter rds)
            {
                ShaderInstanceName = shaderInstanceName;
                ShaderGroupName = shaderGroupName;
                Rds = rds;
                JointNames = new List<string>();
            }
        }

        public static bool SupportsPssg(PssgFile pssg)
        {
            return pssg.Elements<PssgMatrixPaletteJointRenderInstance>().Any();
        }

        public void Convert(ModelRoot gltf, PssgFile pssg)
        {
            // Get a list of nodes in the default scene as a flat list
            var rootNode = gltf.DefaultScene.FindNode(n => n.Name.StartsWith("Scene Root"));
            if (rootNode is null)
                throw new InvalidDataException("The default scene must have node name starting with `Scene Root`.");

            // Determine libraries in which to store data
            var nodeLib = pssg.Elements<PssgLibrary>().FirstOrDefault(x => x.Type == "NODE");
            PssgElement rdsLib; PssgElement ribLib;
            if (nodeLib is not null)
            {
                rdsLib = pssg.Elements<PssgLibrary>().First(x => x.Type == "RENDERDATASOURCE");
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

            var state = new ImportState(rdsLib, ribLib, ShaderInputInfo.CreateFromPssg(pssg).ToDictionary(si => si.ShaderGroupId));

            // Clear out the libraries
            nodeLib.RemoveChildElements(nodeLib.ChildElements.Where(n => n.IsExactType<PssgRootNode>()));
            rdsLib.RemoveChildElements(rdsLib.ChildElements.Where(n => n.IsExactType<PssgRenderDataSource>()));
            ribLib.RemoveChildElements(ribLib.ChildElements.Where(n => n.IsExactType<PssgDataBlock>()));

            // Write the scene graph, and collect mesh data
            ConvertSceneNodes(pssg, nodeLib, rootNode, state);

            // Seems in Dirt Rally 2.0 there is a bunch of useless data in lib SEGMENTSET
            // lets get rid of it
            var ssLibNode = pssg.Elements<PssgLibrary>().FirstOrDefault(x => x.Type == "SEGMENTSET");
            ssLibNode?.ParentElement?.RemoveChild(ssLibNode);
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
            PssgNode element = new PssgMatrixPaletteBundleNode(parent.File, parent);
            element.StopTraversal = false;
            element.Nickname = $"LOD{lodNumber}_";
            element.Id = $"LOD{lodNumber}_";
            parent.ChildElements.Add(element);

            element.Initialize();
            element.Transform.Transform = gltfNode.LocalMatrix;
            element.BoundingBox.BoundsMin = Vector3.Zero;
            element.BoundingBox.BoundsMax = Vector3.Zero;

            state.LodNumber = lodNumber;
            state.MatShaderMapping.Clear();

            foreach (var child in gltfNode.VisualChildren)
            {
                if (child.Mesh is null) continue;
                if (child.Mesh.Primitives.Count == 0) continue;

                CreateMatrixPaletteJointNode(element, child, state);
            }

            CreateMatrixPaletteNode(element, state);

            // Write the mesh data
            WriteMeshData(state);

            return element;
        }

        private static void CreateMatrixPaletteNode(PssgElement parent, ImportState state)
        {
            var node = new PssgMatrixPaletteNode(parent.File, parent);
            node.StopTraversal = false;
            node.Id = $"x{state.LodNumber}_MPN";
            parent.ChildElements.Add(node);

            node.Initialize();
            node.Transform.Transform = Matrix4x4.Identity;
            node.BoundingBox.BoundsMin = Vector3.Zero;
            node.BoundingBox.BoundsMax = Vector3.Zero;

            foreach (var shader in state.MatShaderMapping.Values)
            {
                var rsiNode = new PssgMatrixPaletteRenderInstance(node.File, node);
                rsiNode.JointCount = (uint)shader.JointNames.Count;
                rsiNode.SourceCount = 1u;
                rsiNode.Indices = $"#{shader.Rds.Name}";
                rsiNode.StreamCount = 0;
                rsiNode.Shader = $"#{shader.ShaderInstanceName}";
                rsiNode.Id = shader.Rds.Name.Replace("RDS", "RSI");
                node.ChildElements.Add(rsiNode);

                var risNode = new PssgRenderInstanceSource(rsiNode.File, rsiNode);
                risNode.Source = $"#{shader.Rds.Name}";
                rsiNode.ChildElements.Add(risNode);

                foreach (var jointName in shader.JointNames)
                {
                    var mpsjNode = new PssgMatrixPaletteSkinJoint(rsiNode.File, rsiNode);
                    mpsjNode.Joint = $"#{jointName}";
                    rsiNode.ChildElements.Add(mpsjNode);
                }
            }
        }

        private static void CreateMatrixPaletteJointNode(PssgElement parent, Node gltfNode, ImportState state)
        {
            var node = new PssgMatrixPaletteJointNode(parent.File, parent);
            node.MatrixPalette = $"#x{state.LodNumber}_MPN";
            node.StopTraversal = false;
            node.Nickname = gltfNode.Name;
            node.Id = gltfNode.Name;
            parent.ChildElements.Add(node);

            // Now add a new mesh from mesh builder
            ConvertMesh(node, gltfNode, state);
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
                if (!state.MatShaderMapping.TryGetValue(faceMatId, out ShaderInstanceData? shaderData))
                {
                    throw new InvalidDataException($"Mesh ({gltfMesh.Name}) has an invalid material id " + faceMatId + ".");
                }

                var rds = shaderData.Rds;
                var tris = p.TriangleIndices.ToArray();
                var baseVertexIndex = rds.Positions.Count;

                var mpriNode = new PssgMatrixPaletteJointRenderInstance(mpjnElement.File, mpjnElement);
                mpriNode.StreamOffset = (uint)(rds.Positions.Count);
                mpriNode.ElementCountFromOffset = (uint)(p.VertexCount);
                mpriNode.IndexOffset = (uint)(rds.Indices.Count);
                mpriNode.IndicesCountFromOffset = (uint)(tris.Length * 3);
                mpriNode.JointId = System.Convert.ToUInt16(shaderData.JointNames.Count);
                mpriNode.SourceCount = 1u;
                mpriNode.Indices = $"#{rds.Name}";
                mpriNode.StreamCount = 0;
                mpriNode.Shader = $"#{shaderData.ShaderInstanceName}";
                mpriNode.Id = $"MPJRI{state.MpjriCount}"; state.MpjriCount++;
                mpjnElement.ChildElements.Add(mpriNode);

                var risNode = new PssgRenderInstanceSource(mpriNode.File, mpriNode);
                risNode.Source = $"#{shaderData.Rds.Name}";
                mpriNode.ChildElements.Add(risNode);

                var texCoordSet0 = GetDiffuseBaseColorTexCoord(p.Material);
                var texCoordSet1 = GetOcclusionTexCoord(p.Material);
                var texCoordSet2 = GetEmissiveTexCoord(p.Material);
                var texCoordSet3 = GetNormalTexCoord(p.Material);

                if (state.IsF1)
                {
                    // F1 stores spec occ first, then diffuse
                    (texCoordSet0, texCoordSet1) = (texCoordSet1, texCoordSet0);
                }

                // Make sure we have all the necessary data
                if (p.VertexCount < 3) throw new InvalidDataException($"Mesh ({gltfMesh.Name}) must have at least 3 positions.");

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
                    rds.TexCoords0.Add(p.GetTextureCoord(i, texCoordSet0));
                    rds.TexCoords1.Add(p.GetTextureCoord(i, texCoordSet1));
                    rds.TexCoords2.Add(p.GetTextureCoord(i, texCoordSet2));
                    rds.TexCoords3.Add(p.GetTextureCoord(i, texCoordSet3));
                    rds.Colors.Add(color);
                    rds.SkinIndices.Add(shaderData.JointNames.Count);
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

                // Add the matrixpalletejointnode id to the shader's joint list
                shaderData.JointNames.Add(gltfNode.Name);
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
                        new RenderDataSourceWriter($"x{state.LodNumber}_RDS{state.RenderDataSourceCount}")));
                state.RenderDataSourceCount++;
            }
        }

        private static void WriteMeshData(ImportState state)
        {
            foreach (var shader in state.MatShaderMapping.Values)
            {
                var rds = shader.Rds;
                if (!state.ShaderGroupMap.TryGetValue(shader.ShaderGroupName, out var shaderInputInfo))
                    throw new InvalidDataException($"The pssg does not have existing data blocks to model the layout of the input for shader {shader.ShaderGroupName}.");

                rds.Write(shaderInputInfo, state.RdsLib, state.RibLib, state);
            }
        }
    }
}
