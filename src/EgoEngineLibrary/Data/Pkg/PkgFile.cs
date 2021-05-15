using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace EgoEngineLibrary.Data.Pkg
{
    public enum PkgFileType
    {
        Pkg, Json
    }

    public class PkgFile
    {
        private readonly PkgRootObject rootItem;

        public PkgRootObject RootItem
        {
            get
            {
                return rootItem;
            }
        }

        public PkgFile()
        {
            rootItem = new PkgRootObject(this);
        }

        public static PkgFile Open(Stream stream)
        {
            var header = new byte[4];
            stream.Read(header, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);
            var magic = Encoding.UTF8.GetString(header);
            
            if (magic == "!pkg")
            {
                return ReadPkg(stream);
            }
            else if (magic[0] == '{')
            {
                return ReadJson(stream);
            }
            else
            {
                throw new FileFormatException("This is not a package file!");
            }
        }
        public static PkgFile ReadPkg(Stream stream)
        {
            var file = new PkgFile();
            using (var reader = new PkgBinaryReader(stream))
            {
                file.RootItem.Read(reader);
            }
            return file;
        }
        public static PkgFile ReadJson(Stream stream)
        {
            var file = new PkgFile();
            using (var reader = new JsonTextReader(new StreamReader(stream)))
            {
                file.RootItem.FromJson(reader);
            }
            return file;
        }

        public void Save(Stream stream, PkgFileType type)
        {
            switch (type)
            {
                case PkgFileType.Pkg:
                    WritePkg(stream);
                    break;
                case PkgFileType.Json:
                    WriteJson(stream);
                    break;
                default:
                    throw new Exception("Invalid Pkg file save type!");
            }
        }
        public void WritePkg(Stream stream)
        {
            using var writer = new PkgBinaryWriter(stream);
            rootItem.Write(writer);
        }
        public void WriteJson(Stream stream)
        {
            WriteJson(new StreamWriter(stream));
        }
        public void WriteJson(TextWriter textWriter)
        {
            using var writer = new JsonTextWriter(textWriter);
            writer.Formatting = Formatting.Indented;
            rootItem.ToJson(writer);
        }
    }
}
