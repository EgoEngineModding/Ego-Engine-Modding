namespace EgoEngineLibrary.Archive
{
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using zlib;

    public class ErpResource
    {
        public ErpFile ParentFile { get; set; }

        public string Name { get; set; }

        public UInt64 Offset { get; set; }
        public UInt64 Size { get; set; }
        public Int32 Flags { get; set; }
        public UInt64 PackedSize { get; set; }

        public byte Compression { get; set; }

        internal byte[] _data;

        public ErpResource()
        {
            this.Name = "temp";
            this.Flags = 16;
            this.Compression = 1;
        }
        public ErpResource(ErpFile parentFile)
            : this()
        {
            this.ParentFile = parentFile;
        }

        public void Read(ErpBinaryReader reader)
        {
            this.Name = reader.ReadString(4);

            this.Offset = reader.ReadUInt64();
            this.Size = reader.ReadUInt64();
            this.Flags = reader.ReadInt32();

            if (this.ParentFile.Version > 2)
            {
                this.Compression = reader.ReadByte();
                this.PackedSize = reader.ReadUInt64();
            }
            else
            {
                this.PackedSize = this.Size;
            }

            int pos = (int)reader.BaseStream.Position;
            reader.Seek((int)(this.ParentFile.EntryOffset + this.Offset), SeekOrigin.Begin);
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
                writer.Write(this.Compression);
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

        public byte[] GetDataArray(bool decompress)
        {
            byte[] data;
            if (decompress && this.Compression == 1)
            {
                data = this.Decompress(this._data);
            }
            else
            {
                data = this._data;
            }

            return data;
        }
        public MemoryStream GetDataStream(bool decompress)
        {
            return new MemoryStream(this.GetDataArray(decompress));
        }

        public void SetData(byte[] data)
        {
            if (this.Compression == 1)
            {
                this._data = this.CompressData(data);
            }
            else
            {
                this._data = data;
            }

            this.Size = (UInt64)data.Length;
            this.PackedSize = (UInt64)this._data.Length;
        }

        private byte[] CompressData(byte[] inData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream, zlibConst.Z_BEST_COMPRESSION))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                return outMemoryStream.ToArray();
            }
        }
        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }  
        private byte[] Compress(byte[] data)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (DeflateStream stream = new DeflateStream(memory, CompressionMode.Compress))
                {
                    using (MemoryStream dataStream = new MemoryStream(data))
                    {
                        memory.WriteByte(0x78);
                        memory.WriteByte(0xDA);
                        dataStream.CopyTo(stream);
                        memory.Write(EndianBitConverter.Big.GetBytes(this.Adler32(data)), 0, 4);
                        stream.Close();
                        return memory.ToArray();
                    }
                }
            }
        }
        private int Adler32(byte[] bytes)
        {
            const uint a32mod = 65521;
            uint s1 = 1, s2 = 0;
            foreach (byte b in bytes)
            {
                s1 = (s1 + b) % a32mod;
                s2 = (s2 + s1) % a32mod;
            }
            return unchecked((int)((s2 << 16) + s1));
        }
        private byte[] Decompress(byte[] gzip)
        {
            // Deflate the raw zlib data
            using (MemoryStream zlibStream = new MemoryStream(gzip))
            {
                zlibStream.Seek(2, SeekOrigin.Begin);
                using (DeflateStream stream = new DeflateStream(zlibStream, CompressionMode.Decompress))
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        stream.CopyTo(memory);
                        return memory.ToArray();
                    }
                }
            }
        }
    }
}
