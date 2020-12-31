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

namespace EgoEngineLibrary.Formats.Pssg
{
    // Starting with Dirt 3:
    // - The DataBlocks with ST/Tangent/Binormal now use half4/half4/half4 instead of float4/float3/float3
    public class GltfDirt3PssgConverter
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

            public Dictionary<int, ShaderData> MatShaderMapping { get; }

            public ImportState(PssgNode rdsLib, PssgNode ribLib)
            {
                RdsLib = rdsLib;
                RibLib = ribLib;
                MatShaderMapping = new Dictionary<int, ShaderData>();
            }
        }
        private class ShaderData
        {
            public string ShaderInstanceName { get; }

            public RenderDataSource Rds { get; }

            public List<string> JointNames { get; }

            public ShaderData(string shaderInstanceName, RenderDataSource rds)
            {
                ShaderInstanceName = shaderInstanceName;
                Rds = rds;
                JointNames = new List<string>();
            }
        }

        public static bool SupportsPssg(PssgFile pssg)
        {
            return pssg.FindNodes("MATRIXPALETTEJOINTRENDERINSTANCE").Any() &&
                !pssg.FindNodes("RENDERDATASOURCE", "streamCount", 8u).Any() &&
                pssg.FindNodes("DATABLOCKSTREAM", "renderType", "Tangent").Any(n => n.Attributes["dataType"].GetValue<string>().EndsWith('4')) &&
                pssg.FindNodes("DATABLOCKSTREAM", "renderType", "Binormal").Any(n => n.Attributes["dataType"].GetValue<string>().EndsWith('4'));
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
                nodeLib.RemoveChildNodes();

                rdsLib = pssg.FindNodes("LIBRARY", "type", "RENDERDATASOURCE").First();
                rdsLib.RemoveChildNodes(rdsLib.ChildNodes.Where(n => n.Name == "RENDERDATASOURCE"));

                ribLib = pssg.FindNodes("LIBRARY", "type", "RENDERINTERFACEBOUND").First();
                ribLib.RemoveChildNodes(ribLib.ChildNodes.Where(n => n.Name == "DATABLOCK"));
            }
            else
            {
                // F1 games use YYY, and put almost everything in this lib
                nodeLib = pssg.FindNodes("LIBRARY", "type", "YYY").FirstOrDefault();
                if (nodeLib is null)
                    throw new InvalidDataException("Could not find library with scene nodes.");

                rdsLib = nodeLib;
                ribLib = nodeLib;

                nodeLib.RemoveChildNodes(nodeLib.ChildNodes.Where(n => n.Name == "ROOTNODE"));
                rdsLib.RemoveChildNodes(rdsLib.ChildNodes.Where(n => n.Name == "RENDERDATASOURCE"));
                ribLib.RemoveChildNodes(ribLib.ChildNodes.Where(n => n.Name == "DATABLOCK"));
            }

            // Write the scene graph, and collect mesh data
            var state = new ImportState(rdsLib, ribLib);
            ConvertSceneNodes(pssg, nodeLib, rootNode, state);
        }

        private static void ConvertSceneNodes(PssgFile pssg, PssgNode parent, Node gltfNode, ImportState state)
        {
            PssgNode node;
            if (gltfNode.Name.StartsWith("Scene Root"))
            {
                node = new PssgNode("ROOTNODE", parent.File, parent);
                node.AddAttribute("stopTraversal", 0u);
                node.AddAttribute("nickname", "Scene Root");
                node.AddAttribute("id", "Scene Root");
                parent.ChildNodes.Add(node);
            }
            else if (gltfNode.Name.StartsWith("LOD1_"))
            {
                node = CreateMatrixPaletteBundleNode(parent, gltfNode, 1, state);
                return;
            }
            else if (gltfNode.Name.StartsWith("LOD0_"))
            {
                node = CreateMatrixPaletteBundleNode(parent, gltfNode, 0, state);
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
                ShaderData shaderData;
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
                    new ShaderData(shader.Attributes["id"].GetValue<string>(), new RenderDataSource(state.LodNumber, state.RenderDataSourceCount)));
                state.RenderDataSourceCount++;
            }
        }

        private static void WriteMeshData(ImportState state)
        {
            foreach (var shader in state.MatShaderMapping.Values)
            {
                var rds = shader.Rds;
                rds.Write(state);
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

            public uint StreamCount { get; }

            public List<ushort> Indices { get; }

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
                StreamCount = 7;

                Indices = new List<ushort>();
                Positions = new List<Vector3>();
                Normals = new List<Vector3>();
                Tangents = new List<Vector4>();
                TexCoords0 = new List<Vector2>();
                TexCoords1 = new List<Vector2>();
                Colors = new List<uint>();
                SkinIndices = new List<float>();
            }

            public void Write(ImportState state)
            {
                var rdsNode = new PssgNode("RENDERDATASOURCE", state.RdsLib.File, state.RdsLib);
                rdsNode.AddAttribute("streamCount", StreamCount);
                rdsNode.AddAttribute("id", Name);
                state.RdsLib.ChildNodes.Add(rdsNode);

                var risNode = new PssgNode("RENDERINDEXSOURCE", rdsNode.File, rdsNode);
                risNode.AddAttribute("primitive", "triangles");
                risNode.AddAttribute("format", "ushort");
                risNode.AddAttribute("count", (uint)Indices.Count);
                risNode.AddAttribute("id", Name.Replace("RDS", "RIS"));
                rdsNode.ChildNodes.Add(risNode);

                var isdNode = new PssgNode("INDEXSOURCEDATA", risNode.File, risNode);
                var isdData = new byte[Indices.Count * 2];
                isdNode.Value = isdData;
                risNode.ChildNodes.Add(isdNode);

                var isdSpan = isdData.AsSpan();
                var maxIndex = 0u;
                for (int i = 0; i < Indices.Count; ++i)
                {
                    var index = Indices[i];
                    BinaryPrimitives.WriteUInt16BigEndian(isdSpan, index);
                    isdSpan = isdSpan.Slice(2);

                    maxIndex = Math.Max(index, maxIndex);
                }

                risNode.AddAttribute("maximumIndex", maxIndex);

                // Create pos/col data
                WriteRenderStream(rdsNode, 0, state);
                WriteRenderStream(rdsNode, 1, state);
                WriteRenderStream(rdsNode, 2, state);
                WritePositionColorNormalDataBlock(state.RibLib, state.DataBlockCount);
                state.DataBlockCount++;

                // Create tex coord/normal data
                WriteRenderStream(rdsNode, 0, state);
                WriteRenderStream(rdsNode, 1, state);
                WriteRenderStream(rdsNode, 2, state);
                WriteTexCoordTangentDataBlock(state.RibLib, state.DataBlockCount);
                state.DataBlockCount++;

                // Create Skin Indices data
                WriteRenderStream(rdsNode, 0, state);
                WriteSkinIndexDataBlock(state.RibLib, state.DataBlockCount);
                state.DataBlockCount++;

                static void WriteRenderStream(PssgNode rdsNode, uint subStream, ImportState state)
                {
                    PssgNode rsNode = new PssgNode("RENDERSTREAM", rdsNode.File, rdsNode);
                    rsNode.AddAttribute("dataBlock", $"#block{state.DataBlockCount}");
                    rsNode.AddAttribute("subStream", subStream);
                    rsNode.AddAttribute("id", $"stream{state.RenderStreamCount}");
                    rdsNode.ChildNodes.Add(rsNode);
                    state.RenderStreamCount++;
                }
            }

            private void WritePositionColorNormalDataBlock(PssgNode ribLib, uint dataBlockId)
            {
                var stride = 28u;
                var size = (uint)(stride * Positions.Count);

                var dbNode = new PssgNode("DATABLOCK", ribLib.File, ribLib);
                dbNode.AddAttribute("streamCount", 3u);
                dbNode.AddAttribute("size", size);
                dbNode.AddAttribute("elementCount", (uint)Positions.Count);
                dbNode.AddAttribute("id", $"block{dataBlockId}");
                ribLib.ChildNodes.Add(dbNode);

                var dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "Vertex");
                dbsNode.AddAttribute("dataType", "float3");
                dbsNode.AddAttribute("offset", 0u);
                dbsNode.AddAttribute("stride", stride);
                dbNode.ChildNodes.Add(dbsNode);

                dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "Color");
                dbsNode.AddAttribute("dataType", "uint_color_argb");
                dbsNode.AddAttribute("offset", 12u);
                dbsNode.AddAttribute("stride", stride);
                dbNode.ChildNodes.Add(dbsNode);

                dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "Normal");
                dbsNode.AddAttribute("dataType", "float3");
                dbsNode.AddAttribute("offset", 16u);
                dbsNode.AddAttribute("stride", stride);
                dbNode.ChildNodes.Add(dbsNode);

                var dbdNode = new PssgNode("DATABLOCKDATA", dbNode.File, dbNode);
                var data = new byte[size];
                dbdNode.Value = data;
                dbNode.ChildNodes.Add(dbdNode);

                for (uint i = 0, e = 0; i < size; i += stride, ++e)
                {
                    Vector3 pos = Positions[(int)e];
                    uint color = Colors[(int)e];
                    Vector3 norm = Normals[(int)e];

                    EndianBitConverter.Big.CopyBytes(pos.X, data, (int)i);
                    EndianBitConverter.Big.CopyBytes(pos.Y, data, (int)i + 4);
                    EndianBitConverter.Big.CopyBytes(pos.Z, data, (int)i + 8);
                    EndianBitConverter.Big.CopyBytes(color, data, (int)i + 12);

                    EndianBitConverter.Big.CopyBytes(norm.X, data, (int)i + 16);
                    EndianBitConverter.Big.CopyBytes(norm.Y, data, (int)i + 20);
                    EndianBitConverter.Big.CopyBytes(norm.Z, data, (int)i + 24);
                }
            }

            private void WriteTexCoordTangentDataBlock(PssgNode ribLib, uint dataBlockId)
            {
                var stride = 24u;
                var size = (uint)(stride * TexCoords0.Count);

                var dbNode = new PssgNode("DATABLOCK", ribLib.File, ribLib);
                dbNode.AddAttribute("streamCount", 3u);
                dbNode.AddAttribute("size", size);
                dbNode.AddAttribute("elementCount", (uint)TexCoords0.Count);
                dbNode.AddAttribute("id", $"block{dataBlockId}");
                ribLib.ChildNodes.Add(dbNode);

                var dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "ST");
                dbsNode.AddAttribute("dataType", "half4");
                dbsNode.AddAttribute("offset", 0u);
                dbsNode.AddAttribute("stride", stride);
                dbNode.ChildNodes.Add(dbsNode);

                dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "Tangent");
                dbsNode.AddAttribute("dataType", "half4");
                dbsNode.AddAttribute("offset", 8u);
                dbsNode.AddAttribute("stride", stride);
                dbNode.ChildNodes.Add(dbsNode);

                dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "Binormal");
                dbsNode.AddAttribute("dataType", "half4");
                dbsNode.AddAttribute("offset", 16u);
                dbsNode.AddAttribute("stride", stride);
                dbNode.ChildNodes.Add(dbsNode);

                var dbdNode = new PssgNode("DATABLOCKDATA", dbNode.File, dbNode);
                var data = new byte[size];
                dbdNode.Value = data;
                dbNode.ChildNodes.Add(dbdNode);

                var dataSpan = data.AsSpan();
                for (uint i = 0, e = 0; i < size; i += stride, ++e)
                {
                    Vector2 texc = TexCoords0[(int)e];
                    Vector2 texc1 = TexCoords1[(int)e];
                    Vector3 norm = Normals[(int)e];
                    Vector4 tang = Tangents[(int)e];
                    Vector3 tang3 = new Vector3(tang.X, tang.Y, tang.Z);
                    Vector3 bino = Vector3.Cross(norm, tang3) * tang.W;

                    BigCopyBytes((Half)texc.X, dataSpan.Slice((int)i));
                    BigCopyBytes((Half)texc.Y, dataSpan.Slice((int)i + 2));
                    BigCopyBytes((Half)texc1.X, dataSpan.Slice((int)i + 4));
                    BigCopyBytes((Half)texc1.Y, dataSpan.Slice((int)i + 6));

                    BigCopyBytes((Half)tang.X, dataSpan.Slice((int)i + 8));
                    BigCopyBytes((Half)tang.Y, dataSpan.Slice((int)i + 10));
                    BigCopyBytes((Half)tang.Z, dataSpan.Slice((int)i + 12));
                    BigCopyBytes((Half)tang.W, dataSpan.Slice((int)i + 14));

                    BigCopyBytes((Half)bino.X, dataSpan.Slice((int)i + 16));
                    BigCopyBytes((Half)bino.Y, dataSpan.Slice((int)i + 18));
                    BigCopyBytes((Half)bino.Z, dataSpan.Slice((int)i + 20));
                    BigCopyBytes((Half)tang.W, dataSpan.Slice((int)i + 22));
                }
            }

            private void WriteSkinIndexDataBlock(PssgNode ribLib, uint dataBlockId)
            {
                var stride = 4u;
                var size = (uint)(stride * SkinIndices.Count);

                var dbNode = new PssgNode("DATABLOCK", ribLib.File, ribLib);
                dbNode.AddAttribute("streamCount", 1u);
                dbNode.AddAttribute("size", size);
                dbNode.AddAttribute("elementCount", (uint)Positions.Count);
                dbNode.AddAttribute("id", $"block{dataBlockId}");
                ribLib.ChildNodes.Add(dbNode);

                var dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "SkinIndices");
                dbsNode.AddAttribute("dataType", "float");
                dbsNode.AddAttribute("offset", 0u);
                dbsNode.AddAttribute("stride", stride);
                dbNode.ChildNodes.Add(dbsNode);

                var dbdNode = new PssgNode("DATABLOCKDATA", dbNode.File, dbNode);
                var data = new byte[size];
                dbdNode.Value = data;
                dbNode.ChildNodes.Add(dbdNode);

                for (uint i = 0, e = 0; i < size; i += stride, ++e)
                {
                    float skinIndex = SkinIndices[(int)e];

                    EndianBitConverter.Big.CopyBytes(skinIndex, data, (int)i);
                }
            }

            private static void BigCopyBytes(Half value, Span<byte> destination)
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
