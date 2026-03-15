using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Input;
using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;
using EgoEngineLibrary.Graphics.Pssg;
using EgoPssgEditor.Dialogs.Pssg;

namespace EgoPssgEditor.ViewModels
{
    public partial class ElementsWorkspaceViewModel : WorkspaceViewModel
    {
        #region Data
        PssgElementViewModel _rootElement;
        readonly ObservableCollection<PssgElementViewModel> pssgElements;

        public override string DisplayName
        {
            get
            {
                return "All Elements";
            }
        }
        public PssgElementViewModel RootElement
        {
            get { return _rootElement; }
            private set
            {
                ClearData();
                _rootElement = value;
                pssgElements.Add(_rootElement);
            }
        }
        public ObservableCollection<PssgElementViewModel> PssgElements
        {
            get { return pssgElements; }
        }
        #endregion

        #region Presentation Props
        #endregion

        public ElementsWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            pssgElements = new ObservableCollection<PssgElementViewModel>();
        }

        public override void LoadData(object data)
        {
            RootElement = new PssgElementViewModel(((PssgFile)data).RootElement);
            RootElement.IsExpanded = true;
        }

        public override void ClearData()
        {
            _rootElement = null;
            pssgElements.Clear();
        }

        #region Menu

        private bool Export_CanExecute(object parameter)
        {
            return parameter != null;
        }
        [RelayCommand(CanExecute = nameof(Export_CanExecute))]
        private async Task Export(object parameter)
        {
            PssgElement element = ((PssgElementViewModel)parameter).Element;
            FileSaveOptions saveOptions = new()
            {
                FileTypeChoices = [FilePickerType.Xml, FilePickerType.All],
                Title = "Select the element's save location and file name",
                DefaultExtension = "xml",
                FileName = "element.xml",
            };

            var result = await FileDialog.ShowSaveFileDialog(saveOptions);
            if (result is not null)
            {
                try
                {
                    XDocument xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
                    xDoc.Add(new XElement("PSSGFILE", new XAttribute("version", "1.0.0.0")));
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Encoding = new UTF8Encoding(false);
                    settings.NewLineChars = "\n";
                    settings.Indent = true;
                    settings.IndentChars = "";
                    settings.CloseOutput = true;

                    XElement pssg = (XElement)xDoc.FirstNode;
                    element.WriteXml(pssg);

                    using (XmlWriter writer = XmlWriter.Create(File.Open(result, FileMode.Create, FileAccess.Write, FileShare.Read), settings))
                    {
                        xDoc.Save(writer);
                    }
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not export the element!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool Import_CanExecute(object parameter)
        {
            return parameter != null;
        }
        [RelayCommand(CanExecute = nameof(Import_CanExecute))]
        private async Task Import(object parameter)
        {
            PssgElementViewModel elementView = (PssgElementViewModel)parameter;
            FileOpenOptions openOptions = new()
            {
                FileTypeChoices = [FilePickerType.Xml, FilePickerType.All],
                Title = "Select a xml file",
                FileName = "element.xml",
            };

            var result = await FileDialog.ShowOpenFileDialog(openOptions);
            if (result.Count > 0)
            {
                try
                {
                    PssgElement element = elementView.Element;
                    using (FileStream fileStream = File.Open(result[0], FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        XDocument xDoc = XDocument.Load(fileStream);

                        PssgElement newElement = PssgElement.ReadXml((XElement)((XElement)xDoc.FirstNode).FirstNode, element.File, element.ParentElement);
                        if (element.ParentElement != null)
                        {
                            element = element.ParentElement.SetChild(element, newElement);
                            int index = elementView.Parent.Children.IndexOf(elementView);
                            PssgElementViewModel newElementView = new PssgElementViewModel(element, elementView.Parent);
                            elementView.Parent.Children[index] = newElementView;
                            newElementView.IsSelected = true;
                        }
                        else
                        {
                            element.File.RootElement = newElement;
                            LoadData(element.File);
                            _rootElement.IsSelected = true;
                        }

                        mainView.TexturesWorkspace.ClearData();
                        mainView.TexturesWorkspace.LoadData(RootElement);
                    }
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not import the element!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportData_CanExecute(object parameter)
        {
            if (parameter == null) return false;

            return ((PssgElementViewModel)parameter).IsDataElement;
        }
        [RelayCommand(CanExecute = nameof(ExportData_CanExecute))]
        private async Task ExportData(object parameter)
        {
            PssgElementViewModel elementView = (PssgElementViewModel)parameter;
            FileSaveOptions saveOptions = new()
            {
                FileTypeChoices = [FilePickerType.Bin, FilePickerType.All],
                Title = "Select the byte data save location and file name",
                DefaultExtension = "bin",
                FileName = "elementData.bin",
            };

            var result = await FileDialog.ShowSaveFileDialog(saveOptions);
            if (result is not null)
            {
                try
                {
                    PssgElement element = elementView.Element;
                    using (var fs = File.Open(result, FileMode.Create))
                    using (PssgBinaryWriter writer = new PssgBinaryWriter(EndianBitConverter.Big, fs, false))
                    {
                        writer.Write(element.Value);
                    }
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not export data!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool ImportData_CanExecute(object parameter)
        {
            if (parameter == null) return false;

            return ((PssgElementViewModel)parameter).IsDataElement;
        }
        [RelayCommand(CanExecute = nameof(ImportData_CanExecute))]
        private async Task ImportData(object parameter)
        {
            PssgElementViewModel elementView = (PssgElementViewModel)parameter;
            FileOpenOptions openOptions = new()
            {
                FileTypeChoices = [FilePickerType.Bin, FilePickerType.All],
                Title = "Select a bin file",
                FileName = "elementData.bin",
            };

            var result = await FileDialog.ShowOpenFileDialog(openOptions);
            if (result.Count > 0)
            {
                try
                {
                    PssgElement element = elementView.Element;
                    using (var fs = File.Open(result[0], FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (PssgBinaryReader reader = new PssgBinaryReader(EndianBitConverter.Big, fs, false))
                    {
                        element.Value = reader.ReadElementValue((int)reader.BaseStream.Length);
                        elementView.IsSelected = false;
                        elementView.IsSelected = true;
                    }
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not import data!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool AddElement_CanExecute(object parameter)
        {
            return parameter != null;
        }
        [RelayCommand(CanExecute = nameof(AddElement_CanExecute))]
        private async Task AddElement(object parameter)
        {
            PssgElementViewModel elementView = (PssgElementViewModel)parameter;
            if (elementView.Element.IsDataElement)
            {
                await MessageBox.Show("Adding children to an element with data is not allowed!", Properties.Resources.AppTitleShort, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            var elementName = await PssgDialog.ShowAddElementDialog();
            if (elementName is not null)
            {
                PssgElement newElement = elementView.Element.AppendChild(elementName);

                if (newElement == null)
                {
                    return;
                }

                PssgElementViewModel newElementView = new PssgElementViewModel(newElement, elementView);
                elementView.Children.Add(newElementView);
            }
        }
        private bool RemoveElement_CanExecute(object parameter)
        {
            return parameter != null && parameter != RootElement;
        }
        [RelayCommand(CanExecute = nameof(RemoveElement_CanExecute))]
        private void RemoveElement(object parameter)
        {
            PssgElementViewModel elementView = (PssgElementViewModel)parameter;
            elementView.Element.ParentElement.RemoveChild(elementView.Element);

            elementView.Parent.Children.Remove(elementView);
            mainView.TexturesWorkspace.RemoveTexture(elementView);
        }
        private bool CloneElement_CanExecute(object parameter)
        {
            return parameter != null && parameter != RootElement;
        }
        [RelayCommand(CanExecute = nameof(CloneElement_CanExecute))]
        private async Task CloneElement(object parameter)
        {
            try
            {
                PssgElementViewModel elementView = (PssgElementViewModel)parameter;
                var newElement = elementView.Element.File.CloneElement(elementView.Element);

                PssgElementViewModel newElementView = new(newElement, elementView.Parent);
                var viewIndex = elementView.Parent.Children.IndexOf(elementView);
                if (viewIndex < 0 || viewIndex >= elementView.Parent.Children.Count)
                    elementView.Parent.Children.Add(newElementView);
                else
                    elementView.Parent.Children.Insert(viewIndex + 1, newElementView);

                mainView.TexturesWorkspace.LoadTextures(newElementView);
                newElementView.IsSelected = true;
            }
            catch (Exception ex)
            {
                await MessageBox.Show("Could not clone element!" + Environment.NewLine + Environment.NewLine +
                    ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool AddAttribute_CanExecute(object parameter)
        {
            return parameter != null;
        }
        [RelayCommand(CanExecute = nameof(AddAttribute_CanExecute))]
        private async Task AddAttribute(object parameter)
        {
            PssgElementViewModel elementView = (PssgElementViewModel)parameter;
            var res = await PssgDialog.ShowAddAttributeDialog();
            if (res is not null)
            {
                PssgAttribute attr = elementView.Element.AddAttribute(res.Name, Convert.ChangeType(res.Value, res.Type));
                if (attr == null)
                {
                    return;
                }

                elementView.IsSelected = false;
                elementView.IsSelected = true;
            }
        }
        private bool RemoveAttribute_CanExecute(object parameter)
        {
            return parameter != null;
        }
        [RelayCommand(CanExecute = nameof(RemoveAttribute_CanExecute))]
        private void RemoveAttribute(object parameter)
        {
            PssgAttributeViewModel attrView = (PssgAttributeViewModel)parameter;

            attrView.Attribute.ParentElement.RemoveAttribute(attrView.Attribute.Name);
            attrView.Parent.IsSelected = false;
            attrView.Parent.IsSelected = true;
        }
        #endregion
    }
}
