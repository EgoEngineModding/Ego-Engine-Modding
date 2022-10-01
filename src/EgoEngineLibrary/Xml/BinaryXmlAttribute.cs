using System.Xml;

namespace EgoEngineLibrary.Xml;

public struct BinaryXmlAttribute
{
    public int nameID;
    public int valueID;

    public XmlAttribute CreateAttribute(XmlDocument doc, BinaryXmlString strings)
    {
        var attr = doc.CreateAttribute(strings[nameID]);
        attr.Value = strings[valueID];
        return attr;
    }
}