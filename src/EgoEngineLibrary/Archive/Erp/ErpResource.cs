using System;
using System.Collections.Generic;
using System.IO;

namespace EgoEngineLibrary.Archive.Erp
{
    public class ErpResource
    {
        public ErpFile ParentFile { get; set; }
        public string Identifier { get; private set; }
        public string ResourceType { get; set; }

        public int Unknown { get; set; }
        public short Unknown2 { get; set; }

        public List<ErpFragment> Fragments { get; }

        public byte[] Hash { get; private set; }

        public string FileName
        {
            get
            {
                if (Identifier.StartsWith("eaid", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Path.GetFileName(Identifier[7..]);
                }
                else
                {
                    return Path.GetFileName(Identifier);
                }
            }
        }
        public string Folder
        {
            get
            {
                if (Identifier.StartsWith("eaid", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Path.GetDirectoryName(Identifier[7..])?.Replace('/', '\\') ?? string.Empty;
                }
                else
                {
                    var temp = Identifier;
                    if (temp.Contains('/') || temp.Contains('\\'))
                    {
                        return Path.GetDirectoryName(temp)?.Replace('/', '\\') ?? string.Empty;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        public ulong Size
        {
            get
            {
                ulong size = 0;
                foreach (var res in Fragments)
                {
                    size += res.Size;
                }
                return size;
            }
        }
        public ulong PackedSize
        {
            get
            {
                ulong size = 0;
                foreach (var res in Fragments)
                {
                    size += res.PackedSize;
                }
                return size;
            }
        }

        private uint _resourceInfoLength;

        public ErpResource(ErpFile parentFile)
        {
            ParentFile = parentFile;
            Identifier = string.Empty;
            ResourceType = string.Empty;
            Unknown = 1;
            Unknown2 = 0;
            Fragments = new List<ErpFragment>();
            Hash = new byte[16];
        }

        public void Read(ErpBinaryReader reader)
        {
            reader.ReadBytes(4); // entry info length
            Identifier = reader.ReadString(reader.ReadInt16());
            ResourceType = reader.ReadString(16);

            Unknown = reader.ReadInt32();
            if (ParentFile.Version >= 4)
            {
                Unknown2 = reader.ReadInt16();
            }

            var numResources = reader.ReadByte();

            while (numResources-- > 0)
            {
                var res = new ErpFragment(ParentFile);
                res.Read(reader);
                Fragments.Add(res);
            }

            if (ParentFile.Version > 2)
            {
                Hash = reader.ReadBytes(16);
            }
        }

        public void Write(ErpBinaryWriter writer)
        {
            writer.Write(_resourceInfoLength);
            writer.Write((short)(Identifier.Length + 1));
            writer.Write(Identifier);
            writer.Write(ResourceType, 16);
            writer.Write(Unknown);
            if (ParentFile.Version >= 4)
            {
                writer.Write(Unknown2);
            }
            writer.Write((byte)Fragments.Count);

            foreach (var res in Fragments)
            {
                res.Write(writer);
            }

            if (ParentFile.Version > 2)
            {
                writer.Write(Hash);
            }
        }

        public uint UpdateOffsets()
        {
            if (ParentFile.Version > 2)
            {
                _resourceInfoLength = 33;
            }
            else
            {
                _resourceInfoLength = 24;
            }

            _resourceInfoLength *= (uint)Fragments.Count;
            _resourceInfoLength += (uint)Identifier.Length + 24;

            if (ParentFile.Version >= 4)
            {
                _resourceInfoLength += 2;
            }

            if (ParentFile.Version > 2)
            {
                _resourceInfoLength += 16;
            }

            return _resourceInfoLength;
        }

        public ErpFragment? TryGetFragment(string name, int count)
        {
            foreach (var fragment in Fragments)
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

            return null;
        }
        public ErpFragment GetFragment(string name, int count)
        {
            var fragment = TryGetFragment(name, count);
            if (fragment == null)
                throw new ArgumentOutOfRangeException(nameof(name));
            return fragment;
        }
    }
}
