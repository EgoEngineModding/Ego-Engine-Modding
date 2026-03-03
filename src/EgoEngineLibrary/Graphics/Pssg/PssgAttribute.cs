using System.Globalization;
using System.Text;
using System.Xml.Linq;
using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.Helper;

namespace EgoEngineLibrary.Graphics.Pssg
{
    public class PssgAttribute
    {
        private int size;
        private object data;

        public string Name => SchemaAttribute.Name;
        public int Size => size;
        public object Value
        {
            get { return data; }
            set { data = value; }
        }

        public string DisplayValue
        {
            get
            {
                return this.ToString();
            }
            set
            {
                this.Value = this.FromString(value);
            }
        }
        public override string ToString()
        {
            if (Value is byte[] bval)
            {
                return HexHelper.ByteArrayToHexViaLookup32(bval);
            }
            else
            {
                return this.Value.ToString() ?? string.Empty;
            }
        }
        public object FromString(string value)
        {
            Type valueType = this.SchemaAttribute.DataType;
            if (valueType == typeof(string))
            {
                return value;
            }
            else if (valueType == typeof(UInt16))
            {
                return Convert.ToUInt16(value);
            }
            else if (valueType == typeof(UInt32))
            {
                return Convert.ToUInt32(value);
            }
            else if (valueType == typeof(Int16))
            {
                return Convert.ToInt16(value);
            }
            else if (valueType == typeof(Int32))
            {
                return Convert.ToInt32(value);
            }
            else if (valueType == typeof(Single))
            {
                return Convert.ToSingle(value, CultureInfo.InvariantCulture);
            }
            else if (valueType == typeof(bool))
            {
                return Convert.ToBoolean(value);
            }
            else if (valueType == typeof(byte[]))
            {
                return HexHelper.HexToByteUsingByteManipulation(value);
            }
            else // Null, or Unsupported Type
            {
                return value;
            }
        }

        private PssgFile file;
        public PssgElement ParentElement
        {
            get;
            set;
        }
        public PssgSchemaAttribute SchemaAttribute
        {
            get;
            private set;
        }

        public PssgAttribute(PssgSchemaAttribute schemaAttribute, object data, PssgFile file, PssgElement parentElement)
        {
            this.SchemaAttribute = schemaAttribute;
            this.data = data;
            this.file = file;
            this.ParentElement = parentElement;
        }
        public PssgAttribute(PssgBinaryReader reader, PssgFile file, PssgElement element)
        {
            this.file = file;
            this.ParentElement = element;

            int id = reader.ReadInt32();
            this.SchemaAttribute = reader.GetAttributeById(id);
            this.size = reader.ReadInt32();
            this.data = reader.ReadAttributeValue(this.SchemaAttribute.DataType, size);
            this.SchemaAttribute = PssgSchema.AddAttribute(this.ParentElement.Name, this.Name, this.Value.GetType());
        }
        public PssgAttribute(XAttribute xAttr, PssgFile file, PssgElement element)
        {
            this.file = file;
            this.ParentElement = element;

            string attrName = xAttr.Name.LocalName.StartsWith("___") ? xAttr.Name.LocalName.Substring(3) : xAttr.Name.LocalName;
            this.SchemaAttribute = PssgSchema.AddAttribute(this.ParentElement.Name, attrName);
            this.data = this.FromString(xAttr.Value);
            PssgSchema.SetAttributeDataTypeIfNull(this.SchemaAttribute, this.Value.GetType());
        }
        public PssgAttribute(PssgAttribute attrToCopy)
        {
            this.file = attrToCopy.file;
            this.ParentElement = attrToCopy.ParentElement;

            this.SchemaAttribute = attrToCopy.SchemaAttribute;
            this.size = attrToCopy.size;
            this.data = attrToCopy.data;
        }

        public T GetValue<T>()
            where T : notnull
        {
            return (T)data;
        }

        public void Write(PssgBinaryWriter writer)
        {
            writer.Write(writer.GetAttributeId(SchemaAttribute));
            writer.Write(this.size);
            writer.WriteObject(this.data);
        }

        internal void UpdateSize()
        {
            if (data is string)
            {
                size = 4 + Encoding.UTF8.GetBytes((string)data).Length;
            }
            else if (data is UInt16)
            {
                size = 2;
            }
            else if (data is UInt32)
            {
                size = 4;
            }
            else if (data is Int16)
            {
                size = 2;
            }
            else if (data is Int32)
            {
                size = 4;
            }
            else if (data is Single)
            {
                size = 4;
            }
            else if (data is bool)
            {
                size = EndianBitConverter.Big.GetBytes((bool)data).Length;
            }
            else
            {
                size = ((byte[])data).Length;
            }
        }
    }
}
