using EgoEngineLibrary.Graphics;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace EgoPssgEditor.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Data
        string windowTitle;
        string filePath;
        PssgFile file;
        readonly NodesWorkspaceViewModel nodesWorkspace;
        readonly TexturesWorkspaceViewModel texturesWorkspace;

        public override string DisplayName
        {
            get { return windowTitle; }
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
        
        #endregion


        public MainViewModel()
        {
            windowTitle = Properties.Resources.AppTitleLong;

            nodesWorkspace = new NodesWorkspaceViewModel(this);
            texturesWorkspace = new TexturesWorkspaceViewModel(this);

            // Commands
            newCommand = new RelayCommand(NewCommand_Execute);
            openCommand = new RelayCommand(OpenCommand_Execute);
            saveCommand = new RelayCommand(SaveCommand_Execute, SaveCommand_CanExecute);
        }

        #region MainMenu
        readonly RelayCommand newCommand;
        readonly RelayCommand openCommand;
        readonly RelayCommand saveCommand;

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

        private void NewCommand_Execute(object parameter)
        {
            ClearVars(true);
            file = new PssgFile(PssgFileType.Pssg);
        }
        private void OpenCommand_Execute(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
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
                    file = PssgFile.Open(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                    nodesWorkspace.LoadData(file);
                    texturesWorkspace.LoadData(file);
                }
                catch (Exception excp)
                {
                    // Fail
                    windowTitle = Properties.Resources.AppTitleLong;
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
            SavePssg(1);
        }
        private void SavePssg(int type)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
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
                        file.Save(fileStream); // Auto
                    }
                    else if (type == 1)
                    {
                        file.FileType = PssgFileType.Pssg;
                        file.Save(fileStream); // Pssg
                    }
                    else if (type == 2)
                    {
                        file.FileType = PssgFileType.CompressedPssg;
                        file.Save(fileStream);
                    }
                    else
                    {
                        file.FileType = PssgFileType.Xml;
                        file.Save(fileStream);
                    }
                    filePath = saveFileDialog.FileName;
                    windowTitle = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
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

            windowTitle = Properties.Resources.AppTitleLong;
            if (clearPSSG == true)
            {
                file = null;
            }
        }
        #endregion
    }
}