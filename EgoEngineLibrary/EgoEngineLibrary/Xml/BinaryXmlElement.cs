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

        public XmlElement CreateElement(XmlDocument doc, XmlFile file)
        {
            XmlElement element = doc.CreateElement(file.xmlStrings[elementNameID]);
            for (int i = attributeStartID; i < attributeStartID + attributeCount; i++)
            {
                element.Attributes.Append(file.xmlAttributes[i].CreateAttribute(doc, file));
            }
            for (int i = childElementStartID; i < childElementStartID + childElementCount; i++)
            {
                element.AppendChild(file.xmlElements[i].CreateElement(doc, file));
            }
            // Don't allow TextNode if the Element has ChildElements
            if (elementValueID > 0 && childElementCount == 0)
            {
                element.AppendChild(doc.CreateTextNode(file.xmlStrings[elementValueID]));
            }
            return element;
        }
    }
}
