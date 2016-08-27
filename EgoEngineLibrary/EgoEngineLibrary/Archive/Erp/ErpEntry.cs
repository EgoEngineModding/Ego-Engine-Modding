namespace EgoEngineLibrary.Archive.Erp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class ErpEntry
    {
        public ErpFile ParentFile { get; set; }
        public string FileName { get; set; }
        public string EntryType { get; set; }

        public Int32 Unknown { get; set; }

        public List<ErpResource> Resources { get; set; }

        public byte[] Hash;

        public UInt64 Size
        {
            get
            {
                UInt64 size = 0;
                foreach (ErpResource res in this.Resources)
                {
                    size += res.Size;
                }
                return size;
            }
        }
        public UInt64 PackedSize
        {
            get
            {
                UInt64 size = 0;
                foreach (ErpResource res in this.Resources)
                {
                    size += res.PackedSize;
                }
                return size;
            }
        }

        private UInt32 _entryInfoLength;

        public ErpEntry()
        {
            this.Unknown = 1;
            this.Resources = new List<ErpResource>();
            this.Hash = new byte[16];
        }
        public ErpEntry(ErpFile parentFile)
            : this()
        {
            this.ParentFile = parentFile;
        }

        public void Read(ErpBinaryReader reader)
        {
            reader.ReadBytes(4); // entry info length
            this.FileName = reader.ReadString(reader.ReadInt16());
            this.EntryType = reader.ReadString(16);

            this.Unknown = reader.ReadInt32();

            byte numResources = reader.ReadByte();

            while (numResources-- > 0)
            {
                ErpResource res = new ErpResource(this.ParentFile);
                res.Read(reader);
                this.Resources.Add(res);
            }

            if (this.ParentFile.Version > 2)
            {
                this.Hash = reader.ReadBytes(16);
            }
        }

        public void Write(ErpBinaryWriter writer)
        {
            writer.Write(this._entryInfoLength);
            writer.Write((Int16)(this.FileName.Length + 1));
            writer.Write(this.FileName);
            writer.Write(this.EntryType, 16);
            writer.Write(this.Unknown);
            writer.Write((byte)this.Resources.Count);

            foreach (ErpResource res in this.Resources)
            {
                res.Write(writer);
            }

            if (this.ParentFile.Version > 2)
            {
                writer.Write(this.Hash);
            }
        }

        public UInt32 UpdateOffsets()
        {
            if (this.ParentFile.Version > 2)
            {
                this._entryInfoLength = 33;
            }
            else
            {
                this._entryInfoLength = 24;
            }

            this._entryInfoLength *= (UInt32)this.Resources.Count;
            this._entryInfoLength += (UInt32)this.FileName.Length + 24;

            if (this.ParentFile.Version > 2)
            {
                this._entryInfoLength += 16;
            }

            return this._entryInfoLength;
        }

        public void Export(string folder)
        {
            string outputDir = Path.Combine(folder, Path.GetDirectoryName(this.FileName.Substring(7)));
            Directory.CreateDirectory(outputDir);

            for (int i = 0; i < this.Resources.Count; ++i)
            {
                string name = this.FileName;
                name = name.Replace("?", "^^");
                name = Path.GetFileNameWithoutExtension(name) + "!!!" + this.Resources[i].Name + i.ToString("000") + Path.GetExtension(name);
                this.Resources[i].Export(File.Open(
                    Path.Combine(outputDir, name)
                    , FileMode.Create, FileAccess.Write, FileShare.Read));
            }
        }

        public void Import(Stream stream, int resIndex)
        {
            this.Resources[resIndex].Import(stream);
        }
    }
}
