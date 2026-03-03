using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using EgoEngineLibrary.Collections;
using EgoEngineLibrary.Conversion;

namespace EgoEngineLibrary.Graphics.Pssg
{
    public class PssgFile
    {
        private OrderedSet<PssgSchemaElement> _elementTable;
        private OrderedSet<PssgSchemaAttribute> _attributeTable;
        
        public PssgFileType FileType
        {
            get;
            set;
        }
        public PssgElement RootElement
        {
            get;
            set;
        }

        public PssgFile(PssgFileType fileType)
        {
            this.FileType = fileType;
            this.RootElement = new PssgElement("PSSGDATABASE", this, null);
            _elementTable = [];
            _attributeTable = [];
        }
        public static PssgFile Open(Stream stream)
        {
            PssgFileType fileType = PssgFile.GetPssgType(stream);

            if (fileType == PssgFileType.Pssg)
            {
                return PssgFile.ReadPssg(stream, fileType);
            }
            else if (fileType == PssgFileType.Xml)
            {
                return PssgFile.ReadXml(stream, fileType);
            }
            else // CompressedPssg
            {
                string tempPath = "temp.pssg";
                try
                {
                    using (var fs = File.Open(tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                    {
                        // Decompress stream into temp file
                        using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress, true))
                        {
                            gZipStream.CopyTo(fs);
                        }

                        // Determine the file type after inflate, add CompressedPssg to make compressed
                        fs.Seek(0, SeekOrigin.Begin);
                        fileType = GetPssgType(fs) + (int)PssgFileType.CompressedPssg;
                        PssgFile pFile = fileType switch
                        {
                            PssgFileType.CompressedPssg => PssgFile.ReadPssg(fs, fileType),
                            PssgFileType.CompressedXml => PssgFile.ReadXml(fs, fileType),
                            _ => throw new FileFormatException("This is not a pssg file.")
                        };

                        return pFile;
                    }
                }
                finally
                {
                    // Attempt to delete the temporary file
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch { }
                }
            }
        }
        private static PssgFileType GetPssgType(Stream stream)
        {
            Byte[] header = new Byte[4];
            stream.ReadExactly(header, 0, 4);

            string magic = Encoding.UTF8.GetString(header);

            if (magic == "PSSG")
            {
                stream.Seek(0, SeekOrigin.Begin);
                return PssgFileType.Pssg;
            }
            else if (magic.Contains("<"))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return PssgFileType.Xml;
            }
            else if (header[0] == 31 && header[1] == 139 && header[2] == 8 && header[3] == 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
                return PssgFileType.CompressedPssg;
            }
            else
            {
                throw new FileFormatException("This is not a pssg file!");
            }
        }
        public static PssgFile ReadPssg(Stream fileStream, PssgFileType fileType)
        {
            PssgFile file = new PssgFile(fileType);

            using (PssgBinaryReader reader = new PssgBinaryReader(EndianBitConverter.Big, fileStream, true))
            {
                reader.ReadPSSGString(4); // "PSSG"
                int size = reader.ReadInt32();

                PssgSchema.LoadFromPssg(reader);
                long positionAfterInfo = reader.BaseStream.Position;

                file.RootElement = new PssgElement(reader, file, null, true);
                if (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    reader.BaseStream.Position = positionAfterInfo;
                    file.RootElement = new PssgElement(reader, file, null, false);
                    if (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        throw new Exception("This file is improperly saved and not supported by this version of the PSSG editor." + Environment.NewLine + Environment.NewLine +
                            "Get an older version of the program if you wish to take out its contents, but put it back together using this program and the original version of the pssg file.");
                    }
                }

                file._elementTable = reader.ElementTable;
                file._attributeTable = reader.AttributeTable;
            }

            return file;
        }
        public static PssgFile ReadXml(Stream fileStream, PssgFileType fileType)
        {
            PssgFile file = new PssgFile(fileType);
            XDocument xDoc = XDocument.Load(fileStream);

            var docElem = xDoc.FirstNode as XElement ??
                throw new InvalidDataException("The pssg xml does not have a root element.");

            var firstNode = docElem.FirstNode as XElement ??
                throw new InvalidDataException("The pssg xml does not have an element within the root element.");

            file.RootElement = new PssgElement(firstNode, file, null);

            return file;
        }

