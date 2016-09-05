using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Graphics;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace EgoErpArchiver.ViewModel
{
    public class XmlFilesWorkspaceViewModel : WorkspaceViewModel
    {
        #region Data
        readonly ObservableCollection<ErpXmlFileViewModel> xmlFiles;

        public override string DisplayName
        {
            get { return "Xml Files"; }
        }

        public ObservableCollection<ErpXmlFileViewModel> XmlFiles
        {
            get { return xmlFiles; }
        }
        #endregion

        #region Presentation Data
        readonly CollectionView xmlFilesViewSource;
        string filterText;

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
                    case "AISplineData":
                    case "AIBrakeSettings":
                    case "FFBD":
                    case "PracticeTA":
                    case "Spline":
                    case "TM":
                    case "TS":
                    case "VTF":
                        XmlFiles.Add(new ErpXmlFileViewModel((ErpResourceViewModel)resView));
                        break;
                }
            }
        }

        public override void ClearData()
        {
            xmlFiles.Clear();
        }

        private bool XmlFilesFilter(object item)
        {
            if (String.IsNullOrEmpty(FilterText))
                return true;
            else
                return ((item as ErpXmlFileViewModel).DisplayName.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        #region Menu
        readonly RelayCommand export;
        readonly RelayCommand import;
        readonly RelayCommand exportAll;
        readonly RelayCommand importAll;

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
            ErpXmlFileViewModel xmlView = (ErpXmlFileViewModel)parameter;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Xml files|*.xml|All files|*.*";
            dialog.Title = "Select the xml save location and file name";
            dialog.FileName = xmlView.DisplayName + ".xml";
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
            ErpXmlFileViewModel xmlView = (ErpXmlFileViewModel)parameter;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Xml files|*.xml|All files|*.*";
            dialog.Title = "Select a xml file";
            dialog.FileName = xmlView.DisplayName + ".xml";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    xmlView.ImportXML(File.Open(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read));
                    xmlView.GetPreview();
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
                int success = 0;
                int fail = 0;
                ProgressDialogViewModel progDialogVM = new ProgressDialogViewModel(out mainView.ErpFile.ProgressPercentage, out mainView.ErpFile.ProgressStatus);
                progDialogVM.PercentageMax = xmlFiles.Count;
                View.ProgressDialog progDialog = new View.ProgressDialog();
                progDialog.DataContext = progDialogVM;

                var task = Task.Run(() =>
                {
                    string outputFolder = mainView.FilePath.Replace(".", "_") + "_xmlfiles";
                    Directory.CreateDirectory(outputFolder);

                    for (int i = 0; i < xmlFiles.Count;)
                    {
                        string fileName = outputFolder + "\\" + Path.Combine(xmlFiles[i].XmlFile.Folder, xmlFiles[i].XmlFile.FileName).Replace("?", "%3F") + ".xml";
                        ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report("Exporting " + Path.GetFileName(fileName) + "... ");

                        try
                        {
                            string directoryPath = Path.GetDirectoryName(fileName);
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
                string directory = mainView.FilePath.Replace(".", "_") + "_xmlfiles";
                if (Directory.Exists(directory) == true)
                {
                    int success = 0;
                    int fail = 0;
                    int skip = 0;
                    bool found = false;

                    ProgressDialogViewModel progDialogVM = new ProgressDialogViewModel(out mainView.ErpFile.ProgressPercentage, out mainView.ErpFile.ProgressStatus);
                    progDialogVM.PercentageMax = xmlFiles.Count;
                    View.ProgressDialog progDialog = new View.ProgressDialog();
                    progDialog.DataContext = progDialogVM;

                    var task = Task.Run(() =>
                    {
                        for (int i = 0; i < xmlFiles.Count;)
                        {
                            string fileName = directory + "\\" + Path.Combine(xmlFiles[i].XmlFile.Folder, xmlFiles[i].XmlFile.FileName).Replace("?", "%3F") + ".xml";
                            ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report("Exporting " + Path.GetFileName(fileName) + "... ");

                            try
                            {
                                foreach (string filePath in Directory.GetFiles(directory, "*.xml", SearchOption.AllDirectories))
                                {
                                    if (Path.Equals(filePath, fileName))
                                    {
                                        xmlFiles[i].ImportXML(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                                        if (xmlFiles[i].IsSelected)
                                        {
                                            xmlFiles[i].GetPreview();
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
