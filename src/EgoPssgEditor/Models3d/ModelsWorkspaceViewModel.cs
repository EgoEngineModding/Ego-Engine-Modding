using EgoEngineLibrary.Formats.Pssg;
using EgoEngineLibrary.Graphics;
using EgoPssgEditor.ViewModel;
using Microsoft.Win32;
using SharpGLTF.Schema2;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace EgoPssgEditor.Models3d
{
    public class ModelsWorkspaceViewModel : WorkspaceViewModel
    {
        private PssgFile _pssg;
        PssgNodeViewModel rootNode;
        readonly ObservableCollection<PssgNodeViewModel> pssgNodes;

        public override string DisplayName
        {
            get
            {
                return "Models";
            }
        }
        public PssgNodeViewModel RootNode
        {
            get { return rootNode; }
            private set
            {
                ClearData();
                rootNode = value;
                pssgNodes.Add(rootNode);
            }
        }
        public ObservableCollection<PssgNodeViewModel> PssgNodes
        {
            get { return pssgNodes; }
        }

        public ModelsWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            pssgNodes = new ObservableCollection<PssgNodeViewModel>();

            export = new RelayCommand(Export_Execute, Export_CanExecute);
            import = new RelayCommand(Import_Execute, Import_CanExecute);
            ImportGrid = new RelayCommand(ImportGrid_Execute, ImportGrid_CanExecute);
        }

        public override void LoadData(object data)
        {
            _pssg = (PssgFile)data;
        }

        public override void ClearData()
        {
            _pssg = null;
            rootNode = null;
            pssgNodes.Clear();
        }

        #region Menu
        readonly RelayCommand export;
        readonly RelayCommand import;

        public RelayCommand Export
        {
            get { return export; }
        }
        public RelayCommand Import
        {
            get { return import; }
        }
        public RelayCommand ImportGrid { get; }

        private bool Export_CanExecute(object parameter)
        {
            try
            {
                return _pssg != null && CarPssgGltfConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        private void Export_Execute(object parameter)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Gltf files|*.glb;*.gltf|All files|*.*";
            dialog.Title = "Select the model's save location and file name";
            dialog.DefaultExt = "glb";
            if (!string.IsNullOrEmpty(mainView.FilePath))
            {
                dialog.FileName = Path.GetFileNameWithoutExtension(mainView.FilePath);
                dialog.InitialDirectory = Path.GetDirectoryName(mainView.FilePath);
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    CarPssgGltfConverter converter = new CarPssgGltfConverter();
                    var model = converter.Convert(_pssg);
                    model.Save(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool Import_CanExecute(object parameter)
        {
            try
            {
                return _pssg != null && GltfDirt2F1CarPssgConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        private void Import_Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Gltf files|*.glb;*.gltf|All files|*.*";
            dialog.Title = "Select a gltf model file";
            if (!string.IsNullOrEmpty(mainView.FilePath))
            {
                dialog.FileName = Path.GetFileNameWithoutExtension(mainView.FilePath);
                dialog.InitialDirectory = Path.GetDirectoryName(mainView.FilePath);
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var gltf = ModelRoot.Load(dialog.FileName);

                    var conv = new GltfDirt2F1CarPssgConverter();
                    conv.Convert(gltf, _pssg);

                    mainView.LoadPssg(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ImportGrid_CanExecute(object parameter)
        {
            try
            {
                return _pssg != null && GltfGridCarPssgConverter.SupportsPssg(_pssg);
            }
            catch { return false; }
        }
        private void ImportGrid_Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Gltf files|*.glb;*.gltf|All files|*.*";
            dialog.Title = "Select a gltf model file";
            if (!string.IsNullOrEmpty(mainView.FilePath))
            {
                dialog.FileName = Path.GetFileNameWithoutExtension(mainView.FilePath);
                dialog.InitialDirectory = Path.GetDirectoryName(mainView.FilePath);
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var gltf = ModelRoot.Load(dialog.FileName);

                    var conv = new GltfGridCarPssgConverter();
                    conv.Convert(gltf, _pssg);

                    mainView.LoadPssg(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import the model!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion
    }
}
