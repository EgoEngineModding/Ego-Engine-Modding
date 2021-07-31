using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Formats.Erp;
using EgoEngineLibrary.Xml;
using System;
using System.IO;
using System.Text;

namespace EgoErpArchiver.ViewModel
{
    public class ErpXmlFileViewModel : ViewModelBase
    {
        private readonly ErpResourceViewModel resView;

        public ErpResource Resource => resView.Resource;

        public ErpFragment Fragment { get; }

        public override string DisplayName => ErpResourceExporter.GetFragmentFileName(Resource, Fragment);

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

        public ErpXmlFileViewModel(ErpResourceViewModel resView, ErpFragment fragment)
        {
            this.resView = resView;
            Fragment = fragment;
        }

        public void GetPreview()
        {
            try
            {
                using var sw = new Utf8StringWriter();
                ExportXML(sw);
                Preview = sw.GetStringBuilder().ToString();
            }
            catch (Exception ex)
            {
                Preview = "Could not create preview!" + Environment.NewLine + Environment.NewLine + ex.Message;
            }
        }

        public void ExportXML(TextWriter textWriter)
        {
            using var dataStream = Fragment.GetDataStream(true);
            var xml = new XmlFile(dataStream);
            xml.WriteXml(textWriter);
        }

        public void ImportXML(Stream stream)
        {
            var xml = new XmlFile(stream);
            using var ms = new MemoryStream();
            xml.Write(ms);
            Fragment.SetData(ms.ToArray());
        }

        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;

            public Utf8StringWriter()
                : base()
            {
            }
        }
    }
}
