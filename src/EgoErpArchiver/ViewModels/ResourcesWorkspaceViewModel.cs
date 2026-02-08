using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Formats.Erp;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;

using EgoErpArchiver.Dialogs.Erp;

namespace EgoErpArchiver.ViewModels
{
    public class ResourcesWorkspaceViewModel : WorkspaceViewModel
    {
        private readonly ErpResourceExporter resourceExporter;

        private readonly ObservableCollection<ErpResourceViewModel> resources;
        public ObservableCollection<ErpResourceViewModel> Resources
        {
            get { return resources; }
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

        private ErpResourceViewModel selectedItem;
        public ErpResourceViewModel SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (!object.ReferenceEquals(value, selectedItem))
                {
                    selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public ICommand Export { get; }
        public ICommand Import { get; }
        public ICommand ExportAll { get; }
        public ICommand ImportAll { get; }

        public ResourcesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            resourceExporter = new ErpResourceExporter();
            resources = new ObservableCollection<ErpResourceViewModel>();
            _displayName = "All Resources";

            Export = new AsyncRelayCommand<ErpResourceViewModel>(Export_Execute, Export_CanExecute);
            Import = new AsyncRelayCommand<ErpResourceViewModel>(Import_Execute, Import_CanExecute);
            ExportAll = new AsyncRelayCommand(ExportAll_Execute, ExportAll_CanExecute);
            ImportAll = new AsyncRelayCommand(ImportAll_Execute, ImportAll_CanExecute);
        }

        public override void LoadData(object data)
        {
            foreach (var resource in ((ErpFile)data).Resources)
            {
                resources.Add(new ErpResourceViewModel(resource, this));
            }
            DisplayName = "All Resources " + resources.Count;
        }

        public override void ClearData()
        {
            resources.Clear();
        }

        private bool Export_CanExecute(ErpResourceViewModel? parameter)
        {
            return parameter != null;
        }

        private async Task Export_Execute(ErpResourceViewModel? resView)
        {
            ArgumentNullException.ThrowIfNull(resView);
            var dlg = new FolderOpenOptions
            {
                Title = "Select a folder to export the resource:",
                AllowMultiple = false
            };

            var res = await FileDialog.ShowOpenFolderDialog(dlg);
            if (res.Count > 0)
            {
                try
                {
                    resourceExporter.ExportResource(resView.Resource, res[0]);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Failed Exporting!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool Import_CanExecute(ErpResourceViewModel? parameter)
        {
            return parameter != null;
        }

        private async Task Import_Execute(ErpResourceViewModel? resView)
        {
            ArgumentNullException.ThrowIfNull(resView);
            var dlg = new FolderOpenOptions
            {
                Title = "Select a folder to import the resource from:",
                AllowMultiple = false
            };

            var res = await FileDialog.ShowOpenFolderDialog(dlg);
            if (res.Count > 0)
            {
                try
                {
                    var files = Directory.GetFiles(res[0], "*", SearchOption.AllDirectories);
                    resourceExporter.ImportResource(resView.Resource, files);
                    resView.UpdateSize();
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Failed Importing!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportAll_CanExecute()
        {
            return resources.Count > 0;
        }

        private async Task ExportAll_Execute()
        {
            var dlg = new FolderOpenOptions
            {
                Title = "Select a folder to export the resources:",
                AllowMultiple = false
            };

            var res = await FileDialog.ShowOpenFolderDialog(dlg);
            if (res.Count > 0)
            {
                try
                {
                    var progDialogVM = new ProgressDialogViewModel()
                    {
                        PercentageMax = mainView.ErpFile.Resources.Count
                    };

                    var task = Task.Run(() => resourceExporter.Export(mainView.ErpFile, res[0], progDialogVM.ProgressStatus, progDialogVM.ProgressPercentage));
                    await ErpDialog.ShowProgressDialog(progDialogVM);
                    await task;
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Failed Exporting!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ImportAll_CanExecute()
        {
            return resources.Count > 0;
        }

        private async Task ImportAll_Execute()
        {
            var dlg = new FolderOpenOptions
            {
                Title = "Select a folder to import the resources from:",
                AllowMultiple = false
            };

            var res = await FileDialog.ShowOpenFolderDialog(dlg);
            if (res.Count > 0)
            {
                try
                {
                    var progDialogVM = new ProgressDialogViewModel()
                    {
                        PercentageMax = mainView.ErpFile.Resources.Count
                    };

                    var files = Directory.GetFiles(res[0], "*", SearchOption.AllDirectories);
                    var task = Task.Run(() => resourceExporter.Import(mainView.ErpFile, files, progDialogVM.ProgressStatus, progDialogVM.ProgressPercentage));
                    await ErpDialog.ShowProgressDialog(progDialogVM);
                    await task;

                    foreach (var child in resources)
                    {
                        child.UpdateSize();
                    }
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Failed Importing!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
