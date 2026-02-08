using System.Collections.ObjectModel;
using System.Windows.Input;

using ActiproSoftware.UI.Avalonia.Data;

using CommunityToolkit.Mvvm.Input;

using EgoEngineLibrary.Formats.Erp;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;
using EgoEngineLibrary.Xml;

using EgoErpArchiver.Dialogs.Erp;

namespace EgoErpArchiver.ViewModels
{
    public class XmlFilesWorkspaceViewModel : WorkspaceViewModel
    {
        private readonly ObservableCollection<ErpXmlFileViewModel> xmlFiles;
        public ObservableCollection<ErpXmlFileViewModel> XmlFiles
        {
            get { return xmlFiles; }
        }

        private string _displayName;
        public override string DisplayName
        {
            get { return _displayName; }
            protected set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        private readonly CollectionView<ErpXmlFileViewModel> xmlFilesViewSource;
        private string filterText;
        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;
                xmlFilesViewSource.Refresh();
            }
        }

        public ICommand Export { get; }
        public ICommand Import { get; }
        public ICommand ExportAll { get; }
        public ICommand ImportAll { get; }

        public XmlFilesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            xmlFiles = new ObservableCollection<ErpXmlFileViewModel>();
            _displayName = "XML Files";
            xmlFilesViewSource = new CollectionView<ErpXmlFileViewModel>(XmlFiles);
            xmlFilesViewSource.Filter += XmlFilesFilter;

