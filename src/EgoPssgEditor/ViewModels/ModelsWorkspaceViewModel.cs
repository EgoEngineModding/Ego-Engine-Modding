using CommunityToolkit.Mvvm.Input;
using EgoEngineLibrary.Formats.Pssg;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;
using EgoEngineLibrary.Graphics.Pssg;
using SharpGLTF.Schema2;

namespace EgoPssgEditor.ViewModels
{
    public partial class ModelsWorkspaceViewModel : WorkspaceViewModel
    {
        private PssgFile? _pssg;

        public override string DisplayName
        {
            get
            {
                return "Models";
            }
        }

        public override void LoadData()
        {
            _pssg = MainView.PssgFile;
        }

        public override void ClearData()
        {
            _pssg = null;
        }

        #region Menu

        private bool Export_CanExecute()
        {
            try
            {
                return _pssg != null && CarExteriorPssgGltfConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        [RelayCommand(CanExecute = nameof(Export_CanExecute))]
        private async Task ExportCar()
        {
            FileSaveOptions saveOptions = new()
            {
                FileTypeChoices = [FilePickerType.Gltf, FilePickerType.All],
                Title = "Select the model's save location and file name",
                DefaultExtension = "glb",
            };
            if (!string.IsNullOrEmpty(MainView.FilePath))
            {
                saveOptions.FileName = Path.GetFileNameWithoutExtension(MainView.FilePath);
                saveOptions.InitialDirectory = Path.GetDirectoryName(MainView.FilePath);
            }

            var result = await FileDialog.ShowSaveFileDialog(saveOptions);
            if (result is not null)
            {
                try
                {
                    var converter = new CarExteriorPssgGltfConverter();
                    var model = converter.Convert(_pssg!);
                    model.Save(result);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not export the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool Import_CanExecute()
        {
            try
            {
                return _pssg != null && GltfCarExteriorPssgConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        [RelayCommand(CanExecute = nameof(Import_CanExecute))]
        private async Task ImportCar()
        {
            FileOpenOptions openOptions = new()
            {
                FileTypeChoices = [FilePickerType.Gltf, FilePickerType.All],
                Title = "Select a gltf model file",
            };
            if (!string.IsNullOrEmpty(MainView.FilePath))
            {
                openOptions.FileName = Path.GetFileNameWithoutExtension(MainView.FilePath);
                openOptions.InitialDirectory = Path.GetDirectoryName(MainView.FilePath);
            }

            var result = await FileDialog.ShowOpenFileDialog(openOptions);
            if (result.Count > 0)
            {
                try
                {
                    var gltf = ModelRoot.Load(result[0]);

                    var conv = new GltfCarExteriorPssgConverter();
                    conv.Convert(gltf, _pssg!);

                    MainView.LoadPssg(null);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not import the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportDirt_CanExecute()
        {
            try
            {
                return _pssg != null && DirtCarExteriorPssgGltfConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        [RelayCommand(CanExecute = nameof(ExportDirt_CanExecute))]
        private async Task ExportDirt()
        {
            FileSaveOptions saveOptions = new()
            {
                FileTypeChoices = [FilePickerType.Gltf, FilePickerType.All],
                Title = "Select the model's save location and file name",
                DefaultExtension = "glb",
            };
            if (!string.IsNullOrEmpty(MainView.FilePath))
            {
                saveOptions.FileName = Path.GetFileNameWithoutExtension(MainView.FilePath);
                saveOptions.InitialDirectory = Path.GetDirectoryName(MainView.FilePath);
            }

            var result = await FileDialog.ShowSaveFileDialog(saveOptions);
            if (result is not null)
            {
                try
                {
                    var converter = new DirtCarExteriorPssgGltfConverter();
                    var model = converter.Convert(_pssg!);
                    model.Save(result);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not export the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ImportDirt_CanExecute()
        {
            try
            {
                return _pssg != null && GltfDirtCarExteriorPssgConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        [RelayCommand(CanExecute = nameof(ImportDirt_CanExecute))]
        private async Task ImportDirt()
        {
            FileOpenOptions openOptions = new()
            {
                FileTypeChoices = [FilePickerType.Gltf, FilePickerType.All],
                Title = "Select a gltf model file",
            };
            if (!string.IsNullOrEmpty(MainView.FilePath))
            {
                openOptions.FileName = Path.GetFileNameWithoutExtension(MainView.FilePath);
                openOptions.InitialDirectory = Path.GetDirectoryName(MainView.FilePath);
            }

            var result = await FileDialog.ShowOpenFileDialog(openOptions);
            if (result.Count > 0)
            {
                try
                {
                    var gltf = ModelRoot.Load(result[0]);

                    var conv = new GltfDirtCarExteriorPssgConverter();
                    conv.Convert(gltf, _pssg!);

                    MainView.LoadPssg(null);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not import the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ImportGrid_CanExecute()
        {
            try
            {
                return _pssg != null && GltfGridCarExteriorPssgConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        [RelayCommand(CanExecute = nameof(ImportGrid_CanExecute))]
        private async Task ImportGrid()
        {
            FileOpenOptions openOptions = new()
            {
                FileTypeChoices = [FilePickerType.Gltf, FilePickerType.All],
                Title = "Select a gltf model file",
            };
            if (!string.IsNullOrEmpty(MainView.FilePath))
            {
                openOptions.FileName = Path.GetFileNameWithoutExtension(MainView.FilePath);
                openOptions.InitialDirectory = Path.GetDirectoryName(MainView.FilePath);
            }

            var result = await FileDialog.ShowOpenFileDialog(openOptions);
            if (result.Count > 0)
            {
                try
                {
                    var gltf = ModelRoot.Load(result[0]);

                    var conv = new GltfGridCarExteriorPssgConverter();
                    conv.Convert(gltf, _pssg!);

                    MainView.LoadPssg(null);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not import the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportCarInterior_CanExecute()
        {
            try
            {
                return _pssg != null && CarInteriorPssgGltfConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        [RelayCommand(CanExecute = nameof(ExportCarInterior_CanExecute))]
        private async Task ExportCarInterior()
        {
            FileSaveOptions saveOptions = new()
            {
                FileTypeChoices = [FilePickerType.Gltf, FilePickerType.All],
                Title = "Select the model's save location and file name",
                DefaultExtension = "glb",
            };
            if (!string.IsNullOrEmpty(MainView.FilePath))
            {
                saveOptions.FileName = Path.GetFileNameWithoutExtension(MainView.FilePath);
                saveOptions.InitialDirectory = Path.GetDirectoryName(MainView.FilePath);
            }

            var result = await FileDialog.ShowSaveFileDialog(saveOptions);
            if (result is not null)
            {
                try
                {
                    var converter = new CarInteriorPssgGltfConverter();
                    var model = converter.Convert(_pssg!);
                    model.Save(result);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not export the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private bool ImportCarInterior_CanExecute()
        {
            try
            {
                return _pssg != null && GltfCarInteriorPssgConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        [RelayCommand(CanExecute = nameof(ImportCarInterior_CanExecute))]
        private async Task ImportCarInterior()
        {
            FileOpenOptions openOptions = new()
            {
                FileTypeChoices = [FilePickerType.Gltf, FilePickerType.All],
                Title = "Select a gltf model file",
            };
            if (!string.IsNullOrEmpty(MainView.FilePath))
            {
                openOptions.FileName = Path.GetFileNameWithoutExtension(MainView.FilePath);
                openOptions.InitialDirectory = Path.GetDirectoryName(MainView.FilePath);
            }

            var result = await FileDialog.ShowOpenFileDialog(openOptions);
            if (result.Count > 0)
            {
                try
                {
                    var gltf = ModelRoot.Load(result[0]);

                    var conv = new GltfCarInteriorPssgConverter();
                    conv.Convert(gltf, _pssg!);

                    MainView.LoadPssg(null);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not import the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion
    }
}