        public void Save(Stream stream)
        {
            if (this.FileType == PssgFileType.Pssg)
            {
                WritePssg(stream);
            }
            else if (this.FileType == PssgFileType.Xml)
            {
                WriteXml(stream);
            }
            else // CompressedPssg
            {
                string tempPath = "temp.pssg";
                try
                {
                    using (FileStream fs = File.Open(tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                    {
                        switch (FileType)
                        {
                            case PssgFileType.CompressedXml: { WriteXml(fs); break; }
                            default: { WritePssg(fs); break; }
                        }
                        using (GZipStream gzip = new GZipStream(stream, CompressionMode.Compress, true))
                        {
                            fs.Seek(0, SeekOrigin.Begin);
                            fs.CopyTo(gzip);
                        }
                    }
                }
                finally
                {
                    // Attempt to delete the temporary file
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch { }
                }
            }
        }
        public void WritePssg(Stream fileStream)
        {
            using (PssgBinaryWriter writer = new PssgBinaryWriter(EndianBitConverter.Big, fileStream, true))
            {
                writer.Write(Encoding.ASCII.GetBytes("PSSG"));
                writer.Write(0); // Length, filled in later

                if (RootElement != null)
                {
                    UpdateElementAndAttributeTables(this);

                    writer.Write(_attributeTable.Count);
                    writer.Write(_elementTable.Count);
                    writer.ElementTable = _elementTable;
                    writer.AttributeTable = _attributeTable;
                    PssgSchema.SaveToPssg(writer);

                    RootElement.UpdateSize();
                    RootElement.Write(writer);
                }

                writer.BaseStream.Position = 4;
                writer.Write((int)writer.BaseStream.Length - 8);
            }

            return;

            static void UpdateElementAndAttributeTables(PssgFile file)
            {
                foreach (var element in file.GetElements())
                {
                    file._elementTable.Add(element.SchemaElement);

                    foreach (PssgAttribute attr in element.Attributes)
                    {
                        file._attributeTable.Add(attr.SchemaAttribute);
                    }
                }
            }
        }
        public void WriteXml(Stream fileStream)
        {
            XDocument xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            xDoc.Add(new XElement("PSSGFILE", new XAttribute("version", "1.0.0.0")));
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false);
            settings.NewLineChars = "\n";
            settings.Indent = true;
            settings.IndentChars = "";
            settings.CloseOutput = false;

            XElement pssg = (XElement)xDoc.FirstNode!;
            RootElement.WriteXml(pssg);

            using (XmlWriter writer = XmlWriter.Create(fileStream, settings))
            {
                xDoc.Save(writer);
            }
        }

        /// <summary>
        /// Gets the file's element hierarchy as a flat sequence.
        /// </summary>
        public IEnumerable<PssgElement> GetElements()
        {
            if (RootElement is null)
            {
                return Enumerable.Empty<PssgElement>();
            }
            return RootElement.GetElements();
        }

        public IEnumerable<PssgElement> FindElements(string elementName)
        {
            return GetElements().FindElements(elementName);
        }
        public IEnumerable<PssgElement> FindElements(string elementName, string attributeName)
        {
            return GetElements().FindElements(elementName, attributeName);
        }
        public IEnumerable<PssgElement> FindElements<T>(string elementName, string attributeName, T attributeValue)
            where T : notnull
        {
            return GetElements().FindElements(elementName, attributeName, attributeValue);
        }

        public void MoveElement(PssgElement source, PssgElement target)
        {
            if (source.ParentElement == null) throw new InvalidOperationException("Cannot move root element.");
            if (target.IsDataElement) throw new InvalidOperationException("Cannot append a child element to a data element.");

            source.ParentElement.RemoveChild(source);
            target.AppendChild(source);
        }

        public PssgElement CloneElement(PssgElement elementToClone)
        {
            if (elementToClone.ParentElement == null) throw new InvalidOperationException("Cannot clone root element, or an element without a parent.");
            if (elementToClone.File != this)
                throw new InvalidOperationException("Cannot clone an element that doesn't belong to a file.");

            var cloned = new PssgElement(elementToClone);
            cloned.ParentElement?.AppendChild(cloned);
            return cloned;
        }
    }
}
