using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using EgoEngineLibrary.Graphics.Pssg.Elements;

namespace EgoEngineLibrary.Graphics.Pssg
{
    public static class PssgSchema
    {
        private static Dictionary<string, PssgSchemaElement> entries = new();

        static PssgSchema()
        {
            AddNode(PssgObject.Schema);
            AddNode(Elements.PssgNode.Schema);
            AddNode(PssgRootNode.Schema);
        }

        [RequiresUnreferencedCode("The schema node and attribute DataType may be removed.")]
        public static void LoadSchema(Stream stream)
        {
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

            XDocument xDoc = XDocument.Load(stream);
            foreach (XElement xN in xDoc.Descendants("node"))
            {
                string nodeName = xN.Attribute("name")?.Value ??
                                  throw new InvalidDataException(
                                      $"The schema element {xN.Name} does not have attribute name.");
                string nType = xN.Attribute("dataType")?.Value ??
                               throw new InvalidDataException(
                                   $"The schema element {xN.Name} does not have attribute dataType.");

                PssgSchemaElement node = AddNode(nodeName);
                Type? nodeType = Type.GetType(nType, false);
                if (nodeType != null)
                {
                    node.DataType = nodeType;
                }
                node.ElementsPerRow = Convert.ToInt32(xN.Attribute("elementsPerRow")?.Value ??
                                                      throw new InvalidDataException($"The schema element {xN.Name} does not have attribute elementsPerRow."));
                string linkAttributeName = xN.Attribute("linkAttributeName")?.Value ??
                                           throw new InvalidDataException($"The schema element {xN.Name} does not have attribute linkAttributeName.");
                if (!string.IsNullOrEmpty(linkAttributeName))
                {
                    node.LinkAttributeName = linkAttributeName;
                }

                foreach (XElement attrElem in xN.Descendants("attribute"))
                {
                    string attrName = attrElem.Attribute("name")?.Value ??
                                      throw new InvalidDataException(
                                          $"The schema element {attrElem.Name} does not have attribute name.");
                    Type? attrType =
                        Type.GetType(
                            attrElem.Attribute("dataType")?.Value ??
                            throw new InvalidDataException(
                                $"The schema element {attrElem.Name} does not have attribute dataType."), false);

                    PssgSchemaAttribute attr = AddAttribute(node, attrName);
                    if (attrType != null)
                    {
                        attr.DataType = attrType;
                    }
                }
            }
        }
        public static void SaveSchema(Stream stream)
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

                    PssgSchemaAttribute attr = AddAttribute(element.Name, attributeName);
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

                foreach ((PssgSchemaAttribute attribute, int index) in element.Attributes
                             .Select(x => (x, writer.AttributeTable.IndexOf(x)))
                             .Where(x => x.Item2 != -1)
                             .OrderBy(x => x.Item2))
                {
                    ++attributeCount;
                    writer.Write(index + 1);
                    writer.WritePSSGString(attribute.Name);
                }

                writer.Seek(pos, SeekOrigin.Begin);
                writer.Write(attributeCount);
                writer.Seek(0, SeekOrigin.End);
            }
        }

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

        public static PssgSchemaElement AddNode(string elementName)
        {
            if (entries.TryGetValue(elementName, out PssgSchemaElement? existingElement))
            {
                return existingElement;
            }

            PssgSchemaElement element = new(elementName);
            entries.Add(element.Name, element);
            return element;
        }

        public static PssgSchemaElement AddNode(PssgSchemaElement node)
        {
            if (!entries.TryGetValue(node.Name, out var existingElement))
            {
                entries.Add(node.Name, node);
                return node;
            }

            existingElement = entries[node.Name];
            if (ReferenceEquals(existingElement, node))
            {
                return existingElement;
            }
                
            SetNodeDataTypeIfNull(existingElement, node.DataType);
            foreach (PssgSchemaAttribute attrEntry in node.Attributes)
            {
                AddAttribute(existingElement, attrEntry.Name, attrEntry.DataType);
            }

            return existingElement;
        }

        public static PssgSchemaAttribute AddAttribute(string nodeName, string attributeName, Type? attrType = null)
        {
            PssgSchemaElement element = AddNode(nodeName);
            return AddAttribute(element, attributeName, attrType);
        }

        public static PssgSchemaAttribute AddAttribute(PssgSchemaElement element, string attributeName,
            Type? attrType = null)
        {
            PssgSchemaElement? baseElement = element;
            while (baseElement is not null)
            {
                foreach (var attribute in baseElement.Attributes)
                {
                    if (attribute.Name != attributeName)
                    {
                        continue;
                    }

                    // Allow overwrite if current data type is null
                    SetAttributeDataTypeIfNull(attribute, attrType);
                    return attribute;
                }

                baseElement = baseElement.BaseElement;
            }

            PssgSchemaAttribute attr = new(attributeName, attrType);
            element.Attributes.Add(attr);
            return attr;
        }

        public static void SetNodeDataTypeIfNull(PssgSchemaElement node, Type dataType)
        {
            if (node.DataType == typeof(Exception))
            {
                node.DataType = dataType;
            }
        }
        public static void SetAttributeDataTypeIfNull(PssgSchemaAttribute attribute, Type? attrType = null)
        {
            if (attrType is null)
            {
                return;
            }
            
            if (attribute.DataType == typeof(Exception))
            {
                attribute.DataType = attrType;
            }
        }
    }
}