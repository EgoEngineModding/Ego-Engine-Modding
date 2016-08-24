using EgoEngineLibrary.Graphics;
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

namespace EgoPssgEditor.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Data
        readonly string schemaPath;
        string windowTitle;
        string filePath;
        PssgFile file;
        readonly NodesWorkspaceViewModel nodesWorkspace;
        readonly TexturesWorkspaceViewModel texturesWorkspace;

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

        public PssgFile PssgFile
        {
            get { return file; }
        }
        public NodesWorkspaceViewModel NodesWorkspace
        {
            get { return nodesWorkspace; }
        }
        public TexturesWorkspaceViewModel TexturesWorkspace
        {
            get { return texturesWorkspace; }
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
            schemaPath = AppDomain.CurrentDomain.BaseDirectory + "\\schema.xml";

            nodesWorkspace = new NodesWorkspaceViewModel(this);
            texturesWorkspace = new TexturesWorkspaceViewModel(this);

            // Commands
            newCommand = new RelayCommand(NewCommand_Execute);
            openCommand = new RelayCommand(OpenCommand_Execute);
            saveCommand = new RelayCommand(SaveCommand_Execute, SaveCommand_CanExecute);
            savePssgCommand = new RelayCommand(SavePssgCommand_Execute, SaveCommand_CanExecute);
            saveCompressedCommand = new RelayCommand(SaveCompressedCommand_Execute, SaveCommand_CanExecute);
            saveXmlCommand = new RelayCommand(SaveXmlCommand_Execute, SaveCommand_CanExecute);
            loadSchema = new RelayCommand(LoadSchema_Execute);
            saveSchema = new RelayCommand(SaveSchema_Execute);
            clearSchema = new RelayCommand(ClearSchema_Execute);

            try { LoadSchema_Execute(null); } catch { }
            ParseCommandLineArguments();
        }

        #region MainMenu
        readonly RelayCommand newCommand;
        readonly RelayCommand openCommand;
        readonly RelayCommand saveCommand;
        readonly RelayCommand savePssgCommand;
        readonly RelayCommand saveCompressedCommand;
        readonly RelayCommand saveXmlCommand;
        readonly RelayCommand loadSchema;
        readonly RelayCommand saveSchema;
        readonly RelayCommand clearSchema;

        public RelayCommand NewCommand
        {
            get { return newCommand; }
        }
        public RelayCommand OpenCommand
        {
            get { return openCommand; }
        }
        public RelayCommand SaveCommand
        {
            get { return saveCommand; }
        }
        public RelayCommand SavePssgCommand
        {
            get { return savePssgCommand; }
        }
        public RelayCommand SaveCompressedCommand
        {
            get { return saveCompressedCommand; }
        }
        public RelayCommand SaveXmlCommand
        {
            get { return saveXmlCommand; }
        }
        public RelayCommand LoadSchema
        {
            get { return loadSchema; }
        }
        public RelayCommand SaveSchema
        {
            get { return saveSchema; }
        }
        public RelayCommand ClearSchema
        {
            get { return clearSchema; }
        }

        public void ParseCommandLineArguments()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                try
                {
                    filePath = args[1];
                    ClearVars(true);
                    file = PssgFile.Open(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                    nodesWorkspace.LoadData(file);
                    texturesWorkspace.LoadData(nodesWorkspace.RootNode);
                    if (texturesWorkspace.Textures.Count > 0) SelectedTabIndex = 1;
                    DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
                }
                catch (Exception excp)
                {
                    // Fail
                    DisplayName = Properties.Resources.AppTitleLong;
                    MessageBox.Show("The program could not open this file!" + Environment.NewLine + Environment.NewLine + excp.Message, "Could Not Open", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void NewCommand_Execute(object parameter)
        {
            ClearVars(true);
            file = new PssgFile(PssgFileType.Pssg);
            SaveTag();
        }
        private void OpenCommand_Execute(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PSSG files|*.pssg|DDS files|*.dds|Xml files|*.xml|All files|*.*";
            openFileDialog.FilterIndex = 1;
            if (!string.IsNullOrEmpty(filePath))
            {
                openFileDialog.FileName = Path.GetFileNameWithoutExtension(filePath);
                openFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
            }

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    filePath = openFileDialog.FileName;
                    ClearVars(true);
                    file = Task<PssgFile>.Run(() => { return PssgFile.Open(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)); }).Result;
                    nodesWorkspace.LoadData(file);
                    texturesWorkspace.LoadData(nodesWorkspace.RootNode);
                    if (texturesWorkspace.Textures.Count > 0) SelectedTabIndex = 1;
                    DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
                }
                catch (Exception excp)
                {
                    // Fail
                    DisplayName = Properties.Resources.AppTitleLong;
                    MessageBox.Show("The program could not open this file!" + Environment.NewLine + Environment.NewLine + excp.Message, "Could Not Open", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool SaveCommand_CanExecute(object parameter)
        {
            return file != null;
        }
        private void SaveCommand_Execute(object parameter)
        {
            SavePssg(0);
        }
        private void SavePssgCommand_Execute(object parameter)
        {
            SavePssg(1);
        }
        private void SaveCompressedCommand_Execute(object parameter)
        {
            SavePssg(2);
        }
        private void SaveXmlCommand_Execute(object parameter)
        {
            SavePssg(3);
        }
        private void LoadSchema_Execute(object parameter)
        {
            PssgSchema.LoadSchema(File.Open(schemaPath, FileMode.Open, FileAccess.Read, FileShare.Read));
        }
        private void SaveSchema_Execute(object parameter)
        {
            PssgSchema.SaveSchema(File.Open(schemaPath, FileMode.Create, FileAccess.Write, FileShare.Read));
        }
        private void ClearSchema_Execute(object parameter)
        {
            PssgSchema.ClearSchema();
        }
        private void SavePssg(int type)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PSSG files|*.pssg|DDS files|*.dds|Xml files|*.xml|All files|*.*";
            if (type == 3)
            {
                saveFileDialog.FilterIndex = 3;
            }
            else
            {
                saveFileDialog.FilterIndex = 1;
            }
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(filePath);
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveTag();
                try
                {
                    FileStream fileStream = File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                    if (type == 0)
                    {
                        Task.Run( () => file.Save(fileStream)).Wait(); // Auto
                    }
                    else if (type == 1)
                    {
                        file.FileType = PssgFileType.Pssg;
                        Task.Run(() => file.Save(fileStream)).Wait(); // Pssg
                    }
                    else if (type == 2)
                    {
                        file.FileType = PssgFileType.CompressedPssg;
                        Task.Run(() => file.Save(fileStream)).Wait();
                    }
                    else
                    {
                        file.FileType = PssgFileType.Xml;
                        Task.Run(() => file.Save(fileStream)).Wait();
                    }
                    filePath = saveFileDialog.FileName;
                    DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The program could not save this file! The error is displayed below:" + Environment.NewLine + Environment.NewLine + ex.Message, "Could Not Save", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void SaveTag()
        {
            PssgNode node;
            if (file.RootNode == null)
            {
                node = new PssgNode("PSSGDATABASE", file, null);
                file.RootNode = node;
                nodesWorkspace.LoadData(file);
            }
            else
            {
                node = file.RootNode;
            }

            PssgAttribute attribute = node.AddAttribute("creatorApplication", Properties.Resources.AppTitleLong);
        }
        private void ClearVars(bool clearPSSG)
        {
            if (file == null) return;

            nodesWorkspace.ClearData();
            texturesWorkspace.ClearData();

            DisplayName = Properties.Resources.AppTitleLong;
            if (clearPSSG == true)
            {
                file = null;
            }
        }
        #endregion
    }
}