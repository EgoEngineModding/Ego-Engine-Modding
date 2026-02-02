using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Data.Pkg;
using EgoEngineLibrary.Formats.Erp;
using System;
using System.IO;

namespace EgoErpArchiver.ViewModel
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
                    else preview = string.Empty;
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
                using var sw = new StringWriter();
                ExportPkg(sw);
                Preview = sw.GetStringBuilder().ToString();
            }
            catch (Exception ex)
            {
                Preview = "Could not create preview!" + Environment.NewLine + Environment.NewLine + ex.Message;
            }
        }

        public void ExportPkg(TextWriter textWriter)
        {
            using var dataStream = Fragment.GetDataStream(true);
            var package = PkgFile.ReadPkg(dataStream);
            package.WriteJson(textWriter);
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
