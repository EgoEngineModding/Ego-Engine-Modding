using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace EgoErpArchiver.ViewModel
{
    public class TexturesWorkspaceViewModel : WorkspaceViewModel
    {
        private readonly ObservableCollection<ErpTextureViewModel> textures;
        public ObservableCollection<ErpTextureViewModel> Textures
        {
            get { return textures; }
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

        private readonly CollectionView texturesViewSource;
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
                texturesViewSource.Refresh();
            }
        }

        public RelayCommand Export { get; }
        public RelayCommand Import { get; }
        public RelayCommand ExportTextures { get; }
        public RelayCommand ImportTextures { get; }

        public TexturesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            textures = new ObservableCollection<ErpTextureViewModel>();
            _displayName = "Textures";
            texturesViewSource = (CollectionView)CollectionViewSource.GetDefaultView(Textures);
            texturesViewSource.Filter += TextureFilter;

            Export = new RelayCommand(Export_Execute, Export_CanExecute);
            Import = new RelayCommand(Import_Execute, Import_CanExecute);
            ExportTextures = new RelayCommand(ExportTextures_Execute, ExportTextures_CanExecute);
            ImportTextures = new RelayCommand(ImportTextures_Execute, ImportTextures_CanExecute);
        }

        public override void LoadData(object data)
        {
            ClearData();
            List<ErpTextureViewModel> searchResults = new();
            foreach (var resView in ((ResourcesWorkspaceViewModel)data).Resources)
            {
                if (resView.Resource.ResourceType == "GfxSRVResource")
                {
                    searchResults.Add(new ErpTextureViewModel(resView));
                }
            }
            searchResults.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName) );
            foreach (ErpTextureViewModel rv in searchResults)
                textures.Add(rv);
            DisplayName = "Textures " + textures.Count;
        }

        public override void ClearData()
        {
            textures.Clear();
        }

        private bool TextureFilter(object item)
        {
            return string.IsNullOrEmpty(FilterText)
                || (item as ErpTextureViewModel).DisplayName.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        }

        private bool Export_CanExecute(object parameter)
        {
            return parameter != null;
        }

        private void Export_Execute(object parameter)
        {
            var texView = (ErpTextureViewModel)parameter;
            var dialog = new SaveFileDialog
            {
                Filter = "Dds files|*.dds|All files|*.*",
                Title = "Select the dds save location and file name",
                FileName = texView.DisplayName.Replace("?", "%3F") + ".dds"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    texView.ExportDDS(dialog.FileName, false, false);
                }
                catch (Exception ex) when(!Debugger.IsAttached)
                {
                    MessageBox.Show("Could not export texture!" + Environment.NewLine + Environment.NewLine +
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
            var texView = (ErpTextureViewModel)parameter;
            var dialog = new OpenFileDialog
            {
                Filter = "Dds files|*.dds|All files|*.*",
                Title = "Select a dds file",
                FileName = texView.DisplayName.Replace("?", "%3F") + ".dds"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    texView.ImportDDS(dialog.FileName, null, false);
                    texView.GetPreview();
                }
                catch (Exception ex) when(!Debugger.IsAttached)
                {
                    MessageBox.Show("Could not import texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportTextures_CanExecute(object parameter)
        {
            return Textures.Count > 0;
        }

        private void ExportTextures_Execute(object parameter)
        {
            try
            {
                var success = 0;
                var fail = 0;
                var progDialogVM = new ProgressDialogViewModel
                {
                    PercentageMax = Textures.Count
                };
                var progDialog = new View.ProgressDialog
                {
                    DataContext = progDialogVM
                };

                var task = Task.Run(() =>
                {
                    var outputFolderPath = mainView.FilePath.Replace(".", "_") + "_textures";
                    Directory.CreateDirectory(outputFolderPath);

                    for (var i = 0; i < textures.Count;)
                    {
                        var resource = textures[i].Resource;
                        var folderPath = Path.Combine(outputFolderPath, resource.Folder);
                        var fileName = resource.FileName.Replace("?", "%3F");
                        var filePath = Path.Combine(folderPath, fileName) + ".dds";
                        progDialogVM.ProgressStatus.Report("Exporting " + fileName + "... ");

                        try
                        {
                            Directory.CreateDirectory(folderPath);
                            textures[i].ExportDDS(filePath, true, true);
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
                MessageBox.Show("There was an error, could not export all textures!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ImportTextures_CanExecute(object parameter)
        {
            return Textures.Count > 0;
        }

        private void ImportTextures_Execute(object parameter)
        {
            try
            {
                var directory = mainView.FilePath.Replace(".", "_") + "_textures";
                var mipMapDirectory = mainView.FilePath.Replace(".", "_") + "_mipmaps";
                if (Directory.Exists(directory) == true)
                {
                    var success = 0;
                    var fail = 0;
                    var skip = 0;
                    var found = false;

                    var progDialogVM = new ProgressDialogViewModel
                    {
                        PercentageMax = Textures.Count
                    };
                    var progDialog = new View.ProgressDialog
                    {
                        DataContext = progDialogVM
                    };

                    var task = Task.Run(() =>
                    {
                        for (var i = 0; i < textures.Count;)
                        {
                            var resource = textures[i].Resource;
                            var folderPath = Path.Combine(directory, resource.Folder);
                            var fileName = resource.FileName.Replace("?", "%3F");
                            var expFilePath = Path.Combine(folderPath, fileName) + ".dds";
                            progDialogVM.ProgressStatus.Report("Importing " + fileName + "... ");

                            try
                            {
                                foreach (var filePath in Directory.GetFiles(directory, "*.dds", SearchOption.AllDirectories))
                                {
                                    if (Path.Equals(filePath, expFilePath))
                                    {
                                        var mipMapSaveLocation = filePath.Replace(directory, mipMapDirectory) + ".mipmaps";
                                        Directory.CreateDirectory(Path.GetDirectoryName(mipMapSaveLocation));
                                        textures[i].ImportDDS(filePath, mipMapSaveLocation, true);
                                        if (textures[i].IsSelected)
                                        {
                                            textures[i].GetPreview();
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
                    MessageBox.Show("Could not find textures folder!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("There was an error, could not import all textures!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
