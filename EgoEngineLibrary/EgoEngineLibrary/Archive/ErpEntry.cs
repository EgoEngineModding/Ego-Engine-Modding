namespace EgoEngineLibrary.Archive
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

        public void Export(string folder)
        {
            string outputDir = Path.Combine(folder, Path.GetDirectoryName(this.FileName.Substring(7)));
            Directory.CreateDirectory(outputDir);

            for (int i = 0; i < this.Resources.Count; ++i)
            {
                this.Resources[i].Export(File.Open(
                    Path.Combine(outputDir, i.ToString() + Path.GetFileName(this.FileName))
                    , FileMode.Create, FileAccess.Write, FileShare.Read));
            }
        }

        public void Import(string folder)
        {

        }
    }
}
