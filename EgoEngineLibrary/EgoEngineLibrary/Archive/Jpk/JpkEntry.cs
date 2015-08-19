namespace EgoEngineLibrary.Archive.Jpk
{
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class JpkEntry
    {
        public JpkFile ParentFile { get; set; }

        public string Name { get; set; }
        public byte[] Data { get; set; }

        public int Size
        {
            get
            {
                return this.Data.Length;
            }
        }

        private int _nameOffset;
        private int _fileOffset;

        public JpkEntry()
        {

        }
        public JpkEntry(JpkFile parentFile)
            : this()
        {
            this.ParentFile = parentFile;
        }

        public void Read(JpkBinaryReader reader)
        {
            this._nameOffset = reader.ReadInt32();
            int dataSize = reader.ReadInt32();
            this._fileOffset = reader.ReadInt32();
            //reader.ReadBytes(20); skip remaining 20 bytes

            reader.Seek(this._nameOffset, SeekOrigin.Begin);
            this.Name = reader.ReadString();
            reader.Seek(this._fileOffset, SeekOrigin.Begin);
            this.Data = reader.ReadBytes(dataSize);
        }

        public void Write(JpkBinaryWriter writer)
        {
            writer.Write(this._nameOffset);
            writer.Write(this.Size);
            writer.Write(this._fileOffset);
            writer.Write(this.Size);
            writer.Write(new byte[16]);
        }

        public void Export(Stream stream)
        {
            using (JpkBinaryWriter writer = new JpkBinaryWriter(EndianBitConverter.Little, stream))
            {
                writer.Write(this.Data);
            }
        }

        public void Import(Stream stream)
        {
            using (JpkBinaryReader reader = new JpkBinaryReader(EndianBitConverter.Little, stream))
            {
                this.Data = reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        public void UpdateNameOffset(ref int nameOffset)
        {
            this._nameOffset = nameOffset;

            nameOffset += 1 + this.Name.Length;
        }

        public void UpdateFileOffset(ref int fileOffset)
        {
            this._fileOffset = fileOffset;

            fileOffset += this.Size + ((-this.Size) & (this.ParentFile.Alignment - 1));
        }
    }
}
