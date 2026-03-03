using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using EgoEngineLibrary.Helper;

namespace EgoEngineLibrary.Graphics.Pssg
{
    public class PssgElement
    {
        // id, size, and attributeSize are only used during Reading/Writing
        private int size;
        private int attributeSize;
        private byte[] data;

        public string Name => SchemaElement.Name;

        public PssgAttributeCollection Attributes
        {
            get;
            set;
        }
        public PssgElementCollection ChildElements
        {
            get;
            set;
        }
        public byte[] Value
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
        public bool IsDataElement
        {
            get
            {
                if (this.ChildElements.Count == 0)
                {
                    return data.Length > 0;
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

        public PssgElement(PssgBinaryReader reader, PssgFile file, PssgElement? element, bool useDataElementCheck)
        {
            this.File = file;
            this.ParentElement = element;

            int id = reader.ReadInt32();
            this.SchemaElement = reader.GetElementById(id);
            this.size = reader.ReadInt32();
            long end = reader.BaseStream.Position + size;

            this.attributeSize = reader.ReadInt32();
            long attributeEnd = reader.BaseStream.Position + attributeSize;
            if (attributeEnd > reader.BaseStream.Length || end > reader.BaseStream.Length)
            {
                throw new Exception("This file is improperly saved and not supported by this version of the PSSG editor." + Environment.NewLine + Environment.NewLine +
                            "Get an older version of the program if you wish to take out its contents, but, put it back together using this program and a non-modded version of the pssg file.");
            }
            // Each attr is at least 8 bytes (id + size), so take a conservative guess
            this.Attributes = new PssgAttributeCollection();
            PssgAttribute attr;
            while (reader.BaseStream.Position < attributeEnd)
            {
                attr = new PssgAttribute(reader, file, this);
                this.Attributes.Add(attr);
            }

            bool isDataElement = false;
            switch (Name)
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
            if (isDataElement == false && useDataElementCheck == true)
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
                this.data = reader.ReadElementValue((int)(end - reader.BaseStream.Position));
                this.ChildElements = new PssgElementCollection();
            }
            else
            {
                this.data = Array.Empty<byte>();
                // Each element at least 12 bytes (id + size + arg size)
                this.ChildElements = new PssgElementCollection((int)(end - reader.BaseStream.Position) / 12);
                while (reader.BaseStream.Position < end)
                {
                    this.ChildElements.Add(new PssgElement(reader, file, this, useDataElementCheck));
                }
            }
            PssgSchema.SetElementDataTypeIfNull(this.SchemaElement, Value.GetType());
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
                attr = new PssgAttribute(xAttr, file, this);
                this.Attributes.Add(attr);
            }

            if (elem.FirstNode != null && elem.FirstNode is XText)
            {
                this.data = this.FromString(elem.Value);
                this.ChildElements = new PssgElementCollection();
            }
            else
            {
                this.data = Array.Empty<byte>();
                this.ChildElements = new PssgElementCollection(elem.Elements().Count());
                foreach (XElement child in elem.Elements())
                {
                    this.ChildElements.Add(new PssgElement(child, file, this));
                }
            }
            PssgSchema.SetElementDataTypeIfNull(this.SchemaElement, Value.GetType());
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
                attr = new PssgAttribute(attrToCopy);
                attr.ParentElement = this;
                this.Attributes.Add(attr);
            }


            if (elementToCopy.IsDataElement)
            {
                this.data = elementToCopy.data;
                this.ChildElements = new PssgElementCollection();
            }
            else
            {
                this.data = Array.Empty<byte>();
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
        public PssgElement(string name, PssgFile file, PssgElement? element)
        {
            this.File = file;
            this.ParentElement = element;
            this.SchemaElement = PssgSchema.AddElement(name);
            this.Attributes = new PssgAttributeCollection();
            this.data = Array.Empty<byte>();
            this.ChildElements = new PssgElementCollection();
        }

        private Type GetValueType()
        {
            PssgSchemaElement schema = this.SchemaElement;
            if (!string.IsNullOrEmpty(schema.LinkAttributeName))
            {
                PssgElement element;
                string linkAttrName;
                if (schema.LinkAttributeName[0] == '^')
                {
                    if (this.ParentElement == null) throw new InvalidOperationException("Element without parent cannot reference a parent's attribute.");
                    element = this.ParentElement;
                    linkAttrName = schema.LinkAttributeName.Substring(1);
                }
                else
                {
                    element = this;
                    linkAttrName = schema.LinkAttributeName;
                }

                if (element.HasAttribute(linkAttrName))
                {
                    PssgAttribute attr = element.Attributes[linkAttrName];
                    if (attr.Value is string)
                    {
                        string format = (string)attr.Value;
                        if (format.Contains("float"))
                        {
                            return typeof(Single[]);
                        }
                        else if (format.Contains("ushort"))
                        {
                            return typeof(UInt16[]);
                        }
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
            if (Attributes != null)
            {
                foreach (PssgAttribute attr in Attributes)
                {
                    attr.Write(writer);
                }
            }
            if (this.IsDataElement)
            {
                writer.WriteObject(data);
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
            XElement pNode = new XElement(Name);

            if (Attributes != null)
            {
                foreach (PssgAttribute attr in Attributes)
                {
                    string attrName = attr.Name.StartsWith("2") ? "___" + attr.Name : attr.Name;
                    pNode.Add(new XAttribute(attrName, attr.ToString()));
                }
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
            if (Attributes != null)
            {
                foreach (PssgAttribute attr in Attributes)
                {
                    attr.UpdateSize();
                    attributeSize += 8 + attr.Size;
                }
            }
            size = 4 + attributeSize;

            if (this.IsDataElement)
            {
                size += data.Length;
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
            if (this.IsDataElement == true)
            {
                throw new InvalidOperationException("Cannot append a child element to a data element.");
            }

            if (this.ChildElements == null)
            {
                this.ChildElements = new PssgElementCollection();
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

        public PssgAttribute AddAttribute(string attributeName, object data)
        {
            if (this.Attributes == null)
            {
                this.Attributes = new PssgAttributeCollection();
            }
            else if (this.HasAttribute(attributeName))
            {
                this.Attributes[attributeName].Value = data;
                return this.Attributes[attributeName];
            }

            PssgAttribute newAttr = new PssgAttribute(PssgSchema.AddAttribute(this.Name, attributeName, data.GetType()), data, this.File, this);
            this.Attributes.Add(newAttr);

            return newAttr;
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
                if (this.Attributes == null)
                {
                    return false;
                }
                else
                {
                    return this.Attributes.Count > 0;
                }
            }
        }

        public override string ToString()
        {
            var valueType = GetValueType();
            if (valueType == typeof(float[]))
            {
                return ToString<float>(d => BinaryPrimitives.ReadSingleBigEndian(d).ToString("e9", System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (valueType == typeof(ushort[]))
            {
                return ToString<ushort>(d => BinaryPrimitives.ReadUInt16BigEndian(d).ToString());
            }
            else
            {
                return HexHelper.ByteArrayToHexViaLookup32((byte[])Value, SchemaElement.ElementsPerRow);
            }
        }
        private delegate T ReadDataBigEndian<T>(ReadOnlySpan<byte> source);
        private unsafe string ToString<T>(ReadDataBigEndian<string> readFunc)
            where T : unmanaged
        {
            var elementSize = sizeof(T);
            var elementsPerRow = SchemaElement.ElementsPerRow;

            var sb = new StringBuilder();
            var dataSpan = data.AsSpan();
            for (int e = 0; dataSpan.Length >= elementSize; e++)
            {
                if (e % elementsPerRow == 0)
                {
                    sb.Append(Environment.NewLine);
                }
                sb.Append(readFunc(dataSpan));
                sb.Append(' ');
                dataSpan = dataSpan.Slice(elementSize);
            }
            return sb.ToString();
        }

        public byte[] FromString(string value)
        {
            Type valueType = this.GetValueType();
            if (valueType == typeof(float[]))
            {
                return FromString<float>(value, (s, d) => BinaryPrimitives.WriteSingleBigEndian(d, Convert.ToSingle(s, CultureInfo.InvariantCulture)));
            }
            else if (valueType == typeof(ushort[]))
            {
                return FromString<ushort>(value, (s, d) => BinaryPrimitives.WriteUInt16BigEndian(d, Convert.ToUInt16(s)));
            }
            else
            {
                return HexHelper.HexLineToByteUsingByteManipulation(value);
            }
        }
        private delegate void WriteDataBigEndian(string source, Span<byte> destination);
        private unsafe byte[] FromString<T>(string value, WriteDataBigEndian writeFunc)
            where T : unmanaged
        {
            var elementSize = sizeof(T);
            string[] values = value.Split(new string[] { "\r", "\n", " " }, StringSplitOptions.RemoveEmptyEntries);

            var result = new byte[values.Length * elementSize];
            var resultSpan = result.AsSpan();
            for (int i = 0; i < values.Length; i++)
            {
                writeFunc(values[i], resultSpan);
                resultSpan = resultSpan.Slice(elementSize);
            }
            return result;
        }
    }
}
