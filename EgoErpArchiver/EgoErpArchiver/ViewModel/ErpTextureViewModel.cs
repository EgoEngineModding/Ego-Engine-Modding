using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Archive.Erp.Data;
using EgoEngineLibrary.Graphics;
using EgoEngineLibrary.Graphics.Dds;
using Microsoft.Win32;
using MiscUtil.Conversion;
using Pfim;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
        IImage image;
        GCHandle imageDataHandle;
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
                        GetPreview();
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
                        GetPreview();
                        resView.Select();
                    }
                    else 
                    {
                        CleanPreview();
                    }
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
            try
            {
                CleanPreview();

                using (MemoryStream ms = new MemoryStream())
                {
                    dds = ExportDDS(ms, true, false);
                    ms.Seek(0, SeekOrigin.Begin);
                    image = Pfim.Pfim.FromStream(ms);
                }

                imageDataHandle = System.Runtime.InteropServices.GCHandle.Alloc(image.Data, System.Runtime.InteropServices.GCHandleType.Pinned);
                var addr = imageDataHandle.AddrOfPinnedObject(); 
                var bsource = BitmapSource.Create(image.Width, image.Height, 96.0, 96.0,
                 PixelFormat(image), null, addr, image.DataLen, image.Stride);

                Width = (int)dds.header.width;
                Height = (int)dds.header.height;
                this.Preview = bsource;

                this.PreviewErrorVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                this.PreviewError = "Could not create preview! Export/Import may still work in certain circumstances." + Environment.NewLine + Environment.NewLine + ex.Message;
                this.PreviewErrorVisibility = Visibility.Visible;

                CleanPreview();
            }
            finally
            {
                dds = null;
            }
        }
        private void CleanPreview()
        {
            Preview = null;
            if (imageDataHandle.IsAllocated) imageDataHandle.Free();
            image?.Dispose();
            image = null;
        }
        private PixelFormat PixelFormat(IImage image)
        {
            switch (image.Format)
            {
                case ImageFormat.Rgb24:
                    return PixelFormats.Bgr24;
                case ImageFormat.Rgba32:
                    return PixelFormats.Bgra32;
                case ImageFormat.Rgb8:
                    return PixelFormats.Gray8;
                case ImageFormat.R5g5b5a1:
                case ImageFormat.R5g5b5:
                    return PixelFormats.Bgr555;
                case ImageFormat.R5g6b5:
                    return PixelFormats.Bgr565;
                default:
                    throw new Exception($"Unsupported preview for {image.Format} PixelFormat");
            }
        }

        public DdsFile ExportDDS(string fileName, bool isPreview, bool exportTexArray)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                return ExportDDS(fs, isPreview, exportTexArray);
        }
        private DdsFile ExportDDS(Stream stream, bool isPreview, bool exportTexArray)
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
                case ErpGfxSurfaceFormat.BC2_SRGB:
                    TextureInfo += "BC2_SRGB"; // F1 2020,fom_car,myteam_logo
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
                    TextureInfo += Environment.NewLine + "Mipmaps not found! Make sure they are in the correct game folder!";
                }
                else
                {
                    throw new FileNotFoundException("Mipmaps file not found!", mipMapFullFileName);
                }
            }

            DdsFile dds;
            Stream mipMapStream = foundMipMapFile ? File.Open(mipMapFullFileName, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
            using (mipMapStream)
            {
                dds = srvRes.ToDdsFile(mipMapStream, exportTexArray, _texArrayIndex);
            }

            dds.Write(stream, -1);
            return dds;
        }
        public void ImportDDS(string fileName, string mipMapSaveLocation, bool importTexArray)
        {
            DdsFile dds = new DdsFile(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));

            ErpGfxSRVResource srvRes = new ErpGfxSRVResource();
            srvRes.FromResource(Texture);

            bool foundMipMapSaveLocation = false;
            if (srvRes.SurfaceRes.HasMips)
            {
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

                if (!foundMipMapSaveLocation)
                {
                    return;
                }
            }
            
            Stream mipMapStream = foundMipMapSaveLocation ? File.Open(mipMapSaveLocation, FileMode.Create, FileAccess.Write, FileShare.Read) : null;
            using (mipMapStream)
            {
                dds.ToErpGfxSRVResource(srvRes, mipMapStream, importTexArray, _texArrayIndex);
            }

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
