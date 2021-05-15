using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Data.Pkg;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EgoErpArchiver.ViewModel
{
    public class ErpPackageViewModel : ViewModelBase
    {
        private readonly ErpResourceViewModel resView;

        public ErpResource Package
        {
            get { return resView.Resource; }
        }
        public override string DisplayName
        {
            get { return Package.FileName; }
        }

        #region Presentation Props
        private bool isSelected;
        private string preview;

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
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public string Preview
        {
            get { return preview; }
            set { preview = value; OnPropertyChanged(nameof(Preview)); }
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
                var sb = new StringBuilder();
                ExportPkg(new StringWriter(sb));
                Preview = sb.ToString();
            }
            catch (Exception ex)
            {
                Preview = "Could not create preview!" + Environment.NewLine + Environment.NewLine + ex.Message;
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
            var pkg = PkgFile.ReadJson(stream);
            using var pkgData = new MemoryStream();
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
