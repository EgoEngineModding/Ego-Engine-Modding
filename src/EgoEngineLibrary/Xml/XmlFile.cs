using EgoEngineLibrary.IO;
using MiscUtil.Conversion;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace EgoEngineLibrary.Xml
{
    public class XmlFile
    {
        private static ReadOnlySpan<byte> WhitespaceChars => new byte[] { 0x9, 0xA, 0xB, 0xC, 0xD, 0x20, 0x85, 0xA0 };
        private static ReadOnlySpan<byte> BinXmlMagic => new byte[] { 0x1A, 0x22, 0x52, 0x72 }; // ASCII ."Rr
        private static ReadOnlySpan<byte> BxmlMagic => new byte[] { 0x42, 0x58, 0x4D, 0x4C }; // ASCII BXML

        public XmlType Type { get; }
        
        public XmlDocument Document { get; }

        /// <summary>
        /// Gets the binary xml type represented by the stream, otherwise assumes it is text xml.
        /// </summary>
        /// <param name="stream">A seekable, readable stream.</param>
        /// <returns></returns>
        public static XmlType GetXmlType(Stream stream)
        {
            var pos = stream.Position;
            Span<byte> header = stackalloc byte[5];
            stream.ReadExactly(header);
            try
            {
                var binXmlMagic = header[..4];
                var bxmlMagic = header[1..];
                return binXmlMagic.SequenceEqual(BinXmlMagic) ? XmlType.BinXml :
                    bxmlMagic.SequenceEqual(BxmlMagic) ? header[0] == 0 ? XmlType.BxmlBig : XmlType.BxmlLittle :
                    XmlType.Text;
            }
            finally
            {
                stream.Seek(pos, SeekOrigin.Begin);
            }
        }

        public static bool IsXmlFile(Stream stream)
        {
            var pos = stream.CanSeek ? stream.Position : 0;
            Span<byte> header = stackalloc byte[5];
            stream.ReadExactly(header);
            try
            {
                var binXmlMagic = header[..4];
                var bxmlMagic = header[1..];
                return binXmlMagic.SequenceEqual(BinXmlMagic)
                       || bxmlMagic.SequenceEqual(BxmlMagic)
                       || IsValidXmlText(header, stream, pos);
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Seek(pos, SeekOrigin.Begin);
                }
            }

            static bool IsValidXmlText(ReadOnlySpan<byte> buffer, Stream stream, long startPos)
            {
                // Check for UTF8 BOM
                if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                {
                    buffer = buffer[3..];
                }
                
                // Check for valid start tags
                if (buffer[0] == '<')
                {
                    if (buffer[1] == '?' || buffer[1] == '!')
                    {
                        return true;
                    }

                    // Check valid xml element name
                    var xmlNameBuffer = buffer[1..];
                    var charCount = Encoding.UTF8.GetCharCount(xmlNameBuffer);
                    if (charCount <= 0)
                    {
                        return false;
                    }

                    var chars = ArrayPool<char>.Shared.Rent(charCount);
                    try
                    {
                        Encoding.UTF8.GetChars(xmlNameBuffer, chars);
                        if (XmlConvert.IsStartNCNameChar(chars[0]))
                        {
                            return true;
                        }
                    }
                    finally
                    {
                        ArrayPool<char>.Shared.Return(chars);
                    }
                }

                // Last effort - see if we can read beginning with xml reader if starts with whitespace.
                var noWhitespaceBuffer = buffer.TrimStart(WhitespaceChars);
                if (noWhitespaceBuffer.Length == buffer.Length ||
                    (noWhitespaceBuffer.Length > 0 && noWhitespaceBuffer[0] != '<'))
                {
                    // We didn't find any starting whitespace chars, assume not xml
                    // or found whitespace chars, but no xml start tag
                    return false;
                }

                Stream finalStream;
                Stream? tempStream = null;
                if (stream.CanSeek)
                {
                    stream.Seek(startPos, SeekOrigin.Begin);
                    finalStream = stream;
                }
                else
                {
                    // Copy starting bytes into temporary stream
                    tempStream = new MemoryStream(noWhitespaceBuffer.ToArray());
                    finalStream = new ConcatenatedStream(new[] { tempStream, stream });
                }

                try
                {
                    var xmlReader = XmlReader.Create(finalStream);
                    do
                    {
                        try
                        {
                            if (!xmlReader.Read())
                            {
                                return false;
                            }
                        }
                        catch (XmlException)
                        {
                            return false;
                        }
                    } while (xmlReader.NodeType is XmlNodeType.Whitespace or XmlNodeType.SignificantWhitespace);
                }
                finally
                {
                    if (!stream.CanSeek)
                    {
                        tempStream?.Dispose();
                        finalStream.Dispose();
                    }
                }

                return true;
            }
        }

        public XmlFile(Stream fileStream)
        {
            Type = GetXmlType(fileStream);
            Document = new XmlDocument();

            if (Type == XmlType.Text)
            {
                Document.Load(fileStream);
                foreach (XmlNode child in Document.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Comment)
                    {
                        if (Enum.TryParse<XmlType>(child.Value ?? string.Empty, out var pt) && Enum.IsDefined(pt))
                        {
                            Type = pt;
                            break;
                        }
                    }
                }
            }
            else if (Type == XmlType.BinXml)
            {
                using var reader = new XmlBinaryReader(EndianBitConverter.Little, fileStream);
                
                // Section 1
                reader.ReadByte(); // Unique Byte
                reader.ReadBytes(3); // Same Magic
                reader.ReadInt32(); // File Length/Size

                // Section 2
                reader.ReadByte(); // Unique Byte
                reader.ReadBytes(3); // Same Magic
                reader.ReadInt32(); // Section 3 and 4 Total Length/Size

                // Section 3 and 4
                var xmlStrings = new BinaryXmlString();
                xmlStrings.Read(reader);

                // Section 5
                reader.ReadInt32();
                var xmlElements = new BinaryXmlElement[reader.ReadInt32() / 24];
                for (var i = 0; i < xmlElements.Length; i++)
                {
                    xmlElements[i].elementNameId = reader.ReadInt32();
                    xmlElements[i].elementValueId = reader.ReadInt32();
                    xmlElements[i].attributeCount = reader.ReadInt32();
                    xmlElements[i].attributeStartId = reader.ReadInt32();
                    xmlElements[i].childElementCount = reader.ReadInt32();
                    xmlElements[i].childElementStartId = reader.ReadInt32();
                }

                // Section 6
                reader.ReadInt32();
                var xmlAttributes = new BinaryXmlAttribute[reader.ReadInt32() / 8];
                for (var i = 0; i < xmlAttributes.Length; i++)
                {
                    xmlAttributes[i].nameID = reader.ReadInt32();
                    xmlAttributes[i].valueID = reader.ReadInt32();
                }

                // Build XML
                Document = new XmlDocument();
                Document.AppendChild(Document.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
                Document.AppendChild(Document.CreateComment(Type.ToString()));
                Document.AppendChild(xmlElements[0].CreateElement(Document, xmlStrings, xmlElements, xmlAttributes));
            }
            else
            {
                if (Type == XmlType.BxmlBig)
                {
                    using var reader = new XmlBinaryReader(EndianBitConverter.Big, fileStream);
                    reader.ReadBytes(5);
                    Document = new XmlDocument();
                    Document.AppendChild(Document.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
                    Document.AppendChild(Document.CreateComment(Type.ToString()));
                    var rootElem = reader.ReadBxmlElement(Document);
                    if (rootElem is not null)
                        Document.AppendChild(rootElem);
                }
                else if (Type == XmlType.BxmlLittle)
                {
                    using var reader = new XmlBinaryReader(EndianBitConverter.Little, fileStream);
                    reader.ReadBytes(5);
                    Document = new XmlDocument();
                    Document.AppendChild(Document.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
                    Document.AppendChild(Document.CreateComment(Type.ToString()));
                    var rootElem = reader.ReadBxmlElement(Document);
                    if (rootElem is not null)
                        Document.AppendChild(rootElem);
                }
            }
        }

        public void WriteXml(TextWriter textWriter)
        {
            Document.Save(textWriter);
        }

        public void Write(Stream stream)
        {
            Write(stream, Type);
        }

        public void Write(Stream fileStream, XmlType convertType)
        {
            if (convertType == XmlType.Text)
            {
                // use text writer since by default it doesn't output the Encoding BOM
                using var textWriter = new StreamWriter(fileStream, leaveOpen: true);
                var xmlTextWriter = new XmlTextWriter(textWriter);
                Document.Save(xmlTextWriter);
            }
            else if (convertType == XmlType.BinXml)
            {
                var valuesToId = new Dictionary<string, int>();
                var xmlElems = new List<BinaryXmlElement>();
                var xmlAttrs = new List<BinaryXmlAttribute>();
                var valueLocations = new List<int>();

                if (Document.DocumentElement is null)
                    throw new InvalidDataException("The root xml element does not exist.");

                BuildBinXml(Document.DocumentElement, valuesToId, xmlElems, xmlAttrs);
                valueLocations.Add(0);

                using (var writer = new XmlBinaryWriter(EndianBitConverter.Little, fileStream))
                {
                    writer.Write(0x7252221A);
                    writer.Write(0);

                    // Section 2: Sections 3 and 4 Header and Total Size
                    writer.Write(0x72522217);
                    writer.Write(0); // 16 as Extra, Rest from Section 3 and 4 Length

                    // Section 3: Values/Strings
                    writer.Write(0x7252221D);
                    writer.Write(0);
                    foreach (var s in valuesToId.Keys)
                    {
                        valueLocations.Add(writer.WriteTerminatedString(s) + valueLocations[^1] + 1);
                    }
                    // Write zero bytes to make length the same way CM made it
                    var remainder = (int)writer.BaseStream.Position % 16;
                    var pad = new byte[remainder > 8 ? 24 - remainder : 8 - remainder];
                    writer.Write(pad);

                    // Section 4: Value/String Locations
                    writer.Write(0x7252221E);
                    writer.Write(4 * valuesToId.Count);
                    for (var i = 0; i < valueLocations.Count - 1; i++)
                    {
                        writer.Write(valueLocations[i]);
                    }

                    // Section 5: Element Definitions
                    writer.Write(0x7252221B);
                    writer.Write(24 * xmlElems.Count);
                    for (var i = 0; i < xmlElems.Count; i++)
                    {
                        writer.WriteBinaryXmlElement(xmlElems[i]);
                    }

                    // Section 6: Attribute Definitions
                    writer.Write(0x7252221C);
                    writer.Write(8 * xmlAttrs.Count);
                    for (var i = 0; i < xmlAttrs.Count; i++)
                    {
                        writer.WriteBinaryXmlAttribute(xmlAttrs[i]);
                    }

                    // Update Section Lengths/Sizes
                    writer.Seek(4, SeekOrigin.Begin);
                    writer.Write((int)writer.BaseStream.Length - 8); // Section 1: Total File

                    writer.Seek(12, SeekOrigin.Begin);
                    writer.Write(valueLocations[^1] + 4 * valuesToId.Count + 16 + pad.Length); // Section 2

                    writer.Seek(20, SeekOrigin.Begin);
                    writer.Write(valueLocations[^1] + pad.Length); // Section 3
                }
            }
            else if (convertType == XmlType.BxmlBig)
            {
                using var writer = new XmlBinaryWriter(EndianBitConverter.Big, fileStream);
                writer.Write((byte)0);
                writer.Write(BxmlMagic);

                if (Document.DocumentElement is null)
                    throw new InvalidDataException("The root xml element does not exist.");

                writer.WriteBxmlElement(Document.DocumentElement);

                // File Ending: "0004 06000000" x2
                writer.Write((short)0x0004);
                writer.Write((short)0x0600);
                writer.Write((short)0);
                writer.Write((short)0x0004);
                writer.Write((short)0x0600);
                writer.Write((short)0);
            }
            else if (convertType == XmlType.BxmlLittle)
            {
                using var writer = new XmlBinaryWriter(EndianBitConverter.Little, fileStream);
                writer.Write((byte)1);
                writer.Write(BxmlMagic);

                if (Document.DocumentElement is null)
                    throw new InvalidDataException("The root xml element does not exist.");

                writer.WriteBxmlElement(Document.DocumentElement);

                // File Ending: "0004 06000000" x2
                writer.Write((short)0x0004);
                writer.Write((short)0x0006);
                writer.Write((short)0);
                writer.Write((short)0x0004);
                writer.Write((short)0x0006);
                writer.Write((short)0);
            }
        }

        private static void BuildBinXml(XmlElement elem, Dictionary<string, int> valuesToId,
            List<BinaryXmlElement> xmlElems, List<BinaryXmlAttribute> xmlAttrs, int childElemIndex = 0)
        {
            if (childElemIndex == 0)
            {
                xmlElems.Add(new BinaryXmlElement());
            }

            var binElem = new BinaryXmlElement();
            var currentIndex = xmlElems.Count - 1;

            // Element Name String
            if (!valuesToId.ContainsKey(elem.Name))
            {
                binElem.elementNameId = valuesToId.Count;
                valuesToId.Add(elem.Name, valuesToId.Count);
            }
            else
            {
                binElem.elementNameId = valuesToId[elem.Name];
            }

            binElem.attributeStartId = elem.Attributes.Count > 0 ? xmlAttrs.Count : 0;
            binElem.attributeCount = elem.Attributes.Count;
            foreach (XmlAttribute attr in elem.Attributes)
            {
                var binAttr = new BinaryXmlAttribute();

                // Attribute Name String
                if (!valuesToId.ContainsKey(attr.Name))
                {
                    binAttr.nameID = valuesToId.Count;
                    valuesToId.Add(attr.Name, valuesToId.Count);
                }
                else
                {
                    binAttr.nameID = valuesToId[attr.Name];
                }

                // Attribute Value String
                if (!valuesToId.ContainsKey(attr.Value))
                {
                    binAttr.valueID = valuesToId.Count;
                    valuesToId.Add(attr.Value, valuesToId.Count);
                }
                else
                {
                    binAttr.valueID = valuesToId[attr.Value];
                }

                xmlAttrs.Add(binAttr);
            }

            // Element Value or Child Elements
            // Set its childElementStartID to the next available index for binary xml elements 
            // AKA the current count, will be overwritten if element actually has child nodes
            binElem.childElementStartId = xmlElems.Count;
            if (elem.HasChildNodes)
            {
                // Values inside of an element are considered child nodes like <element>ThisValue</element>
                // More Specifically they're called "XmlText" or Text Nodes
                if (elem.ChildNodes[0] is XmlText textNode)
                {
                    if (textNode.Value != null)
                    {
                        if (!valuesToId.ContainsKey(textNode.Value))
                        {
                            binElem.elementValueId = valuesToId.Count;
                            valuesToId.Add(textNode.Value, valuesToId.Count);
                        }
                        else
                        {
                            binElem.elementValueId = valuesToId[textNode.Value];
                        }
                    }
                }
                else
                {
                    binElem.childElementStartId = xmlElems.Count;
                    binElem.childElementCount = elem.ChildNodes.Count;

                    xmlElems.AddRange(new BinaryXmlElement[elem.ChildNodes.Count]);
                    var i = binElem.childElementStartId;
                    foreach (XmlElement childElem in elem.ChildNodes)
                    {
                        BuildBinXml(childElem, valuesToId, xmlElems, xmlAttrs, i);
                        i++;
                    }
                }
            }

            // Update the xmlElems Entry
            if (childElemIndex == 0)
            {
                xmlElems[currentIndex] = binElem;
            }
            else
            {
                xmlElems[childElemIndex] = binElem;
            }
        }
    }
}
