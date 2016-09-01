using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Data.Pkg;
using EgoEngineLibrary.Graphics;
using Microsoft.Win32;
using MiscUtil.Conversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EgoErpArchiver.ViewModel
{
    public class ErpPackageViewModel : ViewModelBase
    {
        readonly ErpResourceViewModel resView;

        public ErpResource Package
        {
            get { return resView.Resource; }
        }
        public override string DisplayName
        {
            get { return Package.FileName; }
        }

        #region Presentation Props
        bool isSelected;
        string preview;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    if (value)
                    {
                        Task.Run(() => GetPreview()).Wait();
                        resView.Select();
                    }
                    else preview = string.Empty;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        public string Preview
        {
            get { return preview; }
            set { preview = value; OnPropertyChanged("Preview"); }
        }
        #endregion

        public ErpPackageViewModel(ErpResourceViewModel resView)
        {
            this.resView = resView;
        }

        public void GetPreview()
        {
            try
            {
                PkgFile package;

                switch (Package.ResourceType)
                {
                    case "AnimClip":
                        package = PkgFile.ReadPkg(Package.GetFragment("temp", 0).GetDataStream(true));
                        break;
                    case "EventGraph":
                        package = PkgFile.ReadPkg(Package.GetFragment("node", 0).GetDataStream(true));
                        break;
                    default:
                        package = PkgFile.ReadPkg(Package.Fragments[0].GetDataStream(true));
                        break;
                }

                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                {
                    package.WriteJson(sw);
                    this.Preview = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                this.Preview = "Could not create preview!" + Environment.NewLine + Environment.NewLine + ex.Message;
            }
        }
        public DdsFile ExportDDS(string fileName, bool isPreview)
        {
            DdsFile dds = new DdsFile();

            string fNameImage;
            using (ErpBinaryReader reader = new ErpBinaryReader(Package.Fragments[0].GetDataStream(true)))
            {
                reader.Seek(24, SeekOrigin.Begin);
                fNameImage = reader.ReadString();
            }

            ErpResource imageEntry = Package.ParentFile.FindEntry(fNameImage);
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

            string mipMapFileName = string.Empty;
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

                    uint mipPower = (uint)Math.Pow(2.0, mipCount);
                    mipWidth = width * mipPower;
                    mipHeight = height * mipPower;
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
                    break;
                case 55: // ferrari_sfc
                case 57: // ferrari_decal
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (width * height);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT5"), 0);
                    break;
                case 65: // ferrari_wheel_nm
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (width * height);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("ATI2"), 0);
                    break;
                case 70: // flow_boot splash_bg_image; tried just about everything, can't figure it out
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (width * height) / 2;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT1"), 0);
                    //dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    //dds.header.pitchOrLinearSize = (width * height);
                    //dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_ALPHAPIXELS | DdsPixelFormat.Flags.DDPF_RGB;
                    //dds.header.ddspf.fourCC = 0;
                    //dds.header.ddspf.rGBBitCount = 16;
                    //dds.header.ddspf.rBitMask = 0xF00;
                    //dds.header.ddspf.gBitMask = 0xF0;
                    //dds.header.ddspf.bBitMask = 0xF;
                    //dds.header.ddspf.aBitMask = 0xF000;
                    goto default;
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

            string mipMapFullFileName = Path.Combine(Properties.Settings.Default.F12016Dir, mipMapFileName);
            bool foundMipMapFile = File.Exists(mipMapFullFileName);
            if (!string.IsNullOrEmpty(mipMapFileName) && (!isPreview || foundMipMapFile))
            {
                if (!isPreview)
                {
                    OpenFileDialog odialog = new OpenFileDialog();
                    odialog.Filter = "Mipmaps files|*.mipmaps|All files|*.*";
                    odialog.Title = "Select a mipmaps file";
                    odialog.FileName = Path.GetFileName(mipMapFullFileName);
                    mipMapFullFileName = Path.GetDirectoryName(mipMapFullFileName);
                    if (Directory.Exists(mipMapFullFileName))
                    {
                        odialog.InitialDirectory = mipMapFullFileName;
                    }

                    foundMipMapFile = odialog.ShowDialog() == true;
                    mipMapFullFileName = odialog.FileName;
                }

                if (foundMipMapFile)
                {
                    byte[] mipImageData;
                    using (ErpBinaryReader reader = new ErpBinaryReader(File.Open(mipMapFullFileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
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
                    
                    dds.bdata = new byte[mipImageData.Length + imageData.Length];
                    Buffer.BlockCopy(mipImageData, 0, dds.bdata, 0, mipImageData.Length);
                    Buffer.BlockCopy(imageData, 0, dds.bdata, mipImageData.Length, imageData.Length);
                    dds.Write(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), -1);
                }
                else
                {
                    throw new FileNotFoundException("Mipmaps file not found!", mipMapFullFileName);
                }
            }
            else
            {
                dds.bdata = imageData;
                dds.Write(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), -1);
            }

            return dds;
        }
        public void ImportDDS(string fileName, string mipMapSaveLocation)
        {
            DdsFile dds = new DdsFile(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));

            int imageType = 0;
            uint mipWidth, mipHeight;
            switch (dds.header.ddspf.fourCC)
            {
                case 827611204: // DXT1 aka DXGI_FORMAT_BC1_UNORM
                    imageType = 54;
                    mipWidth = dds.header.width / 2;
                    mipHeight = dds.header.height / 2;
                    break;
                case 894720068: // DXT5 aka DXGI_FORMAT_BC3_UNORM
                    imageType = 57;
                    mipWidth = dds.header.width;
                    mipHeight = dds.header.height;
                    break;
                case 843666497: // ATI2 aka DXGI_FORMAT_BC5_UNORM
                    imageType = 65;
                    mipWidth = dds.header.width;
                    mipHeight = dds.header.height;
                    break;
                default:
                    throw new Exception("Image type not supported!");
            }

            MemoryStream tgaData = Package.Fragments[0].GetDataStream(true);
            string fNameImage;
            ErpBinaryReader reader = new ErpBinaryReader(tgaData);
            reader.Seek(24, SeekOrigin.Begin);
            fNameImage = reader.ReadString();
            ErpResource imageEntry = Package.ParentFile.FindEntry(fNameImage);

            byte[] imageByteData;
            if (imageEntry.Fragments.Count >= 3 && imageEntry.Fragments[2].Name == "mips")
            {
                string mipMapFileName;
                uint mipCount = dds.header.mipMapCount / 4;
                MemoryStream mipsData = imageEntry.Fragments[2].GetDataStream(true);
                reader = new ErpBinaryReader(mipsData);
                mipMapFileName = reader.ReadString(reader.ReadByte());
                uint oldMipCount = reader.ReadUInt32();
                mipsData.Dispose();
                if (oldMipCount < dds.header.mipMapCount) mipCount = oldMipCount;

                bool foundMipMapSaveLocation = false;
                if (string.IsNullOrEmpty(mipMapSaveLocation))
                {
                    SaveFileDialog sdialog = new SaveFileDialog();
                    sdialog.Filter = "Mipmaps files|*.mipmaps|All files|*.*";
                    sdialog.Title = "Select the mipmaps save location and file name";
                    sdialog.FileName = Path.GetFileName(mipMapFileName);
                    string mipFullPath = Path.GetDirectoryName(Path.Combine(Properties.Settings.Default.F12016Dir, mipMapFileName));
                    if (Directory.Exists(mipFullPath))
                    {
                        sdialog.InitialDirectory = mipFullPath;
                    }

                    foundMipMapSaveLocation = sdialog.ShowDialog() == true;
                    mipMapSaveLocation = sdialog.FileName;
                }
                else if (Directory.Exists(Path.GetDirectoryName(mipMapSaveLocation)))
                {
                    foundMipMapSaveLocation = true;
                }

                if (foundMipMapSaveLocation)
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

                        uint mipLinearSize = mipWidth * mipHeight;//Math.Max(mipWidth, mipHeight);

                        for (int i = 0; i < mipCount; ++i)
                        {
                            writer.Write((byte)0);
                            writer.Write((UInt64)offset);
                            writer.Write((UInt64)mipLinearSize);
                            writer.Write((UInt64)mipLinearSize);

                            offset += mipLinearSize;
                            mipLinearSize /= 4;
                            //mipWidth /= 4;
                            //mipHeight /= 4;
                        }

                        imageEntry.Fragments[2].SetData(newMipsData.ToArray());
                    }

                    using (ErpBinaryWriter writer = new ErpBinaryWriter(EndianBitConverter.Little, File.Open(mipMapSaveLocation, FileMode.Create, FileAccess.Write, FileShare.Read)))
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
            Package.Fragments[0].SetData(tgaData.ToArray());

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
    }
}
