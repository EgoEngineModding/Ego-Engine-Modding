using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Xml;
using System;
using System.IO;
using System.Text;

namespace EgoErpArchiver.ViewModel
{
    public class ErpXmlFileViewModel : ViewModelBase
    {
        private readonly ErpResourceViewModel resView;

        public ErpResource XmlFile
        {
            get { return resView.Resource; }
        }

        #region Presentation Props
        private bool isSelected;
        private string preview;

        public override string DisplayName
        {
            get { return XmlFile.FileName; }
        }
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
        public string Preview
        {
            get { return preview; }
            set { preview = value; OnPropertyChanged(nameof(Preview)); }
        }
        #endregion

        public ErpXmlFileViewModel(ErpResourceViewModel resView)
        {
            this.resView = resView;
        }

        public void GetPreview()
        {
            try
            {
                var xml = new XmlFile(XmlFile.Fragments[0].GetDataStream(true));
                var set = new System.Xml.XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true
                };
                using var stringWriter = new StringWriter();
                using var xmlTextWriter = System.Xml.XmlWriter.Create(stringWriter, set);
                xml.doc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                Preview = stringWriter.GetStringBuilder().ToString();
            }
            catch (Exception ex)
            {
                Preview = "Could not create preview!" + Environment.NewLine + Environment.NewLine + ex.Message;
            }
        }
        public void ExportXML(Stream stream)
        {
            var xml = new XmlFile(XmlFile.Fragments[0].GetDataStream(true));
            xml.Write(stream, XMLType.Text);
        }
        public void ImportXML(Stream stream)
        {
            var xml = new XmlFile(stream);
            using var xmlData = new MemoryStream();
            xml.Write(xmlData);

            XmlFile.Fragments[0].SetData(xmlData.ToArray());
        }
    }
}
