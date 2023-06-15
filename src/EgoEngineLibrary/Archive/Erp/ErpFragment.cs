using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;
using System;
using System.IO;
using System.IO.Compression;
using ZstdSharp;

namespace EgoEngineLibrary.Archive.Erp
{
    public class ErpFragment
    {
        private byte[] _data;

        public ErpFile ParentFile { get; set; }

        public string Name { get; set; }

        public ulong Offset { get; set; }
        public ulong Size { get; private set; }
        public int Flags { get; set; }
        public ulong PackedSize { get; private set; }

        public ErpCompressionAlgorithm Compression { get; private set; }
        public bool IsCompressed
        {
            get
            {
                return Compression is ErpCompressionAlgorithm.Zlib
                    or ErpCompressionAlgorithm.ZStandard
                    or ErpCompressionAlgorithm.ZStandard2
                    or ErpCompressionAlgorithm.ZStandard3;
            }
        }

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
                if (!Enum.IsDefined(Compression))
                {
                    throw new NotSupportedException(
                        $"{nameof(ErpFragment)} compression type {Compression} is not supported!");
                }

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

        public byte[] GetDataArray(bool decompress)
        {
            if (decompress && IsCompressed)
            {
                using var bufferWriter = new ArrayPoolBufferWriter<byte>(Convert.ToInt32(Size));
                using var decompressStream = GetDecompressDataStream(true);
                decompressStream.CopyTo(bufferWriter.AsStream());
                return bufferWriter.WrittenSpan.ToArray();
            }
            else
            {
                return _data;
            }
        }

        /// <summary>
        /// Gets a stream of the data.
        /// </summary>
        /// <param name="decompress">Whether to fully decompress the raw data if it's compressed.</param>
        /// <returns>a stream of the data.</returns>
        public Stream GetDataStream(bool decompress)
        {
            var stream = GetDecompressDataStream(decompress);

            // the decompression streams don't support seek which is used by lots of other code
            // we'll have to decompress the entire data by copying it into another stream.
            if (decompress && IsCompressed)
            {
                var memStream = new MemoryStream();
                using (stream)
                {
                    stream.CopyTo(memStream);
                }
                stream = memStream;
                memStream.Seek(0, SeekOrigin.Begin);
            }

            return stream;
        }

        /// <summary>
        /// Gets a stream that does the decompression of the raw data if the data is compressed,
        /// otherwise the raw data is wrapped in a stream.
        /// </summary>
        /// <param name="decompress">Whether to wrap the data in a decompression stream if it is compressed.</param>
        /// <returns>a stream wrapped around the raw data.</returns>
        public Stream GetDecompressDataStream(bool decompress)
        {
            if (decompress && IsCompressed)
            {
                return Compression switch
                {
                    ErpCompressionAlgorithm.None or
                        ErpCompressionAlgorithm.None2 or
                        ErpCompressionAlgorithm.None3 or
                        ErpCompressionAlgorithm.None4 => _data.AsMemory().AsStream(),
                    ErpCompressionAlgorithm.Zlib => new ZLibStream(_data.AsMemory().AsStream(),
                        CompressionMode.Decompress),
                    ErpCompressionAlgorithm.ZStandard or
                        ErpCompressionAlgorithm.ZStandard2 or
                        ErpCompressionAlgorithm.ZStandard3 => new DecompressionStream(_data.AsMemory().AsStream()),
                    _ => throw new NotSupportedException(
                        $"{nameof(ErpFragment)} compression type {Compression} is not supported!"),
                };
            }
            else
            {
                return _data.AsMemory().AsStream();
            }
        }

        public void SetData(byte[] data, bool compress = true)
        {
            if (compress && IsCompressed)
            {
                switch (Compression)
                {
                    case ErpCompressionAlgorithm.None:
                    case ErpCompressionAlgorithm.None2:
                    case ErpCompressionAlgorithm.None3:
                    case ErpCompressionAlgorithm.None4:
                        _data = data;
                        break;
                    case ErpCompressionAlgorithm.Zlib:
                        using (var bufferWriter = new ArrayPoolBufferWriter<byte>())
                        using (var dos = new ZLibStream(bufferWriter.AsStream(), CompressionLevel.Optimal))
                        {
                            dos.Write(data, 0, data.Length);
                            dos.Flush();
                            _data = bufferWriter.WrittenSpan.ToArray();
                        }
                        break;
                    case ErpCompressionAlgorithm.ZStandard:
                    case ErpCompressionAlgorithm.ZStandard2:
                    case ErpCompressionAlgorithm.ZStandard3:
                        using (var bufferWriter = new ArrayPoolBufferWriter<byte>())
                        using (var zss = new CompressionStream(bufferWriter.AsStream(), ZstdSharp.Unsafe.Methods.ZSTD_defaultCLevel()))
                        {
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
