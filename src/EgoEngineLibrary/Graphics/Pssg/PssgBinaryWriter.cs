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
            : base(bitConvertor, stream, leaveOpen)
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
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            this.Write(bytes.Length);
            this.Write(bytes);
        }

        public void WriteObject(object value)
        {
            if (value is string)
            {
                this.WritePSSGString((string)value);
            }
            else if (value is UInt16)
            {
                this.Write((UInt16)value);
            }
            else if (value is UInt32)
            {
                this.Write((UInt32)value);
            }
            else if (value is Int16)
            {
                this.Write((Int16)value);
            }
            else if (value is Int32)
            {
                this.Write((Int32)value);
            }
            else if (value is Single)
            {
                this.Write((Single)value);
            }
            else if (value is bool)
            {
                this.Write((bool)value);
            }
            else if (value is byte[])
            {
                this.Write((byte[])value);
            }
            else if (value is Single[])
            {
                for (int i = 0; i < ((Single[])value).Length; i++)
                {
                    this.Write(((Single[])value)[i]);
                }
            }
            else if (value is UInt16[])
            {
                for (int i = 0; i < ((UInt16[])value).Length; i++)
                {
                    this.Write(((UInt16[])value)[i]);
                }
            }
            else
            {
                this.Write((byte[])value);
            }
        }
    }
}
