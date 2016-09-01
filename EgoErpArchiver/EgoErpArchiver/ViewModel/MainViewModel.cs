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
        string windowTitle;
        string filePath;
        ErpFile file;
        readonly ResourcesWorkspaceViewModel resourcesWorkspace;
        readonly PackagesWorkspaceViewModel packagesWorkspace;
        readonly TexturesWorkspaceViewModel texturesWorkspace;
        readonly XmlFilesWorkspaceViewModel xmlFilesWorkspace;

        public override string DisplayName
        {
            get { return windowTitle; }
            protected set
            {
                windowTitle = value;
                OnPropertyChanged("DisplayName");
            }
        }
        public string FilePath
        {
            get { return filePath; }
        }

        public ErpFile ErpFile
        {
            get { return file; }
        }
        public ResourcesWorkspaceViewModel ResourcesWorkspace
        {
            get { return resourcesWorkspace; }
        }
        public PackagesWorkspaceViewModel PackagesWorkspace
        {
            get { return packagesWorkspace; }
        }
        public TexturesWorkspaceViewModel TexturesWorkspace
        {
            get { return texturesWorkspace; }
        }
        public XmlFilesWorkspaceViewModel XmlFilesWorkspace
        {
            get { return xmlFilesWorkspace; }
        }
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
            this.DisplayName = Properties.Resources.AppTitleLong;

            resourcesWorkspace = new ResourcesWorkspaceViewModel(this);
            texturesWorkspace = new TexturesWorkspaceViewModel(this);
            packagesWorkspace = new PackagesWorkspaceViewModel(this);
            xmlFilesWorkspace = new XmlFilesWorkspaceViewModel(this);

            // Commands
            openCommand = new RelayCommand(OpenCommand_Execute);
            saveCommand = new RelayCommand(SaveCommand_Execute, SaveCommand_CanExecute);

            if (string.IsNullOrEmpty(Properties.Settings.Default.F12016Dir))
            {
                Properties.Settings.Default["F12016Dir"] = string.Empty;
            }
        }

        #region MainMenu
        readonly RelayCommand openCommand;
        readonly RelayCommand saveCommand;

        public RelayCommand OpenCommand
        {
            get { return openCommand; }
        }
        public RelayCommand SaveCommand
        {
            get { return saveCommand; }
        }

        public void ParseCommandLineArguments()
        {
            string[] args = (string[])Application.Current.Resources["CommandLineArgs"];

            if (args.Length > 0)
            {
                Open(args[0]);
            }
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
                if (texturesWorkspace.Textures.Count > 0) SelectedTabIndex = 2;
                else if (xmlFilesWorkspace.XmlFiles.Count > 0) SelectedTabIndex = 3;
                else if (packagesWorkspace.Packages.Count > 0) SelectedTabIndex = 1;
                else SelectedTabIndex = 0;
                DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
            }
            catch (Exception excp)
            {
                // Fail
                DisplayName = Properties.Resources.AppTitleLong;
                MessageBox.Show("The program could not open this file!" + Environment.NewLine + Environment.NewLine + excp.Message, "Could Not Open", MessageBoxButton.OK, MessageBoxImage.Error);
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
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(filePath);
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    Task.Run(() => file.Write(File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))).Wait();
                    filePath = saveFileDialog.FileName;
                    DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The program could not save this file! The error is displayed below:" + Environment.NewLine + Environment.NewLine + ex.Message, "Could Not Save", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ClearVars()
        {
            if (file == null) return;

            resourcesWorkspace.ClearData();
            texturesWorkspace.ClearData();

            DisplayName = Properties.Resources.AppTitleLong;
            file = null;
        }
        #endregion
    }
}