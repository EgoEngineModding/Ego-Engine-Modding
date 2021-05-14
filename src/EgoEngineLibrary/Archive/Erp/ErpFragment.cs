namespace EgoEngineLibrary.Archive.Erp
{
    using ICSharpCode.SharpZipLib.Zip.Compression;
    using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
    using Microsoft.Toolkit.HighPerformance;
    using Microsoft.Toolkit.HighPerformance.Buffers;
    using MiscUtil.Conversion;
    using System;
    using System.IO;
    using System.IO.Compression;
    using Zstandard.Net;

    public class ErpFragment
    {
        public ErpFile ParentFile { get; set; }

        public string Name { get; set; }

        public ulong Offset { get; set; }
        public ulong Size { get; private set; }
        public int Flags { get; set; }
        public ulong PackedSize { get; private set; }

        public ErpCompressionAlgorithm Compression { get; private set; }

        private byte[] _data;

        public ErpFragment(ErpFile parentFile)
        {
            ParentFile = parentFile;
            Name = "temp";
            Flags = 16;
            Compression = ErpCompressionAlgorithm.Zlib;
            _data = Array.Empty<byte>();
        }

        public void Read(ErpBinaryReader reader)
        {
            Name = reader.ReadString(4);

            Offset = reader.ReadUInt64();
            Size = reader.ReadUInt64();
            Flags = reader.ReadInt32();

            if (ParentFile.Version > 2)
            {
                Compression = (ErpCompressionAlgorithm)reader.ReadByte();
                PackedSize = reader.ReadUInt64();
            }
            else
            {
                PackedSize = Size;
            }

            var pos = Convert.ToInt32(reader.BaseStream.Position);
            reader.Seek(Convert.ToInt32(ParentFile.ResourceOffset + Offset), SeekOrigin.Begin);
            _data = reader.ReadBytes(Convert.ToInt32(PackedSize));
            reader.Seek(pos, SeekOrigin.Begin);
        }

        public void Write(ErpBinaryWriter writer)
        {
            writer.Write(Name, 4);

            writer.Write(Offset);
            writer.Write(Size);
            writer.Write(Flags);

            if (ParentFile.Version > 2)
            {
                writer.Write((byte)Compression);
                writer.Write(PackedSize);
            }
        }

        public void Export(Stream stream)
        {
            using var decompressStream = GetDataStream(true);
            decompressStream.CopyTo(stream);
        }

        public void Import(Stream stream)
        {
            using var reader = new ErpBinaryReader(EndianBitConverter.Little, stream);
            SetData(reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length)));
        }

        public byte[] GetDataArray(bool decompress)
        {
            if (decompress)
            {
                using var bufferWriter = new ArrayPoolBufferWriter<byte>(Convert.ToInt32(Size));
                using var decompressStream = GetDataStream(true);
                decompressStream.CopyTo(bufferWriter.AsStream());
                return bufferWriter.WrittenSpan.ToArray();
            }
            else
            {
                return _data;
            }
        }
        public Stream GetDataStream(bool decompress)
        {
            if (decompress)
            {
                return Compression switch
                {
                    ErpCompressionAlgorithm.None or
                    ErpCompressionAlgorithm.None2 or
                    ErpCompressionAlgorithm.None3 => _data.AsMemory().AsStream(),
                    ErpCompressionAlgorithm.Zlib => new InflaterInputStream(_data.AsMemory().AsStream()),
                    ErpCompressionAlgorithm.ZStandard => new ZstandardStream(_data.AsMemory().AsStream(), CompressionMode.Decompress),
                    _ => throw new NotSupportedException($"{nameof(ErpFragment)} compression type {Compression} is not supported!"),
                };
            }
            else
            {
                return _data.AsMemory().AsStream();
            }
        }

        public void SetData(byte[] data, bool compress = true)
        {
            if (compress)
            {
                switch (Compression)
                {
                    case ErpCompressionAlgorithm.None:
                    case ErpCompressionAlgorithm.None2:
                    case ErpCompressionAlgorithm.None3:
                        _data = data;
                        break;
                    case ErpCompressionAlgorithm.Zlib:
                        using (var bufferWriter = new ArrayPoolBufferWriter<byte>())
                        using (var dos = new DeflaterOutputStream(bufferWriter.AsStream(), new Deflater(Deflater.BEST_COMPRESSION)))
                        {
                            dos.Write(data, 0, data.Length);
                            dos.Flush();
                            dos.Finish();
                            _data = bufferWriter.WrittenSpan.ToArray();
                        }
                        break;
                    case ErpCompressionAlgorithm.ZStandard:
                        using (var bufferWriter = new ArrayPoolBufferWriter<byte>())
                        using (var zss = new ZstandardStream(bufferWriter.AsStream(), CompressionMode.Compress))
                        {
                            zss.CompressionLevel = 22;
                            zss.Write(data, 0, data.Length);
                            zss.Flush();
                            _data = bufferWriter.WrittenSpan.ToArray();
                        }
                        break;
                    default:
                        throw new NotSupportedException($"{nameof(ErpFragment)} compression type {Compression} is not supported!");
                }
            }
            else
            {
                _data = data;
            }

            Size = (ulong)data.Length;
            PackedSize = (ulong)_data.Length;
        }
    }
}
