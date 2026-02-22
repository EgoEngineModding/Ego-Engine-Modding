using System.Text.Json;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgDataArrayReference : PkgComplexValue
    {
        protected override string ChunkType
        {
            get
            {
                return "!iar";
            }
        }

        string reference;

        public PkgDataArrayReference(PkgFile parentFile)
            : base(parentFile)
        {
            reference = string.Empty;
        }
        
        public override void Read(PkgBinaryReader reader)
        {
            reference = reader.ReadString(8);
        }
        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(ChunkType, 4);
            writer.Write(reference, 8);
        }

        internal override void UpdateOffsets()
        {
            PkgValue._offset += 12;
        }

        public override void FromJson(ref Utf8JsonReader reader)
        {
            reference = (reader.GetString())?[5..] ?? string.Empty;
        }
        public override void ToJson(Utf8JsonWriter writer)
        {
            writer.WriteStringValue(ChunkType + " " + reference);
        }
    }
}
