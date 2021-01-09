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
    public class GltfCarInteriorPssgConverter
    {
        private class ImportState : PssgModelWriterState
        {
            public int RenderStreamInstanceCount { get; set; }

            public int SegmentSetCount { get; set; }

            public int RenderDataSourceCount { get; set; }

            public PssgNode RdsLib { get; }

            public PssgNode RibLib { get; }

            public bool IsF1 { get; }

            public Dictionary<string, ShaderInputInfo> ShaderGroupMap { get; }

            public Dictionary<int, ShaderInstanceData> MatShaderMapping { get; }

            public ImportState(PssgNode rdsLib, PssgNode ribLib, Dictionary<string, ShaderInputInfo> shaderGroupMap)
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

            public ShaderInstanceData(string shaderInstanceName, string shaderGroupName, RenderDataSourceWriter rds)
            {
                ShaderInstanceName = shaderInstanceName;
                ShaderGroupName = shaderGroupName;
                Rds = rds;
            }
        }

        public static bool SupportsPssg(PssgFile pssg)
        {
            return pssg.FindNodes("RENDERNODE").Any();
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
                rdsLib = pssg.FindNodes("LIBRARY", "type", "SEGMENTSET").First();
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
            rdsLib.RemoveChildNodes(rdsLib.ChildNodes.Where(n => n.Name == "SEGMENTSET"));
            ribLib.RemoveChildNodes(ribLib.ChildNodes.Where(n => n.Name == "DATABLOCK"));

            // Write the scene graph, and collect mesh data
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
            else if (gltfNode.Mesh is not null)
            {
                node = CreateRenderNode(parent, gltfNode, state);
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

        private static PssgNode CreateRenderNode(PssgNode parent, Node gltfNode, ImportState state)
        {
            PssgNode node = new PssgNode("RENDERNODE", parent.File, parent);
            node.AddAttribute("stopTraversal", 0u);
            node.AddAttribute("nickname", gltfNode.Name);
            node.AddAttribute("id", gltfNode.Name);
            parent.ChildNodes.Add(node);

            state.MatShaderMapping.Clear();

            // Now add a new mesh from mesh builder
            ConvertMesh(node, gltfNode, state);

            // Write the mesh data
            WriteMeshData(state);

            return node;
        }

        private static void ConvertMesh(PssgNode renderNode, Node gltfNode, ImportState state)
        {
            var mesh = gltfNode.Mesh;
            if (mesh.Primitives.Any(p => p.Material == null))
                throw new NotImplementedException($"The converter does not support primitives ({mesh.Name}) with a null material.");

            var transformNode = new PssgNode("TRANSFORM", renderNode.File, renderNode);
            transformNode.Value = GetTransform(gltfNode.LocalMatrix);
            renderNode.ChildNodes.Add(transformNode);

            var bboxNode = new PssgNode("BOUNDINGBOX", renderNode.File, renderNode);
            renderNode.ChildNodes.Add(bboxNode);

            // Add to the material shader mapping
            var gltfMats = mesh.Primitives.Select(p => p.Material);
            ConvertMaterials(gltfMats, renderNode.File, state);

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

                var rsiNode = new PssgNode("RENDERSTREAMINSTANCE", renderNode.File, renderNode);
                rsiNode.AddAttribute("sourceCount", 1u);
                rsiNode.AddAttribute("indices", $"#{rds.Name}");
                rsiNode.AddAttribute("streamCount", 0u);
                rsiNode.AddAttribute("shader", $"#{shaderData.ShaderInstanceName}");
                rsiNode.AddAttribute("id", $"!RSI{state.RenderStreamInstanceCount}"); state.RenderStreamInstanceCount++;
                renderNode.ChildNodes.Add(rsiNode);

                var risNode = new PssgNode("RENDERINSTANCESOURCE", rsiNode.File, rsiNode);
                risNode.AddAttribute("source", $"#{shaderData.Rds.Name}");
                rsiNode.ChildNodes.Add(risNode);

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
                    rds.Colors.Add(PackColor(color));
                    rds.SkinIndices.Add(0);
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
                        new RenderDataSourceWriter($"!RDS{state.RenderDataSourceCount}")));
                state.RenderDataSourceCount++;
            }
        }

        private static void WriteMeshData(ImportState state)
        {
            PssgNode ssNode = new PssgNode("SEGMENTSET", state.RdsLib.File, state.RdsLib);
            ssNode.AddAttribute("segmentCount", (uint)state.MatShaderMapping.Count);
            ssNode.AddAttribute("id", $"!SS{state.SegmentSetCount}");
            state.SegmentSetCount++;
            state.RdsLib.ChildNodes.Add(ssNode);

            foreach (var shader in state.MatShaderMapping.Values)
            {
                var rds = shader.Rds;
                if (!state.ShaderGroupMap.TryGetValue(shader.ShaderGroupName, out var shaderInputInfo))
                    throw new InvalidDataException($"The pssg does not have existing data blocks to model the layout of the input for shader {shader.ShaderGroupName}.");

                rds.Write(shaderInputInfo, ssNode, state.RibLib, state);
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
