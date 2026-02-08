using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;

namespace EgoErpArchiver.ViewModels
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
            openCommand = new AsyncRelayCommand(OpenCommand_Execute);
            saveCommand = new AsyncRelayCommand(SaveCommand_Execute, SaveCommand_CanExecute);

            if (string.IsNullOrEmpty(Properties.Settings.Default.F12016Dir))
            {
                Properties.Settings.Default.F12016Dir = string.Empty;
            }
        }

        #region MainMenu
        readonly ICommand openCommand;
        readonly ICommand saveCommand;

        public ICommand OpenCommand
        {
            get { return openCommand; }
        }
        public ICommand SaveCommand
        {
            get { return saveCommand; }
        }

        public async void ParseCommandLineArguments()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                await Open(args[1]);
            }
        }

        private async Task OpenCommand_Execute()
        {
            var openFileDialog = new FileOpenOptions();
            openFileDialog.FileTypeChoices = [FilePickerType.Erp, FilePickerType.All];
            openFileDialog.SuggestedFileType = FilePickerType.Erp;
            if (!string.IsNullOrEmpty(filePath))
            {
                openFileDialog.FileName = Path.GetFileNameWithoutExtension(filePath);
                openFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
            }

            var res = await FileDialog.ShowOpenFileDialog(openFileDialog);
            if (res.Count > 0)
            {
                await Open(res[0]);
            }
        }
        private async Task Open(string fileName)
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
            catch (Exception excp)
            {
                // Fail
                DisplayName = Properties.Resources.AppTitleLong;
                await MessageBox.Show("The program could not open this file!" + Environment.NewLine + Environment.NewLine + excp.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
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
                    if (packagesWorkspace.Packages.Count > 0) SelectedTabIndex = 1;
                    else SelectTab(-1);
                    break;
                case 2:
                    if (texturesWorkspace.Textures.Count > 0) SelectedTabIndex = 2;
                    else SelectTab(-1);
                    break;
                case 3:
                    if (xmlFilesWorkspace.XmlFiles.Count > 0) SelectedTabIndex = 3;
                    else SelectTab(-1);
                    break;
                default:
                    if (texturesWorkspace.Textures.Count > 0) SelectedTabIndex = 2;
                    else if (packagesWorkspace.Packages.Count > 0) SelectedTabIndex = 1;
                    else if (xmlFilesWorkspace.XmlFiles.Count > 0) SelectedTabIndex = 3;
                    else SelectedTabIndex = 0;
                    break;
            }
        }
        private bool SaveCommand_CanExecute()
        {
            return file != null;
        }
        private async Task SaveCommand_Execute()
        {
            var saveFileDialog = new FileSaveOptions();
            saveFileDialog.FileTypeChoices = [FilePickerType.Erp, FilePickerType.All];
            saveFileDialog.SuggestedFileType = FilePickerType.Erp;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(filePath);
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);

            var res = await FileDialog.ShowSaveFileDialog(saveFileDialog);
            if (res is not null)
            {
                try
                {
                    Task.Run(() => file.Write(File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))).Wait();
                    filePath = saveFileDialog.FileName;
                    DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("The program could not save this file! The error is displayed below:" + Environment.NewLine + Environment.NewLine + ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
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
