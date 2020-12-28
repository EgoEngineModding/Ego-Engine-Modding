namespace EgoEngineLibrary.Graphics
{
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    public enum PssgFileType
    {
        Pssg, Xml, CompressedPssg
    }

    public class PssgFile
    {
        public PssgFileType FileType
        {
            get;
            set;
        }
        public PssgNode RootNode
        {
            get;
            set;
        }

        public PssgFile(PssgFileType fileType)
        {
            this.FileType = fileType;
            this.RootNode = new PssgNode("PSSGDATABASE", this, null);
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
                return PssgFile.ReadXml(stream);
            }
            else // CompressedPssg
            {
                string tempPath = "temp.pssg";
                try
                {
                    FileStream fs = File.Open(tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

                    // Decompress stream into temp file
                    using (stream)
                    {
                        using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            gZipStream.CopyTo(fs);
                        }
                    }

                    // Read temp file, and close it
                    fs.Seek(0, SeekOrigin.Begin);
                    PssgFile pFile = PssgFile.ReadPssg(fs, fileType);

                    return pFile;
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
            stream.Read(header, 0, 4);

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
                throw new Exception("This is not a PSSG file!");
            }
        }
        public static PssgFile ReadPssg(Stream fileStream, PssgFileType fileType)
        {
            PssgFile file = new PssgFile(fileType);

            using (PssgBinaryReader reader = new PssgBinaryReader(new BigEndianBitConverter(), fileStream))
            {
                reader.ReadPSSGString(4); // "PSSG"
                int size = reader.ReadInt32();

                // Load all the pssg node/attribute names
                PssgSchema.ClearSchemaIds();
                PssgSchema.LoadFromPssg(reader);
                long positionAfterInfo = reader.BaseStream.Position;

                file.RootNode = new PssgNode(reader, file, null, true);
                if (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    reader.BaseStream.Position = positionAfterInfo;
                    file.RootNode = new PssgNode(reader, file, null, false);
                    if (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        throw new Exception("This file is improperly saved and not supported by this version of the PSSG editor." + Environment.NewLine + Environment.NewLine +
                            "Get an older version of the program if you wish to take out its contents, but put it back together using this program and the original version of the pssg file.");
                    }
                }
            }

            return file;
        }
        public static PssgFile ReadXml(Stream fileStream)
        {
            PssgFile file = new PssgFile(PssgFileType.Xml);
            XDocument xDoc = XDocument.Load(fileStream);

            //PssgSchema.CreatePssgInfo(out file.nodeInfo, out file.attributeInfo);

            var docElem = xDoc.FirstNode as XElement ??
                throw new InvalidDataException("The pssg xml does not have a root element.");

            var firstNode = docElem.FirstNode as XElement ??
                throw new InvalidDataException("The pssg xml does not have an element within the root element.");

            file.RootNode = new PssgNode(firstNode, file, null);

            fileStream.Close();
            return file;
        }

        public void Save(Stream stream)
        {
            if (this.FileType == PssgFileType.Pssg)
            {
                this.WritePssg(stream, true);
            }
            else if (this.FileType == PssgFileType.Xml)
            {
                this.WriteXml(stream);
            }
            else // CompressedPssg
            {
                string tempPath = "temp.pssg";
                try
                {
                    using (FileStream fs = File.Open(tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                    {
                        this.WritePssg(fs, false);
                        using (GZipStream gzip = new GZipStream(stream, CompressionMode.Compress))
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
        public void WritePssg(Stream fileStream, bool close)
        {
            PssgBinaryWriter writer = new PssgBinaryWriter(new BigEndianBitConverter(), fileStream);
            try
            {
                writer.Write(Encoding.ASCII.GetBytes("PSSG"));
                writer.Write(0); // Length, filled in later

                if (RootNode != null)
                {
                    int nodeNameCount = 0;
                    int attributeNameCount = 0;
                    PssgSchema.ClearSchemaIds(); // make all ids -1
                    RootNode.UpdateId(ref nodeNameCount, ref attributeNameCount);
                    writer.Write(attributeNameCount);
                    writer.Write(nodeNameCount);
                    PssgSchema.SaveToPssg(writer); // Update Ids again, to make sequential

                    RootNode.UpdateSize();
                    RootNode.Write(writer);
                }
                writer.BaseStream.Position = 4;
                writer.Write((int)writer.BaseStream.Length - 8);

                if (close)
                {
                    writer.Close();
                }
            }
            catch
            {
                if (writer != null)
                {
                    writer.Close();
                }
                throw;
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
            settings.CloseOutput = true;

            XElement pssg = (XElement)xDoc.FirstNode!;
            RootNode.WriteXml(pssg);

            using (XmlWriter writer = XmlWriter.Create(fileStream, settings))
            {
                xDoc.Save(writer);
            }
        }

        /// <summary>
        /// Gets the file's node hierarchy as a flat sequence.
        /// </summary>
        /// <returns>the flat node hierarchy.</returns>
        public IEnumerable<PssgNode> GetNodes()
        {
            if (RootNode is null)
            {
                return Enumerable.Empty<PssgNode>();
            }
            return RootNode.GetNodes();
        }

        public IEnumerable<PssgNode> FindNodes(string nodeName)
        {
            return GetNodes().FindNodes(nodeName);
        }
        public IEnumerable<PssgNode> FindNodes(string nodeName, string attributeName)
        {
            return GetNodes().FindNodes(nodeName, attributeName);
        }
        public IEnumerable<PssgNode> FindNodes<T>(string nodeName, string attributeName, T attributeValue)
            where T : notnull
        {
            return GetNodes().FindNodes(nodeName, attributeName, attributeValue);
        }

        public void MoveNode(PssgNode source, PssgNode target)
        {
            if (source.ParentNode == null) throw new InvalidOperationException("Cannot move root node");
            if (target.IsDataNode) throw new InvalidOperationException("Cannot append a child node to a data node");

            source.ParentNode.RemoveChild(source);
            target.AppendChild(source);
        }

        public PssgNode CloneNode(PssgNode nodeToClone)
        {
            if (nodeToClone.ParentNode == null) throw new InvalidOperationException("Cannot clone root node, or a node without a parent.");
            if (nodeToClone.File != this)
                throw new InvalidOperationException("Cannot clone a node that doesn't belong to a file");

            var clonedNode = new PssgNode(nodeToClone);
            clonedNode.ParentNode?.AppendChild(clonedNode);
            return clonedNode;
        }
    }
}
