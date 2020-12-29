namespace EgoEngineLibrary.Graphics
{
    using EgoEngineLibrary.Helper;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    public class PssgNode
    {
        // id, size, and attributeSize are only used during Reading/Writing
        private int size;
        private int attributeSize;
        private byte[] data;

        public string Name
        {
            get { return NodeInfo.Name; }
        }
        public PssgAttributeCollection Attributes
        {
            get;
            set;
        }
        public PssgNodeCollection ChildNodes
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
        public bool IsDataNode
        {
            get
            {
                if (this.ChildNodes.Count == 0)
                {
                    return data.Length > 0;
                }

                return false;
            }
        }

        public PssgFile File
        {
            get;
            set;
        }
        public PssgNode? ParentNode
        {
            get;
            set;
        }
        // NodeInfo should never be null
        public PssgSchema.Node NodeInfo
        {
            get;
            private set;
        }

        public PssgNode(PssgBinaryReader reader, PssgFile file, PssgNode? node, bool useDataNodeCheck)
        {
            this.File = file;
            this.ParentNode = node;

            int id = reader.ReadInt32();
            this.NodeInfo = PssgSchema.GetNode(id);
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

            bool isDataNode = false;
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
                    isDataNode = true;
                    break;
            }
            if (isDataNode == false && useDataNodeCheck == true)
            {
                long currentPos = reader.BaseStream.Position;
                // Check if it has subnodes
                while (reader.BaseStream.Position < end)
                {
                    int tempID = reader.ReadInt32();
                    if (tempID < 0)//tempID > file.nodeInfo.Length || 
                    {
                        isDataNode = true;
                        break;
                    }
                    else
                    {
                        int tempSize = reader.ReadInt32();
                        if ((reader.BaseStream.Position + tempSize > end) || (tempSize == 0 && tempID == 0) || tempSize < 0)
                        {
                            isDataNode = true;
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

            if (isDataNode)
            {
                this.data = reader.ReadNodeValue((int)(end - reader.BaseStream.Position));
                this.ChildNodes = new PssgNodeCollection();
            }
            else
            {
                this.data = Array.Empty<byte>();
                // Each node at least 12 bytes (id + size + arg size)
                this.ChildNodes = new PssgNodeCollection((int)(end - reader.BaseStream.Position) / 12);
                int nodeCount = 0;
                while (reader.BaseStream.Position < end)
                {
                    this.ChildNodes.Add(new PssgNode(reader, file, this, useDataNodeCheck));
                    nodeCount++;
                }
            }
            PssgSchema.SetNodeDataTypeIfNull(this.NodeInfo, Value.GetType());
        }
        public PssgNode(XElement elem, PssgFile file, PssgNode? node)
        {
            this.File = file;
            this.ParentNode = node;
            this.NodeInfo = PssgSchema.AddNode(elem.Name.LocalName);// PssgSchema.GetNode(elem.Name.LocalName);

            this.Attributes = new PssgAttributeCollection();
            PssgAttribute attr;
            foreach (XAttribute xAttr in elem.Attributes())
            {
                attr = new PssgAttribute(xAttr, file, this);
                this.Attributes.Add(attr);
            }

            // Add data, and sub nodes code here
            if (elem.FirstNode != null && elem.FirstNode is XText)
            {
                this.data = this.FromString(elem.Value);
                this.ChildNodes = new PssgNodeCollection();
            }
            else
            {
                this.data = Array.Empty<byte>();
                this.ChildNodes = new PssgNodeCollection(elem.Elements().Count());
                int nodeCount = 0;
                foreach (XElement subElem in elem.Elements())
                {
                    this.ChildNodes.Add(new PssgNode(subElem, file, this));
                    ++nodeCount;
                }
            }
            PssgSchema.SetNodeDataTypeIfNull(this.NodeInfo, Value.GetType());
        }
        public PssgNode(PssgNode nodeToCopy)
        {
            this.File = nodeToCopy.File;
            this.ParentNode = nodeToCopy.ParentNode;

            this.NodeInfo = nodeToCopy.NodeInfo;
            this.size = nodeToCopy.size;
            this.attributeSize = nodeToCopy.attributeSize;
            this.Attributes = new PssgAttributeCollection();
            PssgAttribute attr;
            foreach (PssgAttribute attrToCopy in nodeToCopy.Attributes)
            {
                attr = new PssgAttribute(attrToCopy);
                attr.ParentNode = this;
                this.Attributes.Add(attr);
            }


            if (nodeToCopy.IsDataNode)
            {
                this.data = nodeToCopy.data;
                this.ChildNodes = new PssgNodeCollection();
            }
            else
            {
                this.data = Array.Empty<byte>();
                // Each node at least 12 bytes (id + size + arg size)
                this.ChildNodes = new PssgNodeCollection(nodeToCopy.ChildNodes.Count);
                foreach (PssgNode subNodeToCopy in nodeToCopy.ChildNodes)
                {
                    PssgNode node = new PssgNode(subNodeToCopy);
                    node.ParentNode = this;
                    this.ChildNodes.Add(node);
                }
            }
        }
        public PssgNode(string name, PssgFile file, PssgNode? node)
        {
            this.File = file;
            this.ParentNode = node;
            this.NodeInfo = PssgSchema.AddNode(name);
            this.Attributes = new PssgAttributeCollection();
            this.data = Array.Empty<byte>();
            this.ChildNodes = new PssgNodeCollection();
        }

        private Type GetValueType()
        {
            PssgSchema.Node sNode = this.NodeInfo;// PssgSchema.GetNode(this.Name);
            if (sNode == null)
            {
                return typeof(byte[]);
            }

            if (!string.IsNullOrEmpty(sNode.LinkAttributeName))
            {
                PssgNode node;
                string linkAttrName;
                if (sNode.LinkAttributeName[0] == '^')
                {
                    if (this.ParentNode == null) throw new InvalidOperationException("Node without parent cannot reference a parent's attribute.");
                    node = this.ParentNode;
                    linkAttrName = sNode.LinkAttributeName.Substring(1);
                }
                else
                {
                    node = this;
                    linkAttrName = sNode.LinkAttributeName;
                }

                if (node.HasAttribute(linkAttrName))
                {
                    PssgAttribute attr = node.Attributes[linkAttrName];
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

            return sNode.DataType;
        }

        public void Write(PssgBinaryWriter writer)
        {
            writer.Write(this.NodeInfo.Id);
            writer.Write(size);
            writer.Write(attributeSize);
            if (Attributes != null)
            {
                foreach (PssgAttribute attr in Attributes)
                {
                    attr.Write(writer);
                }
            }
            if (this.IsDataNode)
            {
                writer.WriteObject(data);
            }
            else
            {
                foreach (PssgNode node in ChildNodes)
                {
                    node.Write(writer);
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


            if (this.IsDataNode)
            {
                pNode.Add(new XText(this.ToString())); //EndianBitConverter.ToString(data)
            }
            else
            {
                foreach (PssgNode node in ChildNodes)
                {
                    node.WriteXml(pNode);
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

            if (this.IsDataNode)
            {
                size += data.Length;
            }
            else
            {
                foreach (PssgNode node in ChildNodes)
                {
                    node.UpdateSize();
                    size += 8 + node.size;
                }
            }
        }

        public void Rename(string nodeName)
        {
            this.NodeInfo = PssgSchema.RenameNode(this, nodeName);
        }

        public PssgNode AppendChild(string nodeName)
        {
            return this.AppendChild(new PssgNode(nodeName, this.File, this));
        }
        public PssgNode AppendChild(PssgNode childNode)
        {
            if (this.IsDataNode == true)
            {
                throw new InvalidOperationException("Cannot append a child node to a data node");
            }

            if (this.ChildNodes == null)
            {
                this.ChildNodes = new PssgNodeCollection();
            }

            childNode.File = this.File;
            childNode.ParentNode = this;
            this.ChildNodes.Add(childNode);
            childNode.NodeInfo = PssgSchema.AddNode(childNode);

            return childNode;
        }
        public PssgNode? SetChild(PssgNode childNode, PssgNode newChildNode)
        {
            newChildNode.File = this.File;
            newChildNode.ParentNode = this;
            PssgNode? node = this.ChildNodes.Set(childNode, newChildNode);
            if (node != null) node.NodeInfo = PssgSchema.AddNode(node);
            return node;
        }
        public void RemoveChild(PssgNode childNode)
        {
            if (this.ChildNodes.Remove(childNode))
                childNode.ParentNode = null;
            else
                throw new InvalidOperationException("Failed to remove child node.");
        }

        public void RemoveChildNodes(IEnumerable<PssgNode>? childNodes = null)
        {
            childNodes ??= ChildNodes;
            while (true)
            {
                var child = childNodes.FirstOrDefault();
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
        /// Gets this node, and it's hierarchy as a flat sequence.
        /// </summary>
        /// <returns>the flat node hierarchy.</returns>
        public IEnumerable<PssgNode> GetNodes()
        {
            yield return this;

            foreach (var c in ChildNodes)
            {
                var cc = c.GetNodes();

                foreach (var ccc in cc) yield return ccc;
            }
        }

        public IEnumerable<PssgNode> FindNodes(string nodeName)
        {
            return GetNodes().FindNodes(nodeName);
        }
        public IEnumerable<PssgNode> FindNodes(string nodeName, string attributeName)
        {
            return GetNodes().FindNodes(nodeName, attributeName);
        }
        public IEnumerable<PssgNode> FindNodes<T>(string nodeName, string attributeName, T attributeValue)
            where T : notnull
        {
            return GetNodes().FindNodes(nodeName, attributeName, attributeValue);
        }

        public PssgNode this[int index]
        {
            get
            {
                return this.ChildNodes[index];
            }
        }

        /// <summary>
        /// Determines whether the current node has an attribute with the specified name.
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
                return HexHelper.ByteArrayToHexViaLookup32((byte[])Value, NodeInfo.ElementsPerRow);
            }
        }
        private delegate T ReadDataBigEndian<T>(ReadOnlySpan<byte> source);
        private unsafe string ToString<T>(ReadDataBigEndian<string> readFunc)
            where T : unmanaged
        {
            var elementSize = sizeof(T);
            var elementsPerRow = NodeInfo.ElementsPerRow;

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
                return FromString<float>(value, (s, d) => BinaryPrimitives.WriteSingleBigEndian(d, Convert.ToSingle(s)));
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
