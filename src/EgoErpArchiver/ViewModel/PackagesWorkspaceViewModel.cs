using EgoEngineLibrary.Data.Pkg;
using EgoEngineLibrary.Formats.Erp;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace EgoErpArchiver.ViewModel
{
    public class PackagesWorkspaceViewModel : WorkspaceViewModel
    {
        private readonly ObservableCollection<ErpPackageViewModel> packages;
        public ObservableCollection<ErpPackageViewModel> Packages
        {
            get { return packages; }
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

        private readonly CollectionView packagesViewSource;
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
                packagesViewSource.Refresh();
            }
        }

        public RelayCommand Export { get; }
        public RelayCommand Import { get; }
        public RelayCommand ExportAll { get; }
        public RelayCommand ImportAll { get; }

        public PackagesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            packages = new ObservableCollection<ErpPackageViewModel>();
            _displayName = "Pkg Files";
            packagesViewSource = (CollectionView)CollectionViewSource.GetDefaultView(Packages);
            packagesViewSource.Filter += PackagesFilter;

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
                        if (PkgFile.IsPkgFile(ds))
                        {
                            Packages.Add(new ErpPackageViewModel(resView, fragment));
                        }
                    }
                    catch
                    {
                        // TODO: log
                    }
                }
            }
            DisplayName = "Pkg Files " + packages.Count;
        }

        public override void ClearData()
        {
            packages.Clear();
        }

        private bool PackagesFilter(object item)
        {
            return string.IsNullOrEmpty(FilterText)
                || (item as ErpPackageViewModel).DisplayName.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        }

        private bool Export_CanExecute(object parameter)
        {
            return parameter != null;
        }

        private void Export_Execute(object parameter)
        {
            var pkgView = (ErpPackageViewModel)parameter;
            var dialog = new SaveFileDialog
            {
                Filter = "Json files|*.json|All files|*.*",
                Title = "Select the pkg save location and file name",
                FileName = pkgView.DisplayName + ".json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using var fs = File.Open(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                    using var sw = new StreamWriter(fs);
                    pkgView.ExportPkg(sw);
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
            var pkgView = (ErpPackageViewModel)parameter;
            var dialog = new OpenFileDialog
            {
                Filter = "Json files|*.json|All files|*.*",
                Title = "Select a pkg file",
                FileName = pkgView.DisplayName + ".json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using var fs = File.Open(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    pkgView.ImportPkg(fs);
                    pkgView.IsSelected = false;
                    pkgView.IsSelected = true;
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
                var success = 0;
                var fail = 0;
                var progDialogVM = new ProgressDialogViewModel()
                {
                    PercentageMax = packages.Count
                };
                var progDialog = new View.ProgressDialog
                {
                    DataContext = progDialogVM
                };

                var task = Task.Run(() =>
                {
                    var outputFolder = mainView.FilePath.Replace(".", "_") + "_pkgfiles";
                    Directory.CreateDirectory(outputFolder);

                    for (var i = 0; i < packages.Count;)
                    {
                        var resource = packages[i].Resource;
                        var fragment = packages[i].Fragment;
                        var folderPath = Path.Combine(outputFolder, resource.Folder);
                        var fileName = ErpResourceExporter.GetFragmentFileName(resource, fragment);
                        var filePath = Path.Combine(folderPath, fileName) + ".json";
                        progDialogVM.ProgressStatus.Report("Exporting " + fileName + "... ");

                        try
                        {
                            Directory.CreateDirectory(folderPath);

                            using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                            using var sw = new StreamWriter(fs);
                            packages[i].ExportPkg(sw);

                            progDialogVM.ProgressStatus.Report("SUCCESS" + Environment.NewLine);
                            ++success;
                        }
                        catch when (!System.Diagnostics.Debugger.IsAttached)
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
                var directory = mainView.FilePath.Replace(".", "_") + "_pkgfiles";
                if (Directory.Exists(directory) == true)
                {
                    var success = 0;
                    var fail = 0;
                    var skip = 0;
                    var found = false;

                    var progDialogVM = new ProgressDialogViewModel
                    {
                        PercentageMax = packages.Count
                    };
                    var progDialog = new View.ProgressDialog
                    {
                        DataContext = progDialogVM
                    };

                    var task = Task.Run(() =>
                    {
                        for (var i = 0; i < packages.Count;)
                        {
                            var resource = packages[i].Resource;
                            var fragment = packages[i].Fragment;
                            var folderPath = Path.Combine(directory, resource.Folder);
                            var fileName = ErpResourceExporter.GetFragmentFileName(resource, fragment);
                            var expFilePath = Path.Combine(folderPath, fileName) + ".json";
                            progDialogVM.ProgressStatus.Report("Importing " + fileName + "... ");

                            try
                            {
                                foreach (var filePath in Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories))
                                {
                                    if (Path.Equals(filePath, expFilePath))
                                    {
                                        using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                                        packages[i].ImportPkg(fs);

                                        if (packages[i].IsSelected)
                                        {
                                            packages[i].IsSelected = false;
                                            packages[i].IsSelected = true;
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
                    MessageBox.Show("Could not find pkgfiles folder!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("There was an error, could not import all pkg files!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
