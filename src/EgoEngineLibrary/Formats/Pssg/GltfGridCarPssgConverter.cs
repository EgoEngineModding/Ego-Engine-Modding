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
    public class GltfGridCarPssgConverter
    {
        private class ImportState
        {
            public int LodNumber { get; set; }

            public int MpjriCount { get; set; }

            public int RenderDataSourceCount { get; set; }

            public uint JointId { get; set; }

            public uint DataBlockCount { get; set; }

            public uint RenderStreamCount { get; set; }

            public Dictionary<int, ShaderData> MatShaderMapping { get; }

            public ImportState()
            {
                MatShaderMapping = new Dictionary<int, ShaderData>();
            }
        }
        private class ShaderData
        {
            public string ShaderInstanceName { get; }

            public RenderDataSource Rds { get; }

            public ShaderData(string shaderInstanceName, RenderDataSource rds)
            {
                ShaderInstanceName = shaderInstanceName;
                Rds = rds;
            }
        }

        public void Convert(ModelRoot gltf, PssgFile pssg)
        {
            // Get a list of nodes in the default scene as a flat list
            Dictionary<int, int> nodeBoneIndexMap = new Dictionary<int, int>();
            var rootNode = gltf.DefaultScene.FindNode(n => n.Name.StartsWith("Scene Root"));
            if (rootNode is null)
                throw new InvalidDataException("The default scene must have node name starting with `Scene Root`.");

            // Clear libraries
            var nodeLib = pssg.FindNodes("LIBRARY", "type", "NODE").First();
            nodeLib.RemoveChildNodes();

            var rdsLib = pssg.FindNodes("LIBRARY", "type", "RENDERDATASOURCE").First();
            rdsLib.RemoveChildNodes(rdsLib.ChildNodes.Where(n => n.Name == "RENDERDATASOURCE"));

            var ribLib = pssg.FindNodes("LIBRARY", "type", "RENDERINTERFACEBOUND").First();
            ribLib.RemoveChildNodes(ribLib.ChildNodes.Where(n => n.Name == "DATABLOCK"));

            // Write the scene graph, and collect mesh data
            var state = new ImportState();
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
            state.JointId = 0;
            state.MatShaderMapping.Clear();

            List<string> jointNames = new List<string>();
            foreach (var child in gltfNode.VisualChildren)
            {
                if (child.Mesh is null) continue;
                if (child.Mesh.Primitives.Count == 0) continue;

                CreateMatrixPaletteJointNode(node, child, state);
                jointNames.Add(node.ChildNodes.Last().Attributes["id"].GetValue<string>());
            }

            CreateMatrixPaletteNode(node, jointNames, state);

            // Write the mesh data
            var rdsLib = node.File.FindNodes("LIBRARY", "type", "RENDERDATASOURCE").First();
            var ribLib = node.File.FindNodes("LIBRARY", "type", "RENDERINTERFACEBOUND").First();
            WriteMeshData(rdsLib, ribLib, state);

            return node;
        }

        private static void CreateMatrixPaletteNode(PssgNode parent, List<string> jointNames, ImportState state)
        {
            var node = new PssgNode("MATRIXPALETTENODE", parent.File, parent);
            node.AddAttribute("jointCount", state.JointId);
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
                var rsiNode = new PssgNode("RENDERSTREAMINSTANCE", node.File, node);
                rsiNode.AddAttribute("sourceCount", 1u);
                rsiNode.AddAttribute("indices", $"#{shader.Rds.Name}");
                rsiNode.AddAttribute("streamCount", 0u);
                rsiNode.AddAttribute("shader", $"#{shader.ShaderInstanceName}");
                rsiNode.AddAttribute("id", shader.Rds.Name.Replace("RDS", "RSI"));
                node.ChildNodes.Add(rsiNode);

                var risNode = new PssgNode("RENDERINSTANCESOURCE", rsiNode.File, rsiNode);
                risNode.AddAttribute("source", $"#{shader.Rds.Name}");
                rsiNode.ChildNodes.Add(risNode);
            }

            foreach (var jointName in jointNames)
            {
                var mpsjNode = new PssgNode("MATRIXPALETTESKINJOINT", node.File, node);
                mpsjNode.AddAttribute("joint", $"#{jointName}");
                node.ChildNodes.Add(mpsjNode);
            }
        }

        private static void CreateMatrixPaletteJointNode(PssgNode parent, Node gltfNode, ImportState state)
        {
            var node = new PssgNode("MATRIXPALETTEJOINTNODE", parent.File, parent);
            node.AddAttribute("stopTraversal", 0u);
            node.AddAttribute("nickname", gltfNode.Name);
            node.AddAttribute("id", gltfNode.Name);
            node.AddAttribute("jointID", state.JointId);
            node.AddAttribute("matrixPalette", $"#x{state.LodNumber}_MPN");
            parent.ChildNodes.Add(node);

            // Now add a new mesh from mesh builder
            ConvertMesh(node, gltfNode, state);

            state.JointId++;
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

                var mpriNode = new PssgNode("MATRIXPALETTERENDERINSTANCE", mpjnNode.File, mpjnNode);
                mpriNode.AddAttribute("streamOffset", (uint)(rds.Positions.Count));
                mpriNode.AddAttribute("elementCountFromOffset", (uint)(p.VertexCount));
                mpriNode.AddAttribute("indexOffset", (uint)(rds.Indices.Count));
                mpriNode.AddAttribute("indicesCountFromOffset", (uint)(tris.Length * 3));
                mpriNode.AddAttribute("sourceCount", 1u);
                mpriNode.AddAttribute("indices", $"#{rds.Name}");
                mpriNode.AddAttribute("streamCount", 0u);
                mpriNode.AddAttribute("shader", $"#{shaderData.ShaderInstanceName}");
                mpriNode.AddAttribute("id", $"MPJRI{state.MpjriCount}"); state.MpjriCount++;
                mpjnNode.ChildNodes.Add(mpriNode);

                var risNode = new PssgNode("RENDERINSTANCESOURCE", mpriNode.File, mpriNode);
                risNode.AddAttribute("source", $"#{shaderData.Rds.Name}");
                mpriNode.ChildNodes.Add(risNode);

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
                    rds.TexCoords.Add(p.GetTextureCoord(i, texCoordSet));
                    rds.Colors.Add(PackColor(color));
                    rds.SkinIndices.Add(state.JointId);
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

        private static void WriteMeshData(PssgNode rdsLib, PssgNode ribLib, ImportState state)
        {
            foreach (var shader in state.MatShaderMapping.Values)
            {
                var rds = shader.Rds;
                rds.Write(rdsLib, ribLib, state);
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

            public List<Vector2> TexCoords { get; }

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
                TexCoords = new List<Vector2>();
                Colors = new List<uint>();
                SkinIndices = new List<float>();
            }

            public void Write(PssgNode rdsLib, PssgNode ribLib, ImportState state)
            {
                var rdsNode = new PssgNode("RENDERDATASOURCE", rdsLib.File, rdsLib);
                rdsNode.AddAttribute("streamCount", StreamCount);
                rdsNode.AddAttribute("id", Name);
                rdsLib.ChildNodes.Add(rdsNode);

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
                WritePositionColorDataBlock(ribLib, state.DataBlockCount);

                var rsNode = new PssgNode("RENDERSTREAM", rdsNode.File, rdsNode);
                rsNode.AddAttribute("dataBlock", $"#block{state.DataBlockCount}");
                rsNode.AddAttribute("subStream", 0u);
                rsNode.AddAttribute("id", $"stream{state.RenderStreamCount}");
                state.RenderStreamCount++;
                rdsNode.ChildNodes.Add(rsNode);

                rsNode = new PssgNode("RENDERSTREAM", rdsNode.File, rdsNode);
                rsNode.AddAttribute("dataBlock", $"#block{state.DataBlockCount}");
                rsNode.AddAttribute("subStream", 1u);
                rsNode.AddAttribute("id", $"stream{state.RenderStreamCount}");
                state.RenderStreamCount++; state.DataBlockCount++;
                rdsNode.ChildNodes.Add(rsNode);

                // Create tex coord/normal data
                WriteTexCoordNormalDataBlock(ribLib, state.DataBlockCount);

                rsNode = new PssgNode("RENDERSTREAM", rdsNode.File, rdsNode);
                rsNode.AddAttribute("dataBlock", $"#block{state.DataBlockCount}");
                rsNode.AddAttribute("subStream", 0u);
                rsNode.AddAttribute("id", $"stream{state.RenderStreamCount}");
                state.RenderStreamCount++;
                rdsNode.ChildNodes.Add(rsNode);

                rsNode = new PssgNode("RENDERSTREAM", rdsNode.File, rdsNode);
                rsNode.AddAttribute("dataBlock", $"#block{state.DataBlockCount}");
                rsNode.AddAttribute("subStream", 1u);
                rsNode.AddAttribute("id", $"stream{state.RenderStreamCount}");
                state.RenderStreamCount++;
                rdsNode.ChildNodes.Add(rsNode);

                rsNode = new PssgNode("RENDERSTREAM", rdsNode.File, rdsNode);
                rsNode.AddAttribute("dataBlock", $"#block{state.DataBlockCount}");
                rsNode.AddAttribute("subStream", 2u);
                rsNode.AddAttribute("id", $"stream{state.RenderStreamCount}");
                state.RenderStreamCount++;
                rdsNode.ChildNodes.Add(rsNode);

                rsNode = new PssgNode("RENDERSTREAM", rdsNode.File, rdsNode);
                rsNode.AddAttribute("dataBlock", $"#block{state.DataBlockCount}");
                rsNode.AddAttribute("subStream", 3u);
                rsNode.AddAttribute("id", $"stream{state.RenderStreamCount}");
                state.RenderStreamCount++; state.DataBlockCount++;
                rdsNode.ChildNodes.Add(rsNode);

                // Create Skin Indices data
                WriteSkinIndexDataBlock(ribLib, state.DataBlockCount);

                rsNode = new PssgNode("RENDERSTREAM", rdsNode.File, rdsNode);
                rsNode.AddAttribute("dataBlock", $"#block{state.DataBlockCount}");
                rsNode.AddAttribute("subStream", 0u);
                rsNode.AddAttribute("id", $"stream{state.RenderStreamCount}");
                state.RenderStreamCount++; state.DataBlockCount++;
                rdsNode.ChildNodes.Add(rsNode);
            }

            private void WritePositionColorDataBlock(PssgNode ribLib, uint dataBlockId)
            {
                var stride = 16u;
                var size = (uint)(stride * Positions.Count);

                var dbNode = new PssgNode("DATABLOCK", ribLib.File, ribLib);
                dbNode.AddAttribute("streamCount", 2u);
                dbNode.AddAttribute("size", size);
                dbNode.AddAttribute("elementCount", (uint)Positions.Count);
                dbNode.AddAttribute("id", $"block{dataBlockId}");
                ribLib.ChildNodes.Add(dbNode);

                var dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "Vertex");
                dbsNode.AddAttribute("dataType", "float3");
                dbsNode.AddAttribute("offset", 0u);
                dbsNode.AddAttribute("stride", 16u);
                dbNode.ChildNodes.Add(dbsNode);

                dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "Color");
                dbsNode.AddAttribute("dataType", "uint_color_argb");
                dbsNode.AddAttribute("offset", 12u);
                dbsNode.AddAttribute("stride", 16u);
                dbNode.ChildNodes.Add(dbsNode);

                var dbdNode = new PssgNode("DATABLOCKDATA", dbNode.File, dbNode);
                var data = new byte[size];
                dbdNode.Value = data;
                dbNode.ChildNodes.Add(dbdNode);

                for (uint i = 0, e = 0; i < size; i += stride, ++e)
                {
                    Vector3 pos = Positions[(int)e];
                    uint color = Colors[(int)e];

                    EndianBitConverter.Big.CopyBytes(pos.X, data, (int)i);
                    EndianBitConverter.Big.CopyBytes(pos.Y, data, (int)i + 4);
                    EndianBitConverter.Big.CopyBytes(pos.Z, data, (int)i + 8);
                    EndianBitConverter.Big.CopyBytes(color, data, (int)i + 12);
                }
            }

            private void WriteTexCoordNormalDataBlock(PssgNode ribLib, uint dataBlockId)
            {
                var stride = 44u;
                var size = (uint)(stride * TexCoords.Count);

                var dbNode = new PssgNode("DATABLOCK", ribLib.File, ribLib);
                dbNode.AddAttribute("streamCount", 4u);
                dbNode.AddAttribute("size", size);
                dbNode.AddAttribute("elementCount", (uint)TexCoords.Count);
                dbNode.AddAttribute("id", $"block{dataBlockId}");
                ribLib.ChildNodes.Add(dbNode);

                var dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "ST");
                dbsNode.AddAttribute("dataType", "float2");
                dbsNode.AddAttribute("offset", 0u);
                dbsNode.AddAttribute("stride", stride);
                dbNode.ChildNodes.Add(dbsNode);

                dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "Normal");
                dbsNode.AddAttribute("dataType", "float3");
                dbsNode.AddAttribute("offset", 8u);
                dbsNode.AddAttribute("stride", stride);
                dbNode.ChildNodes.Add(dbsNode);

                dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "Tangent");
                dbsNode.AddAttribute("dataType", "float3");
                dbsNode.AddAttribute("offset", 20u);
                dbsNode.AddAttribute("stride", stride);
                dbNode.ChildNodes.Add(dbsNode);

                dbsNode = new PssgNode("DATABLOCKSTREAM", dbNode.File, dbNode);
                dbsNode.AddAttribute("renderType", "Binormal");
                dbsNode.AddAttribute("dataType", "float3");
                dbsNode.AddAttribute("offset", 32u);
                dbsNode.AddAttribute("stride", stride);
                dbNode.ChildNodes.Add(dbsNode);

                var dbdNode = new PssgNode("DATABLOCKDATA", dbNode.File, dbNode);
                var data = new byte[size];
                dbdNode.Value = data;
                dbNode.ChildNodes.Add(dbdNode);

                for (uint i = 0, e = 0; i < size; i += stride, ++e)
                {
                    Vector2 texc = TexCoords[(int)e];
                    Vector3 norm = Normals[(int)e];
                    Vector4 tang = Tangents[(int)e];
                    Vector3 tang3 = new Vector3(tang.X, tang.Y, tang.Z);
                    Vector3 bino = Vector3.Cross(norm, tang3) * tang.W;

                    EndianBitConverter.Big.CopyBytes(texc.X, data, (int)i);
                    EndianBitConverter.Big.CopyBytes(texc.Y, data, (int)i + 4);

                    EndianBitConverter.Big.CopyBytes(norm.X, data, (int)i + 8);
                    EndianBitConverter.Big.CopyBytes(norm.Y, data, (int)i + 12);
                    EndianBitConverter.Big.CopyBytes(norm.Z, data, (int)i + 16);

                    EndianBitConverter.Big.CopyBytes(tang.X, data, (int)i + 20);
                    EndianBitConverter.Big.CopyBytes(tang.Y, data, (int)i + 24);
                    EndianBitConverter.Big.CopyBytes(tang.Z, data, (int)i + 28);

                    EndianBitConverter.Big.CopyBytes(bino.X, data, (int)i + 32);
                    EndianBitConverter.Big.CopyBytes(bino.Y, data, (int)i + 36);
                    EndianBitConverter.Big.CopyBytes(bino.Z, data, (int)i + 40);
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
        }
    }
}
