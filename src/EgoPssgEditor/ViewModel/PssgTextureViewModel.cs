using BCnEncoder.Decoder;
using EgoEngineLibrary.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EgoPssgEditor.ViewModel
{
    public class PssgTextureViewModel : ViewModelBase
    {
        readonly PssgNodeViewModel nodeView;

        public PssgNode Texture
        {
            get { return nodeView.Node; }
        }
        public PssgNodeViewModel NodeView
        {
            get { return nodeView; }
        }
        public override string DisplayName
        {
            get { return Texture.Attributes["id"].DisplayValue; }
        }
        public int Width
        {
            get { return (int)(uint)Texture.Attributes["width"].Value; }
        }
        public int Height
        {
            get { return (int)(uint)Texture.Attributes["height"].Value; }
        }
        private string Format => (string)Texture.Attributes["texelFormat"].Value;
        private uint MipMaps => Texture.HasAttribute("numberMipMapLevels") ? Texture.Attributes["numberMipMapLevels"].GetValue<uint>() : 0u;
        public string TextureInfo => $"{Width}x{Height} MipMaps: {MipMaps} Format: {Format}";

        #region Presentation Props
        bool isSelected;
        BitmapSource preview;
        string previewError;
        Visibility previewErrorVisibility;

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
                        NodeView.IsSelected = true;
                    }
                    else 
                    {
                        Preview = null;
                    }
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public BitmapSource Preview
        {
            get { return preview; }
            set { preview = value; OnPropertyChanged(nameof(Preview)); }
        }
        public string PreviewError
        {
            get { return previewError; }
            set { previewError = value; OnPropertyChanged(nameof(PreviewError)); }
        }
        public Visibility PreviewErrorVisibility
        {
            get { return previewErrorVisibility; }
            set { previewErrorVisibility = value; OnPropertyChanged(nameof(PreviewErrorVisibility)); }
        }
        #endregion

        public PssgTextureViewModel(PssgNodeViewModel nodeView)
        {
            this.nodeView = nodeView;
        }

        public void GetPreview()
        {
            var ddsReadSuccess = false;
            try
            {
                Preview = null;
                BCnEncoder.Shared.ImageFiles.DdsFile bcDds;
                using (var ms = new MemoryStream())
                {
                    var dds = Texture.ToDdsFile(false);
                    dds.Write(ms, -1);
                    ddsReadSuccess = true;

                    ms.Seek(0, SeekOrigin.Begin);
                    bcDds = BCnEncoder.Shared.ImageFiles.DdsFile.Load(ms);
                }

                var width = (int)bcDds.header.dwWidth;
                var height = (int)bcDds.header.dwHeight;
                var decoder = new BcDecoder();
                if (decoder.IsHdrFormat(bcDds))
                {
                    var pixels = new byte[width * height * 3];
                    var pixelsSpan = MemoryMarshal.Cast<byte, Bgr24>(pixels);
                    decoder.DecodeDdsToPixels(bcDds, pixelsSpan);
                    var bmSource = BitmapSource.Create(width, height, 96.0, 96.0, PixelFormats.Bgr24, null, pixels, width * 3);
                    Preview = bmSource;
                }
                else
                {
                    var pixels = new byte[width * height * 4];
                    var pixelsSpan = MemoryMarshal.Cast<byte, Bgra32>(pixels);
                    decoder.DecodeDdsToPixels(bcDds, pixelsSpan);
                    var bmSource = BitmapSource.Create(width, height, 96.0, 96.0, PixelFormats.Bgra32, null, pixels, width * 4);
                    Preview = bmSource;
                }

                PreviewErrorVisibility = Visibility.Collapsed;
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(Height));
                OnPropertyChanged(nameof(TextureInfo));
            }
            catch (Exception ex)
            {
                Preview = null;
                if (ddsReadSuccess)
                    PreviewError = "Could not create preview! Export/Import may still work." + Environment.NewLine + Environment.NewLine + ex.Message;
                else
                    PreviewError = "Could not create preview! Failed to convert pssg texture to dds." + Environment.NewLine + Environment.NewLine + ex.Message;
                PreviewErrorVisibility = Visibility.Visible;
            }
        }
    }
}
