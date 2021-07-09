using EgoEngineLibrary.Data.Pkg.Data;
using Newtonsoft.Json;
using System;
using System.IO;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgValue : PkgChunk
    {
        internal static Int32 _offset;

        protected override string ChunkType
        {
            get
            {
                return string.Empty;
            }
        }

        public PkgOffsetType ValueOffsetType { get; set; }

        public PkgComplexValue ComplexValueData { get; set; }

        public bool HasComplexValueData
        {
            get { return ValueOffsetType.Type == 128; }
        }

        public string ValueData
        {
            get
            {
                return ParentFile.RootItem.DataArray.GetData(ValueOffsetType);
            }
            set
            {
                ParentFile.RootItem.DataArray.SetData(value, ValueOffsetType);
            }
        }

        public PkgValue(PkgFile parentFile)
            : base(parentFile)
        {
            ValueOffsetType = new PkgOffsetType();
            ComplexValueData = new PkgObject(parentFile);
        }

        public override void Read(PkgBinaryReader reader)
        {
            ValueOffsetType = reader.ReadOffsetType();

            if (ValueOffsetType.Type == 128)
            {
                long pos = reader.BaseStream.Position;
                reader.Seek(ValueOffsetType.Offset, SeekOrigin.Begin);

                string chunkType = reader.ReadString(4);
                switch (chunkType)
                {
                    case "!idi":
                        ComplexValueData = new PkgObject(ParentFile);
                        ComplexValueData.Read(reader);
                        break;
                    case "!ili":
                        ComplexValueData = new PkgArray(ParentFile);
                        ComplexValueData.Read(reader);
                        break;
                    case "!iar":
                        ComplexValueData = new PkgDataArrayReference(ParentFile);
                        ComplexValueData.Read(reader);
                        break;
                    case "!vca":
                        ComplexValueData = new PkgDataArray(ParentFile);
                        ComplexValueData.Read(reader);
                        break;
                    case "!sbi":
                        ComplexValueData = PkgData.Create(ParentFile, "stri");
                        ComplexValueData.Read(reader);
                        break;
                    case "!vbi":
                        ComplexValueData = PkgData.Create(reader, ParentFile);
                        ComplexValueData.Read(reader);
                        break;
                    default:
                        throw new Exception("Chunk type not supported! " + chunkType);
                }

                reader.Seek((int)pos, SeekOrigin.Begin);
            }
        }

        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(ValueOffsetType);
        }
        public void WriteComplexValue(PkgBinaryWriter writer)
        {
            if (HasComplexValueData)
            {
                ComplexValueData.Write(writer);
            }
        }
        internal override void UpdateOffsets()
        {
            if (ValueOffsetType.Type == 128)
            {
                ValueOffsetType.Offset = PkgValue._offset;
            }

            if (HasComplexValueData)
            {
                ComplexValueData.UpdateOffsets();
            }
        }

        public override void FromJson(JsonTextReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    string val = (string?)reader.Value ?? string.Empty;
                    if (val.StartsWith("!iar "))
                    {
                        ValueOffsetType.Type = 128;
                        ComplexValueData = new PkgDataArrayReference(ParentFile);
                        ComplexValueData.FromJson(reader);
                    }
                    else
                    {
                        ValueData = val;
                    }
                    break;
                case JsonToken.StartObject:
                    ValueOffsetType.Type = 128;
                    ComplexValueData = new PkgObject(ParentFile);
                    ComplexValueData.FromJson(reader);
                    break;
                case JsonToken.StartArray:
                    ValueOffsetType.Type = 128;
                    ComplexValueData = new PkgArray(ParentFile);
                    ComplexValueData.FromJson(reader);
                    break;
                default:
                    new Exception("Unexpected token type! " + reader.TokenType);
                    break;
            }
        }
        public override void ToJson(JsonTextWriter writer)
        {
            if (ValueOffsetType.Type == 128)
            {
                ComplexValueData.ToJson(writer);
            }
            else
            {
                writer.WriteValue(ValueData);
            }
        }
    }
}
