using System.Text;
using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Data.Pkg;
using EgoEngineLibrary.Formats.Erp;
using EgoEngineLibrary.Frontend.ViewModels;

namespace EgoErpArchiver.ViewModels
{
    public class ErpPackageViewModel : ViewModelBase
    {
        private readonly ErpResourceViewModel resView;

        public ErpResource Resource
        {
            get { return resView.Resource; }
        }

        public ErpFragment Fragment { get; }

        public override string DisplayName
        {
            get { return ErpResourceExporter.GetFragmentFileName(Resource, Fragment); }
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
                    else Preview = string.Empty;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private string preview;
        public string Preview
        {
            get { return preview; }
            set { preview = value; OnPropertyChanged(nameof(Preview)); }
        }

        public ErpPackageViewModel(ErpResourceViewModel resView, ErpFragment fragment)
        {
            this.resView = resView;
            Fragment = fragment;
        }

        public void GetPreview()
        {
            try
            {
                using var ms = new MemoryStream();
                ExportPkg(ms);
                Preview = Encoding.UTF8.GetString(ms.GetBuffer().AsSpan(0, Convert.ToInt32(ms.Length)));
            }
            catch (Exception ex)
            {
                Preview = "Could not create preview!" + Environment.NewLine + Environment.NewLine + ex.Message;
            }
        }

        public void ExportPkg(Stream stream)
        {
            using var dataStream = Fragment.GetDataStream(true);
            var package = PkgFile.ReadPkg(dataStream);
            package.WriteJson(stream);
        }

        public void ImportPkg(Stream stream)
        {
            var pkg = PkgFile.ReadJson(stream);
            using var ms = new MemoryStream();
            pkg.WritePkg(ms);
            Fragment.SetData(ms.ToArray());
        }
    }
}
