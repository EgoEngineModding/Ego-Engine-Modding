namespace EgoEngineLibrary.Archive.Erp
{
    using ICSharpCode.SharpZipLib.Zip.Compression;
    using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Zstandard.Net;

    public class ErpFragment
    {
        public ErpFile ParentFile { get; set; }

        public string Name { get; set; }

        public UInt64 Offset { get; set; }
        public UInt64 Size { get; set; }
        public Int32 Flags { get; set; }
        public UInt64 PackedSize { get; set; }

        public ErpCompressionAlgorithm Compression { get; set; }

        internal byte[] _data;

        public ErpFragment(ErpFile parentFile)
        {
            this.ParentFile = parentFile;
            this.Name = "temp";
            this.Flags = 16;
            this.Compression = ErpCompressionAlgorithm.Zlib;
            this._data = Array.Empty<byte>();
        }

        public void Read(ErpBinaryReader reader)
        {
            this.Name = reader.ReadString(4);

            this.Offset = reader.ReadUInt64();
            this.Size = reader.ReadUInt64();
            this.Flags = reader.ReadInt32();

            if (this.ParentFile.Version > 2)
            {
                this.Compression = (ErpCompressionAlgorithm)reader.ReadByte();
                this.PackedSize = reader.ReadUInt64();
            }
            else
            {
                this.PackedSize = this.Size;
            }

            int pos = (int)reader.BaseStream.Position;
            reader.Seek((int)(this.ParentFile.ResourceOffset + this.Offset), SeekOrigin.Begin);
            this._data = reader.ReadBytes((int)this.PackedSize);
            reader.Seek(pos, SeekOrigin.Begin);
        }

        public void Write(ErpBinaryWriter writer)
        {
            writer.Write(this.Name, 4);

            writer.Write(this.Offset);
            writer.Write(this.Size);
            writer.Write(this.Flags);

            if (this.ParentFile.Version > 2)
            {
                writer.Write((byte)this.Compression);
                writer.Write(this.PackedSize);
            }
        }

        public void Export(Stream stream)
        {
            using (ErpBinaryWriter writer = new ErpBinaryWriter(EndianBitConverter.Little, stream))
            {
                writer.Write(this.GetDataArray(true));
            }
        }

        public void Import(Stream stream)
        {
            using (ErpBinaryReader reader = new ErpBinaryReader(EndianBitConverter.Little, stream))
            {
                this.SetData(reader.ReadBytes((int)reader.BaseStream.Length));
            }
        }

        public byte[] GetDataArray(bool decompress)
        {
            byte[] data;

            if (decompress)
            {
                switch (Compression)
                {
                    case ErpCompressionAlgorithm.None:
                    case ErpCompressionAlgorithm.None2:
                    case ErpCompressionAlgorithm.None3:
                        data = this._data;
                        break;
                    case ErpCompressionAlgorithm.Zlib:
                        using (var ms = new MemoryStream(this._data))
                        using (var iis = new InflaterInputStream(ms))
                        using (var mso = new MemoryStream())
                        {
                            iis.CopyTo(mso);
                            data = mso.ToArray();
                        }
                        break;
                    case ErpCompressionAlgorithm.ZStandard:
                        using (var ms = new MemoryStream(this._data))
                        using (var zss = new ZstandardStream(ms, CompressionMode.Decompress))
                        using (var mso = new MemoryStream())
                        {
                            zss.CopyTo(mso);
                            data = mso.ToArray();
                        }
                        break;
                    case ErpCompressionAlgorithm.LZ4:
                    default:
                        throw new NotSupportedException($"{nameof(ErpFragment)} compression type {Compression} is not supported!");
                }
            }
            else
            {
                data = this._data;
            }

            return data;
        }
        public MemoryStream GetDataStream(bool decompress)
        {
            return new MemoryStream(this.GetDataArray(decompress), true);
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
                        this._data = data;
                        break;
                    case ErpCompressionAlgorithm.Zlib:
                        using (var mso = new MemoryStream())
                        using (var dos = new DeflaterOutputStream(mso, new Deflater(Deflater.BEST_COMPRESSION)))
                        {
                            dos.Write(data, 0, data.Length);
                            dos.Flush();
                            dos.Finish();
                            this._data = mso.ToArray();
                        }
                        break;
                    case ErpCompressionAlgorithm.ZStandard:
                        using (var mso = new MemoryStream())
                        using (var zss = new ZstandardStream(mso, CompressionMode.Compress))
                        {
                            zss.CompressionLevel = 22;
                            zss.Write(data, 0, data.Length);
                            zss.Flush();
                            this._data = mso.ToArray();
                        }
                        break;
                    default:
                        throw new NotSupportedException($"{nameof(ErpFragment)} compression type {Compression} is not supported!");
                }
            }
            else
            {
                this._data = data;
            }

            this.Size = (UInt64)data.Length;
            this.PackedSize = (UInt64)this._data.Length;
        }
    }
}
