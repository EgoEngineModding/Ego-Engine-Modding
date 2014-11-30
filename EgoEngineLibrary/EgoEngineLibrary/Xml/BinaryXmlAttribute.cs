namespace EgoEngineLibrary.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    public struct BinaryXmlAttribute
    {
        public int nameID;
        public int valueID;

        public XmlAttribute CreateAttribute(XmlDocument doc, XmlFile file)
        {
            XmlAttribute attr = doc.CreateAttribute(file.xmlStrings[nameID]);
            attr.Value = file.xmlStrings[valueID];
            return attr;
        }
    }
}
