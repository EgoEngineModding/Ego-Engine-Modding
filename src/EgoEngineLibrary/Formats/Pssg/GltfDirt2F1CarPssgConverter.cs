using EgoEngineLibrary.Graphics;
using MiscUtil.Conversion;
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;
using System;
using System.Buffers.Binary;
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
    public class GltfDirt2F1CarPssgConverter
    {
        private class ImportState
        {
            public int LodNumber { get; set; }

            public int MpjriCount { get; set; }

            public int RenderDataSourceCount { get; set; }

            public uint DataBlockCount { get; set; }

            public uint RenderStreamCount { get; set; }

            public PssgNode RdsLib { get; }

            public PssgNode RibLib { get; }

            public bool IsF1 { get; }

            public Dictionary<int, ShaderInstanceData> MatShaderMapping { get; }

            public Dictionary<string, ShaderBlockInputInfo[]> ShaderGroupMap { get; }

            public Regex LodMatcher { get; }

            public ImportState(PssgNode rdsLib, PssgNode ribLib)
            {
                RdsLib = rdsLib;
                RibLib = ribLib;
                MatShaderMapping = new Dictionary<int, ShaderInstanceData>();
                ShaderGroupMap = new Dictionary<string, ShaderBlockInputInfo[]>();
                LodMatcher = new Regex("^LOD([0-9]+)_$", RegexOptions.CultureInvariant);

                if (rdsLib == ribLib)
                    IsF1 = true;
            }
        }
        private class ShaderInstanceData
        {
            public string ShaderInstanceName { get; }

            public string ShaderGroupName { get; }

            public RenderDataSource Rds { get; }

            public List<string> JointNames { get; }

            public ShaderInstanceData(string shaderInstanceName, string shaderGroupName, RenderDataSource rds)
            {
                ShaderInstanceName = shaderInstanceName;
                ShaderGroupName = shaderGroupName;
                Rds = rds;
                JointNames = new List<string>();
            }
        }
        private record ShaderBlockInputInfo(List<ShaderVertexInputInfo> VertexInputs);
        private record ShaderVertexInputInfo(string Name, string DataType, uint Offset, uint Stride);

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

            // Clear libraries
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

            var state = new ImportState(rdsLib, ribLib);

            // Figure out the layout of the vertex data for each shader group
            var rdsNodes = pssg.FindNodes("RENDERDATASOURCE");
            foreach (var rdsNode in rdsNodes)
            {
                GetShaderInfo(rdsNode, state);
            }

            // Clear out the libraries
            nodeLib.RemoveChildNodes(nodeLib.ChildNodes.Where(n => n.Name == "ROOTNODE"));
            rdsLib.RemoveChildNodes(rdsLib.ChildNodes.Where(n => n.Name == "RENDERDATASOURCE"));
            ribLib.RemoveChildNodes(ribLib.ChildNodes.Where(n => n.Name == "DATABLOCK"));

            // Write the scene graph, and collect mesh data
            ConvertSceneNodes(pssg, nodeLib, rootNode, state);
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
                    rds.Colors.Add(PackColor(color));
                    rds.SkinIndices.Add(shaderData.JointNames.Count);
                }

                foreach (var tri in p.TriangleIndices)
                {
                    var a = tri.A + baseVertexIndex;
                    var b = tri.B + baseVertexIndex;
                    var c = tri.C + baseVertexIndex;

                    rds.Indices.Add((ushort)a);
                    rds.Indices.Add((ushort)b);
                    rds.Indices.Add((ushort)c);
                }

                // Add the matrixpalletejointnode id to the shader's joint list
                shaderData.JointNames.Add(gltfNode.Name);
            }

            bboxNode.Value = GetBoundingBoxData(minExtent, maxExtent);

            static uint PackColor(Vector4 vector)
            {
                Vector4 MaxBytes = new Vector4(byte.MaxValue);
                Vector4 Half = new Vector4(0.5f);
                vector *= MaxBytes;
                vector += Half;
                vector = Vector4.Clamp(vector, Vector4.Zero, MaxBytes);

                return (uint)((((byte)vector.W) << 0) | (((byte)vector.X) << 8) | (((byte)vector.Y) << 16) | (((byte)vector.Z) << 24));
            }
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

                return 0;
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
                        new RenderDataSource(state.LodNumber, state.RenderDataSourceCount)));
                state.RenderDataSourceCount++;
            }
        }

        private static void GetShaderInfo(PssgNode rdsNode, ImportState state)
        {
            var rdsId = rdsNode.Attributes["id"].GetValue<string>();
            var risNode = rdsNode.File.FindNodes("RENDERINSTANCESOURCE", "source", '#' + rdsId).First();
            var shaderInstanceId = risNode.ParentNode?.Attributes["shader"].GetValue<string>().Substring(1);
            if (shaderInstanceId is null)
                return;

            var siNode = rdsNode.File.FindNodes("SHADERINSTANCE", "id", shaderInstanceId).First();
            var shaderGroupId = siNode.Attributes["shaderGroup"].GetValue<string>().Substring(1);
            var sgNode = rdsNode.File.FindNodes("SHADERGROUP", "id", shaderGroupId).FirstOrDefault();
            if (sgNode is null)
                return;
            if (state.ShaderGroupMap.ContainsKey(shaderGroupId))
                return;

            var dataBlockIdMap = new Dictionary<string, ShaderBlockInputInfo>();
            var renderStreamNodes = rdsNode.FindNodes("RENDERSTREAM");
            foreach (var rsNode in renderStreamNodes)
            {
                var dbId = rsNode.Attributes["dataBlock"].GetValue<string>().Substring(1);
                var subStream = rsNode.Attributes["subStream"].GetValue<uint>();

                var dbNode = rsNode.File.FindNodes("DATABLOCK", "id", dbId).First();
                var dbStreamNode = dbNode.ChildNodes[(int)subStream];

                var renderType = dbStreamNode.Attributes["renderType"].GetValue<string>();
                var offset = dbStreamNode.Attributes["offset"].GetValue<uint>();
                var stride = dbStreamNode.Attributes["stride"].GetValue<uint>();
                var dataType = dbStreamNode.Attributes["dataType"].GetValue<string>();

                var vi = new ShaderVertexInputInfo(renderType, dataType, offset, stride);
                if (dataBlockIdMap.TryGetValue(dbId, out var bi))
                {
                    bi.VertexInputs.Add(vi);
                }
                else
                {
                    bi = new ShaderBlockInputInfo(new List<ShaderVertexInputInfo>());
                    bi.VertexInputs.Add(vi);
                    dataBlockIdMap.Add(dbId, bi);
                }
            }

            state.ShaderGroupMap.Add(shaderGroupId, dataBlockIdMap.Values.ToArray());
        }

        private static void WriteMeshData(ImportState state)
        {
            foreach (var shader in state.MatShaderMapping.Values)
            {
                var rds = shader.Rds;
                rds.Write(state.ShaderGroupMap[shader.ShaderGroupName], state);
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

        private class RenderDataSource
        {
            public string Name { get; }

            public List<uint> Indices { get; }

            public List<Vector3> Positions { get; }

            public List<Vector3> Normals { get; }

            public List<Vector4> Tangents { get; }

            public List<Vector2> TexCoords0 { get; }

            public List<Vector2> TexCoords1 { get; }

            public List<uint> Colors { get; }

            public List<float> SkinIndices { get; }

            public RenderDataSource(int lodNumber, int rdsCount)
            {
                Name = $"x{lodNumber}_RDS{rdsCount}";

                Indices = new List<uint>();
                Positions = new List<Vector3>();
                Normals = new List<Vector3>();
                Tangents = new List<Vector4>();
                TexCoords0 = new List<Vector2>();
                TexCoords1 = new List<Vector2>();
                Colors = new List<uint>();
                SkinIndices = new List<float>();
            }

            public void Write(ShaderBlockInputInfo[] blockInputs, ImportState state)
            {
                var streamCount = (uint)blockInputs.Sum(bi => bi.VertexInputs.Count);

                var rdsNode = new PssgNode("RENDERDATASOURCE", state.RdsLib.File, state.RdsLib);
                rdsNode.AddAttribute("streamCount", streamCount);
                rdsNode.AddAttribute("id", Name);
                state.RdsLib.ChildNodes.Add(rdsNode);

                WriteIndices(rdsNode);

                // Write the data
                foreach (var bi in blockInputs)
                {
                    for (int i = 0; i < bi.VertexInputs.Count; ++i)
                    {
                        var vi = bi.VertexInputs[i];
                        WriteRenderStream(rdsNode, state.DataBlockCount, state.RenderStreamCount, (uint)i);
                        state.RenderStreamCount++;
                    }

                    WriteDataBlock(bi, state.RibLib, state.DataBlockCount);
                    state.DataBlockCount++;
                }

                static void WriteRenderStream(PssgNode rdsNode, uint dataBlockId, uint streamId, uint subStream)
                {
                    PssgNode rsNode = new PssgNode("RENDERSTREAM", rdsNode.File, rdsNode);
                    rsNode.AddAttribute("dataBlock", $"#block{dataBlockId}");
                    rsNode.AddAttribute("subStream", subStream);
                    rsNode.AddAttribute("id", $"stream{streamId}");
                    rdsNode.ChildNodes.Add(rsNode);
                }
            }

            private void WriteIndices(PssgNode rdsNode)
            {
                var stride = sizeof(ushort);
                var dataType = "ushort";

                // Ideally this should be switching on maxIndex,
                // but I saw other games use indices count so stick with that
                if (Indices.Count > ushort.MaxValue)
                {
                    stride = sizeof(uint);
                    dataType = "uint";
                }

                var isdData = new byte[Indices.Count * stride];
                var isdSpan = isdData.AsSpan();
                var maxIndex = 0u;
                switch (dataType)
                {
                    case "ushort":
                        for (int i = 0; i < Indices.Count; ++i)
                        {
                            var index = Indices[i];
                            BinaryPrimitives.WriteUInt16BigEndian(isdSpan, (ushort)index);
                            isdSpan = isdSpan.Slice(stride);

                            maxIndex = Math.Max(index, maxIndex);
                        }
                        break;
                    case "uint":
                        for (int i = 0; i < Indices.Count; ++i)
                        {
                            var index = Indices[i];
                            BinaryPrimitives.WriteUInt32BigEndian(isdSpan, index);
                            isdSpan = isdSpan.Slice(stride);

                            maxIndex = Math.Max(index, maxIndex);
                        }
                        break;
                    default:
                        throw new NotImplementedException($"Support for {dataType} primitive index format not implemented.");
                }

                var risNode = new PssgNode("RENDERINDEXSOURCE", rdsNode.File, rdsNode);
                risNode.AddAttribute("primitive", "triangles");
                risNode.AddAttribute("maximumIndex", maxIndex);
                risNode.AddAttribute("format", dataType);
                risNode.AddAttribute("count", (uint)Indices.Count);
                risNode.AddAttribute("id", Name.Replace("RDS", "RIS"));
                rdsNode.ChildNodes.Add(risNode);

                var isdNode = new PssgNode("INDEXSOURCEDATA", risNode.File, risNode);
                isdNode.Value = isdData;
                risNode.ChildNodes.Add(isdNode);
            }

            private void WriteDataBlock(ShaderBlockInputInfo bi, PssgNode ribLib, uint dataBlockId)
            {
                var stride = bi.VertexInputs.First().Stride;
                var size = (uint)(stride * Positions.Count);

                var dbNode = new PssgNode("DATABLOCK", ribLib.File, ribLib);
                dbNode.AddAttribute("streamCount", (uint)bi.VertexInputs.Count);
                dbNode.AddAttribute("size", size);
                dbNode.AddAttribute("elementCount", (uint)Positions.Count);
                dbNode.AddAttribute("id", $"block{dataBlockId}");
                ribLib.ChildNodes.Add(dbNode);

                var data = new byte[size];
                var dataSpan = data.AsSpan();
                var texCoordSet = 0;
                foreach (var vi in bi.VertexInputs)
                {
                    var dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                    dbsNode.AddAttribute("renderType", vi.Name);
                    dbsNode.AddAttribute("dataType", vi.DataType);
                    dbsNode.AddAttribute("offset", vi.Offset);
                    dbsNode.AddAttribute("stride", vi.Stride);
                    dbNode.ChildNodes.Add(dbsNode);

                    // Write the data
                    for (uint i = 0, e = 0; i < size; i += stride, ++e)
                    {
                        var destination = dataSpan.Slice((int)(e * vi.Stride + vi.Offset));
                        switch (vi.Name)
                        {
                            case "Vertex":
                                WritePosition(vi, e, destination);
                                break;
                            case "Color":
                                WriteColor(vi, e, destination);
                                break;
                            case "Normal":
                                WriteNormal(vi, e, destination);
                                break;
                            case "Tangent":
                                WriteTangent(vi, e, destination);
                                break;
                            case "Binormal":
                                WriteBinormal(vi, e, destination);
                                break;
                            case "ST":
                                WriteTexCoord(vi, e, texCoordSet, destination);
                                break;
                            case "SkinIndices":
                                WriteSkinIndex(vi, e, destination);
                                break;
                            default:
                                throw new NotImplementedException($"Support for vertex attribute {vi.Name} is not implemented.");
                        }
                    }

                    if (vi.Name == "ST")
                        texCoordSet += GetTexCoordSets(vi);
                }

                var dbdNode = new PssgNode("DATABLOCKDATA", dbNode.File, dbNode);
                dbdNode.Value = data;
                dbNode.ChildNodes.Add(dbdNode);

                static int GetTexCoordSets(ShaderVertexInputInfo vi)
                {
                    switch (vi.DataType)
                    {
                        case "float2":
                        case "half2":
                            return 1;
                        case "half4":
                        case "float4":
                            return 2;
                        default:
                            throw new NotImplementedException($"Support for {vi.Name} data type {vi.DataType} is not implemented.");
                    }
                }
            }

            private void WritePosition(ShaderVertexInputInfo vi, uint elementIndex, Span<byte> destination)
            {
                if (vi.DataType != "float3")
                    throw new NotImplementedException($"Support for {vi.Name} data type {vi.DataType} is not implemented.");

                var value = Positions[(int)elementIndex];
                WriteVector3(destination, value);
            }

            private void WriteNormal(ShaderVertexInputInfo vi, uint elementIndex, Span<byte> destination)
            {
                var value = Normals[(int)elementIndex];

                switch (vi.DataType)
                {
                    case "float3":
                        WriteVector3(destination, value);
                        break;
                    default:
                        throw new NotImplementedException($"Support for {vi.Name} data type {vi.DataType} is not implemented.");
                }
            }

            private void WriteTangent(ShaderVertexInputInfo vi, uint elementIndex, Span<byte> destination)
            {
                var value = Tangents[(int)elementIndex];

                switch (vi.DataType)
                {
                    case "half4":
                        WriteVectorHalf4(destination, value);
                        break;
                    case "float3":
                        WriteVector3(destination, new Vector3(value.X, value.Y, value.Z));
                        break;
                    default:
                        throw new NotImplementedException($"Support for {vi.Name} data type {vi.DataType} is not implemented.");
                }
            }

            private void WriteBinormal(ShaderVertexInputInfo vi, uint elementIndex, Span<byte> destination)
            {
                Vector3 norm = Normals[(int)elementIndex];
                Vector4 tang = Tangents[(int)elementIndex];
                Vector3 tang3 = new Vector3(tang.X, tang.Y, tang.Z);
                var value = new Vector4(Vector3.Cross(norm, tang3) * tang.W, tang.W);

                switch (vi.DataType)
                {
                    case "half4":
                        WriteVectorHalf4(destination, value);
                        break;
                    case "float3":
                        WriteVector3(destination, new Vector3(value.X, value.Y, value.Z));
                        break;
                    default:
                        throw new NotImplementedException($"Support for {vi.Name} data type {vi.DataType} is not implemented.");
                }
            }

            private void WriteTexCoord(ShaderVertexInputInfo vi, uint elementIndex, int texCoordSet, Span<byte> destination)
            {
                switch (vi.DataType)
                {
                    case "half2":
                        WriteVectorHalf2(destination, GetTexCoord(elementIndex, texCoordSet));
                        break;
                    case "float2":
                        WriteVector2(destination, GetTexCoord(elementIndex, texCoordSet));
                        break;
                    case "half4":
                        WriteVectorHalf2(destination, GetTexCoord(elementIndex, texCoordSet));
                        WriteVectorHalf2(destination.Slice(4), GetTexCoord(elementIndex, texCoordSet + 1));
                        break;
                    case "float4":
                        WriteVector2(destination, GetTexCoord(elementIndex, texCoordSet));
                        WriteVector2(destination.Slice(8), GetTexCoord(elementIndex, texCoordSet + 1));
                        break;
                    default:
                        throw new NotImplementedException($"Support for {vi.Name} data type {vi.DataType} is not implemented.");
                }
            }
            private Vector2 GetTexCoord(uint elementIndex, int texCoordSet)
            {
                var texCoords = texCoordSet switch
                {
                    0 => TexCoords0,
                    1 => TexCoords1,
                    _ => TexCoords0
                };

                return texCoords[(int)elementIndex];
            }

            private void WriteColor(ShaderVertexInputInfo vi, uint elementIndex, Span<byte> destination)
            {
                var value = Colors[(int)elementIndex];

                switch (vi.DataType)
                {
                    case "uint_color_argb":
                        BinaryPrimitives.WriteUInt32BigEndian(destination, value);
                        break;
                    default:
                        throw new NotImplementedException($"Support for {vi.Name} data type {vi.DataType} is not implemented.");
                }
            }

            private void WriteSkinIndex(ShaderVertexInputInfo vi, uint elementIndex, Span<byte> destination)
            {
                var value = SkinIndices[(int)elementIndex];

                switch (vi.DataType)
                {
                    case "float":
                        BinaryPrimitives.WriteSingleBigEndian(destination, value);
                        break;
                    default:
                        throw new NotImplementedException($"Support for {vi.Name} data type {vi.DataType} is not implemented.");
                }
            }

            private static void WriteVector3(Span<byte> destination, Vector3 value)
            {
                BinaryPrimitives.WriteSingleBigEndian(destination, value.X);
                BinaryPrimitives.WriteSingleBigEndian(destination.Slice(4), value.Y);
                BinaryPrimitives.WriteSingleBigEndian(destination.Slice(8), value.Z);
            }

            private static void WriteVector2(Span<byte> destination, Vector2 value)
            {
                BinaryPrimitives.WriteSingleBigEndian(destination, value.X);
                BinaryPrimitives.WriteSingleBigEndian(destination.Slice(4), value.Y);
            }

            private static void WriteVectorHalf2(Span<byte> destination, Vector2 value)
            {
                WriteHalfBigEndian(destination, (Half)value.X);
                WriteHalfBigEndian(destination.Slice(2), (Half)value.Y);
            }

            private static void WriteVectorHalf4(Span<byte> destination, Vector4 value)
            {
                WriteHalfBigEndian(destination, (Half)value.X);
                WriteHalfBigEndian(destination.Slice(2), (Half)value.Y);
                WriteHalfBigEndian(destination.Slice(4), (Half)value.Z);
                WriteHalfBigEndian(destination.Slice(6), (Half)value.W);
            }

            private static void WriteHalfBigEndian(Span<byte> destination, Half value)
            {
                BinaryPrimitives.WriteInt16BigEndian(destination, HalfToInt16Bits(value));
            }
            private static unsafe short HalfToInt16Bits(Half value)
            {
                return *(short*)&value;
            }
        }
    }
}
