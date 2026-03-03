using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace EgoEngineLibrary.Graphics.Pssg
{
    public static class PssgSchema
    {
        private static Dictionary<string, PssgSchemaElement> entries = new Dictionary<string, PssgSchemaElement>();

        [RequiresUnreferencedCode("The schema node and attribute DataType may be removed.")]
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

                        string nodeName = elemNode.Attribute("name")?.Value ??
                                          throw new InvalidDataException($"The schema element {elemNode.Name} does not have attribute name.");
                        string nType = elemNode.Attribute("dataType")?.Value ??
                                       throw new InvalidDataException($"The schema element {elemNode.Name} does not have attribute dataType.");

                        PssgSchemaElement node = PssgSchema.AddNode(nodeName);
                        Type? nodeType = Type.GetType(nType, false);
                        if (nodeType != null)
                        {
                            node.DataType = nodeType;
                        }
                        node.ElementsPerRow = Convert.ToInt32(elemNode.Attribute("elementsPerRow")?.Value ??
                                                              throw new InvalidDataException($"The schema element {elemNode.Name} does not have attribute elementsPerRow."));
                        string linkAttributeName = elemNode.Attribute("linkAttributeName")?.Value ??
                                                   throw new InvalidDataException($"The schema element {elemNode.Name} does not have attribute linkAttributeName.");
                        if (!string.IsNullOrEmpty(linkAttributeName))
                        {
                            node.LinkAttributeName = linkAttributeName;
                        }

                        foreach (XNode subNode in elemNode.Descendants("attribute"))
                        {
                            if (subNode is XElement subElem)
                            {
                                string attrName = ((XElement)subNode).Attribute("name")?.Value ??
                                                  throw new InvalidDataException($"The schema element {subElem.Name} does not have attribute name.");
                                Type? attrType = Type.GetType(((XElement)subNode).Attribute("dataType")?.Value ??
                                                              throw new InvalidDataException($"The schema element {subElem.Name} does not have attribute dataType."), false);
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
                                        node.Attributes.Add(new PssgSchemaAttribute(attrName, attrType));
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
                XElement parent = (XElement)xDoc.FirstNode!;

                foreach (KeyValuePair<string, PssgSchemaElement> entry in entries)
                {
                    XElement pNode = new XElement("node");
                    pNode.Add(new XAttribute("name", entry.Key), new XAttribute("dataType", entry.Value.DataType));
                    pNode.Add(new XAttribute("elementsPerRow", entry.Value.ElementsPerRow), new XAttribute("linkAttributeName", entry.Value.LinkAttributeName));

                    foreach (PssgSchemaAttribute attrEntry in entry.Value.Attributes)
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
        }

        public static void LoadFromPssg(PssgBinaryReader reader)
        {
            int attributeInfoCount = reader.ReadInt32();
            int nodeInfoCount = reader.ReadInt32();

            var elementTable = new PssgSchemaElement[nodeInfoCount];
            var attributeTable = new PssgSchemaAttribute[attributeInfoCount];
            for (int i = 0; i < nodeInfoCount; i++)
            {
                int nId = reader.ReadInt32();
                var elementName = reader.ReadPSSGString();

                if (!entries.TryGetValue(elementName, out PssgSchemaElement? element))
                {
                    element = PssgSchema.AddNode(new PssgSchemaElement(elementName));
                }

                elementTable[nId - 1] = element;
                int subAttributeInfoCount = reader.ReadInt32();
                for (int j = 0; j < subAttributeInfoCount; j++)
                {
                    int id = reader.ReadInt32();
                    var attributeName = reader.ReadPSSGString();

                    PssgSchemaAttribute? attr = PssgSchema.GetAttribute(element.Name, attributeName);
                    if (attr == null)
                    {
                        attr = new PssgSchemaAttribute(attributeName);
                        PssgSchema.AddAttribute(element.Name, attr);
                    }
                    
                    attributeTable[id - 1] = attr;
                }
            }
            
            reader.ElementTable.AddRange(elementTable);
            reader.AttributeTable.AddRange(attributeTable);
        }
        public static void SaveToPssg(PssgBinaryWriter writer)
        {
            for (var i = 0; i < writer.ElementTable.Count; ++i)
            {
                var element = writer.ElementTable[i];
                writer.Write(i + 1);
                writer.WritePSSGString(element.Name);

                int pos = (int)writer.BaseStream.Position;
                int attributeCount = 0;
                writer.Write(0); // attributeCount

                foreach (PssgSchemaAttribute attribute in element.Attributes)
                {
                    if (!writer.AttributeTable.TryGetValue(attribute, out _, out var attributeIndex))
                    {
                        continue;
                    }

                    ++attributeCount;
                    writer.Write(attributeIndex + 1);
                    writer.WritePSSGString(attribute.Name);
                }

                writer.Seek(pos, SeekOrigin.Begin);
                writer.Write(attributeCount);
                writer.Seek(0, SeekOrigin.End);
            }
        }

        public static PssgSchemaElement? GetNode(string node)
        {
            if (entries.ContainsKey(node))
            {
                return entries[node];
            }

            return null;
        }
        public static PssgSchemaAttribute? GetAttribute(string node, string attr)
        {
            if (entries.ContainsKey(node))
            {
                foreach (PssgSchemaAttribute attrEntry in entries[node].Attributes)
                {
                    if (attrEntry.Name == attr)
                    {
                        return attrEntry;
                    }
                }
            }

            return null;
        }
        //public static Type GetAttributeType(string node, string attr)
        //{
        //    if (entries.ContainsKey(node))
        //    {
        //        foreach (Attribute attrEntry in entries[node].Attributes)
        //        {
        //            if (attrEntry.Name == attr)
        //            {
        //                return attrEntry.DataType;
        //            }
        //        }
        //    }

        //    return null;
        //}
        public static string[] GetNodeNames()
        {
            return entries.Keys.ToArray();
        }
        public static string[] GetAttributeNames()
        {
            List<string> aNames = new List<string>();
            foreach (KeyValuePair<string, PssgSchemaElement> entry in entries)
            {
                foreach (PssgSchemaAttribute attrEntry in entry.Value.Attributes)
                {
                    aNames.Add(attrEntry.Name);
                }
            }
            return aNames.ToArray();
        }

        public static PssgSchemaElement AddNode(string nodeName)
        {
            if (!entries.ContainsKey(nodeName))
            {
                PssgSchemaElement node = new PssgSchemaElement(nodeName);
                entries.Add(node.Name, node);
                return node;
            }

            return entries[nodeName];
        }
        internal static PssgSchemaElement AddNode(PssgSchemaElement node)
        {
            if (!entries.ContainsKey(node.Name))
            {
                entries.Add(node.Name, node);
                return node;
            }
            else
            {
                PssgSchema.SetNodeDataTypeIfNull(entries[node.Name], node.DataType);

                foreach (PssgSchemaAttribute attrEntry in node.Attributes)
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

        public static PssgSchemaAttribute AddAttribute(string nodeName, string attributeName, Type attrType)
        {
            PssgSchemaElement node = PssgSchema.AddNode(nodeName);

            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name == attributeName)
                {
                    // Allow overwrite if current data type is null
                    PssgSchema.SetAttributeDataTypeIfNull(node.Attributes[i], attrType);
                    return node.Attributes[i];
                }
            }

            PssgSchemaAttribute attr = new PssgSchemaAttribute(attributeName, attrType);
            node.Attributes.Add(attr);
            return attr;
        }
        public static PssgSchemaAttribute AddAttribute(string nodeName, string attributeName)
        {
            PssgSchemaElement node = PssgSchema.AddNode(nodeName);

            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name == attributeName)
                {
                    return node.Attributes[i];
                }
            }

            PssgSchemaAttribute attr = new PssgSchemaAttribute(attributeName);
            node.Attributes.Add(attr);
            return attr;
        }
        public static void AddAttribute(string nodeName, PssgSchemaAttribute attribute)
        {
            PssgSchemaElement node = PssgSchema.AddNode(nodeName);

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

        public static PssgSchemaElement AddNode(PssgNode node)
        {
            PssgSchemaElement sNode = new PssgSchemaElement(node.Name);
            sNode.DataType = node.Value.GetType();

            foreach (PssgAttribute attr in node.Attributes)
            {
                PssgSchemaAttribute sAttr = new PssgSchemaAttribute(attr.Name, attr.Value.GetType());
                sNode.Attributes.Add(sAttr);
            }

            return PssgSchema.AddNode(sNode);
        }
        public static PssgSchemaElement RenameNode(PssgNode pssgNode, string nodeName)
        {
            PssgSchemaElement node = PssgSchema.AddNode(nodeName);

            foreach (PssgAttribute attr in pssgNode.Attributes)
            {
                PssgSchema.AddAttribute(node.Name, attr.AttributeInfo.Name, attr.AttributeInfo.DataType);
            }

            return node;
        }

        public static void SetNodeDataTypeIfNull(PssgSchemaElement node, Type dataType)
        {
            if (node.DataType == typeof(System.Exception))
            {
                node.DataType = dataType;
            }
        }
        public static void SetAttributeDataTypeIfNull(PssgSchemaAttribute attribute, Type attrType)
        {
            if (attribute.DataType == typeof(System.Exception))
            {
                attribute.DataType = attrType;
            }
        }
    }
}