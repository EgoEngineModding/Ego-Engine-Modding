using EgoEngineLibrary.Archive.Erp;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace EgoErpArchiver.ViewModel
{
    public class ResourcesWorkspaceViewModel : WorkspaceViewModel
    {
        #region Data
        readonly ObservableCollection<ErpResourceViewModel> resources;

        public override string DisplayName
        {
            get
            {
                return "All Resources";
            }
        }
        public ObservableCollection<ErpResourceViewModel> Resources
        {
            get { return resources; }
        }
        #endregion

        #region Presentation Props
        ErpResourceViewModel selectedItem;
        
        public ErpResourceViewModel SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (!object.ReferenceEquals(value, selectedItem))
                {
                    selectedItem = value;
                    OnPropertyChanged("SelectedItem");
                }
            }
        }
        #endregion

        public ResourcesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            resources = new ObservableCollection<ErpResourceViewModel>();

            export = new RelayCommand(Export_Execute, Export_CanExecute);
            import = new RelayCommand(Import_Execute, Import_CanExecute);
            exportAll = new RelayCommand(ExportAll_Execute, ExportAll_CanExecute);
            importAll = new RelayCommand(ImportAll_Execute, ImportAll_CanExecute);
        }

        public override void LoadData(object data)
        {
            foreach (ErpResource resource in ((ErpFile)data).Resources)
            {
                resources.Add(new ErpResourceViewModel(resource, this));
            }
        }

        public override void ClearData()
        {
            resources.Clear();
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
            ErpResourceViewModel resView = ((ErpResourceViewModel)parameter);
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select a folder to export the resource:";
            dlg.IsFolderPicker = true;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    resView.Resource.Export(dlg.FileName);
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
            ErpResourceViewModel resView = ((ErpResourceViewModel)parameter);
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select a folder to import the resource from:";
            dlg.IsFolderPicker = true;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    string[] files = Directory.GetFiles(dlg.FileName, "*", SearchOption.AllDirectories);
                    resView.Resource.Import(files);
                    resView.UpdateSize();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed Importing!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportAll_CanExecute(object parameter)
        {
            return resources.Count > 0;
        }
        private void ExportAll_Execute(object parameter)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select a folder to export the resources:";
            dlg.IsFolderPicker = true;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    mainView.ErpFile.Export(dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed Exporting!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool ImportAll_CanExecute(object parameter)
        {
            return resources.Count > 0;
        }
        private void ImportAll_Execute(object parameter)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select a folder to import the resources from:";
            dlg.IsFolderPicker = true;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    mainView.ErpFile.Import(dlg.FileName);
                    foreach (ErpResourceViewModel child in resources)
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
        #endregion
    }
}
