using EgoEngineLibrary.Conversion;

namespace EgoEngineLibrary.Archive.Jpk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class JpkFile
    {
        public Int32 Alignment { get; set; }
        public List<JpkEntry> Entries { get; set; }

        public JpkFile()
        {
            this.Alignment = 16;
            this.Entries = new List<JpkEntry>();
        }

        public void Read(Stream stream)
        {
            using (JpkBinaryReader reader = new JpkBinaryReader(EndianBitConverter.Little, stream))
            {
                uint magic = reader.ReadUInt32();
                if (magic != 1262571594)
                {
                    throw new Exception("This is not a Jpk file!");
                }

                reader.ReadBytes(4); // unk - 0
                int numEntries = reader.ReadInt32();
                this.Alignment = reader.ReadInt32();
                reader.ReadBytes(4); // unk - 0
                int offsetToFileNames = reader.ReadInt32();
                reader.ReadBytes(8); // unk - 0

                for (int i = 0; i < numEntries; i++)
                {
                    reader.Seek(32 + i * 32, SeekOrigin.Begin);
                    JpkEntry entry = new JpkEntry(this);
                    entry.Read(reader);
                    this.Entries.Add(entry);
                }
            }
        }

        public void Write(Stream stream)
        {
            using (JpkBinaryWriter writer = new JpkBinaryWriter(EndianBitConverter.Little, stream))
            {
                writer.Write(1262571594); // JPAK
                writer.Write(new byte[4]);
                writer.Write(this.Entries.Count);
                writer.Write(this.Alignment);
                writer.Write(new byte[4]);
                writer.Write(this.Entries.Count * 32 + 32);
                writer.Write(new byte[8]);

                int nameOffset = this.UpdateOffsets();
                foreach (JpkEntry entry in this.Entries)
                {
                    entry.Write(writer);
                }

                writer.Write(new byte());
                foreach (JpkEntry entry in this.Entries)
                {
                    writer.Write(entry.Name);
                }
                writer.Write(new byte[(-nameOffset) & (this.Alignment - 1)]);

                foreach (JpkEntry entry in this.Entries)
                {
                    writer.Write(entry.Data);
                    writer.Write(new byte[(-entry.Size) & (this.Alignment - 1)]);
                }
            }
        }

        public int UpdateOffsets()
        {
            int nameOffset = 32 + this.Entries.Count * 32 + 1;
            for (int i = 0; i < this.Entries.Count; ++i)
            {
                this.Entries[i].UpdateNameOffset(ref nameOffset);
            }

            int fileOffset = nameOffset + ((-nameOffset) & (this.Alignment - 1));
            for (int i = 0; i < this.Entries.Count; ++i)
            {
                this.Entries[i].UpdateFileOffset(ref fileOffset);
            }

            return nameOffset;
        }

        public bool Contains(string entryName)
        {
            foreach (JpkEntry entry in this.Entries)
            {
                if (entry.Name.Equals(entryName))
                {
                    return true;
                }
            }

            return false;
        }

        public JpkEntry this[string entryName]
        {
            get
            {
                foreach (JpkEntry entry in this.Entries)
                {
                    if (entry.Name.Equals(entryName))
                    {
                        return entry;
                    }
                }

                throw new ArgumentOutOfRangeException("entryName", entryName, "This entry does not exist in the archive!");
            }
        }
    }
}
