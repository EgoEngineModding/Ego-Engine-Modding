using System.Xml.Linq;
using EgoEngineLibrary.Graphics.Pssg.Elements;

namespace EgoEngineLibrary.Graphics.Pssg
{
    public static class PssgSchema
    {
        private static Dictionary<string, PssgSchemaElement> entries = new();

        static PssgSchema()
        {
            ResetSchema();
        }

        public static void LoadSchema(Stream stream)
        {
            if (stream.Length == 0)
            {
                return;
            }

            XDocument xDoc = XDocument.Load(stream);
            foreach (XElement xN in xDoc.Descendants("node"))
            {
                string elementName = xN.Attribute("name")?.Value ??
                                  throw new InvalidDataException(
                                      $"The schema element {xN.Name} does not have attribute name.");
                string nType = xN.Attribute("dataType")?.Value ??
                               throw new InvalidDataException(
                                   $"The schema element {xN.Name} does not have attribute dataType.");

                PssgSchemaElement element = AddElement(elementName);
                var elementType = Enum.Parse<PssgElementType>(nType);
                if (elementType != PssgElementType.Unknown)
                {
                    element.DataType = elementType;
                }
                element.ElementsPerRow = Convert.ToInt32(xN.Attribute("elementsPerRow")?.Value ??
                                                      throw new InvalidDataException($"The schema element {xN.Name} does not have attribute elementsPerRow."));
                string linkAttributeName = xN.Attribute("linkAttributeName")?.Value ??
                                           throw new InvalidDataException($"The schema element {xN.Name} does not have attribute linkAttributeName.");
                if (!string.IsNullOrEmpty(linkAttributeName))
                {
                    element.LinkAttributeName = linkAttributeName;
                }

                foreach (XElement attrElem in xN.Descendants("attribute"))
                {
                    string attrName = attrElem.Attribute("name")?.Value ??
                                      throw new InvalidDataException(
                                          $"The schema element {attrElem.Name} does not have attribute name.");
                    PssgAttributeType attrType = Enum.Parse<PssgAttributeType>(
                        attrElem.Attribute("dataType")?.Value ??
                        throw new InvalidDataException(
                            $"The schema element {attrElem.Name} does not have attribute dataType."));
                    PssgSchemaAttribute attr = AddAttribute(element, attrName);
                    if (attrType != PssgAttributeType.Unknown)
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
                pNode.Add(new XAttribute("name", entry.Key), new XAttribute("dataType", entry.Value.DataType.ToString()));
                pNode.Add(new XAttribute("elementsPerRow", entry.Value.ElementsPerRow), new XAttribute("linkAttributeName", entry.Value.LinkAttributeName));

                foreach (PssgSchemaAttribute attrEntry in entry.Value.Attributes)
                {
                    pNode.Add(new XElement("attribute", new XAttribute("name", attrEntry.Name), new XAttribute("dataType", attrEntry.DataType.ToString())));
                }

                parent.Add(pNode);
            }

            xDoc.Save(stream);
        }

        public static void ResetSchema()
        {
            entries.Clear();
            AddElement(PssgObject.Schema);
            AddElement(PssgUserData.Schema);
            AddElement(PssgDatabase.Schema);
            AddElement(PssgLibrary.Schema);

            AddElement(PssgNode.Schema);
            AddElement(PssgTransform.Schema);
            AddElement(PssgBoundingBox.Schema);
            AddElement(PssgRootNode.Schema);

            AddElement(PssgRenderInterfaceBound.Schema);
            AddElement(PssgDataBlock.Schema);
            AddElement(PssgDataBlockStream.Schema);
            AddElement(PssgDataBlockData.Schema);
        }

        public static void LoadFromPssg(PssgBinaryReader reader)
        {
            int attributeCount = reader.ReadInt32();
            int elementCount = reader.ReadInt32();

            var elementTable = new PssgSchemaElement[elementCount];
            var attributeTable = new PssgSchemaAttribute[attributeCount];
            for (int i = 0; i < elementCount; i++)
            {
                int nId = reader.ReadInt32();
                var elementName = reader.ReadPSSGString();

                if (!entries.TryGetValue(elementName, out PssgSchemaElement? element))
                {
                    element = PssgSchema.AddElement(new PssgSchemaElement(elementName));
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

        public static string[] GetElementNames()
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

        public static PssgSchemaElement AddElement(string elementName)
        {
            if (entries.TryGetValue(elementName, out PssgSchemaElement? existingElement))
            {
                return existingElement;
            }

            PssgSchemaElement element = new(elementName);
            entries.Add(element.Name, element);
            return element;
        }

        public static PssgSchemaElement AddElement(PssgSchemaElement element)
        {
            if (!entries.TryGetValue(element.Name, out var existingElement))
            {
                entries.Add(element.Name, element);
                return element;
            }

            existingElement = entries[element.Name];
            if (ReferenceEquals(existingElement, element))
            {
                return existingElement;
            }
                
            SetElementDataTypeIfNull(existingElement, element.DataType);
            foreach (PssgSchemaAttribute attrEntry in element.Attributes)
            {
                AddAttribute(existingElement, attrEntry.Name, attrEntry.DataType);
            }

            return existingElement;
        }

        public static PssgSchemaAttribute AddAttribute(string elementName, string attributeName,
            PssgAttributeType attrType = PssgAttributeType.Unknown)
        {
            PssgSchemaElement element = AddElement(elementName);
            return AddAttribute(element, attributeName, attrType);
        }

        public static PssgSchemaAttribute AddAttribute(PssgSchemaElement element, string attributeName,
            PssgAttributeType attrType = PssgAttributeType.Unknown)
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

        private static void SetElementDataTypeIfNull(PssgSchemaElement element, PssgElementType dataType)
        {
            if (element.DataType is PssgElementType.Unknown)
            {
                element.DataType = dataType;
            }
        }

        private static void SetAttributeDataTypeIfNull(PssgSchemaAttribute attribute, PssgAttributeType attrType)
        {
            if (attribute.DataType is PssgAttributeType.Unknown)
            {
                attribute.DataType = attrType;
            }
        }
    }
}