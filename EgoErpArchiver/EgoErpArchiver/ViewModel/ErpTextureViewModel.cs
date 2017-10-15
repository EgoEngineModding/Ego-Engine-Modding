using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Archive.Erp.Data;
using EgoEngineLibrary.Graphics;
using EgoEngineLibrary.Graphics.Dds;
using Microsoft.Win32;
using MiscUtil.Conversion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EgoErpArchiver.ViewModel
{
    public class ErpTextureViewModel : ViewModelBase
    {
        readonly ErpResourceViewModel resView;
        int _width;
        int _height;

        public ErpResource Texture
        {
            get { return resView.Resource; }
        }
        public override string DisplayName
        {
            get { return Texture.FileName; }
        }
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged("Width");
            }
        }
        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged("Height");
            }
        }
        private uint _texArraySize;

        public uint TexArraySize
        {
            get { return _texArraySize; }
            set { _texArraySize = value; }
        }

        #region Presentation Props
        bool isSelected;
        string _textureInfo;
        uint _texArrayIndex;
        BitmapSource preview;
        string previewError;
        Visibility previewErrorVisibility;

        public uint TexArrayIndex
        {
            get { return _texArrayIndex; }
            set
            {
                if (value != _texArrayIndex)
                {
                    _texArrayIndex = value;
                    if (IsSelected)
                    {
                        Task.Run(() => GetPreview()).Wait();
                        resView.Select();
                    }
                    OnPropertyChanged(nameof(TexArrayIndex));
                }
            }
        }

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
                    else { preview = null; }
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        public string TextureInfo
        {
            get { return _textureInfo; }
            set
            {
                _textureInfo = value;
                OnPropertyChanged("TextureInfo");
            }
        }
        public BitmapSource Preview
        {
            get { return preview; }
            set { preview = value; OnPropertyChanged("Preview"); }
        }
        public string PreviewError
        {
            get { return previewError; }
            set { previewError = value; OnPropertyChanged("PreviewError"); }
        }
        public Visibility PreviewErrorVisibility
        {
            get { return previewErrorVisibility; }
            set { previewErrorVisibility = value; OnPropertyChanged("PreviewErrorVisibility"); }
        }
        #endregion

        public ErpTextureViewModel(ErpResourceViewModel resView)
        {
            this.resView = resView;
            _width = 0;
            _height = 0;

            _texArrayIndexDownCommand = new RelayCommand(TexArrayIndexDownCommand_Execute, TexArrayIndexDownCommand_CanExecute);
            _texArrayIndexUpCommand = new RelayCommand(TexArrayIndexUpCommand_Execute, TexArrayIndexUpCommand_CanExecute);
        }

        public void GetPreview()
        {
            DdsFile dds;
            CSharpImageLibrary.ImageEngineImage image = null;
            try
            {
                this.Preview = null;
                dds = ExportDDS(System.AppDomain.CurrentDomain.BaseDirectory + "\\temp.dds", true, false);
                int maxDimension = (int)Math.Max(dds.header.width, dds.header.height);
                Width = (int)dds.header.width;
                Height = (int)dds.header.height;
                
                image = new CSharpImageLibrary.ImageEngineImage(System.AppDomain.CurrentDomain.BaseDirectory + "\\temp.dds", maxDimension);
                Preview = null;
                this.Preview = image.GetWPFBitmap(maxDimension, true);

                this.PreviewErrorVisibility = Visibility.Collapsed;
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Preview = null;
                this.PreviewError = "Could not create preview! Export/Import may still work in certain circumstances." + Environment.NewLine + Environment.NewLine + ex.Message;
                this.PreviewErrorVisibility = Visibility.Visible;
            }
            finally
            {
                dds = null;
                image?.Dispose();
                image = null;
            }
        }
        public DdsFile ExportDDS(string fileName, bool isPreview, bool exportTexArray)
        {
            DdsFile dds = new DdsFile();

            ErpGfxSRVResource srvRes = new ErpGfxSRVResource();
            srvRes.FromResource(Texture);

            uint mipPower = (uint)Math.Pow(2.0, srvRes.SurfaceRes.Frag2.Mips.Count);
            uint mipWidth = 0, mipHeight = 0;
            ulong mipLinearSize = 0;
            ulong mipTotalSize = 0;
            if (srvRes.SurfaceRes.HasMips)
            {
                mipLinearSize = Math.Max(srvRes.SurfaceRes.Frag2.Mips[0].PackedSize, srvRes.SurfaceRes.Frag2.Mips[0].Size);
                for (int i = 0; i < srvRes.SurfaceRes.Frag2.Mips.Count; ++i)
                {
                    mipTotalSize += Math.Max(srvRes.SurfaceRes.Frag2.Mips[i].PackedSize, srvRes.SurfaceRes.Frag2.Mips[i].Size);
                }

                mipWidth = srvRes.SurfaceRes.Fragment0.Width * mipPower;
                mipHeight = srvRes.SurfaceRes.Fragment0.Height * mipPower;
            }

            dds.header.width = srvRes.SurfaceRes.Fragment0.Width;
            dds.header.height = srvRes.SurfaceRes.Fragment0.Height;
            dds.header10.arraySize = srvRes.SurfaceRes.Fragment0.ArraySize;
            _texArraySize = srvRes.SurfaceRes.Fragment0.ArraySize;
            _textureInfo = srvRes.SurfaceRes.Fragment0.Width + "x" + srvRes.SurfaceRes.Fragment0.Height + " Mips:" + (srvRes.SurfaceRes.Fragment0.MipMapCount) + " Format:" + srvRes.SurfaceRes.Fragment0.ImageType + ",";
            switch (srvRes.SurfaceRes.Fragment0.ImageType)
            {
                //case (ErpGfxSurfaceFormat)14: // gameparticles k_smoke; application
                case ErpGfxSurfaceFormat.ABGR8:
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (srvRes.SurfaceRes.Fragment0.Width * srvRes.SurfaceRes.Fragment0.Height) * 4;

                    if (srvRes.SurfaceRes.Fragment0.ArraySize > 1 && exportTexArray)
                    {
                        dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                        dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_R8G8B8A8_UNORM;
                        TextureInfo += "RGBA8";
                    }
                    else
                    {
                        dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_ALPHAPIXELS | DdsPixelFormat.Flags.DDPF_RGB;
                        dds.header.ddspf.fourCC = 0;
                        dds.header.ddspf.rGBBitCount = 32;
                        dds.header.ddspf.rBitMask = 0xFF;
                        dds.header.ddspf.gBitMask = 0xFF00;
                        dds.header.ddspf.bBitMask = 0xFF0000;
                        dds.header.ddspf.aBitMask = 0xFF000000;
                        TextureInfo += "ABGR8";
                    }
                    break;
                case ErpGfxSurfaceFormat.DXT1: // ferrari_wheel_sfc
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (srvRes.SurfaceRes.Fragment0.Width * srvRes.SurfaceRes.Fragment0.Height) / 2;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;

                    if (srvRes.SurfaceRes.Fragment0.ArraySize > 1 && exportTexArray)
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                        dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC1_UNORM;
                    }
                    else
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT1"), 0);
                    }

                    TextureInfo += "DXT1";
                    break;
                case ErpGfxSurfaceFormat.DXT1_SRGB: // ferrari_wheel_df, ferrari_paint
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (srvRes.SurfaceRes.Fragment0.Width * srvRes.SurfaceRes.Fragment0.Height) / 2;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB;
                    TextureInfo += "BC1_SRGB";
                    break;
                case ErpGfxSurfaceFormat.DXT5: // ferrari_sfc
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (srvRes.SurfaceRes.Fragment0.Width * srvRes.SurfaceRes.Fragment0.Height);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;

                    if (srvRes.SurfaceRes.Fragment0.ArraySize > 1 && exportTexArray)
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                        dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC3_UNORM;
                    }
                    else
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DXT5"), 0);
                    }

                    TextureInfo += "DXT5";
                    break;
                case ErpGfxSurfaceFormat.DXT5_SRGB: // ferrari_decal
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (srvRes.SurfaceRes.Fragment0.Width * srvRes.SurfaceRes.Fragment0.Height);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB;
                    TextureInfo += "BC3_SRGB";
                    break;
                case ErpGfxSurfaceFormat.ATI1: // gameparticles k_smoke
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (srvRes.SurfaceRes.Fragment0.Width * srvRes.SurfaceRes.Fragment0.Height) / 2;
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;

                    if (srvRes.SurfaceRes.Fragment0.ArraySize > 1 && exportTexArray)
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                        dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC4_UNORM;
                    }
                    else
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("ATI1"), 0);
                    }

                    TextureInfo += "ATI1";
                    break;
                case ErpGfxSurfaceFormat.ATI2: // ferrari_wheel_nm
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (srvRes.SurfaceRes.Fragment0.Width * srvRes.SurfaceRes.Fragment0.Height);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;

                    if (srvRes.SurfaceRes.Fragment0.ArraySize > 1 && exportTexArray)
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                        dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC5_UNORM;
                    }
                    else
                    {
                        dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("ATI2"), 0);
                    }

                    TextureInfo += "ATI2/3Dc";
                    break;
                case ErpGfxSurfaceFormat.BC6: // key0_2016; environment abu_dhabi tree_palm_06
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (srvRes.SurfaceRes.Fragment0.Width * srvRes.SurfaceRes.Fragment0.Height);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC6H_UF16;
                    TextureInfo += "BC6H_UF16";
                    break;
                case ErpGfxSurfaceFormat.BC7:
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (srvRes.SurfaceRes.Fragment0.Width * srvRes.SurfaceRes.Fragment0.Height);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC7_UNORM;
                    TextureInfo += "BC7";
                    break;
                case ErpGfxSurfaceFormat.BC7_SRGB: // flow_boot splash_bg_image
                    dds.header.flags |= DdsHeader.Flags.DDSD_LINEARSIZE;
                    dds.header.pitchOrLinearSize = (srvRes.SurfaceRes.Fragment0.Width * srvRes.SurfaceRes.Fragment0.Height);
                    dds.header.ddspf.flags |= DdsPixelFormat.Flags.DDPF_FOURCC;
                    dds.header.ddspf.fourCC = BitConverter.ToUInt32(Encoding.UTF8.GetBytes("DX10"), 0);
                    dds.header10.dxgiFormat = DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB;
                    TextureInfo += "BC7_SRGB";
                    break;
                default:
                    TextureInfo += "Unknown";
                    throw new Exception("Image format not supported!");
            }
            if (srvRes.SurfaceRes.Fragment0.MipMapCount > 0)
            {
                dds.header.flags |= DdsHeader.Flags.DDSD_MIPMAPCOUNT;
                dds.header.mipMapCount = srvRes.SurfaceRes.Fragment0.MipMapCount;
                dds.header.caps |= DdsHeader.Caps.DDSCAPS_MIPMAP | DdsHeader.Caps.DDSCAPS_COMPLEX;
            }

            byte[] imageData = srvRes.SurfaceRes.Fragment1.Data;
            
            string mipMapFullFileName = Path.Combine(Properties.Settings.Default.F12016Dir, srvRes.SurfaceRes.Frag2.MipMapFileName);
            bool foundMipMapFile = File.Exists(mipMapFullFileName);
            bool hasValidMipMaps = dds.header.pitchOrLinearSize < mipLinearSize && dds.header.pitchOrLinearSize * mipPower * mipPower == mipLinearSize;
            if (srvRes.SurfaceRes.HasMips && hasValidMipMaps && (!isPreview || foundMipMapFile))
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
                    dds.header.pitchOrLinearSize = (uint)mipLinearSize;

                    if (mipTotalSize != (ulong)mipImageData.Length)
                    {
                        throw new Exception($"There is a mismatch with the mipmaps file.{Environment.NewLine}It is either incorrectly modded, or in the wrong folder.");
                    }


                    if (srvRes.SurfaceRes.Frag2.Mips.Count > 0)
                    {
                        dds.header.flags |= DdsHeader.Flags.DDSD_MIPMAPCOUNT;
                        dds.header.mipMapCount += (uint)srvRes.SurfaceRes.Frag2.Mips.Count;
                        dds.header.caps |= DdsHeader.Caps.DDSCAPS_MIPMAP | DdsHeader.Caps.DDSCAPS_COMPLEX;
                    }

                    dds.bdata = new byte[mipImageData.Length + imageData.Length];
                    Buffer.BlockCopy(mipImageData, 0, dds.bdata, 0, mipImageData.Length);
                    Buffer.BlockCopy(imageData, 0, dds.bdata, mipImageData.Length, imageData.Length);
                    dds.Write(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), -1);
                    TextureInfo += Environment.NewLine + mipWidth + "x" + mipHeight + " Mips:" + (srvRes.SurfaceRes.Frag2.Mips.Count);
                }
                else
                {
                    throw new FileNotFoundException("Mipmaps file not found!", mipMapFullFileName);
                }
            }
            else
            {
                if (srvRes.SurfaceRes.HasMips)
                {
                    if (!hasValidMipMaps)
                    {
                        // This usually happens when the mips fragment was never updated with the new offsets/width/height/linearsize
                        TextureInfo += Environment.NewLine + "Mipmaps are incorrectly modded! Try exporting, then importing to fix the issue.";
                    }
                    else if (!foundMipMapFile)
                    {
                        TextureInfo += Environment.NewLine + "Mipmaps not found! Make sure they are in the correct F1 game folder!";
                    }
                    else
                    {
                        // Not found during preview
                        TextureInfo += Environment.NewLine + "Mipmaps not loaded! Make sure they are in the correct F1 game folder!";
                    }
                }

                if (srvRes.SurfaceRes.Fragment0.ArraySize > 1)
                {
                    uint bytesPerArrayImage = (uint)imageData.Length / srvRes.SurfaceRes.Fragment0.ArraySize;
                    byte[] data = new byte[bytesPerArrayImage];

                    if (!exportTexArray)
                    {
                        dds.header10.arraySize = 1;
                        Buffer.BlockCopy(imageData, (int)(bytesPerArrayImage * _texArrayIndex), data, 0, (int)bytesPerArrayImage);
                        dds.bdata = data;
                        dds.Write(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), -1);
                    }
                    else
                    {
                        dds.bdata = imageData;
                        dds.Write(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), -1);

                        // TODO: Add support for exporting individual tex array slices
                        //string output = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
                        //for (int i = 0; i < srvRes.SurfaceRes.Fragment0.ArraySize; ++i)
                        //{
                        //    Buffer.BlockCopy(imageData, (int)(bytesPerArrayImage * i), data, 0, (int)bytesPerArrayImage);
                        //    dds.bdata = data;
                        //    dds.Write(File.Open(output + "!!!" + i.ToString("000") + ".dds", FileMode.Create, FileAccess.Write, FileShare.Read), -1);
                        //}
                    }
                }
                else
                {
                    dds.bdata = imageData;
                    dds.Write(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), -1);
                }
            }

            return dds;
        }
        public void ImportDDS(string fileName, string mipMapSaveLocation, bool importTexArray)
        {
            DdsFile dds = new DdsFile(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));

            ErpGfxSRVResource srvRes = new ErpGfxSRVResource();
            srvRes.FromResource(Texture);

            ErpGfxSurfaceFormat imageType = 0;
            uint mipLinearSize;
            switch (dds.header.ddspf.fourCC)
            {
                case 0:
                    imageType = ErpGfxSurfaceFormat.ABGR8;
                    mipLinearSize = (dds.header.width * dds.header.height);
                    break;
                case 827611204: // DXT1 aka DXGI_FORMAT_BC1_UNORM
                    imageType = ErpGfxSurfaceFormat.DXT1;
                    mipLinearSize = (dds.header.width * dds.header.height) / 2;
                    break;
                case 894720068: // DXT5 aka DXGI_FORMAT_BC3_UNORM
                    imageType = ErpGfxSurfaceFormat.DXT5;
                    mipLinearSize = (dds.header.width * dds.header.height);
                    break;
                case 826889281: // ATI1
                    imageType = ErpGfxSurfaceFormat.ATI1;
                    mipLinearSize = (dds.header.width * dds.header.height) / 2;
                    break;
                case 843666497: // ATI2 aka DXGI_FORMAT_BC5_UNORM
                    imageType = ErpGfxSurfaceFormat.ATI2;
                    mipLinearSize = (dds.header.width * dds.header.height);
                    break;
                case 808540228: // DX10
                    if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_UNORM)
                    {
                        imageType = ErpGfxSurfaceFormat.BC7;
                        mipLinearSize = (dds.header.width * dds.header.height);
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC7_UNORM_SRGB)
                    {
                        imageType = ErpGfxSurfaceFormat.BC7_SRGB;
                        mipLinearSize = (dds.header.width * dds.header.height);
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_UNORM ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_SNORM ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_UINT ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_R8G8B8A8_SINT)
                    {
                        goto case 0;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_UNORM)
                    {
                        goto case 827611204;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC1_UNORM_SRGB)
                    {
                        imageType = ErpGfxSurfaceFormat.DXT1_SRGB;
                        mipLinearSize = (dds.header.width * dds.header.height) / 2;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_UNORM)
                    {
                        goto case 894720068;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC3_UNORM_SRGB)
                    {
                        imageType = ErpGfxSurfaceFormat.DXT5_SRGB;
                        mipLinearSize = (dds.header.width * dds.header.height);
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC4_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC4_UNORM ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC4_SNORM)
                    {
                        goto case 826889281;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC5_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC5_UNORM ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC5_SNORM)
                    {
                        goto case 843666497;
                    }
                    else if (dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC6H_TYPELESS ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC6H_UF16 ||
                        dds.header10.dxgiFormat == DXGI_Format.DXGI_FORMAT_BC6H_SF16)
                    {
                        imageType = ErpGfxSurfaceFormat.BC6;
                        mipLinearSize = (dds.header.width * dds.header.height);
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                default:
                    throw new Exception("Image type not supported!");
            }

            byte[] imageByteData;
            if (srvRes.SurfaceRes.HasMips)
            {
                int mipCount = (int)dds.header.mipMapCount / 4;
                if (srvRes.SurfaceRes.Frag2.Mips.Count < dds.header.mipMapCount) mipCount = srvRes.SurfaceRes.Frag2.Mips.Count;

                bool foundMipMapSaveLocation = false;
                if (string.IsNullOrEmpty(mipMapSaveLocation))
                {
                    SaveFileDialog sdialog = new SaveFileDialog();
                    sdialog.Filter = "Mipmaps files|*.mipmaps|All files|*.*";
                    sdialog.Title = "Select the mipmaps save location and file name";
                    sdialog.FileName = Path.GetFileName(srvRes.SurfaceRes.Frag2.MipMapFileName);
                    string mipFullPath = Path.GetDirectoryName(Path.Combine(Properties.Settings.Default.F12016Dir, srvRes.SurfaceRes.Frag2.MipMapFileName));
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
                    dds.header.mipMapCount -= (uint)mipCount;
                    uint div = (uint)Math.Pow(2.0, mipCount);
                    dds.header.width /= div;
                    dds.header.height /= div;

                    srvRes.SurfaceRes.Frag2.Mips = new List<ErpGfxSurfaceRes2Mips>(mipCount);
                    UInt64 offset = 0;
                    for (int i = 0; i < mipCount; ++i)
                    {
                        ErpGfxSurfaceRes2Mips mip = new ErpGfxSurfaceRes2Mips();

                        mip.Compression = 0;
                        mip.Offset = offset;
                        mip.PackedSize = mipLinearSize;
                        mip.Size = mipLinearSize;

                        offset += mipLinearSize;
                        mipLinearSize /= 4;

                        srvRes.SurfaceRes.Frag2.Mips.Add(mip);
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
                    srvRes.SurfaceRes.Fragment1.Data = imageByteData;
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (srvRes.SurfaceRes.Fragment0.ArraySize > 1)
                {
                    uint bytesPerArrayImage = (uint)srvRes.SurfaceRes.Fragment1.Data.Length / srvRes.SurfaceRes.Fragment0.ArraySize;

                    if (!importTexArray)
                    {
                        Buffer.BlockCopy(dds.bdata, 0, srvRes.SurfaceRes.Fragment1.Data, (int)(bytesPerArrayImage * _texArrayIndex), (int)bytesPerArrayImage);
                    }
                    else
                    {
                        if (dds.header10.arraySize <= 1)
                        {
                            throw new Exception("The texture array size must be greater than 1.");
                        }

                        imageByteData = dds.bdata;
                        srvRes.SurfaceRes.Fragment1.Data = imageByteData;
                        srvRes.SurfaceRes.Fragment0.ArraySize = dds.header10.arraySize;

                        // TODO: Add support for importing individual tex array slices
                        //imageByteData = new byte[bytesPerArrayImage];
                        //string input = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
                        //for (int i = 0; i < srvRes.SurfaceRes.Fragment0.ArraySize; ++i)
                        //{
                        //    Buffer.BlockCopy(imageData, (int)(bytesPerArrayImage * i), data, 0, (int)bytesPerArrayImage);
                        //    dds.bdata = data;
                        //    dds.Write(File.Open(output + "!!!" + i.ToString("000") + ".dds", FileMode.Create, FileAccess.Write, FileShare.Read), -1);
                        //}
                    }
                }
                else
                {
                    imageByteData = dds.bdata;
                    srvRes.SurfaceRes.Fragment1.Data = imageByteData;
                }
            }

            srvRes.Fragment0.ImageType = imageType;
            srvRes.Fragment0.MipMapCount = dds.header.mipMapCount;

            srvRes.SurfaceRes.Fragment0.ImageType = imageType;
            srvRes.SurfaceRes.Fragment0.Width = dds.header.width;
            srvRes.SurfaceRes.Fragment0.Height = dds.header.height;
            srvRes.SurfaceRes.Fragment0.MipMapCount = dds.header.mipMapCount;

            srvRes.ToResource(Texture);
        }

        #region Commands
        private readonly RelayCommand _texArrayIndexDownCommand;
        private readonly RelayCommand _texArrayIndexUpCommand;

        public RelayCommand TexArrayIndexDownCommand
        {
            get { return _texArrayIndexDownCommand; }
        }

        public RelayCommand TexArrayIndexUpCommand
        {
            get { return _texArrayIndexUpCommand; }
        }

        private bool TexArrayIndexDownCommand_CanExecute(object parameter)
        {
            if (TexArrayIndex == 0)
                return false;

            return true;
        }
        private void TexArrayIndexDownCommand_Execute(object parameter)
        {
            --TexArrayIndex;
        }
        private bool TexArrayIndexUpCommand_CanExecute(object parameter)
        {
            if (TexArrayIndex == TexArraySize - 1)
                return false;

            return true;
        }
        private void TexArrayIndexUpCommand_Execute(object parameter)
        {
            ++TexArrayIndex;
        }
        #endregion
    }
}
