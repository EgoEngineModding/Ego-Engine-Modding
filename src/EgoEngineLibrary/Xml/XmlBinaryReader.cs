namespace EgoEngineLibrary.Xml
{
    using MiscUtil.Conversion;
    using MiscUtil.IO;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    public class XmlBinaryReader : EndianBinaryReader
    {
        public XmlBinaryReader(EndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream)
        {
        }

        public string ReadTerminatedString(byte terminator = new byte())
        {
            List<byte> strBytes = new List<byte>();
            do
            {
                strBytes.Add(ReadByte());
            } while (strBytes[strBytes.Count - 1] != terminator);
            strBytes.RemoveAt(strBytes.Count - 1);
            return Encoding.UTF8.GetString(strBytes.ToArray());
        }

        public XmlNode? ReadBxmlElement(XmlDocument doc)
        {
            int nodeLength = this.ReadInt16();
            int nodeType = this.ReadInt16(); // pad
            int attribCount = this.ReadInt16();

            if (nodeType == 0)
            {
                // Read Element
                XmlElement element = doc.CreateElement(this.ReadTerminatedString());
                for (int i = 0; i < attribCount; i++)
                {
                    XmlAttribute attr = doc.CreateAttribute(this.ReadTerminatedString());
                    attr.Value = this.ReadTerminatedString();
                    element.Attributes.Append(attr);
                }

                // Keep Reading Child Elements until the current element ends
                XmlNode? childNode;
                while ((childNode = ReadBxmlElement(doc)) != null)
                {
                    element.AppendChild(childNode);
                }
                return element;
            }
            else if (nodeType == 1)
            {
                return doc.CreateTextNode(ReadTerminatedString());
            }
            else if (nodeType == 5)
            {
                return null;
            }
            else
            {
                throw new NotImplementedException($"Converting xml node of type {nodeType} is not supported.");
            }
        }
    }
}
