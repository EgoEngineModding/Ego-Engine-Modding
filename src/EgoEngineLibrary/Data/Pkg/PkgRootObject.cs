using Newtonsoft.Json;
using System.IO;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgRootObject : PkgArray
    {
        private const uint Magic = 1735094305;
        string name;

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public PkgComplexValue FirstComplexValue
        {
            get { return Elements[0].ComplexValueData; }
            set { Elements[0].ComplexValueData = value; }
        }

        public PkgDataArray DataArray
        {
            get
            {
                return (PkgDataArray)Elements[1].ComplexValueData;
            }
            set
            {
                Elements[1].ComplexValueData = value;
            }
        }

        public PkgRootObject(PkgFile parentFile)
            : base(parentFile)
        {
            name = string.Empty;
            Elements.Add(new PkgValue(parentFile));
            Elements.Add(new PkgValue(parentFile));
        }

        public override void Read(PkgBinaryReader reader)
        {
            var magic = reader.ReadUInt32();
            if (magic != Magic)
                throw new FileFormatException("This is not a pkg file.");
            name = reader.ReadString(4);

            Elements[0].Read(reader);
            Elements[1].Read(reader);
        }
        public override void Write(PkgBinaryWriter writer)
        {
            PkgValue._offset = 0;
            UpdateOffsets();
            writer.Write(Magic);
            writer.Write(name, 4);

            Elements[0].Write(writer);
            Elements[1].Write(writer);
            Elements[0].WriteComplexValue(writer);
            Elements[1].WriteComplexValue(writer);
        }

        public override void FromJson(JsonTextReader reader)
        {
            DataArray = new PkgDataArray(ParentFile);

            reader.Read();
            reader.Read();
            name = (string?)reader.Value ?? string.Empty;

            reader.Read();
            Elements[0].FromJson(reader);
            Elements[1].ValueOffsetType.Type = 128;
        }

        public override void ToJson(JsonTextWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(name);
            FirstComplexValue.ToJson(writer);
            writer.WriteEndObject();
        }
    }
}
