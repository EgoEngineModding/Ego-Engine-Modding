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
                StringBuilder sb = new StringBuilder();
                ExportPkg(new StringWriter(sb));
                this.Preview = sb.ToString();
            }
            catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
            {
                this.Preview = "Could not create preview!" + Environment.NewLine + Environment.NewLine + ex.Message;
            }
        }
        public void ExportPkg(TextWriter textWriter)
        {
            PkgFile package;

            switch (Package.ResourceType)
            {
                case "AnimClip":
                case "AnimClipCrowd":
                    package = PkgFile.ReadPkg(Package.GetFragment("temp", 0).GetDataStream(true));
                    break;
                case "EventGraph":
                    package = PkgFile.ReadPkg(Package.GetFragment("node", 0).GetDataStream(true));
                    break;
                default:
                    package = PkgFile.ReadPkg(Package.Fragments[0].GetDataStream(true));
                    break;
            }

            package.WriteJson(textWriter);
        }
        public void ImportPkg(Stream stream)
        {
            PkgFile pkg = PkgFile.ReadJson(stream);
            using (MemoryStream pkgData = new MemoryStream())
            {
                pkg.WritePkg(pkgData);

                switch (Package.ResourceType)
                {
                    case "AnimClip":
                    case "AnimClipCrowd":
                        Package.GetFragment("temp", 0).SetData(pkgData.ToArray());
                        break;
                    case "EventGraph":
                        Package.GetFragment("node", 0).SetData(pkgData.ToArray());
                        break;
                    default:
                        Package.Fragments[0].SetData(pkgData.ToArray());
                        break;
                }
            }
        }
    }
}
