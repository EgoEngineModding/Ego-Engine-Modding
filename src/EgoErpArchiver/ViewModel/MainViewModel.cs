using EgoEngineLibrary.Archive.Erp;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace EgoErpArchiver.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Data
        private string windowTitle;
        private string filepath;
        private ErpFile file;

        /// <summary>
        /// Environment-specific newline.
        /// </summary>
        private readonly string NL = Environment.NewLine;

        public ResourcesWorkspaceViewModel ResourcesWorkspace { get; init; }
        public PackagesWorkspaceViewModel PackagesWorkspace { get; init; }
        public TexturesWorkspaceViewModel TexturesWorkspace { get; init; }
        public XmlFilesWorkspaceViewModel XmlFilesWorkspace { get; init; }

        public override string DisplayName
        {
            get { return windowTitle; }
            protected set
            {
                windowTitle = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }
        public string FilePath => filepath;
        public ErpFile ErpFile => file;
        #endregion

        #region Presentation Data
        private int selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return selectedTabIndex; }
            set
            {
                selectedTabIndex = value;
                OnPropertyChanged(nameof(SelectedTabIndex));
            }
        }
        #endregion


        public MainViewModel()
        {
            DisplayName = Properties.Resources.AppTitleLong;

            ResourcesWorkspace = new ResourcesWorkspaceViewModel(this);
            TexturesWorkspace = new TexturesWorkspaceViewModel(this);
            PackagesWorkspace = new PackagesWorkspaceViewModel(this);
            XmlFilesWorkspace = new XmlFilesWorkspaceViewModel(this);

            // Commands
            OpenCommand = new RelayCommand(OpenCommand_Execute);
            SaveCommand = new RelayCommand(SaveCommand_Execute, SaveCommand_CanExecute);
            MergePreserveCommand = new RelayCommand(MergePreserve_Execute, SaveCommand_CanExecute);
            MergeOverwriteCommand = new RelayCommand(MergeOverwrite_Execute, SaveCommand_CanExecute);

            if (string.IsNullOrEmpty(Properties.Settings.Default.F12016Dir))
            {
                Properties.Settings.Default.F12016Dir = string.Empty;
            }
        }

        #region MainMenu
        public RelayCommand OpenCommand { get; init; }
        public RelayCommand SaveCommand { get; init; }
        public RelayCommand MergePreserveCommand { get; init; }
        public RelayCommand MergeOverwriteCommand { get; init; }

        public void ParseCommandLineArguments()
        {
            string[] args = (string[])Application.Current.Resources["CommandLineArgs"];

            if (args.Length > 0)
                Open(args[0]);
        }

        private void OpenCommand_Execute(object parameter)
        {
            string filename;
            if (OpenERPFileDialog(out filename))
                Open(filename);
        }

        private bool OpenERPFileDialog(out string filename)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Erp files|*.erp|All files|*.*";
            openFileDialog.FilterIndex = 1;

            // If a file is already open, set the dialog to the current file's location
            if (!string.IsNullOrEmpty(filepath))
            {
                openFileDialog.FileName = Path.GetFileNameWithoutExtension(filepath);
                openFileDialog.InitialDirectory = Path.GetDirectoryName(filepath);
            }

            bool result = openFileDialog.ShowDialog() ?? false;
            filename = openFileDialog.FileName;

            return result;
        }

        /// <summary>
        /// Load data into the workspace.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="workspace"></param>
        internal void UpdateWorkspace()
        {
            ResourcesWorkspace.ClearData();
            TexturesWorkspace.ClearData();

            ResourcesWorkspace.LoadData(file);
            PackagesWorkspace.LoadData(ResourcesWorkspace);
            TexturesWorkspace.LoadData(ResourcesWorkspace);
            XmlFilesWorkspace.LoadData(ResourcesWorkspace);
        }

        private void MergePreserve_Execute(object parameter)
        {
            string filename;
            if (OpenERPFileDialog(out filename))
                Merge(filename, overwrite: false);
        }

        private void MergeOverwrite_Execute(object parameter)
        {
            string filename;
            if (OpenERPFileDialog(out filename))
                Merge(filename, overwrite: true);
        }

        /// <summary>
        /// Merge a new file's data with the existing loading file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="overwrite"></param>
        private void Merge(string filename, bool overwrite = false)
        {
            // This shouldn't ever happen
            if (file == null)
            {
                MessageBox.Show($"Before a merge operation, a file must be opened!",
                   Properties.Resources.AppTitleLong,
                   MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ErpFile mergeFile = new ErpFile();

            try
            {
                using FileStream fin = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                Task.Run(() => mergeFile.Read(fin)).Wait();
            }
            catch (Exception ex)
            {
                // Fail
                DisplayName = Properties.Resources.AppTitleLong;
                MessageBox.Show($"The program could not open this file for merging!{NL + NL}{filename}{NL + NL}{ex.Message}",
                    Properties.Resources.AppTitleLong,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            foreach (ErpResource resource in mergeFile.Resources)
            {
                string id = resource.Identifier;
                ErpResource orig = file.Resources.Find(x => x.Identifier == id);
                bool foundOrig = orig != null;

                if (!overwrite && !foundOrig)
                {
                    file.Resources.Add(resource);
                }
                else if (overwrite)
                {
                    if (foundOrig)
                        file.Resources.Remove(orig);
                    file.Resources.Add(resource);
                }
            }

            file.UpdateOffsets();
            UpdateWorkspace();
        }

        private void Open(string filename)
        {
            try
            {
                ClearVars();
                file = new ErpFile();

                using FileStream fin = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                Task.Run(() => file.Read(fin)).Wait();

                UpdateWorkspace();
                SelectTab(Properties.Settings.Default.StartingTab);
                DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filename);

                filepath = filename;
            }
            catch (Exception ex)
            {
                // Fail
                DisplayName = Properties.Resources.AppTitleLong;
                MessageBox.Show($"The program could not open this file!{NL+NL}{ex.Message}",
                    Properties.Resources.AppTitleLong,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectTab(int index)
        {
            switch (index)
            {
                case 0:
                    SelectedTabIndex = 0;
                    break;

                case 1:
                    if (PackagesWorkspace.Packages.Count > 0)
                        SelectedTabIndex = 1;
                    else
                        SelectTab(-1);
                    break;

                case 2:
                    if (TexturesWorkspace.Textures.Count > 0)
                        SelectedTabIndex = 2;
                    else
                        SelectTab(-1);
                    break;

                case 3:
                    if (XmlFilesWorkspace.XmlFiles.Count > 0)
                        SelectedTabIndex = 3;
                    else
                        SelectTab(-1);
                    break;

                default:
                    if (TexturesWorkspace.Textures.Count > 0)
                        SelectedTabIndex = 2;
                    else if (PackagesWorkspace.Packages.Count > 0)
                        SelectedTabIndex = 1;
                    else if (XmlFilesWorkspace.XmlFiles.Count > 0)
                        SelectedTabIndex = 3;
                    else
                        SelectedTabIndex = 0;
                    break;
            }
        }
        private bool SaveCommand_CanExecute(object parameter)
        {
            return file != null;
        }

        private void SaveCommand_Execute(object parameter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Erp files|*.erp|All files|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(filepath);
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(filepath);

            if (saveFileDialog.ShowDialog() ?? false)
            {
                try
                {
                    using (FileStream fout = File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                        Task.Run(() => file.Write(fout)).Wait();
                    filepath = saveFileDialog.FileName;
                    DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filepath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"The program could not save this file!{NL+NL}{ex.Message}",
                        Properties.Resources.AppTitleLong,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ClearVars()
        {
            if (file == null)
                return;

            ResourcesWorkspace.ClearData();
            TexturesWorkspace.ClearData();

            DisplayName = Properties.Resources.AppTitleLong;
            file = null;
        }
        #endregion
    }
}