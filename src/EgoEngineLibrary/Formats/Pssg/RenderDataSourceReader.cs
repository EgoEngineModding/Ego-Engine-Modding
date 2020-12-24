using EgoEngineLibrary.Graphics;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Formats.Pssg
{
    public class RenderDataSourceReader
    {
        private record VertexAttributeData(string Name, string DataType, uint ElementCount, uint Offset, uint Stride, byte[] Data);

        private readonly string _indexFormat;
        private readonly byte[] _indexData;
        private readonly Dictionary<string, VertexAttributeData> _vertexAttributes;

        public uint VertexCount => _vertexAttributes.TryGetValue("Vertex", out var attr) ? attr.ElementCount : 0;

        public string Primitive { get; }
        public uint IndexCount { get; }

        public RenderDataSourceReader(PssgNode rdsNode)
        {
            var risNode = rdsNode.ChildNodes.FirstOrDefault(n => n.Name == "RENDERINDEXSOURCE") ??
                throw new InvalidDataException($"RDS node {rdsNode.Attributes["id"].GetValue<string>()} must have RENDERINDEXSOURCE as its first child.");
            var isdNode = risNode.ChildNodes.FirstOrDefault(n => n.Name == "INDEXSOURCEDATA") ??
                throw new InvalidDataException($"RENDERINDEXSOURCE node {risNode.Attributes["id"].GetValue<string>()} must have INDEXSOURCEDATA as its first child.");

            // Setup indices
            _indexFormat = risNode.Attributes["format"].GetValue<string>();
            Primitive = risNode.Attributes["primitive"].GetValue<string>();
            IndexCount = risNode.Attributes["count"].GetValue<uint>();
            _indexData = ((byte[])isdNode.Value);

            // Setup vertex attributes
            var renderStreamNodes = rdsNode.FindNodes("RENDERSTREAM").ToList();
            _vertexAttributes = new Dictionary<string, VertexAttributeData>();
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
                // skip multiple ST attributes for now
                if (attr.Name == "ST" && _vertexAttributes.ContainsKey("ST"))
                    continue;
                _vertexAttributes.Add(attr.Name, attr);
            }
        }


        public IEnumerable<(ushort A, ushort B, ushort C)> GetTriangles()
        {
            return GetTriangles(0, (int)IndexCount);
        }
        public IEnumerable<(ushort A, ushort B, ushort C)> GetTriangles(int startIndex, int indexCount)
        {
            if (_indexFormat != "ushort")
                throw new NotImplementedException($"Support for {_indexFormat} primitive index format not implemented.");

            if (Primitive != "triangles")
                throw new InvalidOperationException($"Cannot get triangle from primitive type {Primitive}.");

            var data = _indexData.AsMemory(startIndex * sizeof(ushort), indexCount * sizeof(ushort));
            var triCount = indexCount / 3;
            for (int i = 0; i < triCount; ++i)
            {
                var dataSpan = data.Span;
                var a = BinaryPrimitives.ReadUInt16BigEndian(dataSpan);
                var b = BinaryPrimitives.ReadUInt16BigEndian(dataSpan.Slice(2));
                var c = BinaryPrimitives.ReadUInt16BigEndian(dataSpan.Slice(4));
                yield return(a, b, c);

                data = data.Slice(6);
            }
        }

        public (ushort A, ushort B, ushort C) GetTriangle(int index)
        {
            if (_indexFormat != "ushort")
                throw new NotImplementedException($"Support for {_indexFormat} primitive index format not implemented.");

            if (Primitive != "triangles")
                throw new InvalidOperationException($"Cannot get triangle from primitive type {Primitive}.");

            var offset = index * 3;
            var data = _indexData.AsSpan(offset);

            var a = BinaryPrimitives.ReadUInt16BigEndian(data);
            var b = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2));
            var c = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4));
            return (a, b, c);
        }

        public Vector3 GetPosition(int index)
        {
            bool found = _vertexAttributes.TryGetValue("Vertex", out var attribute);

            if (found && attribute is not null)
            {
                if (attribute.DataType != "float3")
                    throw new NotImplementedException($"Support for {attribute.Name} data type {attribute.DataType} is not implemented.");

                var data = attribute.Data.AsSpan((int)attribute.Stride * index + (int)attribute.Offset);
                return ReadVector3(data);
            }
            else
            {
                return Vector3.Zero;
            }
        }

        public Vector3 GetNormal(int index)
        {
            bool found = _vertexAttributes.TryGetValue("Normal", out var attribute);

            if (found && attribute is not null)
            {
                var offset = (int)attribute.Stride * index + (int)attribute.Offset;
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

        public Vector4 GetTangent(int index)
        {
            bool found = _vertexAttributes.TryGetValue("Tangent", out var attribute);

            if (found && attribute is not null)
            {
                var offset = (int)attribute.Stride * index + (int)attribute.Offset;
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

        public Vector4 GetBinormal(int index)
        {
            bool found = _vertexAttributes.TryGetValue("Binormal", out var attribute);

            if (found && attribute is not null)
            {
                var offset = (int)attribute.Stride * index + (int)attribute.Offset;
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

        public Vector2 GetTexCoord(int index)
        {
            bool found = _vertexAttributes.TryGetValue("ST", out var attribute);

            if (found && attribute is not null)
            {
                var offset = (int)attribute.Stride * index + (int)attribute.Offset;
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
                return Vector2.Zero;
            }
        }

        public uint GetColor(int index)
        {
            bool found = _vertexAttributes.TryGetValue("Color", out var attribute);

            if (found && attribute is not null)
            {
                var offset = (int)attribute.Stride * index + (int)attribute.Offset;
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
