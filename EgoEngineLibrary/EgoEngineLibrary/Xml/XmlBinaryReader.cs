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

        public XmlElement ReadBxmlElement(XmlDocument doc)
        {
            int elemLength = this.ReadInt16();
            this.ReadBytes(2); // pad
            int attribCount = this.ReadInt16();
            if (elemLength == 4)
            {
                return null;
            }
            long endLength = this.BaseStream.Position + elemLength - 4;
            // Read Element
            XmlElement element = doc.CreateElement(this.ReadTerminatedString());
            for (int i = 0; i < attribCount; i++)
            {
                XmlAttribute attr = doc.CreateAttribute(this.ReadTerminatedString());
                attr.Value = this.ReadTerminatedString();
                element.Attributes.Append(attr);
            }
            // Keep Reading Child Elements until the current element ends
            XmlElement nextElement = ReadBxmlElement(doc);
            while (nextElement != null)
            {
                element.AppendChild(nextElement);
                nextElement = ReadBxmlElement(doc);
            }
            return element;
        }
    }
}
