using EgoEngineLibrary.Graphics;
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

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
    public class GltfCarExteriorPssgConverter
    {
        private class ImportState : PssgModelWriterState
        {
            public int LodNumber { get; set; }

            public int MpjriCount { get; set; }

            public int RenderDataSourceCount { get; set; }

            public PssgNode RdsLib { get; }

            public PssgNode RibLib { get; }

            public bool IsF1 { get; }

            public Dictionary<string, ShaderInputInfo> ShaderGroupMap { get; }

            public Dictionary<int, ShaderInstanceData> MatShaderMapping { get; }

            public Regex LodMatcher { get; }

            public ImportState(PssgNode rdsLib, PssgNode ribLib, Dictionary<string, ShaderInputInfo> shaderGroupMap)
            {
                RdsLib = rdsLib;
                RibLib = ribLib;
                ShaderGroupMap = shaderGroupMap;
                MatShaderMapping = new Dictionary<int, ShaderInstanceData>();
                LodMatcher = new Regex("^LOD([0-9]+)_$", RegexOptions.CultureInvariant);

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
            return pssg.FindNodes("MATRIXPALETTEJOINTRENDERINSTANCE").Any();
        }

        public void Convert(ModelRoot gltf, PssgFile pssg)
        {
            // Get a list of nodes in the default scene as a flat list
            Dictionary<int, int> nodeBoneIndexMap = new Dictionary<int, int>();
            var rootNode = gltf.DefaultScene.FindNode(n => n.Name.StartsWith("Scene Root"));
            if (rootNode is null)
                throw new InvalidDataException("The default scene must have node name starting with `Scene Root`.");

            // Determine libraries in which to store data
            var nodeLib = pssg.FindNodes("LIBRARY", "type", "NODE").FirstOrDefault();
            PssgNode rdsLib; PssgNode ribLib;
            if (nodeLib is not null)
            {
                rdsLib = pssg.FindNodes("LIBRARY", "type", "RENDERDATASOURCE").First();
                ribLib = pssg.FindNodes("LIBRARY", "type", "RENDERINTERFACEBOUND").First();
            }
            else
            {
                // F1 games use YYY, and put almost everything in this lib
                nodeLib = pssg.FindNodes("LIBRARY", "type", "YYY").FirstOrDefault();
                if (nodeLib is null)
                    throw new InvalidDataException("Could not find library with scene nodes.");

                rdsLib = nodeLib;
                ribLib = nodeLib;
            }

            var state = new ImportState(rdsLib, ribLib, ShaderInputInfo.CreateFromPssg(pssg).ToDictionary(si => si.ShaderGroupId));

            // Clear out the libraries
            nodeLib.RemoveChildNodes(nodeLib.ChildNodes.Where(n => n.Name == "ROOTNODE"));
            rdsLib.RemoveChildNodes(rdsLib.ChildNodes.Where(n => n.Name == "RENDERDATASOURCE"));
            ribLib.RemoveChildNodes(ribLib.ChildNodes.Where(n => n.Name == "DATABLOCK"));

            // Write the scene graph, and collect mesh data
            ConvertSceneNodes(pssg, nodeLib, rootNode, state);

            // Seems in Dirt Rally 2.0 there is a bunch of useless data in lib SEGMENTSET
            // lets get rid of it
            var ssLibNode = pssg.FindNodes("LIBRARY", "type", "SEGMENTSET").FirstOrDefault();
            if (ssLibNode is not null)
                ssLibNode.ParentNode?.RemoveChild(ssLibNode);
        }

        private static void ConvertSceneNodes(PssgFile pssg, PssgNode parent, Node gltfNode, ImportState state)
        {
            PssgNode node;
            Match lodMatch;
            if (gltfNode.Name.StartsWith("Scene Root"))
            {
                node = new PssgNode("ROOTNODE", parent.File, parent);
                node.AddAttribute("stopTraversal", 0u);
                node.AddAttribute("nickname", "Scene Root");
                node.AddAttribute("id", "Scene Root");
                parent.ChildNodes.Add(node);
            }
            else if ((lodMatch = state.LodMatcher.Match(gltfNode.Name)).Success)
            {
                var lodNumber = int.Parse(lodMatch.Groups[1].Value);
                node = CreateMatrixPaletteBundleNode(parent, gltfNode, lodNumber, state);
                return;
            }
            else
            {
                node = new PssgNode("NODE", parent.File, parent);
                node.AddAttribute("stopTraversal", 0u);
                node.AddAttribute("nickname", gltfNode.Name);
                node.AddAttribute("id", gltfNode.Name);
                parent.ChildNodes.Add(node);
            }

            var transformNode = new PssgNode("TRANSFORM", node.File, node);
            transformNode.Value = GetTransform(gltfNode.LocalMatrix);
            node.ChildNodes.Add(transformNode);

            var bboxNode = new PssgNode("BOUNDINGBOX", node.File, node);
            bboxNode.Value = GetBoundingBoxData(Vector3.Zero, Vector3.Zero);
            node.ChildNodes.Add(bboxNode);

            foreach (var child in gltfNode.VisualChildren)
            {
                ConvertSceneNodes(pssg, node, child, state);
            }
        }

        private static PssgNode CreateMatrixPaletteBundleNode(PssgNode parent, Node gltfNode, int lodNumber, ImportState state)
        {
            PssgNode node = new PssgNode("MATRIXPALETTEBUNDLENODE", parent.File, parent);
            node.AddAttribute("stopTraversal", 0u);
            node.AddAttribute("nickname", $"LOD{lodNumber}_");
            node.AddAttribute("id", $"LOD{lodNumber}_");
            parent.ChildNodes.Add(node);

            var transformNode = new PssgNode("TRANSFORM", node.File, node);
            transformNode.Value = GetTransform(gltfNode.LocalMatrix);
            node.ChildNodes.Add(transformNode);

            var bboxNode = new PssgNode("BOUNDINGBOX", node.File, node);
            bboxNode.Value = GetBoundingBoxData(Vector3.Zero, Vector3.Zero);
            node.ChildNodes.Add(bboxNode);

            state.LodNumber = lodNumber;
            state.MatShaderMapping.Clear();

            foreach (var child in gltfNode.VisualChildren)
            {
                if (child.Mesh is null) continue;
                if (child.Mesh.Primitives.Count == 0) continue;

                CreateMatrixPaletteJointNode(node, child, state);
            }

            CreateMatrixPaletteNode(node, state);

            // Write the mesh data
            WriteMeshData(state);

            return node;
        }

        private static void CreateMatrixPaletteNode(PssgNode parent, ImportState state)
        {
            var node = new PssgNode("MATRIXPALETTENODE", parent.File, parent);
            node.AddAttribute("stopTraversal", 0u);
            node.AddAttribute("id", $"x{state.LodNumber}_MPN");
            parent.ChildNodes.Add(node);

            var transformNode = new PssgNode("TRANSFORM", node.File, node);
            transformNode.Value = GetTransform(Matrix4x4.Identity);
            node.ChildNodes.Add(transformNode);

            var bboxNode = new PssgNode("BOUNDINGBOX", node.File, node);
            bboxNode.Value = GetBoundingBoxData(Vector3.Zero, Vector3.Zero);
            node.ChildNodes.Add(bboxNode);

            foreach (var shader in state.MatShaderMapping.Values)
            {
                var rsiNode = new PssgNode("MATRIXPALETTERENDERINSTANCE", node.File, node);
                rsiNode.AddAttribute("jointCount", (uint)shader.JointNames.Count);
                rsiNode.AddAttribute("sourceCount", 1u);
                rsiNode.AddAttribute("indices", $"#{shader.Rds.Name}");
                rsiNode.AddAttribute("streamCount", 0u);
                rsiNode.AddAttribute("shader", $"#{shader.ShaderInstanceName}");
                rsiNode.AddAttribute("id", shader.Rds.Name.Replace("RDS", "RSI"));
                node.ChildNodes.Add(rsiNode);

                var risNode = new PssgNode("RENDERINSTANCESOURCE", rsiNode.File, rsiNode);
                risNode.AddAttribute("source", $"#{shader.Rds.Name}");
                rsiNode.ChildNodes.Add(risNode);

                foreach (var jointName in shader.JointNames)
                {
                    var mpsjNode = new PssgNode("MATRIXPALETTESKINJOINT", rsiNode.File, rsiNode);
                    mpsjNode.AddAttribute("joint", $"#{jointName}");
                    rsiNode.ChildNodes.Add(mpsjNode);
                }
            }
        }

        private static void CreateMatrixPaletteJointNode(PssgNode parent, Node gltfNode, ImportState state)
        {
            var node = new PssgNode("MATRIXPALETTEJOINTNODE", parent.File, parent);
            node.AddAttribute("matrixPalette", $"#x{state.LodNumber}_MPN");
            node.AddAttribute("stopTraversal", 0u);
            node.AddAttribute("nickname", gltfNode.Name);
            node.AddAttribute("id", gltfNode.Name);
            parent.ChildNodes.Add(node);

            // Now add a new mesh from mesh builder
            ConvertMesh(node, gltfNode, state);
        }

        private static void ConvertMesh(PssgNode mpjnNode, Node gltfNode, ImportState state)
        {
            var mesh = gltfNode.Mesh;
            if (mesh.Primitives.Any(p => p.Material == null)) throw new NotImplementedException($"The converter does not support primitives ({mesh.Name}) with a null material.");

            var transformNode = new PssgNode("TRANSFORM", mpjnNode.File, mpjnNode);
            transformNode.Value = GetTransform(gltfNode.LocalMatrix);
            mpjnNode.ChildNodes.Add(transformNode);

            var bboxNode = new PssgNode("BOUNDINGBOX", mpjnNode.File, mpjnNode);
            mpjnNode.ChildNodes.Add(bboxNode);

            // Add to the material shader mapping
            var gltfMats = mesh.Primitives.Select(p => p.Material);
            ConvertMaterials(gltfMats, mpjnNode.File, state);

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

                var mpriNode = new PssgNode("MATRIXPALETTEJOINTRENDERINSTANCE", mpjnNode.File, mpjnNode);
                mpriNode.AddAttribute("streamOffset", (uint)(rds.Positions.Count));
                mpriNode.AddAttribute("elementCountFromOffset", (uint)(p.VertexCount));
                mpriNode.AddAttribute("indexOffset", (uint)(rds.Indices.Count));
                mpriNode.AddAttribute("indicesCountFromOffset", (uint)(tris.Length * 3));
                mpriNode.AddAttribute("jointID", (uint)shaderData.JointNames.Count);
                mpriNode.AddAttribute("sourceCount", 1u);
                mpriNode.AddAttribute("indices", $"#{rds.Name}");
                mpriNode.AddAttribute("streamCount", 0u);
                mpriNode.AddAttribute("shader", $"#{shaderData.ShaderInstanceName}");
                mpriNode.AddAttribute("id", $"MPJRI{state.MpjriCount}"); state.MpjriCount++;
                mpjnNode.ChildNodes.Add(mpriNode);

                var risNode = new PssgNode("RENDERINSTANCESOURCE", mpriNode.File, mpriNode);
                risNode.AddAttribute("source", $"#{shaderData.Rds.Name}");
                mpriNode.ChildNodes.Add(risNode);

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

            bboxNode.Value = GetBoundingBoxData(minExtent, maxExtent);

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
            var shaders = pssg.FindNodes("SHADERINSTANCE");
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

        private static byte[] GetBoundingBoxData(Vector3 min, Vector3 max)
        {
            byte[] buffer = new byte[6 * 4];
            MiscUtil.Conversion.BigEndianBitConverter bc = new MiscUtil.Conversion.BigEndianBitConverter();

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
            MiscUtil.Conversion.BigEndianBitConverter bc = new MiscUtil.Conversion.BigEndianBitConverter();

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
