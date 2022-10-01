namespace EgoEngineLibrary.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    public struct BinaryXmlElement
    {
        public int elementNameID;
        public int elementValueID;
        public int attributeCount;
        public int attributeStartID;
        public int childElementCount;
        public int childElementStartID;

        public XmlElement CreateElement(XmlDocument doc, BinaryXmlString strings, BinaryXmlElement[] elements,
            BinaryXmlAttribute[] attributes)
        {
            XmlElement element = doc.CreateElement(strings[elementNameID]);
            for (int i = attributeStartID; i < attributeStartID + attributeCount; i++)
            {
                element.Attributes.Append(attributes[i].CreateAttribute(doc, strings));
            }

            for (int i = childElementStartID; i < childElementStartID + childElementCount; i++)
            {
                element.AppendChild(elements[i].CreateElement(doc, strings, elements, attributes));
            }

            // Don't allow TextNode if the Element has ChildElements
            if (elementValueID > 0 && childElementCount == 0)
            {
                element.AppendChild(doc.CreateTextNode(strings[elementValueID]));
            }

            return element;
        }
    }
}
