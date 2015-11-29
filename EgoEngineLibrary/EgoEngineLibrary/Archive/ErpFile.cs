namespace EgoEngineLibrary.Archive
{
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class ErpFile
    {
        public Int32 Version { get; set; }

        public UInt64 EntryOffset { get; set; }

        public List<ErpEntry> Entries { get; set; }

        private UInt64 _entryInfoTotalLength;

        public ErpFile()
        {
            this.Version = 3;
            this.Entries = new List<ErpEntry>();
        }

        public void Read(Stream stream)
        {
            using (ErpBinaryReader reader = new ErpBinaryReader(EndianBitConverter.Little, stream))
            {
                uint magic = reader.ReadUInt32();
                if (magic != 1263555141)
                {
                    throw new Exception("This is not an ERP file!");
                }

                this.Version = reader.ReadInt32();
                reader.ReadBytes(8); // padding
                reader.ReadBytes(8); // info offset
                reader.ReadBytes(8); // info size

                this.EntryOffset = reader.ReadUInt64();
                reader.ReadBytes(8); // padding

                Int32 numFiles = reader.ReadInt32();
                Int32 numTempFile = reader.ReadInt32();

                for (int i = 0; i < numFiles; ++i)
                {
                    ErpEntry entry = new ErpEntry(this);
                    entry.Read(reader);
                    this.Entries.Add(entry);
                }
            }
        }

        public void Write(Stream stream)
        {
            using (ErpBinaryWriter writer = new ErpBinaryWriter(EndianBitConverter.Little, stream))
            {
                Int32 numTempFiles = this.UpdateOffsets();

                writer.Write(1263555141);

                writer.Write(this.Version);
                writer.Write((Int64)0);
                writer.Write((Int64)48);
                writer.Write(this._entryInfoTotalLength);

                writer.Write(this.EntryOffset);
                writer.Write((Int64)0);

                writer.Write(this.Entries.Count);
                writer.Write(numTempFiles);

                foreach (ErpEntry entry in this.Entries)
                {
                    entry.Write(writer);
                }

                foreach (ErpEntry entry in this.Entries)
                {
                    foreach (ErpResource res in entry.Resources)
                    {
                        //writer.Write((UInt16)0xDA78);
                        writer.Write(res._data);
                    }
                }
            }
        }

        public Int32 UpdateOffsets()
        {
            UInt64 resourceDataOffset = 0;
            Int32 numTempFiles = 0;

            this._entryInfoTotalLength = (UInt64)this.Entries.Count * 4 + 8;
            foreach (ErpEntry entry in this.Entries)
            {
                this._entryInfoTotalLength += (UInt64)entry.UpdateOffsets();

                foreach (ErpResource res in entry.Resources)
                {
                    ++numTempFiles;
                    res.Offset = resourceDataOffset;
                    resourceDataOffset += res.PackedSize;
                }
            }

            this.EntryOffset = 48 + this._entryInfoTotalLength;
            return numTempFiles;
        }

        public ErpEntry FindEntry(string fileName)
        {
            foreach (ErpEntry entry in this.Entries)
            {
                if (entry.FileName == fileName)
                {
                    return entry;
                }
            }

            return null;
        }

        public void Export(string folderPath)
        {
            foreach (ErpEntry entry in this.Entries)
            {
                entry.Export(folderPath);
            }
        }

        public void Import(string folderPath)
        {
            foreach (string f in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                string extension = Path.GetExtension(f);
                string name = Path.GetFileNameWithoutExtension(f);
                int resTextIndex = name.LastIndexOf("!!!");
                if (resTextIndex == -1)
                {
                    continue;
                }

                int resIndex = Int32.Parse(name.Substring(resTextIndex + 7, 3));
                name = Path.GetDirectoryName(f) + "\\" + name.Remove(resTextIndex).Replace("^^", "?") + extension;
                foreach (ErpEntry entry in this.Entries)
                {
                    if (name.EndsWith(entry.FileName.Substring(7).Replace('/', '\\')))
                    {
                        entry.Import(File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read), resIndex);
                        break;
                    }
                }
            }
        }
    }
}
