using System.Numerics;
using System.Text.RegularExpressions;
using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.Graphics.Pssg;
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

            public Dictionary<string, ShaderInputInfo> ShaderGroupMap { get; }

            public Dictionary<int, ShaderInstanceData> MatShaderMapping { get; }

            [GeneratedRegex("^LOD([0-9]+)_$", RegexOptions.CultureInvariant)]
            public partial Regex LodMatcher { get; }

            public ImportState(Dictionary<string, ShaderInputInfo> shaderGroupMap)
            {
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
            var rsiNodes = pssg.FindElements("RENDERSTREAMINSTANCE");
            return rsiNodes.Any() && rsiNodes.First().ParentElement?.Name == "MATRIXPALETTENODE";
        }

        public void Convert(ModelRoot gltf, PssgFile pssg)
        {
            // Get a list of nodes in the default scene as a flat list
            Dictionary<int, int> nodeBoneIndexMap = new Dictionary<int, int>();
            var rootNode = gltf.DefaultScene.FindNode(n => n.Name.StartsWith("Scene Root"));
            if (rootNode is null)
                throw new InvalidDataException("The default scene must have node name starting with `Scene Root`.");

            // Determine libraries in which to store data
            var nodeLib = pssg.FindElements("LIBRARY", "type", "NODE").First();
            var rdsLib = pssg.FindElements("LIBRARY", "type", "RENDERDATASOURCE").First();
            var ribLib = pssg.FindElements("LIBRARY", "type", "RENDERINTERFACEBOUND").First();

            var state = new ImportState(ShaderInputInfo.CreateFromPssg(pssg).ToDictionary(si => si.ShaderGroupId));

            // Clear out the libraries
            nodeLib.RemoveChildElements(nodeLib.ChildElements.Where(n => n.Name == "ROOTNODE"));
            rdsLib.RemoveChildElements(rdsLib.ChildElements.Where(n => n.Name == "RENDERDATASOURCE"));
            ribLib.RemoveChildElements(ribLib.ChildElements.Where(n => n.Name == "DATABLOCK"));

            // Write the scene graph, and collect mesh data
            ConvertSceneNodes(pssg, nodeLib, rootNode, state);
        }

        private static void ConvertSceneNodes(PssgFile pssg, PssgElement parent, Node gltfNode, ImportState state)
        {
            PssgElement element;
            Match lodMatch;
            if (gltfNode.Name.StartsWith("Scene Root"))
            {
                element = new PssgElement("ROOTNODE", parent.File, parent);
                element.AddAttribute("stopTraversal", 0u);
                element.AddAttribute("nickname", "Scene Root");
                element.AddAttribute("id", "Scene Root");
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
                element = new PssgElement("NODE", parent.File, parent);
                element.AddAttribute("stopTraversal", 0u);
                element.AddAttribute("nickname", gltfNode.Name);
                element.AddAttribute("id", gltfNode.Name);
                parent.ChildElements.Add(element);
            }

            var transformNode = new PssgElement("TRANSFORM", element.File, element);
            transformNode.Value = GetTransform(gltfNode.LocalMatrix);
            element.ChildElements.Add(transformNode);

            var bboxNode = new PssgElement("BOUNDINGBOX", element.File, element);
            bboxNode.Value = GetBoundingBoxData(Vector3.Zero, Vector3.Zero);
            element.ChildElements.Add(bboxNode);

            foreach (var child in gltfNode.VisualChildren)
            {
                ConvertSceneNodes(pssg, element, child, state);
            }
        }

        private static PssgElement CreateMatrixPaletteBundleNode(PssgElement parent, Node gltfNode, int lodNumber, ImportState state)
        {
            PssgElement element = new PssgElement("MATRIXPALETTEBUNDLENODE", parent.File, parent);
            element.AddAttribute("stopTraversal", 0u);
            element.AddAttribute("nickname", $"LOD{lodNumber}_");
            element.AddAttribute("id", $"LOD{lodNumber}_");
            parent.ChildElements.Add(element);

            var transformNode = new PssgElement("TRANSFORM", element.File, element);
            transformNode.Value = GetTransform(gltfNode.LocalMatrix);
            element.ChildElements.Add(transformNode);

            var bboxNode = new PssgElement("BOUNDINGBOX", element.File, element);
            bboxNode.Value = GetBoundingBoxData(Vector3.Zero, Vector3.Zero);
            element.ChildElements.Add(bboxNode);

            state.LodNumber = lodNumber;
            state.JointId = 0;
            state.MatShaderMapping.Clear();

            List<string> jointNames = new List<string>();
            foreach (var child in gltfNode.VisualChildren)
            {
                if (child.Mesh is null) continue;
                if (child.Mesh.Primitives.Count == 0) continue;

                CreateMatrixPaletteJointNode(element, child, state);
                jointNames.Add(element.ChildElements.Last().Attributes["id"].GetValue<string>());
            }

            CreateMatrixPaletteNode(element, jointNames, state);

            // Write the mesh data
            var rdsLib = element.File.FindElements("LIBRARY", "type", "RENDERDATASOURCE").First();
            var ribLib = element.File.FindElements("LIBRARY", "type", "RENDERINTERFACEBOUND").First();
            WriteMeshData(rdsLib, ribLib, state);

            return element;
        }

        private static void CreateMatrixPaletteNode(PssgElement parent, List<string> jointNames, ImportState state)
        {
            var node = new PssgElement("MATRIXPALETTENODE", parent.File, parent);
            node.AddAttribute("jointCount", state.JointId);
            node.AddAttribute("stopTraversal", 0u);
            node.AddAttribute("id", $"x{state.LodNumber}_MPN");
            parent.ChildElements.Add(node);

            var transformNode = new PssgElement("TRANSFORM", node.File, node);
            transformNode.Value = GetTransform(Matrix4x4.Identity);
            node.ChildElements.Add(transformNode);

            var bboxNode = new PssgElement("BOUNDINGBOX", node.File, node);
            bboxNode.Value = GetBoundingBoxData(Vector3.Zero, Vector3.Zero);
            node.ChildElements.Add(bboxNode);

            foreach (var shader in state.MatShaderMapping.Values)
            {
                var rsiNode = new PssgElement("RENDERSTREAMINSTANCE", node.File, node);
                rsiNode.AddAttribute("sourceCount", 1u);
                rsiNode.AddAttribute("indices", $"#{shader.Rds.Name}");
                rsiNode.AddAttribute("streamCount", 0u);
                rsiNode.AddAttribute("shader", $"#{shader.ShaderInstanceName}");
                rsiNode.AddAttribute("id", shader.Rds.Name.Replace("RDS", "RSI"));
                node.ChildElements.Add(rsiNode);

                var risNode = new PssgElement("RENDERINSTANCESOURCE", rsiNode.File, rsiNode);
                risNode.AddAttribute("source", $"#{shader.Rds.Name}");
                rsiNode.ChildElements.Add(risNode);
            }

            foreach (var jointName in jointNames)
            {
                var mpsjNode = new PssgElement("MATRIXPALETTESKINJOINT", node.File, node);
                mpsjNode.AddAttribute("joint", $"#{jointName}");
                node.ChildElements.Add(mpsjNode);
            }
        }

        private static void CreateMatrixPaletteJointNode(PssgElement parent, Node gltfNode, ImportState state)
        {
            var node = new PssgElement("MATRIXPALETTEJOINTNODE", parent.File, parent);
            node.AddAttribute("stopTraversal", 0u);
            node.AddAttribute("nickname", gltfNode.Name);
            node.AddAttribute("id", gltfNode.Name);
            node.AddAttribute("jointID", state.JointId);
            node.AddAttribute("matrixPalette", $"#x{state.LodNumber}_MPN");
            parent.ChildElements.Add(node);

            // Now add a new mesh from mesh builder
            ConvertMesh(node, gltfNode, state);

            state.JointId++;
        }

        private static void ConvertMesh(PssgElement mpjnElement, Node gltfNode, ImportState state)
        {
            var mesh = gltfNode.Mesh;
            if (mesh.Primitives.Any(p => p.Material == null)) throw new NotImplementedException($"The converter does not support primitives ({mesh.Name}) with a null material.");

            var transformNode = new PssgElement("TRANSFORM", mpjnElement.File, mpjnElement);
            transformNode.Value = GetTransform(gltfNode.LocalMatrix);
            mpjnElement.ChildElements.Add(transformNode);

            var bboxNode = new PssgElement("BOUNDINGBOX", mpjnElement.File, mpjnElement);
            mpjnElement.ChildElements.Add(bboxNode);

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

                var mpriNode = new PssgElement("MATRIXPALETTERENDERINSTANCE", mpjnElement.File, mpjnElement);
                mpriNode.AddAttribute("streamOffset", (uint)(rds.Positions.Count));
                mpriNode.AddAttribute("elementCountFromOffset", (uint)(p.VertexCount));
                mpriNode.AddAttribute("indexOffset", (uint)(rds.Indices.Count));
                mpriNode.AddAttribute("indicesCountFromOffset", (uint)(tris.Length * 3));
                mpriNode.AddAttribute("sourceCount", 1u);
                mpriNode.AddAttribute("indices", $"#{rds.Name}");
                mpriNode.AddAttribute("streamCount", 0u);
                mpriNode.AddAttribute("shader", $"#{shaderData.ShaderInstanceName}");
                mpriNode.AddAttribute("id", $"MPJRI{state.MpjriCount}"); state.MpjriCount++;
                mpjnElement.ChildElements.Add(mpriNode);

                var risNode = new PssgElement("RENDERINSTANCESOURCE", mpriNode.File, mpriNode);
                risNode.AddAttribute("source", $"#{shaderData.Rds.Name}");
                mpriNode.ChildElements.Add(risNode);

                var texCoordSet = GetDiffuseBaseColorTexCoord(p.Material);
                string texCoordAccessorName = $"TEXCOORD_{texCoordSet}";

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

            bboxNode.Value = GetBoundingBoxData(minExtent, maxExtent);

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
            var shaders = pssg.FindElements("SHADERINSTANCE");
            foreach (var gltfMat in gltfMats)
            {
                if (state.MatShaderMapping.ContainsKey(gltfMat.LogicalIndex))
                    continue;

                // Find shader instance with same name as mat
                var shader = shaders.FirstOrDefault(n => n.Attributes["id"].GetValue<string>() == gltfMat.Name);
                if (shader is null)
                    throw new InvalidDataException($"The pssg must already contain a shader instance with name {gltfMat.Name}.");

                state.MatShaderMapping.Add(gltfMat.LogicalIndex,
                    new ShaderInstanceData(
                        shader.Attributes["id"].GetValue<string>(),
                        shader.Attributes["shaderGroup"].GetValue<string>().Substring(1),
                        new RenderDataSourceWriter($"x{state.LodNumber}_RDS{state.RenderDataSourceCount}")));
                state.RenderDataSourceCount++;
            }
        }

        private static void WriteMeshData(PssgElement rdsLib, PssgElement ribLib, ImportState state)
        {
            foreach (var shader in state.MatShaderMapping.Values)
            {
                var rds = shader.Rds;
                if (!state.ShaderGroupMap.TryGetValue(shader.ShaderGroupName, out var shaderInput))
                    throw new InvalidDataException($"The pssg does not have existing data blocks to model the layout of the input for shader {shader.ShaderGroupName}.");

                rds.Write(shaderInput, rdsLib, ribLib, state);
            }
        }

        private static byte[] GetBoundingBoxData(Vector3 min, Vector3 max)
        {
            byte[] buffer = new byte[6 * 4];
            BigEndianBitConverter bc = new BigEndianBitConverter();

            int i = 0;
            bc.CopyBytes(min.X, buffer, i); i += 4;
            bc.CopyBytes(min.Y, buffer, i); i += 4;
            bc.CopyBytes(min.Z, buffer, i); i += 4;

            bc.CopyBytes(max.X, buffer, i); i += 4;
            bc.CopyBytes(max.Y, buffer, i); i += 4;
            bc.CopyBytes(max.Z, buffer, i); i += 4;

            return buffer;
        }

        private static byte[] GetTransform(Matrix4x4 t)
        {
            byte[] buffer = new byte[16 * 4];
            BigEndianBitConverter bc = new BigEndianBitConverter();

            int i = 0;
            bc.CopyBytes(t.M11, buffer, i); i += 4;
            bc.CopyBytes(t.M12, buffer, i); i += 4;
            bc.CopyBytes(t.M13, buffer, i); i += 4;
            bc.CopyBytes(t.M14, buffer, i); i += 4;

            bc.CopyBytes(t.M21, buffer, i); i += 4;
            bc.CopyBytes(t.M22, buffer, i); i += 4;
            bc.CopyBytes(t.M23, buffer, i); i += 4;
            bc.CopyBytes(t.M24, buffer, i); i += 4;

            bc.CopyBytes(t.M31, buffer, i); i += 4;
            bc.CopyBytes(t.M32, buffer, i); i += 4;
            bc.CopyBytes(t.M33, buffer, i); i += 4;
            bc.CopyBytes(t.M34, buffer, i); i += 4;

            bc.CopyBytes(t.M41, buffer, i); i += 4;
            bc.CopyBytes(t.M42, buffer, i); i += 4;
            bc.CopyBytes(t.M43, buffer, i); i += 4;
            bc.CopyBytes(t.M44, buffer, i); i += 4;

            return buffer;
        }
    }
}
