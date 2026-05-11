using BCnEncoder.Decoder;
using EgoEngineLibrary.Graphics;

using SixLabors.ImageSharp.PixelFormats;

using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using EgoEngineLibrary.Graphics.Pssg.Elements;

namespace EgoPssgEditor.ViewModels
{
    public class PssgTextureViewModel : ViewModelBase
    {
        readonly PssgElementViewModel _elementView;

        public PssgTexture Texture
        {
            get { return (PssgTexture)_elementView.Element; }
        }
        public PssgElementViewModel ElementView
        {
            get { return _elementView; }
        }
        public override string DisplayName
        {
            get { return Texture.Id; }
        }
        public uint Width
        {
            get { return Texture.Width; }
        }
        public uint Height
        {
            get { return Texture.Height; }
        }
        private string Format => Texture.TexelFormat;
        private uint MipMaps => Texture.MipMapCount;
        public string TextureInfo => $"{Width}x{Height} MipMaps: {MipMaps} Format: {Format}";

        #region Presentation Props
        bool isSelected;
        Bitmap? preview;
        string previewError;
        bool previewErrorVisibility;

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
                        ElementView.IsSelected = true;
                    }
                    else 
                    {
                        Preview?.Dispose();
                        Preview = null;
                        ElementView.IsSelected = false;
                    }
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public Bitmap? Preview
        {
            get { return preview; }
            set { preview = value; OnPropertyChanged(nameof(Preview)); }
        }
        public string PreviewError
        {
            get { return previewError; }
            set { previewError = value; OnPropertyChanged(nameof(PreviewError)); }
        }
        public bool PreviewErrorVisibility
        {
            get { return previewErrorVisibility; }
            set { previewErrorVisibility = value; OnPropertyChanged(nameof(PreviewErrorVisibility)); }
        }
        #endregion

        public PssgTextureViewModel(PssgElementViewModel elementView)
        {
            this._elementView = elementView;
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
                    var dds = Texture.ToDdsFile();
                    dds.Write(ms);
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

                PreviewErrorVisibility = false;
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(Height));
                OnPropertyChanged(nameof(TextureInfo));
            }
            catch (Exception ex)
            {
                Preview?.Dispose();
                Preview = null;
                if (ddsReadSuccess)
                    PreviewError = "Could not create preview! Export/Import may still work." + Environment.NewLine + Environment.NewLine + ex.Message;
                else
                    PreviewError = "Could not create preview! Failed to convert pssg texture to dds." + Environment.NewLine + Environment.NewLine + ex.Message;
                PreviewErrorVisibility = true;
            }
        }
    }
}
