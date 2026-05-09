using System.Diagnostics;
using System.Xml.Linq;
using EgoEngineLibrary.Helper;

namespace EgoEngineLibrary.Graphics.Pssg
{
    [DebuggerDisplay("{Name}")]
    public class PssgElement
    {
        // id, size, and attributeSize are only used during Reading/Writing
        private int size;
        private int attributeSize;

        public string Name => SchemaElement.Name;

        public PssgAttributeCollection Attributes { get; }
        public PssgElementCollection ChildElements { get; }

        public byte[] Value
        {
            get;
            set
            {
                if (SchemaElement.DataType is PssgElementType.None && value.Length > 0)
                {
                    throw new InvalidOperationException("Element cannot have any data.");
                }
                
                field = value;
            }
        }

        public string DisplayValue
        {
            get
            {
                return ValueToString();
            }
            set
            {
                this.Value = ValueFromString(value);
            }
        }

        public bool IsDataElement
        {
            get
            {
                return SchemaElement.DataType switch
                {
                    PssgElementType.Unknown => ChildElements.Count == 0 && Value.Length > 0,
                    PssgElementType.None => false,
                    _ => true,
                };
            }
        }

        public PssgFile File
        {
            get;
            private set;
        }

        public PssgElement? ParentElement
        {
            get;
            private set;
        }

        public PssgSchemaElement SchemaElement { get; }

        public static PssgElement ReadBinary(PssgBinaryReader reader, PssgFile file, PssgElement? parent)
        {
            int id = reader.ReadInt32();
            PssgSchemaElement schemaElement = reader.GetElementById(id);
            var element = schemaElement.Create(file, parent);
            element.size = reader.ReadInt32();
            long end = reader.BaseStream.Position + element.size;

            element.attributeSize = reader.ReadInt32();
            long attributeEnd = reader.BaseStream.Position + element.attributeSize;
            if (attributeEnd > reader.BaseStream.Length || end > reader.BaseStream.Length)
            {
                throw new Exception("This file is improperly saved and not supported by this version of the PSSG editor." + Environment.NewLine + Environment.NewLine +
                            "Get an older version of the program if you wish to take out its contents, but, put it back together using this program and a non-modded version of the pssg file.");
            }
            // Each attr is at least 8 bytes (id + size), so take a conservative guess
            while (reader.BaseStream.Position < attributeEnd)
            {
                PssgAttribute attr = new(reader, element);
                element.Attributes.Add(attr);
            }

            bool isDataElement = schemaElement.DataType is not PssgElementType.None and not PssgElementType.Unknown;
            if (!isDataElement)
            {
                switch (element.Name)
                {
                    case "DATABLOCKBUFFERED":
                    case "NeAnimPacketData_B1":
                    case "NeAnimPacketData_B4":
                    case "RENDERINTERFACEBOUNDBUFFERED":
                        isDataElement = true;
                        break;
                }
            }

            if (!isDataElement && reader.UseDataElementCheck)
            {
                long currentPos = reader.BaseStream.Position;
                // Check if it has children
                while (reader.BaseStream.Position < end)
                {
                    int tempID = reader.ReadInt32();
                    if (tempID < 0 || tempID > reader.ElementTable.Count) 
                    {
                        isDataElement = true;
                        break;
                    }
                    else
                    {
                        int tempSize = reader.ReadInt32();
                        if ((reader.BaseStream.Position + tempSize > end) || (tempSize == 0 && tempID == 0) || tempSize < 0)
                        {
                            isDataElement = true;
                            break;
                        }
                        else if (reader.BaseStream.Position + tempSize == end)
                        {
                            break;
                        }
                        else
                        {
                            reader.BaseStream.Position += tempSize;
                        }
                    }
                }
                reader.BaseStream.Position = currentPos;
            }

            if (isDataElement)
            {
                element.Value = reader.ReadElementValue((int)(end - reader.BaseStream.Position));
            }
            else
            {
                element.Value = Array.Empty<byte>();
                // Each element at least 12 bytes (id + size + arg size)
                element.ChildElements.EnsureCapacity((int)(end - reader.BaseStream.Position) / 12);
                while (reader.BaseStream.Position < end)
                {
                    element.ChildElements.Add(ReadBinary(reader, file, element));
                }
            }

            return element;
        }

