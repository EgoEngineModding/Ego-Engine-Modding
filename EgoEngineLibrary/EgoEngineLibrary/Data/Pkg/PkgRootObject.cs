using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgRootObject : PkgArray
    {
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
            reader.ReadBytes(4);
            name = reader.ReadString(4);

            Elements[0].Read(reader);
            Elements[1].Read(reader);
        }
        public override void Write(PkgBinaryWriter writer)
        {
            UpdateOffsets();
            writer.Write("!pkg", 4);
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
            name = (string)reader.Value;

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
