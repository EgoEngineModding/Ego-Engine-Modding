using System.Globalization;
using System.Numerics;
using System.Text;
using System.Xml.Linq;
using EgoEngineLibrary.Helper;

namespace EgoEngineLibrary.Graphics.Pssg
{
    public class PssgAttribute
    {
        public string Name => SchemaAttribute.Name;
        internal int Size { get; private set; }

        public object Value
        {
            get;
            set
            {
                VerifyType(value);
                field = value;
            }
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

        public PssgElement ParentElement { get; }
        public PssgSchemaAttribute SchemaAttribute { get; }

        public PssgAttribute(PssgSchemaAttribute schemaAttribute, object data, PssgElement parentElement)
        {
            SchemaAttribute = schemaAttribute;
            Value = data;
            ParentElement = parentElement;
        }
        public PssgAttribute(PssgBinaryReader reader, PssgElement element)
        {
            this.ParentElement = element;

            int id = reader.ReadInt32();
            SchemaAttribute = reader.GetAttributeById(id);
            Size = reader.ReadInt32();
            Value = reader.ReadAttributeValue(this.SchemaAttribute.DataType, Size);
        }
        public PssgAttribute(XAttribute xAttr, PssgElement element)
        {
            ParentElement = element;

            string attrName = xAttr.Name.LocalName.StartsWith("___") ? xAttr.Name.LocalName.Substring(3) : xAttr.Name.LocalName;
            SchemaAttribute = PssgSchema.AddAttribute(this.ParentElement.Name, attrName);
            Value = this.FromString(xAttr.Value);
        }
        public PssgAttribute(PssgAttribute attrToCopy, PssgElement parent)
        {
            this.ParentElement = parent;

            SchemaAttribute = attrToCopy.SchemaAttribute;
            Size = attrToCopy.Size;
            Value = attrToCopy.Value;
        }

        public T GetValue<T>()
            where T : notnull
        {
            return (T)Value;
        }

        public void Write(PssgBinaryWriter writer)
        {
            writer.Write(writer.GetAttributeId(SchemaAttribute));
            writer.Write(this.Size);
            writer.WriteObject(Value);
        }
        
        public override string ToString()
        {
            PssgAttributeType dataType = this.SchemaAttribute.DataType;
            return dataType switch
            {
                PssgAttributeType.Int => Value.ToString() ?? string.Empty,
                PssgAttributeType.String => (string)Value,
                PssgAttributeType.Float => ((float)Value).ToPssgString(),
                PssgAttributeType.Float2 => ((Vector2)Value).ToPssgString(),
                PssgAttributeType.Float3 => ((Vector3)Value).ToPssgString(),
                PssgAttributeType.Float4 => ((Vector4)Value).ToPssgString(),
                PssgAttributeType.Unknown => HexHelper.ByteArrayToHexViaLookup32((byte[])Value),
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
            };
        }

        public object FromString(string value)
        {
            PssgAttributeType dataType = this.SchemaAttribute.DataType;
            return dataType switch
            {
                PssgAttributeType.Int => Convert.ToInt32(value),
                PssgAttributeType.String => value,
                PssgAttributeType.Float => Convert.ToSingle(value, CultureInfo.InvariantCulture),
                PssgAttributeType.Float2 => value.ToPssgVector2(),
                PssgAttributeType.Float3 => value.ToPssgVector3(),
                PssgAttributeType.Float4 => value.ToPssgVector4(),
                PssgAttributeType.Unknown => HexHelper.HexToByteUsingByteManipulation(value),
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
            };
        }

        internal void UpdateSize()
        {
            PssgAttributeType dataType = this.SchemaAttribute.DataType;
            Size = dataType switch
            {
                PssgAttributeType.Int => 4,
                PssgAttributeType.String => 4 + Encoding.UTF8.GetByteCount((string)Value),
                PssgAttributeType.Float => 4,
                PssgAttributeType.Float2 => 8,
                PssgAttributeType.Float3 => 12,
                PssgAttributeType.Float4 => 16,
                PssgAttributeType.Unknown => ((byte[])Value).Length,
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
            };
        }

        private void VerifyType(object value)
        {
            var valueType = value.GetType();
            PssgAttributeType dataType = this.SchemaAttribute.DataType;
            bool isValid = dataType switch
            {
                PssgAttributeType.Int => valueType == typeof(int),
                PssgAttributeType.String => valueType == typeof(string),
                PssgAttributeType.Float => valueType == typeof(float),
                PssgAttributeType.Float2 => valueType == typeof(Vector2),
                PssgAttributeType.Float3 => valueType == typeof(Vector3),
                PssgAttributeType.Float4 => valueType == typeof(Vector4),
                PssgAttributeType.Unknown => valueType == typeof(byte[]),
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
            };

            if (!isValid)
            {
                throw new InvalidCastException($"Cannot cast {valueType} to {dataType}.");
            }
        }
    }
}
