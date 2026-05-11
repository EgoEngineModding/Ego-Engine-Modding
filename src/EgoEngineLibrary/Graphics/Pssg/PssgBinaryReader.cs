using System.Numerics;
using System.Text;
using EgoEngineLibrary.Collections;
using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.IO;

namespace EgoEngineLibrary.Graphics.Pssg
{
    public class PssgBinaryReader : EndianBinaryReader
    {
        public OrderedSet<PssgSchemaElement> ElementTable { get; set; }
        public OrderedSet<PssgSchemaAttribute> AttributeTable { get; set; }
        internal bool UseDataElementCheck { get; set; }
        
        public PssgBinaryReader(EndianBitConverter bitConvertor, Stream stream, bool leaveOpen)
            : base(bitConvertor, stream, PssgStringHelper.Encoding, leaveOpen)
        {
            ElementTable = [];
            AttributeTable = [];
        }

        public PssgSchemaElement GetElementById(int id)
        {
            return this.ElementTable[id - 1];
        }

        public PssgSchemaAttribute GetAttributeById(int id)
        {
            return this.AttributeTable[id - 1];
        }

        public string ReadPSSGString()
        {
            int length = this.ReadInt32();
            return Encoding.GetString(this.ReadBytes(length));
        }
        public string ReadPSSGString(int length)
        {
            return Encoding.GetString(this.ReadBytes(length));
        }

        public object ReadAttributeValue(PssgAttributeType valueType, int size)
        {
            return valueType switch
            {
                PssgAttributeType.Int => ReadInt32(),
                PssgAttributeType.String => ReadPSSGString(),
                PssgAttributeType.Float => ReadSingle(),
                PssgAttributeType.Float2 => new Vector2(ReadSingle(), ReadSingle()),
                PssgAttributeType.Float3 => new Vector3(ReadSingle(), ReadSingle(), ReadSingle()),
                PssgAttributeType.Float4 => new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle()),
                PssgAttributeType.Unknown => ReadBytes(size),
                _ => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null)
            };
        }
        public byte[] ReadElementValue(int size)
        {
            var ret = ReadBytes(size);
            if (ret.Length != size)
                throw new EndOfStreamException($"End of stream reached with {size - ret.Length} byte(s) left to read.");
            return ret;
        }
    }
}
