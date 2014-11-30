namespace EgoEngineLibrary.Graphics
{
    using EgoEngineLibrary.Helper;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml.Linq;

    public class PssgNode
    {
        // id, size, and attributeSize are only used during Reading/Writing
        private int size;
        private int attributeSize;
        public PssgAttributeCollection attributes;
        public PssgNodeCollection subNodes;
        private object data;

        public int Id
        {
            get { return this.NodeInfo.Id; }
        }
        public string Name
        {
            get { return NodeInfo.Name; }
        }
        public object Value
        {
            get { return data; }
            set { data = value; }
        }
        public Type ValueType
        {
            get
            {
                if (data != null)
                {
                    return data.GetType();
                }
                else
                {
                    return null;
                }
            }
        }
        public bool IsDataNode
        {
            get
            {
                if (this.subNodes.Count == 0)
                {
                    if (ValueType == typeof(Single[]))
                    {
                        return ((Single[])data).Length > 0;
                    }
                    else if (ValueType == typeof(UInt16[]))
                    {
                        return ((UInt16[])data).Length > 0;
                    }
                    else
                    {
                        return ((byte[])data).Length > 0;
                    }
                }

                return false;
            }
        }

        public PssgFile File;
        public PssgNode ParentNode;
        public TreeNode TreeNode;
        public PssgSchema.Node NodeInfo
        {
            get;
            private set;
        }

        public PssgNode(PssgBinaryReader reader, PssgFile file, PssgNode node, bool useDataNodeCheck)
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
            this.attributes = new PssgAttributeCollection();
            PssgAttribute attr;
            while (reader.BaseStream.Position < attributeEnd)
            {
                attr = new PssgAttribute(reader, file, this);
                this.attributes.Add(attr);
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
                this.data = reader.ReadNodeValue(GetValueType(), (int)(end - reader.BaseStream.Position));
                this.subNodes = new PssgNodeCollection();
                //data = reader.ReadBytes((int)(end - reader.BaseStream.Position));
            }
            else
            {
                this.data = new byte[0];
                // Each node at least 12 bytes (id + size + arg size)
                this.subNodes = new PssgNodeCollection((int)(end - reader.BaseStream.Position) / 12);
                int nodeCount = 0;
                while (reader.BaseStream.Position < end)
                {
                    this.subNodes.Add(new PssgNode(reader, file, this, useDataNodeCheck));
                    nodeCount++;
                }
            }
            PssgSchema.SetNodeDataTypeIfNull(this.NodeInfo, this.ValueType);
        }
        public PssgNode(XElement elem, PssgFile file, PssgNode node)
        {
            this.File = file;
            this.ParentNode = node;
            this.NodeInfo = PssgSchema.AddNode(elem.Name.LocalName);// PssgSchema.GetNode(elem.Name.LocalName);

            this.attributes = new PssgAttributeCollection();
            PssgAttribute attr;
            foreach (XAttribute xAttr in elem.Attributes())
            {
                attr = new PssgAttribute(xAttr, file, this);
                this.attributes.Add(attr);
            }

            // Add data, and sub nodes code here
            if (elem.FirstNode != null && elem.FirstNode is XText)
            {
                this.data = this.FromString(elem.Value);
                this.subNodes = new PssgNodeCollection();
            }
            else
            {
                this.data = new byte[0];
                this.subNodes = new PssgNodeCollection(elem.Elements().Count());
                int nodeCount = 0;
                foreach (XElement subElem in elem.Elements())
                {
                    this.subNodes.Add(new PssgNode(subElem, file, this));
                    ++nodeCount;
                }
            }
            PssgSchema.SetNodeDataTypeIfNull(this.NodeInfo, this.ValueType);
        }
        public PssgNode(PssgNode nodeToCopy)
        {
            this.File = nodeToCopy.File;
            this.ParentNode = nodeToCopy.ParentNode;

            this.NodeInfo = nodeToCopy.NodeInfo;
            this.size = nodeToCopy.size;
            this.attributeSize = nodeToCopy.attributeSize;
            this.attributes = new PssgAttributeCollection();
            PssgAttribute attr;
            foreach (PssgAttribute attrToCopy in nodeToCopy.attributes)
            {
                attr = new PssgAttribute(attrToCopy);
                this.attributes.Add(attr);
            }


            if (nodeToCopy.IsDataNode)
            {
                this.data = nodeToCopy.data;
                this.subNodes = new PssgNodeCollection();
            }
            else
            {
                this.data = new byte[0];
                // Each node at least 12 bytes (id + size + arg size)
                this.subNodes = new PssgNodeCollection(nodeToCopy.subNodes.Count);
                int nodeCount = 0;
                foreach (PssgNode subNodeToCopy in nodeToCopy.subNodes)
                {
                    this.subNodes.Add(new PssgNode(subNodeToCopy));
                    nodeCount++;
                }
            }
        }
        public PssgNode(string name, PssgFile file, PssgNode node)
        {
            this.File = file;
            this.ParentNode = node;
            this.NodeInfo = PssgSchema.AddNode(name);
            this.attributes = new PssgAttributeCollection();
            this.data = new byte[0];
            this.subNodes = new PssgNodeCollection();
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
                PssgAttributeCollection attrs;
                string linkAttrName;
                if (sNode.LinkAttributeName[0] == '^')
                {
                    attrs = this.ParentNode.attributes;
                    linkAttrName = sNode.LinkAttributeName.Substring(1);
                }
                else
                {
                    attrs = this.attributes;
                    linkAttrName = sNode.LinkAttributeName;
                }

                PssgAttribute attr = attrs[linkAttrName];
                if (attr != null && attr.Value is string)
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

            return sNode.DataType;
        }

        public void Write(PssgBinaryWriter writer)
        {
            writer.Write(this.Id);
            writer.Write(size);
            writer.Write(attributeSize);
            if (attributes != null)
            {
                foreach (PssgAttribute attr in attributes)
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
                foreach (PssgNode node in subNodes)
                {
                    node.Write(writer);
                }
            }
        }
        public void WriteXml(XElement parent)
        {
            XElement pNode = new XElement(Name);

            if (attributes != null)
            {
                foreach (PssgAttribute attr in attributes)
                {
                    pNode.Add(new XAttribute(attr.Name, attr.ToString()));
                }
            }


            if (this.IsDataNode)
            {
                pNode.Add(new XText(this.ToString())); //EndianBitConverter.ToString(data)
            }
            else
            {
                foreach (PssgNode node in subNodes)
                {
                    node.WriteXml(pNode);
                }
            }

            parent.Add(pNode);
        }
        public void UpdateId(ref int nodeNameCount, ref int attributeNameCount)
        {
            PssgSchema.Node sNode = this.NodeInfo;
            if (sNode.Id == -1)
            {
                sNode.Id = ++nodeNameCount;
            }

            //this.id = sNode.Id;
            if (attributes != null)
            {
                foreach (PssgAttribute attr in attributes)
                {
                    attr.UpdateId(ref attributeNameCount);
                }
            }

            if (subNodes != null)
            {
                foreach (PssgNode node in subNodes)
                {
                    node.UpdateId(ref nodeNameCount, ref attributeNameCount);
                }
            }
        }
        public void UpdateSize()
        {
            attributeSize = 0;
            if (attributes != null)
            {
                foreach (PssgAttribute attr in attributes)
                {
                    attr.UpdateSize();
                    attributeSize += 8 + attr.Size;
                }
            }
            size = 4 + attributeSize;

            if (this.IsDataNode)
            {
                if (ValueType == typeof(Single[]))
                {
                    size += ((Single[])data).Length * 4;
                }
                else if (ValueType == typeof(UInt16[]))
                {
                    size += ((UInt16[])data).Length * 2;
                }
                else
                {
                    size += ((byte[])data).Length;
                }
            }
            else
            {
                foreach (PssgNode node in subNodes)
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
                MessageBox.Show("Adding sub nodes to a node with data is not allowed!", "PSSG Editor", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return null;
            }

            if (this.subNodes == null)
            {
                this.subNodes = new PssgNodeCollection();
            }

            childNode.File = this.File;
            childNode.ParentNode = this;
            this.subNodes.Add(childNode);
            childNode.NodeInfo = PssgSchema.AddNode(childNode);

            return childNode;
        }
        public PssgNode SetChild(PssgNode childNode, PssgNode newChildNode)
        {
            newChildNode.File = this.File;
            newChildNode.ParentNode = this;
            PssgNode node = this.subNodes.Set(childNode, newChildNode);
            if (node != null) node.NodeInfo = PssgSchema.AddNode(node);
            return node;
        }
        public void RemoveChild(PssgNode childNode)
        {
            this.subNodes.Remove(childNode);
            childNode.ParentNode = null;
            childNode.File = null;
        }

        public PssgAttribute AddAttribute(string attributeName, object data)
        {
            if (this.attributes == null)
            {
                this.attributes = new PssgAttributeCollection();
            }
            else if (this.HasAttribute(attributeName))
            {
                this.GetAttribute(attributeName).Value = data;
                return this.GetAttribute(attributeName);
            }

            PssgAttribute newAttr = new PssgAttribute(PssgSchema.AddAttribute(this.Name, attributeName, data.GetType()), data, this.File, this);
            this.attributes.Add(newAttr);

            return newAttr;
        }
        public void RemoveAttribute(string attributeName)
        {
            this.attributes.Remove(this.GetAttribute(attributeName));
        }

        public List<PssgNode> FindNodes(string nodeName, string attributeName = null, string attributeValue = null)
        {
            List<PssgNode> ret = new List<PssgNode>();
            if (this.Name == nodeName)
            {
                if (attributeName != null && attributeValue != null)
                {
                    PssgAttribute attr = this.GetAttribute(attributeName);
                    if (attr != null && attr.ToString() == attributeValue)
                    {
                        ret.Add(this);
                    }
                }
                else if (attributeName != null)
                {
                    if (this.HasAttribute(attributeName) == true)
                    {
                        ret.Add(this);
                    }
                }
                else if (attributeValue != null)
                {
                    foreach (PssgAttribute pair in attributes)
                    {
                        if (pair.ToString() == attributeValue)
                        {
                            ret.Add(this);
                            break;
                        }
                    }
                }
                else
                {
                    ret.Add(this);
                }
            }
            if (subNodes != null)
            {
                foreach (PssgNode subNode in subNodes)
                {
                    ret.AddRange(subNode.FindNodes(nodeName, attributeName, attributeValue));
                }
            }
            return ret;
        }

        public PssgNode this[int index]
        {
            get
            {
                return this.subNodes[index];
            }
        }
        //public PssgNode this[string nodeName]
        //{
        //    get
        //    {
        //        return this.subNodes[nodeName];
        //    }
        //}

        /// <summary>
        /// Gets the node attribute associated with the specified attribute name.
        /// </summary>
        /// <param name="attributeName">The name of the attribute to get.</param>
        public PssgAttribute GetAttribute(string attributeName)
        {
            return this.attributes[attributeName];
        }
        /// <summary>
        /// Determines whether the current node has an attribute with the specified name.
        /// </summary>
        /// <param name="attributeName">The name of the attribute to find.</param>
        public bool HasAttribute(string attributeName)
        {
            return this.attributes[attributeName] != null;
        }
        public bool HasAttributes
        {
            get
            {
                if (this.attributes == null)
                {
                    return false;
                }
                else
                {
                    return this.attributes.Count > 0;
                }
            }
        }

        public override string ToString()
        {
            PssgSchema.Node sNode = this.NodeInfo;// PssgSchema.GetNode(this.Name);
            if (ValueType == typeof(Single[]))
            {
                string result = string.Empty;
                for (int i = 0; i < ((Single[])Value).Length; i++)
                {
                    if (i % sNode.ElementsPerRow == 0)
                    {
                        result += Environment.NewLine;
                    }
                    result += ((Single[])Value)[i].ToString("e9", System.Globalization.CultureInfo.InvariantCulture);
                    result += " ";
                }
                return result;
            }
            else if (ValueType == typeof(UInt16[]))
            {
                string result = string.Empty;
                for (int i = 0; i < ((UInt16[])Value).Length; i++)
                {
                    if (i % sNode.ElementsPerRow == 0)
                    {
                        result += Environment.NewLine;
                    }
                    result += ((UInt16[])Value)[i].ToString();
                    result += " ";
                }
                return result;
            }
            else
            {
                return HexHelper.ByteArrayToHexViaLookup32((byte[])Value, sNode.ElementsPerRow);
            }
        }
        public object FromString(string value)
        {
            Type valueType = this.GetValueType();
            if (valueType == typeof(Single[]))
            {
                string[] values = value.Split(new string[] { "\r", "\n", " " }, StringSplitOptions.RemoveEmptyEntries);
                Single[] result = new Single[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    result[i] = Convert.ToSingle(values[i]);
                }
                return result;
            }
            else if (valueType == typeof(UInt16[]))
            {
                string[] values = value.Split(new string[] { "\r", "\n", " " }, StringSplitOptions.RemoveEmptyEntries);
                UInt16[] result = new UInt16[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    result[i] = Convert.ToUInt16(values[i]);
                }
                return result;
            }
            else
            {
                string[] values = value.Split(new string[] { "\r", "\n", " " }, StringSplitOptions.RemoveEmptyEntries);
                byte[] bytes = new byte[values.Length];
                for (int i = 0; i < bytes.Length; i++)
                {
                    int hi;
                    int lo;

                    if (values[i].Length == 1)
                    {
                        lo = values[i][0] - 97;

                        lo = lo + 10 + ((lo >> 31) & 7) & 0x0f;
                        bytes[i] = (byte)lo;
                    }
                    else
                    {
                        hi = values[i][0] - 97;
                        lo = values[i][1] - 97;

                        hi = hi + 10 + ((hi >> 31) & 7);
                        lo = lo + 10 + ((lo >> 31) & 7) & 0x0f;
                        bytes[i] = (byte)(lo | hi << 4);
                    }
                }
                return bytes;
            }
        }
    }
}
