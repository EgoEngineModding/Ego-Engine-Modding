using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;
using EgoEngineLibrary.Frontend.ViewModels;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EgoErpArchiver.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ILogger<MainViewModel> _logger;

        public override string DisplayName
        {
            get;
            protected set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public ErpFileViewModel FileViewModel { get; }
        
        public SettingsViewModel SettingsViewModel { get; }

        public ResourcesWorkspaceViewModel ResourcesWorkspace { get; }

        public PackagesWorkspaceViewModel PackagesWorkspace { get; }

        public TexturesWorkspaceViewModel TexturesWorkspace { get; }

        public XmlFilesWorkspaceViewModel XmlFilesWorkspace { get; }

        public int SelectedTabIndex
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
            : this(new ErpFileViewModel(), new SettingsViewModel(), new ResourcesWorkspaceViewModel(),
                new TexturesWorkspaceViewModel(), new PackagesWorkspaceViewModel(), new XmlFilesWorkspaceViewModel(),
                NullLogger<MainViewModel>.Instance)
        {
        }

        public MainViewModel(ErpFileViewModel fileViewModel,
            SettingsViewModel settingsViewModel,
            ResourcesWorkspaceViewModel resourcesWorkspace,
            TexturesWorkspaceViewModel texturesWorkspace,
            PackagesWorkspaceViewModel packagesWorkspace,
            XmlFilesWorkspaceViewModel xmlFilesWorkspace,
            ILogger<MainViewModel> logger)
        {
            _logger = logger;
            FileViewModel = fileViewModel;
            SettingsViewModel = settingsViewModel;
            DisplayName = Properties.Resources.AppTitleLong;

            ResourcesWorkspace = resourcesWorkspace;
            TexturesWorkspace = texturesWorkspace;
            PackagesWorkspace = packagesWorkspace;
            XmlFilesWorkspace = xmlFilesWorkspace;

            openCommand = new AsyncRelayCommand(OpenCommand_Execute);
            saveCommand = new AsyncRelayCommand(SaveCommand_Execute, SaveCommand_CanExecute);
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
                _logger.LogDebug("Parsing arguments");
                await Open(args[1]);
            }
        }

        private async Task OpenCommand_Execute()
        {
            var openFileDialog = new FileOpenOptions();
            openFileDialog.FileTypeChoices = [FilePickerType.Erp, FilePickerType.All];
            openFileDialog.SuggestedFileType = FilePickerType.Erp;
            if (!string.IsNullOrEmpty(FileViewModel.FilePath))
            {
                openFileDialog.FileName = Path.GetFileNameWithoutExtension(FileViewModel.FilePath);
                openFileDialog.InitialDirectory = Path.GetDirectoryName(FileViewModel.FilePath);
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
                ClearVars();
                FileViewModel.FilePath = fileName;
                var erp = new ErpFile();
                erp.Read(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
                FileViewModel.File = erp;
                ResourcesWorkspace.OnFileOpened();
                PackagesWorkspace.OnFileOpened();
                TexturesWorkspace.OnFileOpened();
                XmlFilesWorkspace.OnFileOpened();
                SelectTab(SettingsViewModel.StartingTab);
                DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(fileName);
            }
            catch (Exception excp)
            {
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
                    if (PackagesWorkspace.Packages.Count > 0) SelectedTabIndex = 1;
                    else SelectTab(-1);
                    break;
                case 2:
                    if (TexturesWorkspace.Textures.Count > 0) SelectedTabIndex = 2;
                    else SelectTab(-1);
                    break;
                case 3:
                    if (XmlFilesWorkspace.XmlFiles.Count > 0) SelectedTabIndex = 3;
                    else SelectTab(-1);
                    break;
                default:
                    if (TexturesWorkspace.Textures.Count > 0) SelectedTabIndex = 2;
                    else if (PackagesWorkspace.Packages.Count > 0) SelectedTabIndex = 1;
                    else if (XmlFilesWorkspace.XmlFiles.Count > 0) SelectedTabIndex = 3;
                    else SelectedTabIndex = 0;
                    break;
            }
        }
        private bool SaveCommand_CanExecute()
        {
            return FileViewModel.File is not null;
        }
        private async Task SaveCommand_Execute()
        {
            var saveFileDialog = new FileSaveOptions();
            saveFileDialog.FileTypeChoices = [FilePickerType.Erp, FilePickerType.All];
            saveFileDialog.SuggestedFileType = FilePickerType.Erp;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(FileViewModel.FilePath);
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(FileViewModel.FilePath);

            var res = await FileDialog.ShowSaveFileDialog(saveFileDialog);
            if (res is not null)
            {
                try
                {
                    FileViewModel.File!.Write(File.Open(res, FileMode.Create, FileAccess.Write, FileShare.Read));
                    FileViewModel.FilePath = res;
                    DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(res);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("The program could not save this file! The error is displayed below:" + Environment.NewLine + Environment.NewLine + ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ClearVars()
        {
            if (FileViewModel.File is null) return;

            ResourcesWorkspace.OnFileClosed();
            TexturesWorkspace.OnFileClosed();

            DisplayName = Properties.Resources.AppTitleLong;
            FileViewModel.File = null;
        }
        #endregion
    }
}
