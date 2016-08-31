using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgPair : PkgValue
    {
        PkgOffsetType nameOffsetType;

        public PkgOffsetType NameOffsetType
        {
            get
            {
                return nameOffsetType;
            }

            set
            {
                nameOffsetType = value;
            }
        }

        public string NameData
        {
            get
            {
                return ParentFile.RootItem.DataArray.GetData(nameOffsetType).Substring(5);
            }
            set
            {
                ParentFile.RootItem.DataArray.SetData("stri " + value, nameOffsetType);
            }
        }

        public PkgPair(PkgFile parentFile)
            : base(parentFile)
        {
            nameOffsetType = new PkgOffsetType();
        }

        public override void Read(PkgBinaryReader reader)
        {
            nameOffsetType = reader.ReadOffsetType();
            base.Read(reader);
        }

        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(nameOffsetType);
            base.Write(writer);
        }

        public override void FromJson(JsonTextReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    NameData = (string)reader.Value;
                    break;
                default:
                    new Exception("Unexpected token type! " + reader.TokenType);
                    break;
            }
            reader.Read();
            base.FromJson(reader);
        }
        public override void ToJson(JsonTextWriter writer)
        {
            writer.WritePropertyName(NameData);
            base.ToJson(writer);
        }
    }
}
