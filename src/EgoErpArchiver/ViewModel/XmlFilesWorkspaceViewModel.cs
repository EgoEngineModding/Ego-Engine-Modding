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
        #region Data
        private readonly ObservableCollection<ErpXmlFileViewModel> xmlFiles;

        public ObservableCollection<ErpXmlFileViewModel> XmlFiles
        {
            get { return xmlFiles; }
        }
        #endregion

        #region Presentation Data
        private string _displayName;
        private readonly CollectionView xmlFilesViewSource;
        private string filterText;

        public override string DisplayName
        {
            get { return _displayName; }
            protected set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

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
        #endregion

        public XmlFilesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            xmlFiles = new ObservableCollection<ErpXmlFileViewModel>();
            _displayName = "XML Files";
            xmlFilesViewSource = (CollectionView)CollectionViewSource.GetDefaultView(XmlFiles);
            xmlFilesViewSource.Filter += XmlFilesFilter;

            export = new RelayCommand(Export_Execute, Export_CanExecute);
            import = new RelayCommand(Import_Execute, Import_CanExecute);
            exportAll = new RelayCommand(ExportAll_Execute, ExportAll_CanExecute);
            importAll = new RelayCommand(ImportAll_Execute, ImportAll_CanExecute);
        }

        public override void LoadData(object data)
        {
            ClearData();
            foreach (var resView in ((ResourcesWorkspaceViewModel)data).Resources)
            {
                switch (resView.Resource.ResourceType)
                {
                    // TRN~ abu_dhabi.erp is JPK file
                    case "ActorSceneData":
                    case "AICornering": // Grid 2019
                    case "AIGrip": // seen in Grid 2019 as xml (erp v4)
                    case "AISplineData":
                    case "AIBrakeSettings":
                    case "CrowdAnimation":
                    case "CrowdConfig":
                    case "CrowdLODs":
                    case "CrowdPalette":
                    case "CrowdPrpPalette":
                    case "ECA~":
                    case "FemsStrengthBox": // Grid 2019
                    case "FFBD":
                    case "IBLConfig":
                    case "PPRP":
                    case "PracticeTA":
                    case "RainfallData":
                    case "Sequence":
                    case "SkidmarksData":
                    case "Spline":
                    case "SURF":
                    case "TimeOfDayConfig":
                    case "TM":
                    case "TrackInfoData":
                    case "TrackSpace":
                    case "TS":
                    case "VCMPD":
                    case "VTF":
                        XmlFiles.Add(new ErpXmlFileViewModel((ErpResourceViewModel)resView));
                        break;
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
            return string.IsNullOrEmpty(FilterText) ||
                (item as ErpXmlFileViewModel).DisplayName.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        }

        #region Menu
        private readonly RelayCommand export;
        private readonly RelayCommand import;
        private readonly RelayCommand exportAll;
        private readonly RelayCommand importAll;

        public RelayCommand Export
        {
            get { return export; }
        }
        public RelayCommand Import
        {
            get { return import; }
        }
        public RelayCommand ExportAll
        {
            get { return exportAll; }
        }
        public RelayCommand ImportAll
        {
            get { return importAll; }
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
                FileName = xmlView.DisplayName.Replace("?", "%3F") + ".xml"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    xmlView.ExportXML(File.Open(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read));
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
                FileName = xmlView.DisplayName.Replace("?", "%3F") + ".xml"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    xmlView.ImportXML(File.Open(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read));
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
                var progDialogVM = new ProgressDialogViewModel(out mainView.ErpFile.ProgressPercentage, out mainView.ErpFile.ProgressStatus)
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
                        var fileName = outputFolder + "\\" + Path.Combine(xmlFiles[i].XmlFile.Folder, xmlFiles[i].XmlFile.FileName).Replace("?", "%3F") + ".xml";
                        ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report("Exporting " + Path.GetFileName(fileName) + "... ");

                        try
                        {
                            var directoryPath = Path.GetDirectoryName(fileName);
                            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
                            xmlFiles[i].ExportXML(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read));
                            ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report("SUCCESS" + Environment.NewLine);
                            ++success;
                        }
                        catch
                        {
                            ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report("FAIL" + Environment.NewLine);
                            ++fail;
                        }

                        ((IProgress<int>)mainView.ErpFile.ProgressPercentage).Report(++i);
                    }

                    ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report(string.Format("{0} Succeeded, {1} Failed", success, fail));
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

                    var progDialogVM = new ProgressDialogViewModel(out mainView.ErpFile.ProgressPercentage, out mainView.ErpFile.ProgressStatus)
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
                            var fileName = directory + "\\" + Path.Combine(xmlFiles[i].XmlFile.Folder, xmlFiles[i].XmlFile.FileName).Replace("?", "%3F") + ".xml";
                            ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report("Exporting " + Path.GetFileName(fileName) + "... ");

                            try
                            {
                                foreach (var filePath in Directory.GetFiles(directory, "*.xml", SearchOption.AllDirectories))
                                {
                                    if (Path.Equals(filePath, fileName))
                                    {
                                        xmlFiles[i].ImportXML(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
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
                                    ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report("SUCCESS" + Environment.NewLine);
                                    ++success;
                                }
                                else
                                {
                                    ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report("SKIP" + Environment.NewLine);
                                    ++skip;
                                }
                            }
                            catch
                            {
                                ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report("FAIL" + Environment.NewLine);
                                ++fail;
                            }

                            ((IProgress<int>)mainView.ErpFile.ProgressPercentage).Report(++i);
                        }

                        ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report(string.Format("{0} Succeeded, {1} Skipped, {2} Failed", success, skip, fail));
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
        #endregion
    }
}
