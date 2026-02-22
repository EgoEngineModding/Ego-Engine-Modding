using System.Text;

using EgoEngineLibrary.IO.Hashing;

using Newtonsoft.Json;

namespace EgoEngineLibrary.Data.Pkg
{
    public abstract class PkgPairBase : PkgValue
    {
        private const string Ppv1Id = ".ppv1.";
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

        public override void FromJson(JsonTextReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    var name = (string?)reader.Value ?? string.Empty;
                    var v1Index = name.LastIndexOf(Ppv1Id, StringComparison.Ordinal);
                    NameData = v1Index >= 0 ? name[..v1Index] : name;
                    break;
                default:
                    throw new JsonException("Unexpected token type! " + reader.TokenType);
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
    }

    public class PkgPairV1 : PkgPairBase
    {
        public PkgPairV1(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public override void Read(PkgBinaryReader reader)
        {
            NameOffsetType = reader.ReadOffsetType();
            base.Read(reader);
            reader.ReadUInt32();
        }

        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(NameOffsetType);
            base.Write(writer);
            writer.Write(Fnv1a32.HashToUInt32(Encoding.UTF8.GetBytes(NameData)));
        }
    }
}
