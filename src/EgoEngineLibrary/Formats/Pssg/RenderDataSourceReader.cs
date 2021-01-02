using EgoEngineLibrary.Graphics;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Formats.Pssg
{
    public class RenderDataSourceReader
    {
        private record VertexAttributeData(string Name, string DataType, uint ElementCount, uint Offset, uint Stride, byte[] Data);

        private readonly string _indexFormat;
        private readonly int _indexStride;
        private readonly byte[] _indexData;
        private readonly Dictionary<string, VertexAttributeData> _vertexAttributes;
        private readonly List<VertexAttributeData> _texCoordSets;

        public uint VertexCount => _vertexAttributes.TryGetValue("Vertex", out var attr) ? attr.ElementCount : 0;

        public string Primitive { get; }
        public uint IndexCount { get; }
        public int TexCoordSetCount => _texCoordSets.Count;

        public RenderDataSourceReader(PssgNode rdsNode)
        {
            var risNode = rdsNode.ChildNodes.FirstOrDefault(n => n.Name == "RENDERINDEXSOURCE") ??
                throw new InvalidDataException($"RDS node {rdsNode.Attributes["id"].GetValue<string>()} must have RENDERINDEXSOURCE as its first child.");
            var isdNode = risNode.ChildNodes.FirstOrDefault(n => n.Name == "INDEXSOURCEDATA") ??
                throw new InvalidDataException($"RENDERINDEXSOURCE node {risNode.Attributes["id"].GetValue<string>()} must have INDEXSOURCEDATA as its first child.");

            // Setup indices
            _indexFormat = risNode.Attributes["format"].GetValue<string>();
            _indexStride = GetIndexStride(_indexFormat);
            Primitive = risNode.Attributes["primitive"].GetValue<string>();
            IndexCount = risNode.Attributes["count"].GetValue<uint>();
            _indexData = ((byte[])isdNode.Value);

            // Setup vertex attributes
            var renderStreamNodes = rdsNode.FindNodes("RENDERSTREAM").ToList();
            _vertexAttributes = new Dictionary<string, VertexAttributeData>();
            _texCoordSets = new List<VertexAttributeData>();
            foreach (var rsNode in renderStreamNodes)
            {
                var dbId = rsNode.Attributes["dataBlock"].GetValue<string>().Substring(1);
                var subStream = rsNode.Attributes["subStream"].GetValue<uint>();

                var dbNode = rsNode.File.FindNodes("DATABLOCK", "id", dbId).First();
                var dbStreamNode = dbNode.ChildNodes[(int)subStream];

                var size = dbNode.Attributes["size"].GetValue<uint>();
                var elemCount = dbNode.Attributes["elementCount"].GetValue<uint>();

                var dataBlockDataNode = dbNode.FindNodes("DATABLOCKDATA").First();
                var data = (byte[])dataBlockDataNode.Value;

                var renderType = dbStreamNode.Attributes["renderType"].GetValue<string>();
                var offset = dbStreamNode.Attributes["offset"].GetValue<uint>();
                var stride = dbStreamNode.Attributes["stride"].GetValue<uint>();
                var dataType = dbStreamNode.Attributes["dataType"].GetValue<string>();

                if (data.Length != size)
                    throw new InvalidDataException($"The data block size ({size}) is different than data block data size ({data.Length}).");

                var attr = new VertexAttributeData(renderType, dataType, elemCount, offset, stride, data);
                if (attr.Name == "ST")
                {
                    // tex coords are a different case since I need to differentiate them into different uv sets
                    _texCoordSets.AddRange(HandleSTAttribute(attr));
                }
                else
                {
                    _vertexAttributes.Add(attr.Name, attr);
                }
            }

            static int GetIndexStride(string indexFormat)
            {
                return indexFormat switch
                {
                    "ushort" => sizeof(ushort),
                    "uint" => sizeof(uint),
                    _ => throw new NotImplementedException($"Support for {indexFormat} primitive index format not implemented.")
                };
            }
        }

        private IEnumerable<VertexAttributeData> HandleSTAttribute(VertexAttributeData attribute)
        {
            switch (attribute.DataType)
            {
                case "float2":
                case "half2":
                    yield return attribute;
                    break;
                case "half4":
                    attribute = attribute with 
                    {
                        DataType = "half2" 
                    };
                    yield return attribute;

                    attribute = attribute with
                    {
                        DataType = "half2",
                        Offset = attribute.Offset + (uint)(Unsafe.SizeOf<Half>() * 2)
                    };
                    yield return attribute;
                    break;
                case "float4":
                    attribute = attribute with
                    {
                        DataType = "float2"
                    };
                    yield return attribute;

                    attribute = attribute with
                    {
                        DataType = "float2",
                        Offset = attribute.Offset + sizeof(float) * 2
                    };
                    yield return attribute;
                    break;
                default:
                    throw new NotImplementedException($"Support for {attribute.Name} data type {attribute.DataType} is not implemented.");
            }
        }

        public IEnumerable<(uint A, uint B, uint C)> GetTriangles()
        {
            return GetTriangles(0, (int)IndexCount);
        }
        public IEnumerable<(uint A, uint B, uint C)> GetTriangles(int startIndex, int indexCount)
        {
            if (Primitive != "triangles")
                throw new InvalidOperationException($"Cannot get triangle from primitive type {Primitive}.");

            var data = _indexData.AsMemory(startIndex * _indexStride, indexCount * _indexStride);
            var triCount = indexCount / 3;
            switch (_indexFormat)
            {
                case "ushort":
                    {
                        for (int i = 0; i < triCount; ++i)
                        {
                            var dataSpan = data.Span;
                            var a = BinaryPrimitives.ReadUInt16BigEndian(dataSpan);
                            var b = BinaryPrimitives.ReadUInt16BigEndian(dataSpan.Slice(2));
                            var c = BinaryPrimitives.ReadUInt16BigEndian(dataSpan.Slice(4));
                            yield return (a, b, c);

                            data = data.Slice(6);
                        }
                        break;
                    }
                case "uint":
                    {
                        for (int i = 0; i < triCount; ++i)
                        {
                            var dataSpan = data.Span;
                            var a = BinaryPrimitives.ReadUInt32BigEndian(dataSpan);
                            var b = BinaryPrimitives.ReadUInt32BigEndian(dataSpan.Slice(4));
                            var c = BinaryPrimitives.ReadUInt32BigEndian(dataSpan.Slice(8));
                            yield return (a, b, c);

                            data = data.Slice(12);
                        }
                        break;
                    }
                default:
                    throw new NotImplementedException($"Support for {_indexFormat} primitive index format not implemented.");
            }
        }

        public uint GetIndex(int index)
        {
            if (Primitive != "triangles")
                throw new InvalidOperationException($"Cannot get triangle from primitive type {Primitive}.");

            var offset = index * _indexStride;
            var data = _indexData.AsSpan(offset);
            switch (_indexFormat)
            {
                case "ushort":
                    {
                        var a = BinaryPrimitives.ReadUInt16BigEndian(data);
                        return a;
                    }
                case "uint":
                    {
                        var a = BinaryPrimitives.ReadUInt32BigEndian(data);
                        return a;
                    }
                default:
                    throw new NotImplementedException($"Support for {_indexFormat} primitive index format not implemented.");
            }
        }

        public Vector3 GetPosition(uint index)
        {
            bool found = _vertexAttributes.TryGetValue("Vertex", out var attribute);

            if (found && attribute is not null)
            {
                if (attribute.DataType != "float3")
                    throw new NotImplementedException($"Support for {attribute.Name} data type {attribute.DataType} is not implemented.");

                var offset = (int)(attribute.Stride * index + attribute.Offset);
                var data = attribute.Data.AsSpan(offset);
                return ReadVector3(data);
            }
            else
            {
                return Vector3.Zero;
            }
        }

        public Vector3 GetNormal(uint index)
        {
            bool found = _vertexAttributes.TryGetValue("Normal", out var attribute);

            if (found && attribute is not null)
            {
                var offset = (int)(attribute.Stride * index + attribute.Offset);
                var data = attribute.Data.AsSpan(offset);

                switch (attribute.DataType)
                {
                    case "float3":
                        return ReadVector3(data);
                    default:
                        throw new NotImplementedException($"Support for {attribute.Name} data type {attribute.DataType} is not implemented.");
                }
            }
            else
            {
                return Vector3.Zero;
            }
        }

        public Vector4 GetTangent(uint index)
        {
            bool found = _vertexAttributes.TryGetValue("Tangent", out var attribute);

            if (found && attribute is not null)
            {
                var offset = (int)(attribute.Stride * index + attribute.Offset);
                var data = attribute.Data.AsSpan(offset);

                switch (attribute.DataType)
                {
                    case "half4":
                        return ReadVectorHalf4(data);
                    case "float3":
                        return new Vector4(ReadVector3(data), 0);
                    default:
                        throw new NotImplementedException($"Support for {attribute.Name} data type {attribute.DataType} is not implemented.");
                }
            }
            else
            {
                return Vector4.Zero;
            }
        }

        public Vector4 GetBinormal(uint index)
        {
            bool found = _vertexAttributes.TryGetValue("Binormal", out var attribute);

            if (found && attribute is not null)
            {
                var offset = (int)(attribute.Stride * index + attribute.Offset);
                var data = attribute.Data.AsSpan(offset);

                switch (attribute.DataType)
                {
                    case "half4":
                        return ReadVectorHalf4(data);
                    case "float3":
                        return new Vector4(ReadVector3(data), 0);
                    default:
                        throw new NotImplementedException($"Support for {attribute.Name} data type {attribute.DataType} is not implemented.");
                }
            }
            else
            {
                return Vector4.Zero;
            }
        }

        public Vector2 GetTexCoord(uint index, int texCoordSetIndex)
        {
            var attribute = texCoordSetIndex >= 0 && texCoordSetIndex < _texCoordSets.Count ?
                _texCoordSets[texCoordSetIndex] : null;

            if (attribute is not null)
            {
                var offset = (int)(attribute.Stride * index + attribute.Offset);
                var data = attribute.Data.AsSpan(offset);

                switch (attribute.DataType)
                {
                    case "half2":
                        return ReadVectorHalf2(data);
                    case "half4":
                        // read just vec2 for now
                        return ReadVectorHalf2(data);
                    case "float2":
                        return ReadVector2(data);
                    case "float4":
                        // read just vec2 for now
                        return ReadVector2(data);
                    default:
                        throw new NotImplementedException($"Support for {attribute.Name} data type {attribute.DataType} is not implemented.");
                }
            }
            else
            {
                if (texCoordSetIndex <= 0)
                    return Vector2.Zero;
                else
                    return GetTexCoord(index, texCoordSetIndex - 1);
            }
        }

        public uint GetColor(uint index)
        {
            bool found = _vertexAttributes.TryGetValue("Color", out var attribute);

            if (found && attribute is not null)
            {
                var offset = (int)(attribute.Stride * index + attribute.Offset);
                var data = attribute.Data.AsSpan(offset);

                switch (attribute.DataType)
                {
                    case "uint_color_argb":
                        return BinaryPrimitives.ReadUInt32BigEndian(data);
                    default:
                        throw new NotImplementedException($"Support for {attribute.Name} data type {attribute.DataType} is not implemented.");
                }
            }
            else
            {
                return uint.MinValue;
            }
        }

        private static Vector3 ReadVector3(ReadOnlySpan<byte> data)
        {
            var vec = new Vector3();
            vec.X = BinaryPrimitives.ReadSingleBigEndian(data);
            vec.Y = BinaryPrimitives.ReadSingleBigEndian(data.Slice(4));
            vec.Z = BinaryPrimitives.ReadSingleBigEndian(data.Slice(8));
            return vec;
        }

        private static Vector2 ReadVector2(ReadOnlySpan<byte> data)
        {
            var vec = new Vector2();
            vec.X = BinaryPrimitives.ReadSingleBigEndian(data);
            vec.Y = BinaryPrimitives.ReadSingleBigEndian(data.Slice(4));
            return vec;
        }

        private static Vector4 ReadVectorHalf4(ReadOnlySpan<byte> data)
        {
            var vec = new Vector4();
            vec.X = (float)BigToHalf(data);
            vec.Y = (float)BigToHalf(data.Slice(2));
            vec.Z = (float)BigToHalf(data.Slice(4));
            vec.W = (float)BigToHalf(data.Slice(6));
            return vec;
        }

        private static Vector2 ReadVectorHalf2(ReadOnlySpan<byte> data)
        {
            var vec = new Vector2();
            vec.X = (float)BigToHalf(data);
            vec.Y = (float)BigToHalf(data.Slice(2));
            return vec;
        }

        static Half BigToHalf(ReadOnlySpan<byte> source)
        {
            return Int16BitsToHalf(BinaryPrimitives.ReadInt16BigEndian(source));
        }
        static unsafe Half Int16BitsToHalf(short value)
        {
            return *(Half*)&value;
        }
    }
}
