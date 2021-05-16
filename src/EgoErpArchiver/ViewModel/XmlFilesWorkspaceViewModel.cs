using EgoEngineLibrary.Formats.Erp;
using EgoEngineLibrary.Xml;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace EgoErpArchiver.ViewModel
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

        private readonly CollectionView xmlFilesViewSource;
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

        public RelayCommand Export { get; }
        public RelayCommand Import { get; }
        public RelayCommand ExportAll { get; }
        public RelayCommand ImportAll { get; }

        public XmlFilesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            xmlFiles = new ObservableCollection<ErpXmlFileViewModel>();
            _displayName = "XML Files";
            xmlFilesViewSource = (CollectionView)CollectionViewSource.GetDefaultView(XmlFiles);
            xmlFilesViewSource.Filter += XmlFilesFilter;

            Export = new RelayCommand(Export_Execute, Export_CanExecute);
            Import = new RelayCommand(Import_Execute, Import_CanExecute);
            ExportAll = new RelayCommand(ExportAll_Execute, ExportAll_CanExecute);
            ImportAll = new RelayCommand(ImportAll_Execute, ImportAll_CanExecute);
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

        private bool Export_CanExecute(object parameter)
        {
            return parameter != null;
        }

        private void Export_Execute(object parameter)
        {
            var xmlView = (ErpXmlFileViewModel)parameter;
            var dialog = new SaveFileDialog
            {
                Filter = "Xml files|*.xml|All files|*.*",
                Title = "Select the xml save location and file name",
                FileName = xmlView.DisplayName + ".xml"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using var fs = File.Open(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                    using var sw = new StreamWriter(fs);
                    xmlView.ExportXML(sw);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export xml file!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool Import_CanExecute(object parameter)
        {
            return parameter != null;
        }

        private void Import_Execute(object parameter)
        {
            var xmlView = (ErpXmlFileViewModel)parameter;
            var dialog = new OpenFileDialog
            {
                Filter = "Xml files|*.xml|All files|*.*",
                Title = "Select a xml file",
                FileName = xmlView.DisplayName + ".xml"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using var fs = File.Open(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    xmlView.ImportXML(fs);
                    xmlView.IsSelected = false;
                    xmlView.IsSelected = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import xml file!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportAll_CanExecute(object parameter)
        {
            return xmlFiles.Count > 0;
        }

        private void ExportAll_Execute(object parameter)
        {
            try
            {
                var success = 0;
                var fail = 0;
                var progDialogVM = new ProgressDialogViewModel()
                {
                    PercentageMax = xmlFiles.Count
                };
                var progDialog = new View.ProgressDialog
                {
                    DataContext = progDialogVM
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

                progDialog.ShowDialog();
                task.Wait();
            }
            catch
            {
                MessageBox.Show("There was an error, could not export all xml files!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ImportAll_CanExecute(object parameter)
        {
            return xmlFiles.Count > 0;
        }

        private void ImportAll_Execute(object parameter)
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
                    var progDialog = new View.ProgressDialog
                    {
                        DataContext = progDialogVM
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
                            progDialogVM.ProgressStatus.Report("Exporting " + fileName + "... ");

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

                    progDialog.ShowDialog();
                    task.Wait();
                }
                else
                {
                    MessageBox.Show("Could not find xmlfiles folder!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("There was an error, could not import all xml files!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