            Export = new AsyncRelayCommand<ErpXmlFileViewModel>(Export_Execute, Export_CanExecute);
            Import = new AsyncRelayCommand<ErpXmlFileViewModel>(Import_Execute, Import_CanExecute);
            ExportAll = new AsyncRelayCommand(ExportAll_Execute, ExportAll_CanExecute);
            ImportAll = new AsyncRelayCommand(ImportAll_Execute, ImportAll_CanExecute);
        }

        public override void LoadData(object data)
        {
            ClearData();
            foreach (var resView in ((ResourcesWorkspaceViewModel)data).Resources)
            {
                var resource = resView.Resource;
                foreach (var fragment in resource.Fragments)
                {
                    try
                    {
                        using var ds = fragment.GetDecompressDataStream(true);
                        if (XmlFile.IsXmlFile(ds))
                        {
                            XmlFiles.Add(new ErpXmlFileViewModel(resView, fragment));
                        }
                    }
                    catch
                    {
                        // TODO: log
                    }
                }
            }
            DisplayName = "XML Files " + xmlFiles.Count;
        }

        public override void ClearData()
        {
            xmlFiles.Clear();
        }

        private bool XmlFilesFilter(object item)
        {
            return string.IsNullOrEmpty(FilterText)
                || (item as ErpXmlFileViewModel).DisplayName.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        }

        private bool Export_CanExecute(ErpXmlFileViewModel? parameter)
        {
            return parameter != null;
        }

        private async Task Export_Execute(ErpXmlFileViewModel? xmlView)
        {
            ArgumentNullException.ThrowIfNull(xmlView);
            var dialog = new FileSaveOptions
            {
                FileTypeChoices = [FilePickerType.Xml, FilePickerType.All],
                Title = "Select the xml save location and file name",
                FileName = xmlView.DisplayName + ".xml"
            };

            var res = await FileDialog.ShowSaveFileDialog(dialog);
            if (res is not null)
            {
                try
                {
                    using var fs = File.Open(res, FileMode.Create, FileAccess.Write, FileShare.Read);
                    using var sw = new StreamWriter(fs);
                    xmlView.ExportXML(sw);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not export xml file!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool Import_CanExecute(ErpXmlFileViewModel? parameter)
        {
            return parameter != null;
        }

        private async Task Import_Execute(ErpXmlFileViewModel? xmlView)
        {
            ArgumentNullException.ThrowIfNull(xmlView);
            var dialog = new FileOpenOptions
            {
                FileTypeChoices = [FilePickerType.Xml, FilePickerType.All],
                Title = "Select a xml file",
                FileName = xmlView.DisplayName + ".xml"
            };

            var res = await FileDialog.ShowOpenFileDialog(dialog);
            if (res.Count > 0)
            {
                try
                {
                    using var fs = File.Open(res[0], FileMode.Open, FileAccess.Read, FileShare.Read);
                    xmlView.ImportXML(fs);
                    xmlView.IsSelected = false;
                    xmlView.IsSelected = true;
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not import xml file!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportAll_CanExecute()
        {
            return xmlFiles.Count > 0;
        }

        private async Task ExportAll_Execute()
        {
            try
            {
                var success = 0;
                var fail = 0;
                var progDialogVM = new ProgressDialogViewModel()
                {
                    PercentageMax = xmlFiles.Count
                };

                var task = Task.Run(() =>
                {
                    var outputFolder = mainView.FilePath.Replace(".", "_") + "_xmlfiles";
                    Directory.CreateDirectory(outputFolder);

                    for (var i = 0; i < xmlFiles.Count;)
                    {
                        var resource = xmlFiles[i].Resource;
                        var fragment = xmlFiles[i].Fragment;
                        var folderPath = Path.Combine(outputFolder, resource.Folder);
                        var fileName = ErpResourceExporter.GetFragmentFileName(resource, fragment);
                        var filePath = Path.Combine(folderPath, fileName) + ".xml";
                        progDialogVM.ProgressStatus.Report("Exporting " + fileName + "... ");

                        try
                        {
                            Directory.CreateDirectory(folderPath);

                            using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                            using var sw = new StreamWriter(fs);
                            xmlFiles[i].ExportXML(sw);

                            progDialogVM.ProgressStatus.Report("SUCCESS" + Environment.NewLine);
                            ++success;
                        }
                        catch
                        {
                            progDialogVM.ProgressStatus.Report("FAIL" + Environment.NewLine);
                            ++fail;
                        }

                        progDialogVM.ProgressPercentage.Report(++i);
                    }

                    progDialogVM.ProgressStatus.Report(string.Format("{0} Succeeded, {1} Failed", success, fail));
                });

                await ErpDialog.ShowProgressDialog(progDialogVM);
                await task;
            }
            catch
            {
                await MessageBox.Show("There was an error, could not export all xml files!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ImportAll_CanExecute()
        {
            return xmlFiles.Count > 0;
        }

        private async Task ImportAll_Execute()
        {
            try
            {
                var directory = mainView.FilePath.Replace(".", "_") + "_xmlfiles";
                if (Directory.Exists(directory) == true)
                {
                    var success = 0;
                    var fail = 0;
                    var skip = 0;
                    var found = false;

                    var progDialogVM = new ProgressDialogViewModel()
                    {
                        PercentageMax = xmlFiles.Count
                    };

                    var task = Task.Run(() =>
                    {
                        for (var i = 0; i < xmlFiles.Count;)
                        {
                            var resource = xmlFiles[i].Resource;
                            var fragment = xmlFiles[i].Fragment;
                            var folderPath = Path.Combine(directory, resource.Folder);
                            var fileName = ErpResourceExporter.GetFragmentFileName(resource, fragment);
                            var expFilePath = Path.Combine(folderPath, fileName) + ".xml";
                            progDialogVM.ProgressStatus.Report("Importing " + fileName + "... ");

                            try
                            {
                                foreach (var filePath in Directory.GetFiles(directory, "*.xml", SearchOption.AllDirectories))
                                {
                                    if (Path.Equals(filePath, expFilePath))
                                    {
                                        using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                                        xmlFiles[i].ImportXML(fs);

                                        if (xmlFiles[i].IsSelected)
                                        {
                                            xmlFiles[i].IsSelected = false;
                                            xmlFiles[i].IsSelected = true;
                                        }
                                        found = true;
                                        break;
                                    }
                                }

                                if (found)
                                {
                                    progDialogVM.ProgressStatus.Report("SUCCESS" + Environment.NewLine);
                                    ++success;
                                }
                                else
                                {
                                    progDialogVM.ProgressStatus.Report("SKIP" + Environment.NewLine);
                                    ++skip;
                                }
                            }
                            catch
                            {
                                progDialogVM.ProgressStatus.Report("FAIL" + Environment.NewLine);
                                ++fail;
                            }

                            progDialogVM.ProgressPercentage.Report(++i);
                        }

                        progDialogVM.ProgressStatus.Report(string.Format("{0} Succeeded, {1} Skipped, {2} Failed", success, skip, fail));
                    });

                    await ErpDialog.ShowProgressDialog(progDialogVM);
                    await task;
                }
                else
                {
                    await MessageBox.Show("Could not find xmlfiles folder!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                await MessageBox.Show("There was an error, could not import all xml files!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
