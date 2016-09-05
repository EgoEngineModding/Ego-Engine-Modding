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
    public class PackagesWorkspaceViewModel : WorkspaceViewModel
    {
        #region Data
        readonly ObservableCollection<ErpPackageViewModel> packages;

        public override string DisplayName
        {
            get { return "Pkg Files"; }
        }

        public ObservableCollection<ErpPackageViewModel> Packages
        {
            get { return packages; }
        }
        #endregion

        #region Presentation Data
        readonly CollectionView packagesViewSource;
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
                packagesViewSource.Refresh();
            }
        }
        #endregion

        public PackagesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            packages = new ObservableCollection<ErpPackageViewModel>();
            packagesViewSource = (CollectionView)CollectionViewSource.GetDefaultView(Packages);
            packagesViewSource.Filter += PackagesFilter;

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
                    case "AICorner":
                    case "AIGrip":
                    case "AIThrottle":
                    case "AnimClip": // temp
                    case "CrowdPlacement":
                    case "EventGraph": // node
                    case "TyreWearData":
                    case "TyreWearGran":
                    case "UIDB":
                    case "UIDF":
                    case "UILayout":
                    case "WOInstances":
                    case "World":
                    case "WOTypes":
                        Packages.Add(new ErpPackageViewModel(resView));
                        break;
                }
            }
        }

        public override void ClearData()
        {
            packages.Clear();
        }

        private bool PackagesFilter(object item)
        {
            if (String.IsNullOrEmpty(FilterText))
                return true;
            else
                return ((item as ErpPackageViewModel).DisplayName.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0);
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
            ErpPackageViewModel pkgView = (ErpPackageViewModel)parameter;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Json files|*.json|All files|*.*";
            dialog.Title = "Select the pkg save location and file name";
            dialog.FileName = pkgView.DisplayName + ".json";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    pkgView.ExportPkg(new StreamWriter(File.Open(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read)));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export pkg file!" + Environment.NewLine + Environment.NewLine +
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
            ErpPackageViewModel pkgView = (ErpPackageViewModel)parameter;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Json files|*.json|All files|*.*";
            dialog.Title = "Select a pkg file";
            dialog.FileName = pkgView.DisplayName + ".json";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    pkgView.ImportPkg(File.Open(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read));
                    pkgView.GetPreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import pkg file!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportAll_CanExecute(object parameter)
        {
            return packages.Count > 0;
        }
        private void ExportAll_Execute(object parameter)
        {
            try
            {
                int success = 0;
                int fail = 0;
                ProgressDialogViewModel progDialogVM = new ProgressDialogViewModel(out mainView.ErpFile.ProgressPercentage, out mainView.ErpFile.ProgressStatus);
                progDialogVM.PercentageMax = packages.Count;
                View.ProgressDialog progDialog = new View.ProgressDialog();
                progDialog.DataContext = progDialogVM;

                var task = Task.Run(() =>
                {
                    string outputFolder = mainView.FilePath.Replace(".", "_") + "_pkgfiles";
                    Directory.CreateDirectory(outputFolder);

                    for (int i = 0; i < packages.Count;)
                    {
                        string fileName = outputFolder + "\\" + Path.Combine(packages[i].Package.Folder, packages[i].Package.FileName).Replace("?", "%3F") + ".json";
                        ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report("Exporting " + Path.GetFileName(fileName) + "... ");

                        try
                        {
                            string directoryPath = Path.GetDirectoryName(fileName);
                            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
                            packages[i].ExportPkg(new StreamWriter(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read)));
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
                MessageBox.Show("There was an error, could not export all pkg files!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool ImportAll_CanExecute(object parameter)
        {
            return packages.Count > 0;
        }
        private void ImportAll_Execute(object parameter)
        {
            try
            {
                string directory = mainView.FilePath.Replace(".", "_") + "_pkgfiles";
                if (Directory.Exists(directory) == true)
                {
                    int success = 0;
                    int fail = 0;
                    int skip = 0;
                    bool found = false;

                    ProgressDialogViewModel progDialogVM = new ProgressDialogViewModel(out mainView.ErpFile.ProgressPercentage, out mainView.ErpFile.ProgressStatus);
                    progDialogVM.PercentageMax = packages.Count;
                    View.ProgressDialog progDialog = new View.ProgressDialog();
                    progDialog.DataContext = progDialogVM;

                    var task = Task.Run(() =>
                    {
                        for (int i = 0; i < packages.Count;)
                        {
                            string fileName = directory + "\\" + Path.Combine(packages[i].Package.Folder, packages[i].Package.FileName).Replace("?", "%3F") + ".json";
                            ((IProgress<string>)mainView.ErpFile.ProgressStatus).Report("Exporting " + Path.GetFileName(fileName) + "... ");

                            try
                            {
                                foreach (string filePath in Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories))
                                {
                                    if (Path.Equals(filePath, fileName))
                                    {
                                        packages[i].ImportPkg(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                                        if (packages[i].IsSelected)
                                        {
                                            packages[i].GetPreview();
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
                    MessageBox.Show("Could not find pkgfiles folder!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("There was an error, could not import all pkg files!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
