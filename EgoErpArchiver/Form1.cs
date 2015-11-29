namespace EgoErpArchiver
{
    using BrightIdeasSoftware;
    using EgoEngineLibrary.Archive;
    using EgoEngineLibrary.Graphics;
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        ErpFile file = new ErpFile();

        public Form1(string[] Args)
        {
            InitializeComponent();
            this.Icon = Properties.Resources.Ryder25;
            this.Text = Properties.Resources.AppTitleLong;
            this.openFileDialog.Filter = "Erp files|*.erp|All files|*.*";
            //this.openFileDialog.FileName = "ferrari_paint.tga.erp";
            this.saveFileDialog.Filter = "Erp files|*.erp|All files|*.*";
            //this.saveFileDialog.FileName = "ferrari_paint.tga.erp";

            this.TreeListView.ShowGroups = false;
            this.TreeListView.CellEditActivation = ObjectListView.CellEditActivateMode.DoubleClick;
            this.TreeListView.CanExpandGetter = delegate(object rowObject)
            {
                if (rowObject is ErpEntry)
                {
                    return ((ErpEntry)rowObject).Resources.Count > 0;
                }

                return false;
            };
            this.TreeListView.ChildrenGetter = delegate(object rowObject)
            {
                if (rowObject is ErpEntry)
                {
                    return ((ErpEntry)rowObject).Resources;
                }

                return null;
            };


            OLVColumn nameCol = new OLVColumn("Name", string.Empty);
            nameCol.Width = 325;
            nameCol.IsEditable = false;
            nameCol.AspectGetter = delegate(object rowObject)
            {
                if (rowObject is ErpEntry)
                {
                    string str = ((ErpEntry)rowObject).FileName.Substring(7).Replace("/", "\\");
                    return Path.GetFileName(((ErpEntry)rowObject).FileName);//.Substring(7));
                }
                else if (rowObject is ErpResource)
                {
                    return ((ErpResource)rowObject).Name;
                }

                return string.Empty;
            };
            this.TreeListView.Columns.Add(nameCol);

            OLVColumn fileTypeCol = new OLVColumn("File Type", "EntryType");
            fileTypeCol.Width = 150;
            this.TreeListView.Columns.Add(fileTypeCol);

            OLVColumn sizeCol = new OLVColumn("Size", "Size");
            sizeCol.Width = 100;
            sizeCol.IsEditable = false;
            this.TreeListView.Columns.Add(sizeCol);

            OLVColumn pSizeCol = new OLVColumn("Packed Size", "PackedSize");
            pSizeCol.Width = 100;
            pSizeCol.IsEditable = false;
            this.TreeListView.Columns.Add(pSizeCol);

            OLVColumn fullPathCol = new OLVColumn("Full Path", "FileName");
            fullPathCol.Width = 500;
            fullPathCol.IsEditable = false;
            this.TreeListView.Columns.Add(fullPathCol);

            if (Args.Length > 0)
            {
                this.file = new ErpFile();
                this.file.Read(File.Open(Args[0], FileMode.Open, FileAccess.Read, FileShare.Read));

                this.TreeListView.SetObjects(this.file.Entries);
                this.Text = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(Args[0]);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.file = new ErpFile();
                this.file.Read(File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read));

                this.TreeListView.SetObjects(this.file.Entries);
                this.Text = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(openFileDialog.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (file == null || file.Entries.Count == 0)
            {
                return;
            }

            if (this.saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.file.Write(File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read));
                this.Text = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(saveFileDialog.FileName);
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (file == null || file.Entries.Count == 0)
            {
                return;
            }

            folderBrowserDialog.Description = "Select a folder to export the files:";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    file.Export(folderBrowserDialog.SelectedPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed Exporting!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = "Select a folder to import the files from:";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    file.Import(folderBrowserDialog.SelectedPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed Importing!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void exportTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.file == null || !(TreeListView.SelectedObject is ErpEntry))
            {
                return;
            }

            ErpEntry entry = (ErpEntry)TreeListView.SelectedObject;
            if (Path.GetExtension(entry.FileName) != ".tga")
            {
                MessageBox.Show("Please select a *.tga file!", "Select a tga file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Dds files|*.dds|All files|*.*";
            dialog.Title = "Select the dds save location and file name";
            dialog.FileName = Path.GetFileName(entry.FileName) + ".dds";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DdsFile dds = new DdsFile();

                    string fNameImage;
                    using (ErpBinaryReader reader = new ErpBinaryReader(entry.Resources[0].GetDataStream(true)))
                    {
                        reader.Seek(24, SeekOrigin.Begin);
                        fNameImage = reader.ReadString();
                    }

                    ErpEntry imageEntry = this.file.FindEntry(fNameImage);
                    int imageType;
                    uint width;
                    uint height;
                    uint mipmaps;
                    using (ErpBinaryReader reader = new ErpBinaryReader(imageEntry.Resources[0].GetDataStream(true)))
                    {
                        reader.Seek(8, SeekOrigin.Begin);
                        imageType = reader.ReadInt32();
                        width = reader.ReadUInt32();
                        height = reader.ReadUInt32();

                        reader.Seek(4, SeekOrigin.Current);
                        mipmaps = reader.ReadUInt32();
                    }

                    dds.header.width = width;
                    dds.header.height = height;
                    switch (imageType)
                    {
                        case 52:
                        case 54:
                            dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                            dds.header.pitchOrLinearSize = (width * height) / 2;
                            dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                            dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT1"), 0);
                            break;
                        case 57:
                            dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                            dds.header.pitchOrLinearSize = (width * height);
                            dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                            dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT5"), 0);
                            break;
                    }
                    if (mipmaps > 0)
                    {
                        dds.header.flags |= DdsHeader.Flags.DDSD_MIPMAPCOUNT;
                        dds.header.mipMapCount = mipmaps;
                        dds.header.caps |= DdsHeader.Caps.DDSCAPS_MIPMAP | DdsHeader.Caps.DDSCAPS_COMPLEX;
                    }
                    dds.header.reserved1 = new uint[11];
                    dds.header.ddspf.size = 32;
                    dds.header.caps |= DdsHeader.Caps.DDSCAPS_TEXTURE;

                    dds.bdata = imageEntry.Resources[1].GetDataArray(true);

                    dds.Write(File.Open(dialog.FileName, FileMode.Create), -1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void importTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.file == null || !(TreeListView.SelectedObject is ErpEntry))
            {
                return;
            }

            ErpEntry entry = (ErpEntry)TreeListView.SelectedObject;
            if (Path.GetExtension(entry.FileName) != ".tga")
            {
                MessageBox.Show("Please select a *.tga file!", "Select a tga file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Dds files|*.dds|All files|*.*";
            dialog.Title = "Select a dds file";
            dialog.FileName = Path.GetFileName(entry.FileName) + ".dds";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DdsFile dds = new DdsFile(File.Open(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read));

                    int imageType = 0;
                    switch(dds.header.ddspf.fourCC)
                    {
                        case 827611204: // DXT1
                            imageType = 54;
                            break;
                        case 894720068: // DXT5
                            imageType = 57;
                            break;
                    }

                    MemoryStream tgaData = entry.Resources[0].GetDataStream(true);
                    string fNameImage;
                    ErpBinaryReader reader = new ErpBinaryReader(tgaData);
                    reader.Seek(24, SeekOrigin.Begin);
                    fNameImage = reader.ReadString();
                    using (ErpBinaryWriter writer = new ErpBinaryWriter(EndianBitConverter.Little, tgaData))
                    {
                        writer.Seek(4, SeekOrigin.Begin);
                        writer.Write(imageType);
                        writer.Seek(4, SeekOrigin.Current);
                        writer.Write(dds.header.mipMapCount);
                    }
                    entry.Resources[0].SetData(tgaData.ToArray());

                    ErpEntry imageEntry = this.file.FindEntry(fNameImage);
                    MemoryStream imageData = imageEntry.Resources[0].GetDataStream(true);
                    using (ErpBinaryWriter writer = new ErpBinaryWriter(EndianBitConverter.Little, imageData))
                    {
                        writer.Seek(8, SeekOrigin.Begin);
                        writer.Write(imageType);
                        writer.Write(dds.header.width);
                        writer.Write(dds.header.height);
                        writer.Seek(4, SeekOrigin.Current);
                        writer.Write(dds.header.mipMapCount);
                    }
                    imageEntry.Resources[0].SetData(imageData.ToArray());
                    imageEntry.Resources[1].SetData(dds.bdata);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void readMeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://modding.petartasev.com/Ego/EEA/ReadME.html");
        }
    }
}
