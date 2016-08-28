using EgoEngineLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
                        Task.Run(() => GetPreview()).Wait();
                        NodeView.IsSelected = true;
                    }
                    else { preview = null; }
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
            DdsFile dds;
            CSharpImageLibrary.ImageEngineImage image = null;
            try
            {
                this.Preview = null;
                dds = new DdsFile(Texture, false);
                dds.Write(File.Open(System.AppDomain.CurrentDomain.BaseDirectory + "\\temp.dds", FileMode.Create, FileAccess.ReadWrite, FileShare.Read), -1);
                int maxDimension = (int)Math.Max(dds.header.width, dds.header.height);

                image = new CSharpImageLibrary.ImageEngineImage(System.AppDomain.CurrentDomain.BaseDirectory + "\\temp.dds", maxDimension, false);
                Preview = null;
                this.Preview = image.GetWPFBitmap();

                dds = null;
                image.Dispose();
                image = null;
                this.PreviewErrorVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Preview = null;
                dds = null;
                image?.Dispose();
                image = null;
                this.PreviewError = "Could not create preview! Export/Import may still work in certain circumstances." + Environment.NewLine + Environment.NewLine + ex.Message;
                this.PreviewErrorVisibility = Visibility.Visible;
            }
        }
    }
}
