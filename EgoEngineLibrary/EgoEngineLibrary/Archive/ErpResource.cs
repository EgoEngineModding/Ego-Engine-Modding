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
                data = Ionic.Zlib.ZlibStream.UncompressBuffer(this._data);
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
                this._data = Ionic.Zlib.ZlibStream.CompressBuffer(data);
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
