using System.Collections.ObjectModel;
using System.Windows.Input;

using ActiproSoftware.UI.Avalonia.Data;

using CommunityToolkit.Mvvm.Input;

using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;

using EgoErpArchiver.Dialogs.Erp;

namespace EgoErpArchiver.ViewModels
{
    public class TexturesWorkspaceViewModel : WorkspaceViewModel
    {
        private readonly ErpFileViewModel _fileViewModel;
        private readonly ResourcesWorkspaceViewModel _resourcesWorkspaceViewModel;
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

        private readonly CollectionView<ErpTextureViewModel> texturesViewSource;
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

        public ICommand Export { get; }
        public ICommand Import { get; }
        public ICommand ExportTextures { get; }
        public ICommand ImportTextures { get; }

        public TexturesWorkspaceViewModel() : this(new ErpFileViewModel(), new ResourcesWorkspaceViewModel())
        {
        }

        public TexturesWorkspaceViewModel(ErpFileViewModel fileViewModel, ResourcesWorkspaceViewModel resourcesWorkspaceViewModel)
        {
            _fileViewModel = fileViewModel;
            _resourcesWorkspaceViewModel = resourcesWorkspaceViewModel;
            textures = new ObservableCollection<ErpTextureViewModel>();
            _displayName = "Textures";
            texturesViewSource = new CollectionView<ErpTextureViewModel>(Textures);
            texturesViewSource.Filter += TextureFilter;

            Export = new AsyncRelayCommand<ErpTextureViewModel>(Export_Execute, Export_CanExecute);
            Import = new AsyncRelayCommand<ErpTextureViewModel>(Import_Execute, Import_CanExecute);
            ExportTextures = new AsyncRelayCommand(ExportTextures_Execute, ExportTextures_CanExecute);
            ImportTextures = new AsyncRelayCommand(ImportTextures_Execute, ImportTextures_CanExecute);
        }

        public override void OnFileOpened()
        {
            OnFileClosed();
            foreach (var resView in _resourcesWorkspaceViewModel.Resources)
            {
                if (resView.Resource.ResourceType == "GfxSRVResource")
                {
                    Textures.Add(new ErpTextureViewModel(resView));
                }
            }
            DisplayName = "Textures " + textures.Count;
        }

        public override void OnFileClosed()
        {
            textures.Clear();
        }

        private bool TextureFilter(object item)
        {
            return string.IsNullOrEmpty(FilterText)
                || (item as ErpTextureViewModel).DisplayName.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        }

        private bool Export_CanExecute(ErpTextureViewModel? parameter)
        {
            return parameter != null;
        }

        private async Task Export_Execute(ErpTextureViewModel? texView)
        {
            ArgumentNullException.ThrowIfNull(texView);
            var dialog = new FileSaveOptions
            {
                FileTypeChoices = [FilePickerType.Dds, FilePickerType.All],
                Title = "Select the dds save location and file name",
                FileName = texView.DisplayName.Replace("?", "%3F") + ".dds"
            };

            var res = await FileDialog.ShowSaveFileDialog(dialog);
            if (res is not null)
            {
                try
                {
                    await texView.ExportDDS(res, false, false);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not export texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool Import_CanExecute(ErpTextureViewModel? parameter)
        {
            return parameter != null;
        }

        private async Task Import_Execute(ErpTextureViewModel? texView)
        {
            ArgumentNullException.ThrowIfNull(texView);
            var dialog = new FileOpenOptions
            {
                FileTypeChoices = [FilePickerType.Dds, FilePickerType.All],
                Title = "Select a dds file",
                FileName = texView.DisplayName.Replace("?", "%3F") + ".dds"
            };

            var res = await FileDialog.ShowOpenFileDialog(dialog);
            if (res.Count > 0)
            {
                try
                {
                    await texView.ImportDDS(res[0], null, false);
                    texView.GetPreview();
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not import texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportTextures_CanExecute()
        {
            return Textures.Count > 0;
        }

        private async Task ExportTextures_Execute()
        {
            try
            {
                var success = 0;
                var fail = 0;
                var progDialogVM = new ProgressDialogViewModel
                {
                    PercentageMax = Textures.Count
                };

                var task = Task.Run(async () =>
                {
                    var outputFolderPath = _fileViewModel.FilePath.Replace(".", "_") + "_textures";
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
                            await textures[i].ExportDDS(filePath, true, true);
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
                await MessageBox.Show("There was an error, could not export all textures!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ImportTextures_CanExecute()
        {
            return Textures.Count > 0;
        }

        private async Task ImportTextures_Execute()
        {
            try
            {
                var directory = _fileViewModel.FilePath.Replace(".", "_") + "_textures";
                var mipMapDirectory = _fileViewModel.FilePath.Replace(".", "_") + "_mipmaps";
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

                    var task = Task.Run(async () =>
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
                                        await textures[i].ImportDDS(filePath, mipMapSaveLocation, true);
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

                    await ErpDialog.ShowProgressDialog(progDialogVM);
                    await task;
                }
                else
                {
                    await MessageBox.Show("Could not find textures folder!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                await MessageBox.Show("There was an error, could not import all textures!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
