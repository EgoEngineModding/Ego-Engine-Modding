namespace EgoEngineLibrary.Xml
{
    using MiscUtil.Conversion;
    using MiscUtil.IO;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    public class XmlBinaryWriter : EndianBinaryWriter
    {
        public XmlBinaryWriter(EndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream)
        {
        }

        public int WriteTerminatedString(string s, byte terminator = new byte())
        {
            byte[] sBytes = Encoding.UTF8.GetBytes(s);
            this.Write(sBytes);
            this.Write(terminator);
            return sBytes.Length;
        }

        public void WriteBxmlElement(XmlElement elem)
        {
            int elemLength = 4;

            // Element Length, Place Holder for Now
            this.Write((Int16)0);
            // Pad/Special Case byte for signaling end of element or file
            this.Write((Int16)0);
            // Attribute Count
            this.Write((Int16)elem.Attributes.Count);

            elemLength += this.WriteTerminatedString(elem.Name) + 1;

            foreach (XmlAttribute attr in elem.Attributes)
            {
                elemLength += this.WriteTerminatedString(attr.Name) + 1;
                elemLength += this.WriteTerminatedString(attr.Value) + 1;
            }

            // Go back, Update Element Length, and Come Back to Continue Writing
            this.Seek(-2 - elemLength, System.IO.SeekOrigin.Current);
            this.Write((Int16)elemLength);
            this.Seek(elemLength, System.IO.SeekOrigin.Current);

            foreach (XmlNode childNode in elem.ChildNodes)
            {
                if (childNode is XmlText)
                {
                    elemLength = 4;

                    // Element Length, Place Holder for Now
                    this.Write((Int16)0);
                    // Pad/Special Case byte for signaling end of element or file
                    this.Write((Int16)1);
                    // Attribute Count
                    this.Write((Int16)0);

                    elemLength += this.WriteTerminatedString(childNode.Value) + 1;

                    // Go back, Update Element Length, and Come Back to Continue Writing
                    this.Seek(-2 - elemLength, System.IO.SeekOrigin.Current);
                    this.Write((Int16)elemLength);
                    this.Seek(elemLength, System.IO.SeekOrigin.Current);
                }
                else if (childNode is XmlElement)
                {
                    WriteBxmlElement((XmlElement)childNode);
                }
            }

            // Closing Tag of the Element (</XX>) 00BXML: "0004 05000000" 01BXML: "0400 05000000"
            this.Write((Int16)0x0004);
            if (this.BitConverter.Endianness == Endianness.BigEndian)
            {
                this.Write((Int16)0x0500);
            }
            else
            {
                this.Write((Int16)0x0005);
            }
            this.Write((Int16)0);
        }

        public void WriteBinaryXmlElement(BinaryXmlElement binElem)
        {
            this.Write(binElem.elementNameID);
            this.Write(binElem.elementValueID);
            this.Write(binElem.attributeCount);
            this.Write(binElem.attributeStartID);
            this.Write(binElem.childElementCount);
            this.Write(binElem.childElementStartID);
        }
        public void WriteBinaryXmlAttribute(BinaryXmlAttribute binAttr)
        {
            this.Write(binAttr.nameID);
            this.Write(binAttr.valueID);
        }
    }
}
