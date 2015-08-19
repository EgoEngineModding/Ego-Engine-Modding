namespace EgoEngineLibrary.Archive
{
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

    public class ErpResource
    {
        public ErpFile ParentFile { get; set; }

        public string Name { get; set; }

        public UInt64 Offset { get; set; }
        public UInt64 Size { get; set; }
        public Int32 Flags { get; set; }
        public UInt64 PackedSize { get; set; }

        public byte Compression { get; set; }

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
        }

        public void Export(Stream stream)
        {
            byte[] data;
            ErpBinaryReader reader = new ErpBinaryReader(EndianBitConverter.Little, this.ParentFile.ErpStream);
            reader.Seek((int)(this.ParentFile.EntryOffset + this.Offset) + 2, SeekOrigin.Begin);
            data = reader.ReadBytes((int)this.PackedSize);

            if (this.Compression == 1)
            {
                data = this.Decompress(data);
            }

            using (ErpBinaryWriter writer = new ErpBinaryWriter(EndianBitConverter.Little, stream))
            {
                writer.Write(data);
            }
        }

        private byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (DeflateStream stream = new DeflateStream(new MemoryStream(gzip), CompressionMode.Decompress))
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
