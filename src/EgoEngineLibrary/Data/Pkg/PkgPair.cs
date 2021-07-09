using System;
using Newtonsoft.Json;

namespace EgoEngineLibrary.Data.Pkg
{
    public abstract class PkgPairBase : PkgValue
    {
        public PkgOffsetType NameOffsetType { get; set; }

        public string NameData
        {
            get
            {
                return ParentFile.RootItem.DataArray.GetData(NameOffsetType).Substring(5);
            }
            set
            {
                ParentFile.RootItem.DataArray.SetData("stri " + value, NameOffsetType);
            }
        }

        public PkgPairBase(PkgFile parentFile)
            : base(parentFile)
        {
            NameOffsetType = new PkgOffsetType();
        }
    }

    public class PkgPair : PkgPairBase
    {
        public PkgPair(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            NameOffsetType = reader.ReadOffsetType();
            base.Read(reader);
        }

        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(NameOffsetType);
            base.Write(writer);
        }

        public override void FromJson(JsonTextReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    NameData = (string?)reader.Value ?? string.Empty;
                    break;
                default:
                    throw new Exception("Unexpected token type! " + reader.TokenType);
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

    public class PkgPairV1 : PkgPairBase
    {
        private const string Ppv1Id = ".ppv1.";

        public uint Unknown { get; set; }

        public PkgPairV1(PkgFile parentFile)
            : base(parentFile)
        {
            Unknown = 0;
        }

        public override void Read(PkgBinaryReader reader)
        {
            NameOffsetType = reader.ReadOffsetType();
            base.Read(reader);
            Unknown = reader.ReadUInt32();
        }

        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(NameOffsetType);
            base.Write(writer);
            writer.Write(Unknown);
        }

        public override void FromJson(JsonTextReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    var name = (string?)reader.Value ?? string.Empty;
                    var v1Index = name.LastIndexOf(Ppv1Id);
                    if (v1Index >= 0)
                    {
                        Unknown = uint.Parse(name[(v1Index + Ppv1Id.Length)..]);
                        NameData = name[..v1Index];
                    }
                    else
                    {
                        NameData = name;
                    }
                    break;
                default:
                    throw new Exception("Unexpected token type! " + reader.TokenType);
            }
            reader.Read();
            base.FromJson(reader);
        }

        public override void ToJson(JsonTextWriter writer)
        {
            writer.WritePropertyName($"{NameData}{Ppv1Id}{Unknown}");
            base.ToJson(writer);
        }
    }
}
