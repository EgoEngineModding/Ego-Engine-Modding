using EgoEngineLibrary.Graphics;
using EgoEngineLibrary.Graphics.Dds;

using CommunityToolkit.Mvvm.Input;

using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;
using EgoEngineLibrary.Graphics.Pssg;
using EgoPssgEditor.Dialogs.Pssg;

using ObservableCollections;

namespace EgoPssgEditor.ViewModels
{
    public sealed partial class TexturesWorkspaceViewModel : WorkspaceViewModel
    {
        private readonly ObservableList<PssgTextureViewModel> _textures;
        private readonly ISynchronizedView<PssgTextureViewModel, PssgTextureViewModel> _texturesView;

        public override string DisplayName
        {
            get { return "Textures"; }
        }

        public NotifyCollectionChangedSynchronizedViewList<PssgTextureViewModel> Textures { get; }

        public string FilterText
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
                _texturesView.AttachFilter(TextureFilter);
            }
        }

        public TexturesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            _textures = [];
            _texturesView = _textures.CreateView(x => x);
            _texturesView.AttachFilter(TextureFilter);
            Textures = _texturesView.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        }

        public override void LoadData(object data)
        {
            ClearData();
            LoadTextures((PssgElementViewModel)data);
        }
        public void LoadTextures(PssgElementViewModel elementView)
        {
            if (elementView.Element.Name == "TEXTURE" && elementView.Element.HasAttribute("id"))
            {
                _textures.Add(new PssgTextureViewModel(elementView));
            }

            foreach (PssgElementViewModel child in elementView.Children)
            {
                LoadTextures(child);
            }
        }

        public override void ClearData()
        {
            _textures.Clear();
        }

        public void RemoveTexture(PssgElementViewModel elementView)
        {
            int index = -1;
            for (int i = 0; i < _textures.Count; ++i)
            {
                if (object.ReferenceEquals(_textures[i].ElementView, elementView))
                {
                    index = i;
                    break;
                }
            }
            if (index >= 0)
            {
                _textures.RemoveAt(index);
            }

            foreach (PssgElementViewModel child in elementView.Children)
            {
                RemoveTexture(child);
            }
        }

        private bool TextureFilter(PssgTextureViewModel item)
        {
            if (String.IsNullOrEmpty(FilterText))
                return true;
            else
                return (item.DisplayName.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        #region Menu

        private bool Export_CanExecute(object parameter)
        {
            return parameter != null;
        }
        [RelayCommand(CanExecute = nameof(Export_CanExecute))]
        private async Task Export(object parameter)
        {
            PssgElement element = ((PssgTextureViewModel)parameter).Texture;
            FileSaveOptions saveOptions = new()
            {
                FileTypeChoices = [FilePickerType.Dds, FilePickerType.All],
                Title = "Select the dds save location and file name",
                FileName = element.Attributes["id"].DisplayValue + ".dds",
            };

            var result = await FileDialog.ShowSaveFileDialog(saveOptions);
            if (result is not null)
            {
                try
                {
                    DdsFile dds = element.ToDdsFile(false);
                    using (var fs = File.Open(result, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                        dds.Write(fs, -1);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not export texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool Import_CanExecute(object parameter)
        {
            return parameter != null;
        }
        [RelayCommand(CanExecute = nameof(Import_CanExecute))]
        private async Task Import(object parameter)
        {
            PssgTextureViewModel texView = (PssgTextureViewModel)parameter;
            PssgElement element = texView.Texture;
            FileOpenOptions openOptions = new()
            {
                FileTypeChoices = [FilePickerType.Dds, FilePickerType.All],
                Title = "Select a dds file",
                FileName = element.Attributes["id"].DisplayValue + ".dds",
            };

            var result = await FileDialog.ShowOpenFileDialog(openOptions);
            if (result.Count > 0)
            {
                try
                {
                    DdsFile dds = new DdsFile(File.Open(result[0], FileMode.Open));
                    dds.ToPssgElement(element);
                    texView.GetPreview();
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not import texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportTextures_CanExecute()
        {
            return Textures.Count > 0;
        }
        [RelayCommand(CanExecute = nameof(ExportTextures_CanExecute))]
        private async Task ExportTextures()
        {
            try
            {
                var texDir = mainView.FilePath + "_textures";
                Directory.CreateDirectory(texDir);
                DdsFile dds;
                foreach (PssgTextureViewModel texView in Textures)
                {
                    dds = texView.Texture.ToDdsFile(false);
                    string filePath = Path.Combine(texDir, texView.Texture.Attributes["id"].DisplayValue + ".dds");
                    using (var fs = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                        dds.Write(fs, -1);
                }
                await MessageBox.Show("Textures exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                await MessageBox.Show("There was an error, could not export all textures!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool ImportTextures_CanExecute()
        {
            return Textures.Count > 0;
        }
        [RelayCommand(CanExecute = nameof(ImportTextures_CanExecute))]
        private async Task ImportTextures()
        {
            try
            {
                string directory = mainView.FilePath + "_textures";
                if (Directory.Exists(directory) == true)
                {
                    DdsFile dds;
                    foreach (string filePath in Directory.GetFiles(directory, "*.dds"))
                    {
                        foreach (PssgTextureViewModel texView in Textures)
                        {
                            if (Path.GetFileNameWithoutExtension(filePath) == texView.Texture.Attributes["id"].ToString())
                            {
                                dds = new DdsFile(File.Open(filePath, FileMode.Open));
                                dds.ToPssgElement(texView.Texture);
                                if (texView.IsSelected)
                                {
                                    texView.GetPreview();
                                }
                                break;
                            }
                        }
                    }

                    await MessageBox.Show("Textures imported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await MessageBox.Show("Could not find textures folder!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                await MessageBox.Show("There was an error, could not import all textures!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool DuplicateTexture_CanExecute(object parameter)
        {
            return parameter != null;
        }
        [RelayCommand(CanExecute = nameof(DuplicateTexture_CanExecute))]
        private async Task DuplicateTexture(object parameter)
        {
            PssgTextureViewModel texView = (PssgTextureViewModel)parameter;
            var dtVm = new DuplicateTextureViewModel
            {
                TextureName = texView.DisplayName + "_2"
            };

            if (await PssgDialog.ShowDuplicateTextureDialog(dtVm))
            {
                // Copy and Edit Name
                PssgElement elementToCopy = texView.Texture;
                PssgElement newTexture = new PssgElement(elementToCopy);
                newTexture.Attributes["id"].Value = dtVm.TextureName;

                // Add to Library
                if (elementToCopy.ParentElement != null)
                {
                    elementToCopy.ParentElement.AppendChild(newTexture);
                    PssgElementViewModel newElementView = new PssgElementViewModel(newTexture, texView.ElementView.Parent);
                    texView.ElementView.Parent.Children.Add(newElementView);
                    Textures.Add(new PssgTextureViewModel(newElementView));
                }
                else
                {
                    elementToCopy.AppendChild(newTexture);
                    PssgElementViewModel newElementView = new PssgElementViewModel(newTexture, texView.ElementView);
                    texView.ElementView.Children.Add(newElementView);
                    Textures.Add(new PssgTextureViewModel(newElementView));
                }
            }
        }
        private bool RemoveTextureC_CanExecute(object parameter)
        {
            return parameter != null && ((PssgTextureViewModel)parameter).Texture != mainView.PssgFile.RootElement;
        }
        [RelayCommand(CanExecute = nameof(RemoveTextureC_CanExecute))]
        private void RemoveTextureC(object parameter)
        {
            PssgTextureViewModel texView = (PssgTextureViewModel)parameter;

            texView.Texture.ParentElement.RemoveChild(texView.Texture);
            texView.ElementView.Parent.Children.Remove(texView.ElementView);
            RemoveTexture(texView.ElementView);
        }
        #endregion
    }
}
