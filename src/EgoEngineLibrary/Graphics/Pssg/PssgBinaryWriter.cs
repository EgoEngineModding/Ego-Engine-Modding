using System.Numerics;
using System.Text;
using EgoEngineLibrary.Collections;
using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.IO;

namespace EgoEngineLibrary.Graphics.Pssg
{
    public class PssgBinaryWriter : EndianBinaryWriter
    {
        public OrderedSet<PssgSchemaElement> ElementTable { get; set; }
        public OrderedSet<PssgSchemaAttribute> AttributeTable { get; set; }
        
        public PssgBinaryWriter(EndianBitConverter bitConvertor, Stream stream, bool leaveOpen)
            : base(bitConvertor, stream, PssgStringHelper.Encoding, leaveOpen)
        {
            ElementTable = [];
            AttributeTable = [];
        }

        public int GetElementId(PssgSchemaElement element)
        {
            int index = ElementTable.IndexOf(element);
            if (index == -1)
            {
                throw new ArgumentException(@"Element not found", nameof(element));
            }

            return index + 1;
        }

        public int GetAttributeId(PssgSchemaAttribute attribute)
        {
            int index = AttributeTable.IndexOf(attribute);
            if (index == -1)
            {
                throw new ArgumentException(@"Attribute not found", nameof(attribute));
            }

            return index + 1;
        }

        public void WritePSSGString(string str)
        {
            byte[] bytes = Encoding.GetBytes(str);
            this.Write(bytes.Length);
            this.Write(bytes);
        }

        public void WriteAttributeValue(object value)
        {
            switch (value)
            {
                case int i:
                    Write(i);
                    break;
                case string s:
                    WritePSSGString(s);
                    break;
                case float f:
                    Write(f);
                    break;
                case Vector2 v2:
                    Write(v2.X);
                    Write(v2.Y);
                    break;
                case Vector3 v3:
                    Write(v3.X);
                    Write(v3.Y);
                    Write(v3.Z);
                    break;
                case Vector4 v4:
                    Write(v4.X);
                    Write(v4.Y);
                    Write(v4.Z);
                    Write(v4.W);
                    break;
                case byte[] b:
                    Write(b);
                    break;
                default:
                    throw new InvalidDataException($"Attribute type '{value.GetType().Name}' is not valid.");
            }
        }
    }
}
