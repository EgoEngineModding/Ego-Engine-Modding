namespace EgoEngineLibrary.Graphics
{
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
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
        public PssgNode rootNode;

        public PssgFile(PssgFileType fileType)
        {
            this.FileType = fileType;
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
                using (stream)
                {
                    MemoryStream mStream = new MemoryStream();

                    using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        gZipStream.CopyTo(mStream);
                    }

                    mStream.Seek(0, SeekOrigin.Begin);
                    return PssgFile.ReadPssg(mStream, fileType);
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

                file.rootNode = new PssgNode(reader, file, null, true);
                if (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    reader.BaseStream.Position = positionAfterInfo;
                    file.rootNode = new PssgNode(reader, file, null, false);
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

            file.rootNode = new PssgNode((XElement)((XElement)xDoc.FirstNode).FirstNode, file, null);

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
                using (MemoryStream memory = new MemoryStream())
                {
                    this.WritePssg(memory, false);
                    using (GZipStream gzip = new GZipStream(stream, CompressionMode.Compress))
                    {
                        gzip.Write(memory.ToArray(), 0, (int)memory.Length);
                    }
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

                if (rootNode != null)
                {
                    int nodeNameCount = 0;
                    int attributeNameCount = 0;
                    PssgSchema.ClearSchemaIds(); // make all ids -1
                    rootNode.UpdateId(ref nodeNameCount, ref attributeNameCount);
                    writer.Write(attributeNameCount);
                    writer.Write(nodeNameCount);
                    PssgSchema.SaveToPssg(writer); // Update Ids again, to make sequential

                    rootNode.UpdateSize();
                    rootNode.Write(writer);
                }
                writer.BaseStream.Position = 4;
                writer.Write((int)writer.BaseStream.Length - 8);

                if (close)
                {
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                if (writer != null)
                {
                    writer.Close();
                }
                throw ex;
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

            XElement pssg = (XElement)xDoc.FirstNode;
            rootNode.WriteXml(pssg);

            using (XmlWriter writer = XmlWriter.Create(fileStream, settings))
            {
                xDoc.Save(writer);
            }
        }
        public void WriteAsModel(Stream fileStream)
        {
            XmlDocument pssg = new XmlDocument();
            pssg.AppendChild(pssg.CreateXmlDeclaration("1.0", "utf-8", string.Empty));
            pssg.AppendChild(pssg.CreateElement("COLLADA", "http://www.collada.org/2008/03/COLLADASchema"));
            pssg.DocumentElement.AppendChild(pssg.CreateAttribute("version"));
            pssg.DocumentElement.Attributes["version"].InnerText = "1.5.0";

            if (rootNode.HasAttributes)
            {
                XmlElement asset = pssg.CreateElement("asset");
                if (rootNode.HasAttribute("creator"))
                {
                    asset.AppendChild(pssg.CreateElement("contributor"));
                    asset.LastChild.AppendChild(pssg.CreateElement("author"));
                    asset.LastChild.LastChild.InnerText = rootNode.GetAttribute("creator").ToString();
                }
                // TODO: unit meter 1, created, up axis, scale?, creatorMachine


            }
        }

        public TreeNode CreateTreeViewNode(PssgNode node)
        {
            TreeNode treeNode = new TreeNode();
            treeNode.Text = node.Name;
            treeNode.Tag = node;
            if (node.subNodes != null)
            {
                foreach (PssgNode subNode in node.subNodes)
                {
                    treeNode.Nodes.Add(CreateTreeViewNode(subNode));
                }
            }
            node.TreeNode = treeNode;
            return treeNode;
        }
        public void CreateSpecificTreeViewNode(TreeView tv, string nodeName)
        {
            List<PssgNode> textureNodes = FindNodes(nodeName);
            TreeNode treeNode = new TreeNode();
            foreach (PssgNode texture in textureNodes)
            {
                if (texture.HasAttribute("id") == false)
                {
                    continue;
                }
                treeNode.Text = texture.GetAttribute("id").ToString();
                treeNode.Tag = texture;
                tv.Nodes.Add(treeNode);
                treeNode = new TreeNode();
            }
        }
        public void CreateSpecificTreeViewNode(TreeView tv, string nodeName, string attributeName, string attributeValue)
        {
            List<PssgNode> textureNodes = FindNodes(nodeName, attributeName, attributeValue);
            TreeNode treeNode = new TreeNode();
            foreach (PssgNode texture in textureNodes)
            {
                treeNode.Text = texture.attributes["id"].ToString();
                treeNode.Tag = texture;
                tv.Nodes.Add(treeNode);
                treeNode = new TreeNode();
            }
        }

        public List<PssgNode> FindNodes(string name, string attributeName = null, string attributeValue = null)
        {
            if (rootNode == null)
            {
                return new List<PssgNode>();
            }
            return rootNode.FindNodes(name, attributeName, attributeValue);
        }

        public void MoveNode(PssgNode source, PssgNode target)
        {
            source.ParentNode.RemoveChild(source);
            target.AppendChild(source);
        }
    }
}
