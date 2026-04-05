using System.Diagnostics;
using System.Numerics;
using System.Xml.Linq;

namespace EgoEngineLibrary.Graphics.Pssg
{
    [DebuggerDisplay("{Name}")]
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
                return Value.ToPssgString(SchemaAttribute.DataType);
            }
            set
            {
                Value = value.ToPssgValue(SchemaAttribute.DataType);
            }
        }

        public PssgElement ParentElement { get; }
        public PssgSchemaAttribute SchemaAttribute { get; }

        public PssgAttribute(PssgSchemaAttribute schemaAttribute, PssgElement parentElement)
        {
            SchemaAttribute = schemaAttribute;
            Value = schemaAttribute.DataType.GetDefaultValue();
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
            Value = xAttr.Value.ToPssgValue(SchemaAttribute.DataType);
        }
        public PssgAttribute(PssgAttribute attrToCopy, PssgElement parent)
        {
            this.ParentElement = parent;

            SchemaAttribute = attrToCopy.SchemaAttribute;
            Size = attrToCopy.Size;
            Value = attrToCopy.Value;
        }

        public void Write(PssgBinaryWriter writer)
        {
            writer.Write(writer.GetAttributeId(SchemaAttribute));
            writer.Write(this.Size);
            writer.WriteAttributeValue(Value);
        }

        internal void UpdateSize()
        {
            PssgAttributeType dataType = this.SchemaAttribute.DataType;
            Size = dataType switch
            {
                PssgAttributeType.Int => 4,
                PssgAttributeType.String => 4 + PssgStringHelper.Encoding.GetByteCount((string)Value),
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
