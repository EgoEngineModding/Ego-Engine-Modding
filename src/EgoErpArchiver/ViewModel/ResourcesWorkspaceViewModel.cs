using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Formats.Erp;
using Microsoft.VisualBasic;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.XPath;

namespace EgoErpArchiver.ViewModel
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

        public RelayCommand Rename { get; }
        public RelayCommand Repath { get; }
        public RelayCommand Export { get; }
        public RelayCommand Import { get; }
        public RelayCommand ExportAll { get; }
        public RelayCommand ExportAllFilter { get; }
        public RelayCommand ImportAll { get; }

        public ResourcesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            resourceExporter = new ErpResourceExporter();
            resources = new ObservableCollection<ErpResourceViewModel>();
            _displayName = "All Resources";

            Rename = new RelayCommand(Rename_Execute, Rename_CanExecute);
            Repath = new RelayCommand(Repath_Execute, Rename_CanExecute);
            Export = new RelayCommand(Export_Execute, Export_CanExecute);
            Import = new RelayCommand(Import_Execute, Import_CanExecute);
            ExportAll = new RelayCommand(ExportAll_Execute, ExportAll_CanExecute);
            ExportAllFilter = new RelayCommand(ExportAllFilter_Execute, ExportAll_CanExecute);
            ImportAll = new RelayCommand(ImportAll_Execute, ImportAll_CanExecute);
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

        private bool Rename_CanExecute(object parameter)
        {
            return parameter != null;
        }

        private void Rename_Execute(object parameter)
        {
            var resView = (ErpResourceViewModel)parameter;
            var res = resView.Resource;

            string result = Interaction.InputBox(
                Prompt: "Enter a new name for this resource:",
                Title: "Rename resource",
                DefaultResponse: res.FileName);

            if (string.IsNullOrWhiteSpace(result))
                return;

            res.FileName = result;
            mainView.ErpFile.UpdateOffsets();
            mainView.UpdateWorkspace();
        }

        private void Repath_Execute(object parameter)
        {
            var resView = (ErpResourceViewModel)parameter;
            var res = resView.Resource;

            string result = Interaction.InputBox(
                Prompt: "Enter a new URI for this resource:",
                Title: "Change resource URI",
                DefaultResponse: res.Identifier);

            if (string.IsNullOrWhiteSpace(result))
                return;

            res.Identifier = result;
            mainView.ErpFile.UpdateOffsets();
            mainView.UpdateWorkspace();
        }

        private bool Export_CanExecute(object parameter)
        {
            return parameter != null;
        }

        private void Export_Execute(object parameter)
        {
            var resView = (ErpResourceViewModel)parameter;
            var dlg = new CommonOpenFileDialog
            {
                Title = "Select a folder to export the resource:",
                IsFolderPicker = true,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    resourceExporter.ExportResource(resView.Resource, dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed Exporting!" + Environment.NewLine + Environment.NewLine +
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
            var resView = (ErpResourceViewModel)parameter;
            var dlg = new CommonOpenFileDialog
            {
                Title = "Select a folder to import the resource from:",
                IsFolderPicker = true,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    var files = Directory.GetFiles(dlg.FileName, "*", SearchOption.AllDirectories);
                    resourceExporter.ImportResource(resView.Resource, files);
                    resView.UpdateSize();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed Importing!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Prompt user to enter a file filter.
        /// </summary>
        /// <param name="promptFilter"></param>
        private void ExportAllFunc(bool promptFilter)
        {
            string filter = "";

            if (promptFilter)
            {
                filter = Interaction.InputBox(
                    Prompt: "Enter a suffix filter, e.g. '.material' for all files\n" +
                    "with the .material extension",
                    Title: "Filter");
                if (string.IsNullOrWhiteSpace(filter))
                    return;
            }
            var dlg = new CommonOpenFileDialog
            {
                Title = "Select a folder to export the resources:",
                IsFolderPicker = true,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    var progDialogVM = new ProgressDialogViewModel()
                    {
                        PercentageMax = mainView.ErpFile.Resources.Count
                    };
                    var progDialog = new View.ProgressDialog
                    {
                        DataContext = progDialogVM
                    };

                    var task = Task.Run(() => resourceExporter.Export(mainView.ErpFile, dlg.FileName,
                        progDialogVM.ProgressStatus, progDialogVM.ProgressPercentage,
                        filter: filter));
                    progDialog.ShowDialog();
                    task.Wait();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed Exporting!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportAll_CanExecute(object parameter)
        {
            return resources.Count > 0;
        }

        private void ExportAllFilter_Execute(object parameter)
        {
            ExportAllFunc(promptFilter: true);
        }

        private void ExportAll_Execute(object parameter)
        {
            ExportAllFunc(promptFilter: false);
        }

        private bool ImportAll_CanExecute(object parameter)
        {
            return resources.Count > 0;
        }

        private void ImportAll_Execute(object parameter)
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "Select a folder to import the resources from:",
                IsFolderPicker = true,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    var progDialogVM = new ProgressDialogViewModel()
                    {
                        PercentageMax = mainView.ErpFile.Resources.Count
                    };
                    var progDialog = new View.ProgressDialog
                    {
                        DataContext = progDialogVM
                    };

                    var files = Directory.GetFiles(dlg.FileName, "*", SearchOption.AllDirectories);
                    var task = Task.Run(() => resourceExporter.Import(mainView.ErpFile, files, progDialogVM.ProgressStatus, progDialogVM.ProgressPercentage));
                    progDialog.ShowDialog();
                    task.Wait();

                    foreach (var child in resources)
                    {
                        child.UpdateSize();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed Importing!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
