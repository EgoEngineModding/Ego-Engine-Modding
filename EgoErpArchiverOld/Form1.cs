namespace EgoErpArchiver
{
    using BrightIdeasSoftware;
    using EgoEngineLibrary.Archive.Erp;
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
                if (rowObject is ErpResource)
                {
                    return ((ErpResource)rowObject).Fragments.Count > 0;
                }

                return false;
            };
            this.TreeListView.ChildrenGetter = delegate(object rowObject)
            {
                if (rowObject is ErpResource)
                {
                    return ((ErpResource)rowObject).Fragments;
                }

                return null;
            };


            OLVColumn nameCol = new OLVColumn("Name", string.Empty);
            nameCol.Width = 325;
            nameCol.IsEditable = false;
            nameCol.AspectGetter = delegate(object rowObject)
            {
                if (rowObject is ErpResource)
                {
                    string str = ((ErpResource)rowObject).FileName.Substring(7).Replace("/", "\\");
                    return Path.GetFileName(((ErpResource)rowObject).FileName);//.Substring(7));
                }
                else if (rowObject is ErpFragment)
                {
                    return ((ErpFragment)rowObject).Name;
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

                this.TreeListView.SetObjects(this.file.Resources);
                this.Text = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(Args[0]);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.file = new ErpFile();
                this.file.Read(File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read));

                this.TreeListView.SetObjects(this.file.Resources);
                this.Text = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(openFileDialog.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (file == null || file.Resources.Count == 0)
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
            if (file == null || file.Resources.Count == 0)
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
            if (this.file == null || !(TreeListView.SelectedObject is ErpResource))
            {
                return;
            }

            ErpResource entry = (ErpResource)TreeListView.SelectedObject;
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
                    using (ErpBinaryReader reader = new ErpBinaryReader(entry.Fragments[0].GetDataStream(true)))
                    {
                        reader.Seek(24, SeekOrigin.Begin);
                        fNameImage = reader.ReadString();
                    }

                    ErpResource imageEntry = this.file.FindEntry(fNameImage);
                    int imageType;
                    uint width;
                    uint height;
                    uint mipmaps;
                    using (ErpBinaryReader reader = new ErpBinaryReader(imageEntry.Fragments[0].GetDataStream(true)))
                    {
                        reader.Seek(8, SeekOrigin.Begin);
                        imageType = reader.ReadInt32();
                        width = reader.ReadUInt32();
                        height = reader.ReadUInt32();

                        reader.Seek(4, SeekOrigin.Current);
                        mipmaps = reader.ReadUInt32();
                    }

                    string mipMapFileName = null;
                    uint mipCount = 0;
                    uint mipWidth = 0, mipHeight = 0;
                    uint mipLinearSize = 0;
                    if (imageEntry.Fragments.Count >= 3 && imageEntry.Fragments[2].Name == "mips")
                    {
                        using (ErpBinaryReader reader = new ErpBinaryReader(imageEntry.Fragments[2].GetDataStream(true)))
                        {
                            byte strLength = reader.ReadByte();
                            mipMapFileName = reader.ReadString(strLength);
                            mipCount = reader.ReadUInt32();

                            reader.Seek(9, SeekOrigin.Current);
                            mipWidth = (uint)reader.ReadUInt64();
                            mipHeight = (uint)reader.ReadUInt64();
                            mipLinearSize = Math.Max(mipWidth, mipHeight);
                        }
                    }

                    dds.header.width = width;
                    dds.header.height = height;
                    switch (imageType)
                    {
                        case 52: // ferrari_wheel_sfc
                        case 54: // ferrari_wheel_df, ferrari_paint
                            dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                            dds.header.pitchOrLinearSize = (width * height) / 2;
                            dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                            dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT1"), 0);
                            mipWidth = (uint)Math.Sqrt(mipWidth * 2);
                            mipHeight = (uint)Math.Sqrt(mipHeight * 2);
                            break;
                        case 55: // ferrari_sfc
                        case 57: // ferrari_decal
                            dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                            dds.header.pitchOrLinearSize = (width * height);
                            dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                            dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT5"), 0);
                            mipWidth = (uint)Math.Sqrt(mipWidth);
                            mipHeight = (uint)Math.Sqrt(mipHeight);
                            break;
                        case 65: // ferrari_wheel_nm
                            dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                            dds.header.pitchOrLinearSize = (width * height);
                            dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                            dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("ATI2"), 0);
                            mipWidth = (uint)Math.Sqrt(mipWidth);
                            mipHeight = (uint)Math.Sqrt(mipHeight);
                            break;
                        //case 65: // ferrari_wheel_nm
                        //    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                        //    dds.header.pitchOrLinearSize = (width * height);
                        //    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_ALPHAPIXELS | DdsPixelFormat.Flags.DDPF_RGB;
                        //    dds.header.ddspf.fourCC = 0;
                        //    dds.header.ddspf.rGBBitCount = 32;
                        //    dds.header.ddspf.rBitMask = 0xFF0000;
                        //    dds.header.ddspf.gBitMask = 0xFF00;
                        //    dds.header.ddspf.bBitMask = 0xFF;
                        //    dds.header.ddspf.aBitMask = 0xFF000000;
                        //    mipWidth = (uint)Math.Sqrt(mipWidth);
                        //    mipHeight = (uint)Math.Sqrt(mipHeight);
                        //    break;
                        //case 65:
                        //    dds.header.flags |= DdsHeader.Flags.DDSD_PITCH;
                        //    dds.header.pitchOrLinearSize = (width * height);
                        //    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_LUMINANCE;
                        //    //header.ddspf.flags |= DDS_PIXELFORMAT.Flags.DDPF_ALPHA;
                        //    dds.header.ddspf.fourCC = 0;
                        //    dds.header.ddspf.rGBBitCount = 8;
                        //    dds.header.ddspf.rBitMask = 0xFF;
                        //    //header.ddspf.aBitMask = 0xFF;
                        //    mipWidth = (uint)Math.Sqrt(mipWidth);
                        //    mipHeight = (uint)Math.Sqrt(mipHeight);
                        //    break;
                        default:
                            throw new Exception("Image type not supported!");
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

                    byte[] imageData = imageEntry.Fragments[1].GetDataArray(true);

                    if (!string.IsNullOrEmpty(mipMapFileName))
                    {
                        OpenFileDialog odialog = new OpenFileDialog();
                        odialog.Filter = "Mipmaps files|*.mipmaps|All files|*.*";
                        odialog.Title = "Select a mipmaps file";
                        odialog.FileName = Path.GetFileName(mipMapFileName);

                        if (odialog.ShowDialog() == DialogResult.OK)
                        {
                            byte[] mipImageData;
                            using (ErpBinaryReader reader = new ErpBinaryReader(File.Open(odialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
                            {
                                mipImageData = reader.ReadBytes((int)reader.BaseStream.Length);
                            }

                            dds.header.width = mipWidth;
                            dds.header.height = mipHeight;
                            dds.header.pitchOrLinearSize = mipLinearSize;

                            if (mipCount > 0)
                            {
                                dds.header.flags |= DdsHeader.Flags.DDSD_MIPMAPCOUNT;
                                dds.header.mipMapCount += mipCount;
                                dds.header.caps |= DdsHeader.Caps.DDSCAPS_MIPMAP | DdsHeader.Caps.DDSCAPS_COMPLEX;
                            }
                            else
                            {
                                //dds.header.flags &= ~DdsHeader.Flags.DDSD_MIPMAPCOUNT;
                                //dds.header.mipMapCount = 0;
                                //dds.header.caps &= ~(DdsHeader.Caps.DDSCAPS_MIPMAP | DdsHeader.Caps.DDSCAPS_COMPLEX);
                            }

                            //dds.Write(File.Open(Path.GetDirectoryName(dialog.FileName) + "\\" + Path.GetFileName(mipMapFileName) + ".dds", FileMode.Create, FileAccess.Write, FileShare.Read), -1);
                            dds.bdata = new byte[mipImageData.Length + imageData.Length];
                            Buffer.BlockCopy(mipImageData, 0, dds.bdata, 0, mipImageData.Length);
                            Buffer.BlockCopy(imageData, 0, dds.bdata, mipImageData.Length, imageData.Length);
                            dds.Write(File.Open(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read), -1);
                        }
                    }
                    else
                    {
                        dds.bdata = imageData;
                        dds.Write(File.Open(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read), -1);
                    }
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
            if (this.file == null || !(TreeListView.SelectedObject is ErpResource))
            {
                return;
            }

            ErpResource entry = (ErpResource)TreeListView.SelectedObject;
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
                    uint mipWidth, mipHeight;
                    switch (dds.header.ddspf.fourCC)
                    {
                        case 827611204: // DXT1 aka DXGI_FORMAT_BC1_UNORM
                            imageType = 54;
                            mipWidth = (uint)Math.Pow(dds.header.width, 2) / 2;
                            mipHeight = (uint)Math.Pow(dds.header.height, 2) / 2;
                            break;
                        case 894720068: // DXT5 aka DXGI_FORMAT_BC3_UNORM
                            imageType = 57;
                            mipWidth = (uint)Math.Pow(dds.header.width, 2);
                            mipHeight = (uint)Math.Pow(dds.header.height, 2);
                            break;
                        case 843666497: // ATI2 aka DXGI_FORMAT_BC5_UNORM
                            imageType = 65;
                            mipWidth = (uint)Math.Pow(dds.header.width, 2);
                            mipHeight = (uint)Math.Pow(dds.header.height, 2);
                            break;
                        default:
                            throw new Exception("Image type not supported!");
                    }

                    MemoryStream tgaData = entry.Fragments[0].GetDataStream(true);
                    string fNameImage;
                    ErpBinaryReader reader = new ErpBinaryReader(tgaData);
                    reader.Seek(24, SeekOrigin.Begin);
                    fNameImage = reader.ReadString();
                    ErpResource imageEntry = this.file.FindEntry(fNameImage);

                    byte[] imageByteData;
                    string mipMapFileName;
                    uint mipCount = dds.header.mipMapCount / 4;
                    if (imageEntry.Fragments.Count >= 3 && imageEntry.Fragments[2].Name == "mips")
                    {
                        MemoryStream mipsData = imageEntry.Fragments[2].GetDataStream(true);
                        reader = new ErpBinaryReader(mipsData);
                        mipMapFileName = reader.ReadString(reader.ReadByte());
                        mipsData.Dispose();

                        SaveFileDialog sdialog = new SaveFileDialog();
                        sdialog.Filter = "Mipmaps files|*.mipmaps|All files|*.*";
                        sdialog.Title = "Select the mipmaps save location and file name";
                        sdialog.FileName = Path.GetFileName(mipMapFileName);

                        if (sdialog.ShowDialog() == DialogResult.OK)
                        {
                            dds.header.mipMapCount -= mipCount;
                            uint div = (uint)Math.Pow(2.0, mipCount);
                            dds.header.width /= div;
                            dds.header.height /= div;

                            MemoryStream newMipsData = new MemoryStream();
                            UInt64 offset = 0;
                            using (ErpBinaryWriter writer = new ErpBinaryWriter(EndianBitConverter.Little, newMipsData))
                            {
                                writer.Write((byte)mipMapFileName.Length);
                                writer.Write(mipMapFileName, mipMapFileName.Length);
                                writer.Write(mipCount);
                                for (int i = 0; i < mipCount; ++i)
                                {
                                    writer.Write((byte)0);
                                    writer.Write((UInt64)offset);
                                    writer.Write((UInt64)mipWidth);
                                    writer.Write((UInt64)mipHeight);

                                    offset += mipWidth;
                                    mipWidth /= 4;
                                    mipHeight /= 4;
                                }

                                imageEntry.Fragments[2].SetData(newMipsData.ToArray());
                            }

                            using (ErpBinaryWriter writer = new ErpBinaryWriter(EndianBitConverter.Little, File.Open(sdialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read)))
                            {
                                byte[] mipImageData = new byte[offset];
                                Buffer.BlockCopy(dds.bdata, 0, mipImageData, 0, (int)offset);
                                writer.Write(mipImageData);
                            }

                            int remainingBytes = dds.bdata.Length - (int)offset;
                            imageByteData = new byte[remainingBytes];
                            Buffer.BlockCopy(dds.bdata, (int)offset, imageByteData, 0, remainingBytes);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        imageByteData = dds.bdata;
                    }


                    using (ErpBinaryWriter writer = new ErpBinaryWriter(EndianBitConverter.Little, tgaData))
                    {
                        writer.Seek(4, SeekOrigin.Begin);
                        writer.Write(imageType);
                        writer.Seek(4, SeekOrigin.Current);
                        writer.Write(dds.header.mipMapCount);
                    }
                    entry.Fragments[0].SetData(tgaData.ToArray());

                    MemoryStream imageData = imageEntry.Fragments[0].GetDataStream(true);
                    using (ErpBinaryWriter writer = new ErpBinaryWriter(EndianBitConverter.Little, imageData))
                    {
                        writer.Seek(8, SeekOrigin.Begin);
                        writer.Write(imageType);
                        writer.Write(dds.header.width);
                        writer.Write(dds.header.height);
                        writer.Seek(4, SeekOrigin.Current);
                        writer.Write(dds.header.mipMapCount);
                    }

                    imageEntry.Fragments[0].SetData(imageData.ToArray());
                    imageEntry.Fragments[1].SetData(imageByteData);
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
