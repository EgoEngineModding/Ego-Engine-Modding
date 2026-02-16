using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using EgoEngineLibrary.Data.Pkg;
using EgoEngineLibrary.Formats.Erp;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;

using EgoErpArchiver.Dialogs.Erp;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using ObservableCollections;

namespace EgoErpArchiver.ViewModels
{
    public sealed class PackagesWorkspaceViewModel : WorkspaceViewModel
    {
        private readonly ErpFileViewModel _fileViewModel;
        private readonly ResourcesWorkspaceViewModel _resourcesWorkspaceViewModel;
        private readonly ILogger<PackagesWorkspaceViewModel> _logger;
        private readonly ObservableList<ErpPackageViewModel> _packages;
        private readonly ISynchronizedView<ErpPackageViewModel, ErpPackageViewModel> _packagesView;
        public NotifyCollectionChangedSynchronizedViewList<ErpPackageViewModel> Packages { get; }

        public override string DisplayName
        {
            get;
            protected set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public string? FilterText
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
                _packagesView.AttachFilter(PackagesFilter);
            }
        }

        public ICommand Export { get; }
        public ICommand Import { get; }
        public ICommand ExportAll { get; }
        public ICommand ImportAll { get; }

        public PackagesWorkspaceViewModel() : this(new ErpFileViewModel(), new ResourcesWorkspaceViewModel(),
            NullLogger<PackagesWorkspaceViewModel>.Instance)
        {
        }

        public PackagesWorkspaceViewModel(ErpFileViewModel fileViewModel,
            ResourcesWorkspaceViewModel resourcesWorkspaceViewModel, ILogger<PackagesWorkspaceViewModel> logger)
        {
            _fileViewModel = fileViewModel;
            _resourcesWorkspaceViewModel = resourcesWorkspaceViewModel;
            _logger = logger;
            DisplayName = "Pkg Files";

            _packages = [];
            _packagesView = _packages.CreateView(x => x);
            _packagesView.AttachFilter(PackagesFilter);
            Packages = _packagesView.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            Export = new AsyncRelayCommand<ErpPackageViewModel>(Export_Execute, Export_CanExecute);
            Import = new AsyncRelayCommand<ErpPackageViewModel>(Import_Execute, Import_CanExecute);
            ExportAll = new AsyncRelayCommand(ExportAll_Execute, ExportAll_CanExecute);
            ImportAll = new AsyncRelayCommand(ImportAll_Execute, ImportAll_CanExecute);
        }

        public override void OnFileOpened()
        {
            OnFileClosed();
            foreach (var resView in _resourcesWorkspaceViewModel.Resources)
            {
                var resource = resView.Resource;
                foreach (var fragment in resource.Fragments)
                {
                    try
                    {
                        using var ds = fragment.GetDecompressDataStream(true);
                        if (PkgFile.IsPkgFile(ds))
                        {
                            _packages.Add(new ErpPackageViewModel(resView, fragment));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to detect pkg file {resource} {fragment}.", resource.Identifier,
                            fragment.Name);
                    }
                }
            }
            DisplayName = "Pkg Files " + _packages.Count;
        }

        public override void OnFileClosed()
        {
            _packages.Clear();
        }

        private bool PackagesFilter(ErpPackageViewModel item)
        {
            return string.IsNullOrEmpty(FilterText)
                || item.DisplayName.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        }

        private bool Export_CanExecute(ErpPackageViewModel? parameter)
        {
            return parameter != null;
        }

        private async Task Export_Execute(ErpPackageViewModel? pkgView)
        {
            ArgumentNullException.ThrowIfNull(pkgView);
            var dialog = new FileSaveOptions
            {
                FileTypeChoices = [FilePickerType.Json, FilePickerType.All],
                Title = "Select the pkg save location and file name",
                FileName = pkgView.DisplayName + ".json"
            };

            var res = await FileDialog.ShowSaveFileDialog(dialog);
            if (res is not null)
            {
                try
                {
                    using var fs = File.Open(res, FileMode.Create, FileAccess.Write, FileShare.Read);
                    using var sw = new StreamWriter(fs);
                    pkgView.ExportPkg(sw);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not export pkg file!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool Import_CanExecute(ErpPackageViewModel? parameter)
        {
            return parameter != null;
        }

        private async Task Import_Execute(ErpPackageViewModel? pkgView)
        {
            ArgumentNullException.ThrowIfNull(pkgView);
            var dialog = new FileOpenOptions
            {
                FileTypeChoices = [FilePickerType.Json, FilePickerType.All],
                Title = "Select a pkg file",
                FileName = pkgView.DisplayName + ".json"
            };

            var res = await FileDialog.ShowOpenFileDialog(dialog);
            if (res.Count > 0)
            {
                try
                {
                    using var fs = File.Open(res[0], FileMode.Open, FileAccess.Read, FileShare.Read);
                    pkgView.ImportPkg(fs);
                    pkgView.IsSelected = false;
                    pkgView.IsSelected = true;
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not import pkg file!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportAll_CanExecute()
        {
            return _packages.Count > 0;
        }

        private async Task ExportAll_Execute()
        {
            try
            {
                var success = 0;
                var fail = 0;
                var progDialogVM = new ProgressDialogViewModel()
                {
                    PercentageMax = _packages.Count
                };

                var task = Task.Run(() =>
                {
                    var outputFolder = _fileViewModel.FilePath.Replace(".", "_") + "_pkgfiles";
                    Directory.CreateDirectory(outputFolder);

                    for (var i = 0; i < _packages.Count;)
                    {
                        var resource = _packages[i].Resource;
                        var fragment = _packages[i].Fragment;
                        var folderPath = Path.Combine(outputFolder, resource.Folder);
                        var fileName = ErpResourceExporter.GetFragmentFileName(resource, fragment);
                        var filePath = Path.Combine(folderPath, fileName) + ".json";
                        progDialogVM.ProgressStatus.Report("Exporting " + fileName + "... ");

                        try
                        {
                            Directory.CreateDirectory(folderPath);

                            using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                            using var sw = new StreamWriter(fs);
                            _packages[i].ExportPkg(sw);

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

                await ErpDialog.ShowProgressDialog(progDialogVM);
                await task;
            }
            catch
            {
                await MessageBox.Show("There was an error, could not export all pkg files!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ImportAll_CanExecute()
        {
            return _packages.Count > 0;
        }

        private async Task ImportAll_Execute()
        {
            try
            {
                var directory = _fileViewModel.FilePath.Replace(".", "_") + "_pkgfiles";
                if (Directory.Exists(directory) == true)
                {
                    var success = 0;
                    var fail = 0;
                    var skip = 0;
                    var found = false;

                    var progDialogVM = new ProgressDialogViewModel
                    {
                        PercentageMax = _packages.Count
                    };

                    var task = Task.Run(() =>
                    {
                        for (var i = 0; i < _packages.Count;)
                        {
                            var resource = _packages[i].Resource;
                            var fragment = _packages[i].Fragment;
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
                                        _packages[i].ImportPkg(fs);

                                        if (_packages[i].IsSelected)
                                        {
                                            _packages[i].IsSelected = false;
                                            _packages[i].IsSelected = true;
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
                    await MessageBox.Show("Could not find pkgfiles folder!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                await MessageBox.Show("There was an error, could not import all pkg files!", Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
