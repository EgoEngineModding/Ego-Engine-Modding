using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
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
                    OnPropertyChanged("IsSelected");
                }
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

        public PssgTextureViewModel(PssgNodeViewModel nodeView)
        {
            this.nodeView = nodeView;
        }

        public void GetPreview()
        {
            Image<Rgba32> image = null;
            bool ddsReadSuccess = false;
            try
            {
                Preview = null;

                // Write and decode dds
                using (MemoryStream ms = new MemoryStream())
                {
                    var dds = Texture.ToDdsFile(false);
                    dds.Write(ms, -1);
                    ddsReadSuccess = true;
                    ms.Seek(0, SeekOrigin.Begin);

                    BcDecoder decoder = new BcDecoder();
                    image = decoder.DecodeToImageRgba32(ms);
                }

                // Copy pixels to WPF format
                var pixels = new byte[image.Width * image.Height * 4];
                var pixelsSpan = MemoryMarshal.Cast<byte, Bgra32>(pixels);
                for (int r = 0; r < image.Height; ++r)
                {
                    var destRow = pixelsSpan.Slice(r * image.Width, image.Width);
                    var sorcRow = image.GetPixelRowSpan(r);
                    PixelOperations<Rgba32>.Instance.ToBgra32(Configuration.Default, sorcRow, destRow);
                    
                }
                var bmSource = BitmapSource.Create(image.Width, image.Height, 96.0, 96.0, PixelFormats.Bgra32, null, pixels, image.Width * 4);
                this.Preview = bmSource;
                
                this.PreviewErrorVisibility = Visibility.Collapsed;
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(Height));
                OnPropertyChanged(nameof(TextureInfo));
            }
            catch (Exception ex)
            {
                Preview = null;
                if (ddsReadSuccess)
                    this.PreviewError = "Could not create preview! Export/Import may still work." + Environment.NewLine + Environment.NewLine + ex.Message;
                else
                    this.PreviewError = "Could not create preview! Failed to convert pssg texture to dds." + Environment.NewLine + Environment.NewLine + ex.Message;
                this.PreviewErrorVisibility = Visibility.Visible;
            }
            finally
            {
                image?.Dispose();
            }
        }
    }
}
