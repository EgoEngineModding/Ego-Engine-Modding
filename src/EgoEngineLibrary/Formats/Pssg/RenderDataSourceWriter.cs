using System.Buffers.Binary;
using System.Numerics;
using EgoEngineLibrary.Graphics.Pssg;
using EgoEngineLibrary.Graphics.Pssg.Elements;

namespace EgoEngineLibrary.Formats.Pssg
{
    public class RenderDataSourceWriter
    {
        public string Name { get; }

        public List<uint> Indices { get; }

        public List<Vector3> Positions { get; }

        public List<Vector3> Normals { get; }

        public List<Vector4> Tangents { get; }

        public List<Vector2> TexCoords0 { get; }

        public List<Vector2> TexCoords1 { get; }

        public List<Vector2> TexCoords2 { get; }

        public List<Vector2> TexCoords3 { get; }

        public List<Vector4> Colors { get; }

        public List<float> SkinIndices { get; }

        public RenderDataSourceWriter(string name)
        {
            Name = name;

            Indices = new List<uint>();
            Positions = new List<Vector3>();
            Normals = new List<Vector3>();
            Tangents = new List<Vector4>();
            TexCoords0 = new List<Vector2>();
            TexCoords1 = new List<Vector2>();
            TexCoords2 = new List<Vector2>();
            TexCoords3 = new List<Vector2>();
            Colors = new List<Vector4>();
            SkinIndices = new List<float>();
        }

        public void Write(ShaderInputInfo shaderInput, PssgElement rdsLib, PssgElement ribLib, PssgModelWriterState state)
        {
            var streamCount = (uint)shaderInput.BlockInputs.Sum(bi => bi.VertexInputs.Count);

            var rdsNode = new PssgRenderDataSource(rdsLib.File, rdsLib);
            rdsNode.StreamCount = streamCount;
            rdsNode.Id = Name;
            rdsLib.ChildElements.Add(rdsNode);

            WriteIndices(rdsNode);

            // Write the data
            foreach (var bi in shaderInput.BlockInputs)
            {
                for (int i = 0; i < bi.VertexInputs.Count; ++i)
                {
                    var vi = bi.VertexInputs[i];
                    WriteRenderStream(rdsNode, state.DataBlockCount, state.RenderStreamCount, (uint)i);
                    state.RenderStreamCount++;
                }

                WriteDataBlock(bi, ribLib, state.DataBlockCount);
                state.DataBlockCount++;
            }

            static void WriteRenderStream(PssgElement rdsElement, uint dataBlockId, uint streamId, uint subStream)
            {
                PssgRenderStream rsElement = new(rdsElement.File, rdsElement);
                rsElement.DataBlock = $"#block{dataBlockId}";
                rsElement.SubStream = subStream;
                rsElement.Id = $"stream{streamId}";
                rdsElement.ChildElements.Add(rsElement);
            }
        }

        private void WriteIndices(PssgElement rdsElement)
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
                        BinaryPrimitives.WriteUInt16BigEndian(isdSpan, Convert.ToUInt16(index));
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

            var risNode = new PssgRenderIndexSource(rdsElement.File, rdsElement);
            risNode.Primitive = "triangles";
            risNode.MaximumIndex = maxIndex;
            risNode.Format = dataType;
            risNode.Count = (uint)Indices.Count;
            risNode.Id = Name.Replace("RDS", "RIS");
            rdsElement.ChildElements.Add(risNode);

            var isdNode = new PssgIndexSourceData(risNode.File, risNode);
            isdNode.Value = isdData;
            risNode.ChildElements.Add(isdNode);
        }

        private void WriteDataBlock(ShaderBlockInputInfo bi, PssgElement ribLib, uint dataBlockId)
        {
            var stride = bi.VertexInputs.First().Stride;
            var size = (uint)(stride * Positions.Count);

            var dbNode = new PssgDataBlock(ribLib.File, ribLib);
            dbNode.StreamCount = Convert.ToUInt16(bi.VertexInputs.Count);
            dbNode.Size = size;
            dbNode.ElementCount = (uint)Positions.Count;
            dbNode.Id = $"block{dataBlockId}";
            ribLib.ChildElements.Add(dbNode);

            var data = new byte[size];
            var dataSpan = data.AsSpan();
            var texCoordSet = 0;
            foreach (var vi in bi.VertexInputs)
            {
                var dbsNode = new PssgDataBlockStream(dbNode.File, dbNode);
                dbsNode.RenderType = vi.Name;
                dbsNode.DataType = vi.DataType;
                dbsNode.Offset = vi.Offset;
                dbsNode.Stride = vi.Stride;
                dbNode.ChildElements.Add(dbsNode);

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

            var dbdNode = new PssgDataBlockData(dbNode.File, dbNode);
            dbdNode.Value = data;
            dbNode.ChildElements.Add(dbdNode);
            return;

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
            var value = Positions[(int)elementIndex];

            switch (vi.DataType)
            {
                case "float3":
                    WriteVector3(destination, value);
                    break;
                case "half4":
                    WriteVectorHalf4(destination, new Vector4(value, 1));
                    break;
                default:
                    throw new NotImplementedException($"Support for {vi.Name} data type {vi.DataType} is not implemented.");
            }
        }

        private void WriteNormal(ShaderVertexInputInfo vi, uint elementIndex, Span<byte> destination)
        {
            var value = Normals[(int)elementIndex];

            switch (vi.DataType)
            {
                case "float3":
                    WriteVector3(destination, value);
                    break;
                case "half4":
                    WriteVectorHalf4(destination, new Vector4(value, 1));
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
                2 => TexCoords2,
                3 => TexCoords3,
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
                    BinaryPrimitives.WriteUInt32BigEndian(destination, PackArgbColor(value));
                    break;
                default:
                    throw new NotImplementedException($"Support for {vi.Name} data type {vi.DataType} is not implemented.");
            }

            static uint PackArgbColor(Vector4 vector)
            {
                Vector4 MaxBytes = new Vector4(byte.MaxValue);
                Vector4 Half = new Vector4(0.5f);
                vector *= MaxBytes;
                vector += Half;
                vector = Vector4.Clamp(vector, Vector4.Zero, MaxBytes);

                return (uint)((((byte)vector.W) << 0) | (((byte)vector.X) << 8) | (((byte)vector.Y) << 16) | (((byte)vector.Z) << 24));
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
