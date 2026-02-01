using EgoEngineLibrary.Graphics;

using CommunityToolkit.Mvvm.Input;

using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;

namespace EgoPssgEditor.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        #region Data
        readonly string schemaPath;
        string windowTitle;
        string filePath;
        PssgFile file;
        readonly NodesWorkspaceViewModel nodesWorkspace;
        readonly TexturesWorkspaceViewModel texturesWorkspace;
        readonly ModelsWorkspaceViewModel _modelsWorkspace;

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
        public ModelsWorkspaceViewModel ModelsWorkspace
        {
            get { return _modelsWorkspace; }
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
            schemaPath = Path.Combine(AppContext.BaseDirectory, "schema.xml");

            nodesWorkspace = new NodesWorkspaceViewModel(this);
            texturesWorkspace = new TexturesWorkspaceViewModel(this);
            _modelsWorkspace = new ModelsWorkspaceViewModel(this);

            try { LoadSchema(); } catch { }
            ParseCommandLineArguments();
        }

        #region MainMenu

        public async void ParseCommandLineArguments()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                try
                {
                    filePath = args[1];
                    using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var pssg = PssgFile.Open(fs);
                        LoadPssg(pssg);
                    }
                    DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
                }
                catch (Exception excp)
                {
                    // Fail
                    DisplayName = Properties.Resources.AppTitleLong;
                    await MessageBox.Show("The program could not open this file!" + Environment.NewLine + Environment.NewLine + excp.Message, "Could Not Open", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void New()
        {
            ClearVars(true);
            file = new PssgFile(PssgFileType.Pssg);
            SaveTag();
        }
        [RelayCommand]
        private async Task Open()
        {
            FileOpenOptions openOptions = new()
            {
                Title = "Open pssg",
                FileTypeChoices = [FilePickerType.Pssg, FilePickerType.Xml, FilePickerType.All],
                SuggestedFileType = FilePickerType.Pssg,
            };
            if (!string.IsNullOrEmpty(filePath))
            {
                openOptions.FileName = Path.GetFileNameWithoutExtension(filePath);
                openOptions.InitialDirectory = Path.GetDirectoryName(filePath);
            }

            var result = await FileDialog.ShowOpenFileDialog(openOptions);
            if (result.Count > 0)
            {
                try
                {
                    filePath = result[0];
                    using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var pssg = PssgFile.Open(fs);
                        LoadPssg(pssg);
                    }
                    DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
                }
                catch (Exception excp)
                {
                    // Fail
                    DisplayName = Properties.Resources.AppTitleLong;
                    await MessageBox.Show("The program could not open this file!" + Environment.NewLine + Environment.NewLine + excp.Message, "Could Not Open", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool SaveCommand_CanExecute()
        {
            return file != null;
        }
        [RelayCommand(CanExecute = nameof(SaveCommand_CanExecute))]
        private Task Save()
        {
            return SavePssg(file.FileType);
        }
        [RelayCommand(CanExecute = nameof(SaveCommand_CanExecute))]
        private Task SavePssg()
        {
            return SavePssg(PssgFileType.Pssg);
        }
        [RelayCommand(CanExecute = nameof(SaveCommand_CanExecute))]
        private Task SaveCompressed()
        {
            return SavePssg(PssgFileType.CompressedPssg);
        }
        [RelayCommand(CanExecute = nameof(SaveCommand_CanExecute))]
        private Task SaveXml()
        {
            return SavePssg(PssgFileType.Xml);
        }
        [RelayCommand]
        private void LoadSchema()
        {
            PssgSchema.LoadSchema(File.Open(schemaPath, FileMode.Open, FileAccess.Read, FileShare.Read));
        }
        [RelayCommand]
        private void SaveSchema()
        {
            PssgSchema.SaveSchema(File.Open(schemaPath, FileMode.Create, FileAccess.Write, FileShare.Read));
        }
        [RelayCommand]
        private void ClearSchema()
        {
            PssgSchema.ClearSchema();
        }

        public void LoadPssg(PssgFile pssg)
        {
            // if pssg is null, we just want to reload the workspaces
            if (pssg is null)
            {
                ClearVars(false);
            }
            else
            {
                ClearVars(true);
                file = pssg;
            }

            nodesWorkspace.LoadData(file);
            texturesWorkspace.LoadData(nodesWorkspace.RootNode);
            SelectedTabIndex = texturesWorkspace.Textures.Count > 0 ? 1 : 0;
            _modelsWorkspace.LoadData(file);
        }
        private async Task SavePssg(PssgFileType type)
        {
            FileSaveOptions saveOptions = new()
            {
                Title = "Save pssg",
                FileTypeChoices = [FilePickerType.Pssg, FilePickerType.Xml, FilePickerType.All],
                SuggestedFileType = type switch
                {
                    PssgFileType.Pssg => FilePickerType.Pssg,
                    PssgFileType.Xml => FilePickerType.Xml,
                    PssgFileType.CompressedPssg => FilePickerType.Pssg,
                    PssgFileType.CompressedXml => FilePickerType.Xml,
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                },
            };
            saveOptions.FileName = Path.GetFileNameWithoutExtension(filePath);
            saveOptions.InitialDirectory = Path.GetDirectoryName(filePath);

            var result = await FileDialog.ShowSaveFileDialog(saveOptions);
            if (result is not null)
            {
                SaveTag();
                try
                {
                    using (var fileStream = File.Open(result, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                    {
                        if (type == file.FileType)
                        {
                            file.Save(fileStream); // Auto
                        }
                        else
                        {
                            file.FileType = type;
                            file.Save(fileStream);
                        }
                    }
                    filePath = result;
                    DisplayName = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("The program could not save this file! The error is displayed below:" + Environment.NewLine + Environment.NewLine + ex.Message, "Could Not Save", MessageBoxButton.OK, MessageBoxImage.Error);
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
            _modelsWorkspace.ClearData();

            if (clearPSSG == true)
            {
                DisplayName = Properties.Resources.AppTitleLong;
                file = null;
            }
        }
        #endregion
    }
}