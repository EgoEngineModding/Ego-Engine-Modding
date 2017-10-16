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
            ErpGfxSRVResource srvRes = new ErpGfxSRVResource();
            srvRes.FromResource(Texture);

            _texArraySize = srvRes.SurfaceRes.Fragment0.ArraySize;
            _textureInfo = srvRes.SurfaceRes.Fragment0.Width + "x" + srvRes.SurfaceRes.Fragment0.Height + " Mips:" + (srvRes.SurfaceRes.Fragment0.MipMapCount) + " Format:" + srvRes.SurfaceRes.Fragment0.ImageType + ",";
            switch (srvRes.SurfaceRes.Fragment0.ImageType)
            {
                //case (ErpGfxSurfaceFormat)14: // gameparticles k_smoke; application
                case ErpGfxSurfaceFormat.ABGR8:
                    if (srvRes.SurfaceRes.Fragment0.ArraySize > 1 && exportTexArray)
                    {
                        TextureInfo += "RGBA8";
                    }
                    else
                    {
                        TextureInfo += "ABGR8";
                    }
                    break;
                case ErpGfxSurfaceFormat.DXT1: // ferrari_wheel_sfc
                    TextureInfo += "DXT1";
                    break;
                case ErpGfxSurfaceFormat.DXT1_SRGB: // ferrari_wheel_df, ferrari_paint
                    TextureInfo += "BC1_SRGB";
                    break;
                case ErpGfxSurfaceFormat.DXT5: // ferrari_sfc
                    TextureInfo += "DXT5";
                    break;
                case ErpGfxSurfaceFormat.DXT5_SRGB: // ferrari_decal
                    TextureInfo += "BC3_SRGB";
                    break;
                case ErpGfxSurfaceFormat.ATI1: // gameparticles k_smoke
                    TextureInfo += "ATI1";
                    break;
                case ErpGfxSurfaceFormat.ATI2: // ferrari_wheel_nm
                    TextureInfo += "ATI2/3Dc";
                    break;
                case ErpGfxSurfaceFormat.BC6: // key0_2016; environment abu_dhabi tree_palm_06
                    TextureInfo += "BC6H_UF16";
                    break;
                case ErpGfxSurfaceFormat.BC7:
                    TextureInfo += "BC7";
                    break;
                case ErpGfxSurfaceFormat.BC7_SRGB: // flow_boot splash_bg_image
                    TextureInfo += "BC7_SRGB";
                    break;
                default:
                    TextureInfo += "Unknown";
                    throw new Exception("Image format not supported!");
            }

            string mipMapFullFileName = Path.Combine(Properties.Settings.Default.F12016Dir, srvRes.SurfaceRes.Frag2.MipMapFileName);
            bool foundMipMapFile = File.Exists(mipMapFullFileName);
            bool hasValidMips = srvRes.SurfaceRes.HasValidMips;
            if (srvRes.SurfaceRes.HasMips)
            {
                if (hasValidMips && !foundMipMapFile && !isPreview)
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
                    foundMipMapFile = foundMipMapFile && File.Exists(mipMapFullFileName);
                }

                if (!hasValidMips)
                {
                    // This usually happens when the mips fragment was never updated with the new offsets/sizes
                    TextureInfo += Environment.NewLine + "Mipmaps are incorrectly modded! Try exporting, then importing to fix the issue.";
                }
                else if (foundMipMapFile)
                {
                    TextureInfo += Environment.NewLine + srvRes.SurfaceRes.MipWidth + "x" + srvRes.SurfaceRes.MipHeight + " Mips:" + (srvRes.SurfaceRes.Frag2.Mips.Count);
                }
                else if (isPreview)
                {
                    // Not found during preview
                    TextureInfo += Environment.NewLine + "Mipmaps not found! Make sure they are in the correct F1 game folder!";
                }
                else
                {
                    throw new FileNotFoundException("Mipmaps file not found!", mipMapFullFileName);
                }
            }

            var dds = srvRes.ToDdsFile(mipMapFullFileName, exportTexArray, _texArrayIndex);
            dds.Write(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), -1);

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
