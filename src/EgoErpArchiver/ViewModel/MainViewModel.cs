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
        public ResourcesWorkspaceViewModel ResourcesWorkspace { get; init; }
        public PackagesWorkspaceViewModel PackagesWorkspace { get; init; }
        public TexturesWorkspaceViewModel TexturesWorkspace { get; init; }
        public XmlFilesWorkspaceViewModel XmlFilesWorkspace { get; init; }
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Erp files|*.erp|All files|*.*";
            openFileDialog.FilterIndex = 1;
            if (!string.IsNullOrEmpty(filePath))
            {
                openFileDialog.FileName = Path.GetFileNameWithoutExtension(filePath);
                openFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
            }

            if (openFileDialog.ShowDialog() == true)
            {
                Open(openFileDialog.FileName);
            }
        }
        private void Open(string fileName)
        {
            try
            {
                filePath = fileName;
                ClearVars();
                this.file = new ErpFile();
                Task.Run(() => this.file.Read(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))).Wait();
                resourcesWorkspace.LoadData(file);
                packagesWorkspace.LoadData(resourcesWorkspace);
                texturesWorkspace.LoadData(resourcesWorkspace);
                xmlFilesWorkspace.LoadData(resourcesWorkspace);
                SelectTab(Properties.Settings.Default.StartingTab);
                DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
            }
            catch (Exception ex)
            {
                // Fail
                DisplayName = Properties.Resources.AppTitleLong;
                MessageBox.Show($"The program could not open this file!\n\n{ex.Message}",
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
                    MessageBox.Show($"The program could not save this file!\n\n{ex.Message}",
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