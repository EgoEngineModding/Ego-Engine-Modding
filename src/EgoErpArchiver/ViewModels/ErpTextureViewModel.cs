using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using BCnEncoder.Decoder;

using CommunityToolkit.Mvvm.Input;

using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Archive.Erp.Data;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Graphics;
using EgoEngineLibrary.Graphics.Dds;

using SixLabors.ImageSharp.PixelFormats;

namespace EgoErpArchiver.ViewModels
{
    public class ErpTextureViewModel : ViewModelBase
    {
        private readonly ErpResourceViewModel resView;

        public ErpResource Resource
        {
            get { return resView.Resource; }
        }

        public override string DisplayName
        {
            get { return Resource.FileName; }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }

        private int _height;
        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }

        private uint _texArraySize;
        public uint TexArraySize
        {
            get { return _texArraySize; }
            set { _texArraySize = value; }
        }

        private uint _texArrayIndex;
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

        private bool isSelected;
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
                        Preview?.Dispose();
                        Preview = null;
                    }
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private string _textureInfo;
        public string TextureInfo
        {
            get { return _textureInfo; }
            set
            {
                _textureInfo = value;
                OnPropertyChanged(nameof(TextureInfo));
            }
        }

        public Bitmap Preview
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        private string previewError;
        public string PreviewError
        {
            get { return previewError; }
            set { previewError = value; OnPropertyChanged(nameof(PreviewError)); }
        }

        public bool PreviewErrorVisibility
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand TexArrayIndexDownCommand { get; }

        public RelayCommand TexArrayIndexUpCommand { get; }

        public ErpTextureViewModel(ErpResourceViewModel resView)
        {
            this.resView = resView;
            _width = 0;
            _height = 0;

            TexArrayIndexDownCommand = new RelayCommand(TexArrayIndexDownCommand_Execute, TexArrayIndexDownCommand_CanExecute);
            TexArrayIndexUpCommand = new RelayCommand(TexArrayIndexUpCommand_Execute, TexArrayIndexUpCommand_CanExecute);
        }

        public void GetPreview()
        {
            var ddsReadSuccess = false;
            try
            {
                Preview?.Dispose();
                Preview = null;
                BCnEncoder.Shared.ImageFiles.DdsFile bcDds;
                using (var ms = new MemoryStream())
                {
                    var dds = ExportDDS(ms, true, false);
                    ddsReadSuccess = true;

                    ms.Seek(0, SeekOrigin.Begin);
                    bcDds = BCnEncoder.Shared.ImageFiles.DdsFile.Load(ms);
                }

                var width = (int)bcDds.header.dwWidth;
                var height = (int)bcDds.header.dwHeight;
                var decoder = new BcDecoder();
                if (decoder.IsHdrFormat(bcDds))
                {
                    var bmSource = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96),
                        PixelFormats.Bgr24);
                    using (var frameBuffer = bmSource.Lock())
                    {
                        unsafe
                        {
                            var pixelsSpan = new Span<Bgr24>(frameBuffer.Address.ToPointer(), width * height * 3);
                            decoder.DecodeDdsToPixels(bcDds, pixelsSpan);
                        }
                    }

                    Preview = bmSource;
                }
                else
                {
                    var bmSource = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96),
                        PixelFormats.Bgra8888, AlphaFormat.Unpremul);
                    using (var frameBuffer = bmSource.Lock())
                    {
                        unsafe
                        {
                            var pixelsSpan = new Span<Bgra32>(frameBuffer.Address.ToPointer(), width * height * 4);
                            decoder.DecodeDdsToPixels(bcDds, pixelsSpan);
                        }
                    }

                    Preview = bmSource;
                }

                Width = width;
                Height = height;
                PreviewErrorVisibility = false;
            }
            catch (Exception ex)
            {
                Preview?.Dispose();
                Preview = null;
                if (ddsReadSuccess)
                    PreviewError = "Could not create preview! Export/Import may still work." + Environment.NewLine + Environment.NewLine + ex.Message;
                else
                    PreviewError = "Could not create preview! Failed to convert texture to dds." + Environment.NewLine + Environment.NewLine + ex.Message;
                PreviewErrorVisibility = true;
            }
        }

        public async Task<DdsFile> ExportDDS(string fileName, bool isPreview, bool exportTexArray)
        {
            await using var fs = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            return await ExportDDS(fs, isPreview, exportTexArray);
        }

        private async Task<DdsFile> ExportDDS(Stream stream, bool isPreview, bool exportTexArray)
        {
            var srvRes = new ErpGfxSRVResource();
            srvRes.FromResource(Resource);

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

            var mipMapFullFileName = Path.Combine(Properties.Settings.Default.F12016Dir, srvRes.SurfaceRes.Frag2.MipMapFileName);
            var foundMipMapFile = File.Exists(mipMapFullFileName);
            var hasValidMips = srvRes.SurfaceRes.HasValidMips;
            if (srvRes.SurfaceRes.HasMips)
            {
                if (hasValidMips && !foundMipMapFile && !isPreview)
                {
                    var odialog = new FileOpenOptions
                    {
                        FileTypeChoices = [FilePickerType.Mipmaps, FilePickerType.All],
                        Title = "Select a mipmaps file",
                        FileName = Path.GetFileName(mipMapFullFileName)
                    };
                    mipMapFullFileName = Path.GetDirectoryName(mipMapFullFileName);
                    if (Directory.Exists(mipMapFullFileName))
                    {
                        odialog.InitialDirectory = mipMapFullFileName;
                    }

                    var res = await FileDialog.ShowOpenFileDialog(odialog);
                    foundMipMapFile = res.Count > 0;
                    mipMapFullFileName = res[0];
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
                    // User hasn't specified mipmaps file
                    throw new FileNotFoundException("Mipmaps file not found!", mipMapFullFileName);
                }
            }

            if (foundMipMapFile)
            {
                // Hack to allow exporting when mipmaps file has unknown compression types. Will ignore mipmaps data
                var unsupportedCompressionTypes = string.Join(',',
                    srvRes.SurfaceRes.Frag2.Mips.Select(x => x.Compression).Distinct().Where(x => x.IsUnknown()));
                if (unsupportedCompressionTypes is not "")
                {
                    TextureInfo += Environment.NewLine + $"Mipmaps compression type(s) not supported! {unsupportedCompressionTypes}";
                    foundMipMapFile = false;
                }
            }

            DdsFile dds;
            Stream mipMapStream = foundMipMapFile ? File.Open(mipMapFullFileName, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
            await using (mipMapStream)
            {
                dds = srvRes.ToDdsFile(mipMapStream, exportTexArray, _texArrayIndex);
            }

            dds.Write(stream, -1);
            return dds;
        }

        public async Task ImportDDS(string fileName, string? mipMapSaveLocation, bool importTexArray)
        {
            var dds = new DdsFile(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));

            var srvRes = new ErpGfxSRVResource();
            srvRes.FromResource(Resource);

            var foundMipMapSaveLocation = false;
            if (srvRes.SurfaceRes.HasMips)
            {
                if (string.IsNullOrEmpty(mipMapSaveLocation))
                {
                    var sdialog = new FileSaveOptions
                    {
                        FileTypeChoices = [FilePickerType.Mipmaps, FilePickerType.All],
                        Title = "Select the mipmaps save location and file name",
                        FileName = Path.GetFileName(srvRes.SurfaceRes.Frag2.MipMapFileName)
                    };
                    var mipFullPath = Path.GetDirectoryName(Path.Combine(Properties.Settings.Default.F12016Dir, srvRes.SurfaceRes.Frag2.MipMapFileName));
                    if (Directory.Exists(mipFullPath))
                    {
                        sdialog.InitialDirectory = mipFullPath;
                    }

                    mipMapSaveLocation = await FileDialog.ShowSaveFileDialog(sdialog);
                    foundMipMapSaveLocation = mipMapSaveLocation is not null;
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

            srvRes.ToResource(Resource);
        }

        private bool TexArrayIndexDownCommand_CanExecute()
        {
            if (TexArrayIndex == 0)
                return false;

            return true;
        }

        private void TexArrayIndexDownCommand_Execute()
        {
            --TexArrayIndex;
        }

        private bool TexArrayIndexUpCommand_CanExecute()
        {
            if (TexArrayIndex == TexArraySize - 1)
                return false;

            return true;
        }

        private void TexArrayIndexUpCommand_Execute()
        {
            ++TexArrayIndex;
        }
    }
}
