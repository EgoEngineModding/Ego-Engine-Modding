using System.Xml;

namespace EgoEngineLibrary.Xml;

public struct BinaryXmlElement
{
    public int elementNameId;
    public int elementValueId;
    public int attributeCount;
    public int attributeStartId;
    public int childElementCount;
    public int childElementStartId;

    public XmlElement CreateElement(XmlDocument doc, BinaryXmlString strings, BinaryXmlElement[] elements,
        BinaryXmlAttribute[] attributes)
    {
        var element = doc.CreateElement(strings[elementNameId]);
        for (var i = attributeStartId; i < attributeStartId + attributeCount; i++)
        {
            element.Attributes.Append(attributes[i].CreateAttribute(doc, strings));
        }

        for (var i = childElementStartId; i < childElementStartId + childElementCount; i++)
        {
            element.AppendChild(elements[i].CreateElement(doc, strings, elements, attributes));
        }

        // Don't allow TextNode if the Element has ChildElements
        if (elementValueId > 0 && childElementCount == 0)
        {
            element.AppendChild(doc.CreateTextNode(strings[elementValueId]));
        }

        return element;
    }
}