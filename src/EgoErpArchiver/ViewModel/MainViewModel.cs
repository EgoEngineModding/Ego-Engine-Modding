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

        /// <summary>
        /// A temporary file object, used only for reading
        /// and merging data into the primary file.
        /// </summary>
        private ErpFile mergeFile;

        private readonly ResourcesWorkspaceViewModel resourcesWorkspace;
        private readonly PackagesWorkspaceViewModel packagesWorkspace;
        private readonly TexturesWorkspaceViewModel texturesWorkspace;
        private readonly XmlFilesWorkspaceViewModel xmlFilesWorkspace;

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
        int selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return selectedTabIndex; }
            set
            {
                selectedTabIndex = value;
                OnPropertyChanged("SelectedTabIndex");
            }
        }
        #endregion


        public MainViewModel()
        {
            DisplayName = Properties.Resources.AppTitleLong;

            resourcesWorkspace = new ResourcesWorkspaceViewModel(this);
            texturesWorkspace = new TexturesWorkspaceViewModel(this);
            packagesWorkspace = new PackagesWorkspaceViewModel(this);
            xmlFilesWorkspace = new XmlFilesWorkspaceViewModel(this);

            // Commands
            OpenCommand = new RelayCommand(OpenCommand_Execute);
            SaveCommand = new RelayCommand(SaveCommand_Execute, SaveCommand_CanExecute);

            if (string.IsNullOrEmpty(Properties.Settings.Default.F12016Dir))
            {
                Properties.Settings.Default.F12016Dir = string.Empty;
            }
        }

        #region MainMenu
        public RelayCommand OpenCommand { get; init; }
        public RelayCommand SaveCommand { get; init; }

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
        private void LoadData(ErpFile file, ResourcesWorkspaceViewModel workspace)
        {
            resourcesWorkspace.LoadData(file);
            packagesWorkspace.LoadData(resourcesWorkspace);
            texturesWorkspace.LoadData(resourcesWorkspace);
            xmlFilesWorkspace.LoadData(resourcesWorkspace);
        }

        private void Merge(string filename, bool overwrite = true)
        {

        }

        private void Open(string filename)
        {
            try
            {
                ClearVars();
                file = new ErpFile();

                using (FileStream fin = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    Task.Run(() => file.Read(fin)).Wait();

                LoadData(file, resourcesWorkspace);
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
                    if (packagesWorkspace.Packages.Count > 0)
                        SelectedTabIndex = 1;
                    else
                        SelectTab(-1);
                    break;

                case 2:
                    if (texturesWorkspace.Textures.Count > 0)
                        SelectedTabIndex = 2;
                    else
                        SelectTab(-1);
                    break;

                case 3:
                    if (xmlFilesWorkspace.XmlFiles.Count > 0)
                        SelectedTabIndex = 3;
                    else
                        SelectTab(-1);
                    break;

                default:
                    if (texturesWorkspace.Textures.Count > 0)
                        SelectedTabIndex = 2;
                    else if (packagesWorkspace.Packages.Count > 0)
                        SelectedTabIndex = 1;
                    else if (xmlFilesWorkspace.XmlFiles.Count > 0)
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

            resourcesWorkspace.ClearData();
            texturesWorkspace.ClearData();

            DisplayName = Properties.Resources.AppTitleLong;
            file = null;
            mergeFile = null;
        }
        #endregion
    }
}