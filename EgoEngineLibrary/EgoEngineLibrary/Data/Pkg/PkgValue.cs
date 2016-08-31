using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgValue : PkgChunk
    {
        internal static Int32 _offset;
        PkgOffsetType valueOffsetType;
        PkgComplexValue complexValueData;

        protected override string ChunkType
        {
            get
            {
                return string.Empty;
            }
        }

        public PkgOffsetType ValueOffsetType
        {
            get
            {
                return valueOffsetType;
            }

            set
            {
                valueOffsetType = value;
            }
        }

        public PkgComplexValue ComplexValueData
        {
            get
            {
                return complexValueData;
            }

            set
            {
                complexValueData = value;
            }
        }

        public bool HasComplexValueData
        {
            get { return complexValueData != null; }
        }

        public string ValueData
        {
            get
            {
                return ParentFile.RootItem.DataArray.GetData(valueOffsetType);
            }
            set
            {
                ParentFile.RootItem.DataArray.SetData(value, valueOffsetType);
            }
        }

        public PkgValue(PkgFile parentFile)
            : base(parentFile)
        {
            valueOffsetType = new PkgOffsetType();
        }

        public override void Read(PkgBinaryReader reader)
        {
            valueOffsetType = reader.ReadOffsetType();


            if (valueOffsetType.Type == 128)
            {
                long pos = reader.BaseStream.Position;
                reader.Seek(valueOffsetType.Offset, SeekOrigin.Begin);

                string chunkType = reader.ReadString(4);
                switch (chunkType)
                {
                    case "!idi":
                        complexValueData = new PkgObject(ParentFile);
                        complexValueData.Read(reader);
                        break;
                    case "!ili":
                        complexValueData = new PkgArray(ParentFile);
                        complexValueData.Read(reader);
                        break;
                    case "!iar":
                        reader.ReadBytes(8);
                        goto default;
                        break;
                    case "!vca":
                        complexValueData = new PkgDataArray(ParentFile);
                        complexValueData.Read(reader);
                        break;
                    case "!sbi":
                        complexValueData = new PkgStringData(ParentFile);
                        complexValueData.Read(reader);
                        break;
                    case "!vbi":
                        complexValueData = new PkgByteData(ParentFile);
                        complexValueData.Read(reader);
                        break;
                    default:
                        throw new Exception("Chunk type not supported! " + chunkType);
                }

                reader.Seek((int)pos, SeekOrigin.Begin);
            }
        }
        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(valueOffsetType);
        }
        public void WriteComplexValue(PkgBinaryWriter writer)
        {
            if (HasComplexValueData)
            {
                complexValueData.Write(writer);
            }
        }
        internal override void UpdateOffsets()
        {
            if (valueOffsetType.Type == 128)
            {
                valueOffsetType.Offset = PkgValue._offset;
            }

            if (HasComplexValueData)
            {
                complexValueData.UpdateOffsets();
            }
        }

        public override void FromJson(JsonTextReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    ValueData = (string)reader.Value;
                    break;
                case JsonToken.StartObject:
                    valueOffsetType.Type = 128;
                    complexValueData = new PkgObject(ParentFile);
                    complexValueData.FromJson(reader);
                    break;
                case JsonToken.StartArray:
                    valueOffsetType.Type = 128;
                    complexValueData = new PkgArray(ParentFile);
                    complexValueData.FromJson(reader);
                    break;
                default:
                    new Exception("Unexpected token type! " + reader.TokenType);
                    break;
            }
        }
        public override void ToJson(JsonTextWriter writer)
        {
            if (valueOffsetType.Type == 128)
            {
                complexValueData.ToJson(writer);
            }
            else
            {
                writer.WriteValue(ValueData);
            }
        }
    }
}
