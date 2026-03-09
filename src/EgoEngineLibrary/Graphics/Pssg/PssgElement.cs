using System.Xml.Linq;
using EgoEngineLibrary.Helper;

namespace EgoEngineLibrary.Graphics.Pssg
{
    public class PssgElement
    {
        // id, size, and attributeSize are only used during Reading/Writing
        private int size;
        private int attributeSize;

        public string Name => SchemaElement.Name;

        public PssgAttributeCollection Attributes { get; }
        public PssgElementCollection ChildElements { get; }
        public byte[] Value { get; set; }

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
        public bool IsDataElement
        {
            get
            {
                if (this.ChildElements.Count == 0)
                {
                    return Value.Length > 0;
                }

                return false;
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

            bool isDataElement = false;
            switch (element.Name)
            {
                case "BOUNDINGBOX":
                case "DATA":
                case "DATABLOCKDATA":
                case "DATABLOCKBUFFERED":
                case "INDEXSOURCEDATA":
                case "INVERSEBINDMATRIX":
                case "MODIFIERNETWORKINSTANCEUNIQUEMODIFIERINPUT":
                case "NeAnimPacketData_B1":
                case "NeAnimPacketData_B4":
                case "RENDERINTERFACEBOUNDBUFFERED":
                case "SHADERINPUT":
                case "TEXTUREIMAGEBLOCKDATA":
                case "TRANSFORM":
                    isDataElement = true;
                    break;
            }
            if (!isDataElement && reader.UseDataElementCheck)
            {
                long currentPos = reader.BaseStream.Position;
                // Check if it has children
                while (reader.BaseStream.Position < end)
                {
                    int tempID = reader.ReadInt32();
                    if (tempID < 0)//tempID > file.nodeInfo.Length || 
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
        public PssgElement(XElement elem, PssgFile file, PssgElement? element)
        {
            this.File = file;
            this.ParentElement = element;
            this.SchemaElement = PssgSchema.AddElement(elem.Name.LocalName);

            this.Attributes = new PssgAttributeCollection();
            PssgAttribute attr;
            foreach (XAttribute xAttr in elem.Attributes())
            {
                attr = new PssgAttribute(xAttr, this);
                this.Attributes.Add(attr);
            }

            if (elem.FirstNode != null && elem.FirstNode is XText)
            {
                this.Value = this.FromString(elem.Value);
                this.ChildElements = new PssgElementCollection();
            }
            else
            {
                this.Value = Array.Empty<byte>();
                this.ChildElements = new PssgElementCollection(elem.Elements().Count());
                foreach (XElement child in elem.Elements())
                {
                    this.ChildElements.Add(new PssgElement(child, file, this));
                }
            }
        }
        public PssgElement(PssgElement elementToCopy)
        {
            this.File = elementToCopy.File;
            this.ParentElement = elementToCopy.ParentElement;

            this.SchemaElement = elementToCopy.SchemaElement;
            this.size = elementToCopy.size;
            this.attributeSize = elementToCopy.attributeSize;
            this.Attributes = new PssgAttributeCollection();
            PssgAttribute attr;
            foreach (PssgAttribute attrToCopy in elementToCopy.Attributes)
            {
                attr = new PssgAttribute(attrToCopy, this);
                this.Attributes.Add(attr);
            }


            if (elementToCopy.IsDataElement)
            {
                this.Value = elementToCopy.Value;
                this.ChildElements = new PssgElementCollection();
            }
            else
            {
                this.Value = Array.Empty<byte>();
                // Each element at least 12 bytes (id + size + arg size)
                this.ChildElements = new PssgElementCollection(elementToCopy.ChildElements.Count);
                foreach (PssgElement childElementToCopy in elementToCopy.ChildElements)
                {
                    PssgElement element = new PssgElement(childElementToCopy);
                    element.ParentElement = this;
                    this.ChildElements.Add(element);
                }
            }
        }
        public PssgElement(string name, PssgFile file, PssgElement? parent)
            : this(PssgSchema.AddElement(name), file, parent)
        {
        }
        internal PssgElement(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        {
            this.File = file;
            this.ParentElement = parent;
            this.SchemaElement = schemaElement;
            this.Attributes = new PssgAttributeCollection();
            this.Value = Array.Empty<byte>();
            this.ChildElements = new PssgElementCollection();
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

                if (element.HasAttribute(linkAttrName))
                {
                    PssgAttribute attr = element.Attributes[linkAttrName];
                    if (attr.Value is string format &&
                        Enum.TryParsePssgElementComplexType(format, out var simpleType))
                    {
                        return simpleType.ToSimpleType();
                    }
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
                writer.WriteObject(Value);
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
                pNode.Add(new XAttribute(attrName, attr.ToString()));
            }

            if (this.IsDataElement)
            {
                pNode.Add(new XText(this.ToString())); //EndianBitConverter.ToString(data)
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
            return this.AppendChild(new PssgElement(elementName, this.File, this));
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
        public PssgAttribute AddAttribute(string attributeName, object data)
        {
            if (this.HasAttribute(attributeName))
            {
                this.Attributes[attributeName].Value = data;
                return this.Attributes[attributeName];
            }

            PssgAttribute newAttr = new(PssgSchema.AddAttribute(this.Name, attributeName), data, this);
            this.Attributes.Add(newAttr);

            return newAttr;
        }

        public T GetAttributeValue<T>(string attributeName, T defaultValue)
            where T : notnull
        {
            var value = (Attributes.Get(attributeName)?.Value);
            return value is null ? defaultValue : (T)value;
        }

        public void RemoveAttribute(string attributeName)
        {
            if (this.HasAttribute(attributeName))
                this.Attributes.Remove(this.Attributes[attributeName]);
        }

        /// <summary>
        /// Gets this element, and its hierarchy as a flat sequence.
        /// </summary>
        public IEnumerable<PssgElement> GetElements()
        {
            yield return this;

            foreach (var c in ChildElements)
            {
                var cc = c.GetElements();

                foreach (var ccc in cc) yield return ccc;
            }
        }

        public IEnumerable<PssgElement> FindElements(string elementName)
        {
            return GetElements().FindElements(elementName);
        }
        public IEnumerable<PssgElement> FindElements(string elementName, string attributeName)
        {
            return GetElements().FindElements(elementName, attributeName);
        }
        public IEnumerable<PssgElement> FindElements<T>(string elementName, string attributeName, T attributeValue)
            where T : notnull
        {
            return GetElements().FindElements(elementName, attributeName, attributeValue);
        }

        public PssgElement this[int index]
        {
            get
            {
                return this.ChildElements[index];
            }
        }

        /// <summary>
        /// Determines whether the current element has an attribute with the specified name.
        /// </summary>
        /// <param name="attributeName">The name of the attribute to find.</param>
        public bool HasAttribute(string attributeName)
        {
            return this.Attributes.Contains(attributeName);
        }
        public bool HasAttributes
        {
            get
            {
                return this.Attributes.Count > 0;
            }
        }

        public override string ToString()
        {
            return GetValueType() switch
            {
                PssgElementType.Float => Value.ToPssgFloatString(SchemaElement.ElementsPerRow),
                PssgElementType.UInt => Value.ToPssgUIntString(SchemaElement.ElementsPerRow),
                PssgElementType.Short => Value.ToPssgShortString(SchemaElement.ElementsPerRow),
                PssgElementType.UShort => Value.ToPssgUShortString(SchemaElement.ElementsPerRow),
                PssgElementType.Int => Value.ToPssgIntString(SchemaElement.ElementsPerRow),
                PssgElementType.Half => Value.ToPssgHalfString(SchemaElement.ElementsPerRow),
                _ => HexHelper.ByteArrayToHexViaLookup32(Value, SchemaElement.ElementsPerRow),
            };
        }

        public byte[] FromString(string value)
        {
            return GetValueType() switch
            {
                PssgElementType.Float => value.ToPssgFloatByteArray(),
                PssgElementType.UInt => value.ToPssgUIntByteArray(),
                PssgElementType.Short => value.ToPssgShortByteArray(),
                PssgElementType.UShort => value.ToPssgUShortByteArray(),
                PssgElementType.Int => value.ToPssgIntByteArray(),
                PssgElementType.Half => value.ToPssgHalfByteArray(),
                _ => HexHelper.HexLineToByteUsingByteManipulation(value),
            };
        }
    }
}
