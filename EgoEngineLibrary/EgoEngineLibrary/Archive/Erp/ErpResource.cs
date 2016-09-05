namespace EgoEngineLibrary.Archive.Erp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class ErpResource
    {
        public ErpFile ParentFile { get; set; }
        public string Identifier { get; private set; }
        public string ResourceType { get; set; }

        public Int32 Unknown { get; set; }

        public List<ErpFragment> Fragments { get; set; }

        public byte[] Hash;

        public string FileName
        {
            get
            {
                Uri uri = new Uri(Identifier, UriKind.RelativeOrAbsolute);
                if (uri.IsAbsoluteUri)
                {
                    return Path.GetFileName(uri.PathAndQuery);
                }
                else
                {
                    return Path.GetFileName(uri.ToString());
                }
            }
        }
        public string Folder
        {
            get
            {
                Uri uri = new Uri(Identifier, UriKind.RelativeOrAbsolute);
                if (uri.IsAbsoluteUri)
                {
                    return Path.GetDirectoryName(uri.Host + uri.PathAndQuery).Replace("/", "\\");
                }
                else
                {
                    string temp = uri.ToString();
                    if (temp.Contains("/") || temp.Contains("\\"))
                    {
                        return Path.GetDirectoryName(temp).Replace("/", "\\");
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        public UInt64 Size
        {
            get
            {
                UInt64 size = 0;
                foreach (ErpFragment res in this.Fragments)
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
                foreach (ErpFragment res in this.Fragments)
                {
                    size += res.PackedSize;
                }
                return size;
            }
        }

        private UInt32 _resourceInfoLength;

        public ErpResource()
        {
            this.Unknown = 1;
            this.Fragments = new List<ErpFragment>();
            this.Hash = new byte[16];
        }
        public ErpResource(ErpFile parentFile)
            : this()
        {
            this.ParentFile = parentFile;
        }

        public void Read(ErpBinaryReader reader)
        {
            reader.ReadBytes(4); // entry info length
            this.Identifier = reader.ReadString(reader.ReadInt16());
            this.ResourceType = reader.ReadString(16);

            this.Unknown = reader.ReadInt32();

            byte numResources = reader.ReadByte();

            while (numResources-- > 0)
            {
                ErpFragment res = new ErpFragment(this.ParentFile);
                res.Read(reader);
                this.Fragments.Add(res);
            }

            if (this.ParentFile.Version > 2)
            {
                this.Hash = reader.ReadBytes(16);
            }
        }

        public void Write(ErpBinaryWriter writer)
        {
            writer.Write(this._resourceInfoLength);
            writer.Write((Int16)(this.Identifier.Length + 1));
            writer.Write(this.Identifier);
            writer.Write(this.ResourceType, 16);
            writer.Write(this.Unknown);
            writer.Write((byte)this.Fragments.Count);

            foreach (ErpFragment res in this.Fragments)
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
                this._resourceInfoLength = 33;
            }
            else
            {
                this._resourceInfoLength = 24;
            }

            this._resourceInfoLength *= (UInt32)this.Fragments.Count;
            this._resourceInfoLength += (UInt32)this.Identifier.Length + 24;

            if (this.ParentFile.Version > 2)
            {
                this._resourceInfoLength += 16;
            }

            return this._resourceInfoLength;
        }

        public void Export(string folder)
        {
            string outputDir = Path.Combine(folder, this.Folder);
            Directory.CreateDirectory(outputDir);

            for (int i = 0; i < this.Fragments.Count; ++i)
            {
                string name = this.FileName;
                name = name.Replace("?", "^^");
                name = Path.GetFileNameWithoutExtension(name) + "!!!" + this.Fragments[i].Name + i.ToString("000") + Path.GetExtension(name);
                this.Fragments[i].Export(File.Open(
                    Path.Combine(outputDir, name)
                    , FileMode.Create, FileAccess.Write, FileShare.Read));
            }
        }

        public bool Import(string[] files)
        {
            int fragmentsImported = 0;

            foreach (string f in files)
            {
                string extension = Path.GetExtension(f);
                string name = Path.GetFileNameWithoutExtension(f);
                int resTextIndex = name.LastIndexOf("!!!");
                if (resTextIndex == -1)
                {
                    continue;
                }

                int resIndex = Int32.Parse(name.Substring(resTextIndex + 7, 3));
                name = Path.GetDirectoryName(f) + "\\" + (name.Remove(resTextIndex) + extension).Replace("^^", "?");
                if (name.EndsWith(Path.Combine(this.Folder, this.FileName)))
                {
                    Fragments[resIndex].Import(File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read));
                    ++fragmentsImported;
                    continue;
                }
            }

            if (fragmentsImported == Fragments.Count)
                return true;
            return false;
        }

        public ErpFragment TryGetFragment(string name, int count)
        {
            try
            {
                return GetFragment(name, count);
            }
            catch
            {
                return null;
            }
        }
        public ErpFragment GetFragment(string name, int count)
        {
            foreach (ErpFragment fragment in Fragments)
            {
                if (fragment.Name == name)
                {
                    if (count == 0)
                    {
                        return fragment;
                    }
                    --count;
                }
            }

            throw new ArgumentOutOfRangeException("name", name);
        }
    }
}
