namespace EgoEngineLibrary.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    public static class PssgSchema
    {
        // Pssg Node Names and Ids seem to be unique
        // Pssg Attribute Id does not need to be unique (At least not in CMR Dirt)
        // Attribute Names do not need to be unique across multiple nodes
        // Ids start from 1
        public class Node
        {
            public int Id
            {
                get;
                set;
            }
            public string Name
            {
                get;
                private set;
            }
            public Type DataType
            {
                get;
                set;
            }
            public int ElementsPerRow
            {
                get;
                set;
            }
            public string LinkAttributeName
            {
                get;
                set;
            }
            public List<Attribute> Attributes
            {
                get;
                set;
            }

            public Node(string name)
            {
                this.Id = -1;
                this.Name = nameTable.Add(name);
                this.DataType = typeof(System.Exception);
                this.ElementsPerRow = 32;
                this.LinkAttributeName = string.Empty;
                this.Attributes = new List<Attribute>();
            }
            public Node(string name, Type dataType)
            {
                this.Id = -1;
                this.Name = nameTable.Add(name);
                this.DataType = dataType;
                this.ElementsPerRow = 32;
                this.LinkAttributeName = string.Empty;
                this.Attributes = new List<Attribute>();
            }
        }
        public class Attribute
        {
            public int Id
            {
                get;
                set;
            }
            public string Name
            {
                get;
                private set;
            }
            public Type DataType
            {
                get;
                set;
            }

            public Attribute(string name)
            {
                this.Id = -1;
                this.Name = nameTable.Add(name);
                this.DataType = typeof(System.Exception);
            }
            public Attribute(string name, Type dataType)
            {
                this.Id = -1;
                this.Name = nameTable.Add(name);
                this.DataType = dataType;
            }
        }

        private static Dictionary<string, Node> entries = new Dictionary<string, Node>();
        private static NameTable nameTable = new NameTable();

        public static void LoadSchema(Stream stream)
        {
            entries.Clear();

            PssgSchema.AddAttribute("FETEXTLAYOUT", "height", typeof(Single));
            PssgSchema.AddAttribute("FETEXTLAYOUT", "depth", typeof(Single));
            PssgSchema.AddAttribute("FETEXTLAYOUT", "tracking", typeof(Single));

            PssgSchema.AddAttribute("NEGLYPHMETRICS", "advanceWidth", typeof(Single));
            PssgSchema.AddAttribute("NEGLYPHMETRICS", "horizontalBearing", typeof(Single));
            PssgSchema.AddAttribute("NEGLYPHMETRICS", "verticalBearing", typeof(Single));
            PssgSchema.AddAttribute("NEGLYPHMETRICS", "physicalWidth", typeof(Single));
            PssgSchema.AddAttribute("NEGLYPHMETRICS", "physicalHeight", typeof(Single));

            PssgSchema.AddAttribute("FEATLASINFODATA", "u0", typeof(Single));
            PssgSchema.AddAttribute("FEATLASINFODATA", "v0", typeof(Single));
            PssgSchema.AddAttribute("FEATLASINFODATA", "u1", typeof(Single));
            PssgSchema.AddAttribute("FEATLASINFODATA", "v1", typeof(Single));

            if (stream.Length == 0)
            {
                return;
            }

            using (stream)
            {
                XDocument xDoc = XDocument.Load(stream);

                foreach (XNode xN in xDoc.Descendants("node"))
                {
                    if (xN is XElement)
                    {
                        XElement elemNode = (XElement)xN;
                        string nodeName = elemNode.Attribute("name").Value;
                        Node node = PssgSchema.AddNode(nodeName);
                        Type nodeType = Type.GetType(elemNode.Attribute("dataType").Value, false);
                        if (nodeType != null)
                        {
                            node.DataType = nodeType;
                        }
                        node.ElementsPerRow = Convert.ToInt32(elemNode.Attribute("elementsPerRow").Value);
                        string linkAttributeName = elemNode.Attribute("linkAttributeName").Value;
                        if (!string.IsNullOrEmpty(linkAttributeName))
                        {
                            node.LinkAttributeName = linkAttributeName;
                        }

                        foreach (XNode subNode in elemNode.Descendants("attribute"))
                        {
                            if (xN is XElement)
                            {
                                string attrName = ((XElement)subNode).Attribute("name").Value;
                                Type attrType = Type.GetType(((XElement)subNode).Attribute("dataType").Value, false);
                                if (attrType != null)
                                {
                                    bool add = true;
                                    for (int i = 0; i < node.Attributes.Count; i++)
                                    {
                                        if (node.Attributes[i].Name == attrName)
                                        {
                                            add = false;
                                            node.Attributes[i].DataType = attrType;
                                            break;
                                        }
                                    }

                                    if (add)
                                    {
                                        node.Attributes.Add(new Attribute(attrName, attrType));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void SaveSchema(Stream stream)
        {
            using (stream)
            {
                XDocument xDoc = new XDocument();
                xDoc.Add(new XElement("PSSGFILE", new XAttribute("version", "1.0.0.0")));
                XElement parent = (XElement)xDoc.FirstNode;

                foreach (KeyValuePair<string, Node> entry in entries)
                {
                    XElement pNode = new XElement("node");
                    pNode.Add(new XAttribute("name", entry.Key), new XAttribute("dataType", entry.Value.DataType));
                    pNode.Add(new XAttribute("elementsPerRow", entry.Value.ElementsPerRow), new XAttribute("linkAttributeName", entry.Value.LinkAttributeName));

                    foreach (Attribute attrEntry in entry.Value.Attributes)
                    {
                        pNode.Add(new XElement("attribute", new XAttribute("name", attrEntry.Name), new XAttribute("dataType", attrEntry.DataType)));
                    }

                    parent.Add(pNode);
                }

                xDoc.Save(stream);
            }
        }
        public static void ClearSchema()
        {
            entries.Clear();
            nameTable = new NameTable();
        }
        public static void ClearSchemaIds()
        {
            foreach (KeyValuePair<string, Node> entry in entries)
            {
                entry.Value.Id = -1;

                foreach (Attribute attrEntry in entry.Value.Attributes)
                {
                    attrEntry.Id = -1;
                }
            }
        }

        public static void LoadFromPssg(PssgBinaryReader reader)
        {
            int attributeInfoCount = reader.ReadInt32();
            int nodeInfoCount = reader.ReadInt32();
            Node node;
            Attribute attribute;

            for (int i = 0; i < nodeInfoCount; i++)
            {
                int nId = reader.ReadInt32();
                node = new Node(reader.ReadPSSGString());
                node.Id = nId;

                if (entries.ContainsKey(node.Name))
                {
                    entries[node.Name].Id = node.Id;
                }
                else
                {
                    PssgSchema.AddNode(node);
                }

                int subAttributeInfoCount = reader.ReadInt32();
                for (int j = 0; j < subAttributeInfoCount; j++)
                {
                    int id = reader.ReadInt32();
                    attribute = new Attribute(reader.ReadPSSGString());
                    attribute.Id = id;

                    Attribute attr = PssgSchema.GetAttribute(node.Name, attribute.Name);
                    if (attr == null)
                    {
                        PssgSchema.AddAttribute(node.Name, attribute);
                    }
                    else
                    {
                        attr.Id = attribute.Id;
                    }
                }
            }
        }
        public static void SaveToPssg(PssgBinaryWriter writer)
        {
            // Update Ids to make sequential, and Unique
            int currentId = 0;
            int currentAttrId = 0;
            // Minimizing the attributes to unique names only seems to crash the game
            //Dictionary<string, int> unique = new Dictionary<string, int>();

            foreach (KeyValuePair<string, Node> entry in entries)
            {
                if (entry.Value.Id > 0)
                {
                    entry.Value.Id = ++currentId;
                    writer.Write(entry.Value.Id);
                    writer.WritePSSGString((string)entry.Key);

                    int pos = (int)writer.BaseStream.Position;
                    int attributeCount = 0;
                    writer.Write(0); // attributeCount

                    foreach (Attribute attrEntry in entry.Value.Attributes)
                    {
                        if (attrEntry.Id > 0)
                        {
                            attrEntry.Id = ++currentAttrId;
                            writer.Write(attrEntry.Id);
                            writer.WritePSSGString(attrEntry.Name);
                            ++attributeCount;
                        }
                    }

                    writer.Seek(pos, SeekOrigin.Begin);
                    writer.Write(attributeCount);
                    writer.Seek(0, SeekOrigin.End);
                }
            }
        }

        public static Node GetNode(string node)
        {
            if (entries.ContainsKey(node))
            {
                return entries[node];
            }

            return null;
        }
        public static Node GetNode(int nodeId)
        {
            foreach (KeyValuePair<string, Node> entry in entries)
            {
                if (entry.Value.Id == nodeId)
                {
                    return entry.Value;
                }
            }

            return null;
        }
        public static Attribute GetAttribute(string node, string attr)
        {
            if (entries.ContainsKey(node))
            {
                foreach (Attribute attrEntry in entries[node].Attributes)
                {
                    if (attrEntry.Name == attr)
                    {
                        return attrEntry;
                    }
                }
            }

            return null;
        }
        public static Attribute GetAttribute(int attrId)
        {
            foreach (KeyValuePair<string, Node> entry in entries)
            {
                foreach (Attribute attrEntry in entry.Value.Attributes)
                {
                    if (attrEntry.Id == attrId)
                    {
                        return attrEntry;
                    }
                }
            }

            return null;
        }
        public static Type GetAttributeType(string node, string attr)
        {
            if (entries.ContainsKey(node))
            {
                foreach (Attribute attrEntry in entries[node].Attributes)
                {
                    if (attrEntry.Name == attr)
                    {
                        return attrEntry.DataType;
                    }
                }
            }

            return null;
        }
        public static string[] GetNodeNames()
        {
            return entries.Keys.ToArray();
        }
        public static string[] GetAttributeNames()
        {
            List<string> aNames = new List<string>();
            foreach (KeyValuePair<string, Node> entry in entries)
            {
                foreach (Attribute attrEntry in entry.Value.Attributes)
                {
                    aNames.Add(attrEntry.Name);
                }
            }
            return aNames.ToArray();
        }

        public static Node AddNode(string nodeName)
        {
            if (!entries.ContainsKey(nodeName))
            {
                Node node = new Node(nodeName);
                entries.Add(node.Name, node);
                return node;
            }

            return entries[nodeName];
        }
        internal static Node AddNode(Node node)
        {
            if (!entries.ContainsKey(node.Name))
            {
                entries.Add(node.Name, node);
                return node;
            }
            else
            {
                PssgSchema.SetNodeDataTypeIfNull(entries[node.Name], node.DataType);

                foreach (Attribute attrEntry in node.Attributes)
                {
                    bool add = true;
                    for (int i = 0; i < entries[node.Name].Attributes.Count; i++)
                    {
                        if (entries[node.Name].Attributes[i].Name == attrEntry.Name)
                        {
                            add = false;
                            PssgSchema.SetAttributeDataTypeIfNull(entries[node.Name].Attributes[i], attrEntry.DataType);
                            break;
                        }
                    }

                    if (add)
                    {
                        entries[node.Name].Attributes.Add(attrEntry);
                    }
                }

                return entries[node.Name];
            }
        }

        public static Attribute AddAttribute(string nodeName, string attributeName, Type attrType)
        {
            Node node = PssgSchema.AddNode(nodeName);

            if (attrType != null)
            {
                bool add = true;
                for (int i = 0; i < node.Attributes.Count; i++)
                {
                    if (node.Attributes[i].Name == attributeName)
                    {
                        add = false;
                        // Allow overwrite if current data type is null
                        PssgSchema.SetAttributeDataTypeIfNull(node.Attributes[i], attrType);
                        return node.Attributes[i];
                    }
                }

                if (add)
                {
                    PssgSchema.Attribute attr = new Attribute(attributeName, attrType);
                    node.Attributes.Add(attr);
                    return attr;
                }
            }

            return null;
        }
        public static Attribute AddAttribute(string nodeName, string attributeName)
        {
            Node node = PssgSchema.AddNode(nodeName);

            bool add = true;
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name == attributeName)
                {
                    add = false;
                    return node.Attributes[i];
                }
            }

            if (add)
            {
                PssgSchema.Attribute attr = new Attribute(attributeName);
                node.Attributes.Add(attr);
                return attr;
            }

            return null;
        }
        public static void AddAttribute(string nodeName, Attribute attribute)
        {
            Node node = PssgSchema.AddNode(nodeName);

            bool add = true;
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name == attribute.Name)
                {
                    add = false;
                    // Allow overwrite if current data type is null
                    PssgSchema.SetAttributeDataTypeIfNull(node.Attributes[i], attribute.DataType);
                    break;
                }
            }

            if (add)
            {
                node.Attributes.Add(attribute);
            }
        }

        //public static void CreatePssgInfo(out PssgNodeInfo[] nodeInfo, out PssgAttributeInfo[] attributeInfo)
        //{
        //    nodeInfo = new PssgNodeInfo[entries.Count];
        //    List<PssgAttributeInfo> attrInfo = new List<PssgAttributeInfo>();

        //    int i = 0, j = 0;
        //    foreach (KeyValuePair<string, Node> node in entries)
        //    {
        //        nodeInfo[i] = new PssgNodeInfo(i + 1, node.Key);

        //        foreach (Attribute attr in node.Value.Attributes)
        //        {
        //            attr.Id = ++j;
        //            PssgAttributeInfo aInfo = new PssgAttributeInfo(attr.Id, attr.Name);
        //            attrInfo.Add(aInfo);
        //            nodeInfo[i].attributeInfo.Add(attr.Id, aInfo);
        //        }

        //        node.Value.Id = ++i;
        //    }

        //    attributeInfo = attrInfo.ToArray();
        //}

        public static PssgSchema.Node AddNode(PssgNode node)
        {
            Node sNode = new Node(node.Name);
            sNode.DataType = node.ValueType;

            foreach (PssgAttribute attr in node.Attributes)
            {
                Attribute sAttr = new Attribute(attr.Name, attr.ValueType);
                sNode.Attributes.Add(sAttr);
            }

            return PssgSchema.AddNode(sNode);
        }
        public static PssgSchema.Node RenameNode(PssgNode pssgNode, string nodeName)
        {
            PssgSchema.Node node = PssgSchema.AddNode(nodeName);

            foreach (PssgAttribute attr in pssgNode.Attributes)
            {
                PssgSchema.AddAttribute(node.Name, attr.AttributeInfo.Name, attr.AttributeInfo.DataType);
            }

            return node;
        }

        public static void SetNodeDataTypeIfNull(PssgSchema.Node node, Type dataType)
        {
            if (node.DataType == typeof(System.Exception))
            {
                node.DataType = dataType;
            }
        }
        public static void SetAttributeDataTypeIfNull(PssgSchema.Attribute attribute, Type attrType)
        {
            if (attribute.DataType == typeof(System.Exception))
            {
                attribute.DataType = attrType;
            }
        }
    }
}