        public static PssgElement ReadXml(XElement elem, PssgFile file, PssgElement? parent)
        {
            PssgSchemaElement schemaElement = PssgSchema.AddElement(elem.Name.LocalName);
            var element = schemaElement.Create(file, parent);

            PssgAttribute attr;
            foreach (XAttribute xAttr in elem.Attributes())
            {
                attr = new PssgAttribute(xAttr, element);
                element.Attributes.Add(attr);
            }

            if (elem.FirstNode != null && elem.FirstNode is XText)
            {
                element.DisplayValue = elem.Value;
            }
            else
            {
                element.Value = [];
                element.ChildElements.EnsureCapacity(elem.Elements().Count());
                foreach (XElement child in elem.Elements())
                {
                    element.ChildElements.Add(ReadXml(child, file, element));
                }
            }
            
            return element;
        }

        public static PssgElement Create(string name, PssgFile file, PssgElement? parent)
        {
            return PssgSchema.AddElement(name).Create(file, parent);
        }

        internal PssgElement(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        {
            File = file;
            ParentElement = parent;
            SchemaElement = schemaElement;
            Attributes = [];
            Value = [];
            ChildElements = [];
        }

        private PssgElementType GetValueType()
        {
            PssgSchemaElement schema = this.SchemaElement;
            if (!string.IsNullOrEmpty(schema.LinkAttributeName))
            {
                PssgElement element;
                string linkAttrName;
                if (schema.LinkAttributeName[0] == '^')
                {
                    element = this.ParentElement ??
                              throw new InvalidOperationException(
                                  "Element without parent cannot reference a parent's attribute.");
                    linkAttrName = schema.LinkAttributeName[1..];
                }
                else
                {
                    element = this;
                    linkAttrName = schema.LinkAttributeName;
                }

                var attr = element.Attributes.Get(linkAttrName);
                if (attr?.Value is string format &&
                    Enum.TryParsePssgElementComplexType(format, out var simpleType))
                {
                    return simpleType.ToSimpleType();
                }
            }

            return schema.DataType;
        }

        public void Write(PssgBinaryWriter writer)
        {
            writer.Write(writer.GetElementId(SchemaElement));
            writer.Write(size);
            writer.Write(attributeSize);
            foreach (PssgAttribute attr in Attributes)
            {
                attr.Write(writer);
            }

            if (this.IsDataElement)
            {
                writer.Write(Value);
            }
            else
            {
                foreach (PssgElement child in ChildElements)
                {
                    child.Write(writer);
                }
            }
        }
        public void WriteXml(XElement parent)
        {
            XElement pNode = new(Name);
            foreach (PssgAttribute attr in Attributes)
            {
                string attrName = attr.Name.StartsWith("2") ? "___" + attr.Name : attr.Name;
                pNode.Add(new XAttribute(attrName, attr.DisplayValue));
            }

            if (this.IsDataElement)
            {
                pNode.Add(new XText(DisplayValue));
            }
            else
            {
                foreach (PssgElement child in ChildElements)
                {
                    child.WriteXml(pNode);
                }
            }

            parent.Add(pNode);
        }
        internal void UpdateSize()
        {
            attributeSize = 0;
            foreach (PssgAttribute attr in Attributes)
            {
                attr.UpdateSize();
                attributeSize += 8 + attr.Size;
            }

            size = 4 + attributeSize;
            if (this.IsDataElement)
            {
                size += Value.Length;
            }
            else
            {
                foreach (PssgElement child in ChildElements)
                {
                    child.UpdateSize();
                    size += 8 + child.size;
                }
            }
        }

        public PssgElement AppendChild(string elementName)
        {
            return this.AppendChild(Create(elementName, File, this));
        }
        public PssgElement AppendChild(PssgElement childElement)
        {
            if (this.IsDataElement)
            {
                throw new InvalidOperationException("Cannot append a child element to a data element.");
            }

            childElement.File = this.File;
            childElement.ParentElement = this;
            this.ChildElements.Add(childElement);

            return childElement;
        }
        public PssgElement? SetChild(PssgElement childElement, PssgElement newChildElement)
        {
            newChildElement.File = this.File;
            newChildElement.ParentElement = this;
            PssgElement? element = this.ChildElements.Set(childElement, newChildElement);
            return element;
        }
        public void RemoveChild(PssgElement childElement)
        {
            if (this.ChildElements.Remove(childElement))
                childElement.ParentElement = null;
            else
                throw new InvalidOperationException("Failed to remove child element.");
        }

        public void RemoveChildElements(IEnumerable<PssgElement>? childElements = null)
        {
            childElements ??= ChildElements;
            while (true)
            {
                var child = childElements.FirstOrDefault();
                if (child is null)
                    break;

                RemoveChild(child);
            }
        }

        /// <summary>
        /// Add an attribute if it doesn't exist, or get the existing one and set its value.
        /// </summary>
        public PssgAttribute AddAttribute<T>(string attributeName, T value)
            where T : notnull
        {
            PssgAttribute? attribute = Attributes.Get(attributeName);
            var attributeSchema =
                attribute?.SchemaAttribute ?? PssgSchema.AddAttribute(this.SchemaElement, attributeName);
            object val = attributeSchema.DataType.CastFrom(value);
            
            if (attribute is null)
            {
                attribute = new PssgAttribute(attributeSchema, this);
                Attributes.Add(attribute);
            }
            
            attribute.Value = val;
            return attribute;
        }

        public T GetAttributeValue<T>(string attributeName)
            where T : notnull
        {
            var attribute = Attributes.Get(attributeName);
            var attributeSchema = attribute?.SchemaAttribute ?? PssgSchema.AddAttribute(SchemaElement, attributeName);
            object attributeValue = attribute?.Value ?? attributeSchema.DataType.GetDefaultValue();
            return attributeSchema.DataType.CastTo<T>(attributeValue);
        }

        public void RemoveAttribute(string attributeName)
        {
            var attr = Attributes.Get(attributeName);
            if (attr is not null)
                this.Attributes.Remove(attr);
        }

        /// <summary>
        /// Gets this element, and its descendants as a flat sequence.
        /// </summary>
        public IEnumerable<T> Elements<T>()
            where T : PssgElement
        {
            if (this is T)
            {
                yield return (T)this;
            }

            foreach (var c in ChildElements)
            {
                var cc = c.Elements<T>();

                foreach (var ccc in cc) yield return ccc;
            }
        }

        private string ValueToString()
        {
            return GetValueType() switch
            {
                PssgElementType.None => string.Empty,
                PssgElementType.Float => Value.ToPssgFloatString(SchemaElement.ElementsPerRow),
                PssgElementType.UInt => Value.ToPssgUIntString(SchemaElement.ElementsPerRow),
                PssgElementType.Short => Value.ToPssgShortString(SchemaElement.ElementsPerRow),
                PssgElementType.UShort => Value.ToPssgUShortString(SchemaElement.ElementsPerRow),
                PssgElementType.Int => Value.ToPssgIntString(SchemaElement.ElementsPerRow),
                PssgElementType.Half => Value.ToPssgHalfString(SchemaElement.ElementsPerRow),
                _ => HexHelper.ByteArrayToHexViaLookup32(Value, SchemaElement.ElementsPerRow),
            };
        }

        private byte[] ValueFromString(string value)
        {
            return GetValueType() switch
            {
                PssgElementType.None => string.IsNullOrWhiteSpace(value)
                    ? []
                    : throw new InvalidOperationException("Element cannot have any data."),
                PssgElementType.Float => value.ToPssgFloatByteArray(),
                PssgElementType.UInt => value.ToPssgUIntByteArray(),
                PssgElementType.Short => value.ToPssgShortByteArray(),
                PssgElementType.UShort => value.ToPssgUShortByteArray(),
                PssgElementType.Int => value.ToPssgIntByteArray(),
                PssgElementType.Half => value.ToPssgHalfByteArray(),
                _ => HexHelper.HexLineToByteUsingByteManipulation(value),
            };
        }

        public PssgElement DeepClone()
        {
            PssgElement elementToCopy = this;
            PssgElement copy = elementToCopy.SchemaElement.Create(elementToCopy.File, elementToCopy.ParentElement);
            copy.size = elementToCopy.size;
            copy.attributeSize = elementToCopy.attributeSize;
            foreach (PssgAttribute attrToCopy in elementToCopy.Attributes)
            {
                PssgAttribute attr = new(attrToCopy, copy);
                copy.Attributes.Add(attr);
            }

            if (elementToCopy.IsDataElement)
            {
                copy.Value = elementToCopy.Value;
            }
            else
            {
                copy.ChildElements.EnsureCapacity(elementToCopy.ChildElements.Count);
                foreach (PssgElement childElementToCopy in elementToCopy.ChildElements)
                {
                    PssgElement element = childElementToCopy.DeepClone();
                    element.ParentElement = copy;
                    copy.ChildElements.Add(element);
                }
            }

            return copy;
        }
    }
}
