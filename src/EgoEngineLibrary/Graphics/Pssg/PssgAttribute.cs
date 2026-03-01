using EgoEngineLibrary.Helper;

using System;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

using EgoEngineLibrary.Conversion;

namespace EgoEngineLibrary.Graphics
{
    public class PssgAttribute
    {
        private int size;
        private object data;

        public string Name => AttributeInfo.Name;
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
            Type valueType = this.AttributeInfo.DataType;
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
        public PssgNode ParentNode
        {
            get;
            set;
        }
        public PssgSchema.Attribute AttributeInfo
        {
            get;
            private set;
        }

        public PssgAttribute(PssgSchema.Attribute attributeInfo, object data, PssgFile file, PssgNode ParentNode)
        {
            this.AttributeInfo = attributeInfo;
            this.data = data;
            this.file = file;
            this.ParentNode = ParentNode;
        }
        public PssgAttribute(PssgBinaryReader reader, PssgFile file, PssgNode node)
        {
            this.file = file;
            this.ParentNode = node;

            int id = reader.ReadInt32();
            this.AttributeInfo = PssgSchema.GetAttribute(id);
            this.size = reader.ReadInt32();
            this.data = reader.ReadAttributeValue(this.AttributeInfo.DataType, size);
            this.AttributeInfo = PssgSchema.AddAttribute(this.ParentNode.Name, this.Name, this.Value.GetType());
        }
        public PssgAttribute(XAttribute xAttr, PssgFile file, PssgNode node)
        {
            this.file = file;
            this.ParentNode = node;

            //this.id = PssgSchema.GetAttributeId(ParentNode.Name, xAttr.Name.LocalName);
            string attrName = xAttr.Name.LocalName.StartsWith("___") ? xAttr.Name.LocalName.Substring(3) : xAttr.Name.LocalName;
            this.AttributeInfo = PssgSchema.AddAttribute(this.ParentNode.Name, attrName);// PssgSchema.GetAttribute(this.ParentNode.Name, xAttr.Name.LocalName);
            this.data = this.FromString(xAttr.Value);
            PssgSchema.SetAttributeDataTypeIfNull(this.AttributeInfo, this.Value.GetType());
        }
        public PssgAttribute(PssgAttribute attrToCopy)
        {
            this.file = attrToCopy.file;
            this.ParentNode = attrToCopy.ParentNode;

            this.AttributeInfo = attrToCopy.AttributeInfo;
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
            writer.Write(this.AttributeInfo.Id);
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
