using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EgoEngineLibrary.Data.Pkg
{
    public enum PkgFileType
    {
        Pkg, Json
    }

    public class PkgFile
    {
        private const uint Magic = 1735094305; // !pkg
        private static readonly JsonSerializerOptions jsonSerializerOptions =
            new() { Converters = { PkgFileJsonConverter.Instance } };
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
        public PkgFile(PkgRootObject rootObject)
        {
            rootItem = rootObject;
        }

        public static bool IsPkgFile(Stream stream)
        {
            var header = new byte[4];
            stream.ReadExactly(header, 0, 4);
            var magic = BitConverter.ToUInt32(header);
            return magic == Magic;
        }

        public static PkgFile Open(Stream stream)
        {
            var header = new byte[4];
            stream.ReadExactly(header, 0, 4);
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
            var file = JsonSerializer.Deserialize<PkgFile>(stream, jsonSerializerOptions) ??
                       throw new JsonException("Failed to read pkg json file!");
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
            using var writer = new PkgBinaryWriter(stream, leaveOpen: true);
            rootItem.Write(writer);
        }
        public void WriteJson(Stream stream)
        {
            using var jw = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            WriteJson(jw);
        }
        public void WriteJson(Utf8JsonWriter jsonWriter)
        {
            rootItem.ToJson(jsonWriter);
        }

        private class PkgFileJsonConverter : JsonConverter<PkgFile>
        {
            public static PkgFileJsonConverter Instance { get; } = new();
            
            public override PkgFile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var pkg = new PkgFile();
                pkg.RootItem.FromJson(ref reader);
                return pkg;
            }

            public override void Write(Utf8JsonWriter writer, PkgFile value, JsonSerializerOptions options)
            {
                value.WriteJson(writer);
            }
        }
    }
}
