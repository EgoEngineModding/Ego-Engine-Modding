using EgoEngineLibrary.Graphics;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace EgoPssgEditor.ViewModel
{
    public class TexturesWorkspaceViewModel : WorkspaceViewModel
    {
        #region Data
        readonly ObservableCollection<PssgTextureViewModel> textures;

        public override string DisplayName
        {
            get { return "Textures"; }
        }

        public ObservableCollection<PssgTextureViewModel> Textures
        {
            get { return textures; }
        }
        #endregion

        #region Presentation Data
        CollectionView texturesViewSource;
        string filterText;

        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;
                texturesViewSource.Refresh();
            }
        }
        #endregion

        public TexturesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            textures = new ObservableCollection<PssgTextureViewModel>();
            texturesViewSource = (CollectionView)CollectionViewSource.GetDefaultView(Textures);
            texturesViewSource.Filter += TextureFilter;

            export = new RelayCommand(Export_Execute, Export_CanExecute);
            import = new RelayCommand(Import_Execute, Import_CanExecute);
            exportTextures = new RelayCommand(ExportTextures_Execute, ExportTextures_CanExecute);
            importTextures = new RelayCommand(ImportTextures_Execute, ImportTextures_CanExecute);
        }

        public override void LoadData(PssgFile file)
        {
            ClearData();
            foreach (PssgNode texture in file.RootNode.FindNodes("TEXTURE", "id"))
            {
                textures.Add(new PssgTextureViewModel(texture));
            }
        }

        public override void ClearData()
        {
            textures.Clear();
        }

        public void SetTexture(PssgNode node, PssgNode newNode)
        {
            int index = -1;
            for (int i = 0; i < textures.Count; ++i)
            {
                if (textures[i].Texture == node)
                {
                    index = i;
                    break;
                }
            }
            if (index >= 0)
            {
                textures.RemoveAt(index);
            }

            foreach (PssgNode texture in newNode.FindNodes("TEXTURE", "id"))
            {
                textures.Add(new PssgTextureViewModel(texture));
            }
        }

        private bool TextureFilter(object item)
        {
            if (String.IsNullOrEmpty(FilterText))
                return true;
            else
                return ((item as PssgTextureViewModel).DisplayName.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        #region Menu
        readonly RelayCommand export;
        readonly RelayCommand import;
        readonly RelayCommand exportTextures;
        readonly RelayCommand importTextures;

        public RelayCommand Export
        {
            get { return export; }
        }
        public RelayCommand Import
        {
            get { return import; }
        }
        public RelayCommand ExportTextures
        {
            get { return exportTextures; }
        }
        public RelayCommand ImportTextures
        {
            get { return importTextures; }
        }

        private bool Export_CanExecute(object parameter)
        {
            return parameter != null;
        }
        private void Export_Execute(object parameter)
        {
            PssgNode node = ((PssgTextureViewModel)parameter).Texture;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Dds files|*.dds|All files|*.*";
            dialog.Title = "Select the dds save location and file name";
            dialog.FileName = node.Attributes["id"].DisplayValue + ".dds";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    DdsFile dds = new DdsFile(node, false);
                    dds.Write(File.Open(dialog.FileName, FileMode.Create), -1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool Import_CanExecute(object parameter)
        {
            return parameter != null;
        }
        private void Import_Execute(object parameter)
        {
            PssgTextureViewModel texView = (PssgTextureViewModel)parameter;
            PssgNode node = texView.Texture;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Dds files|*.dds|All files|*.*";
            dialog.Title = "Select a dds file";
            dialog.FileName = node.Attributes["id"].DisplayValue + ".dds";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    DdsFile dds = new DdsFile(File.Open(dialog.FileName, FileMode.Open));
                    dds.Write(node);
                    texView.GetPreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportTextures_CanExecute(object parameter)
        {
            return Textures.Count > 0;
        }
        private void ExportTextures_Execute(object parameter)
        {
            try
            {
                Directory.CreateDirectory(mainView.FilePath.Replace(".", "_") + "_textures");
                DdsFile dds;
                foreach (PssgTextureViewModel texView in Textures)
                {
                    dds = new DdsFile(texView.Texture, false);
                    dds.Write(File.Open(mainView.FilePath.Replace(".", "_") + "_textures" + "\\" + texView.Texture.GetAttribute("id").DisplayValue + ".dds", FileMode.Create), -1);
                }
                MessageBox.Show("Textures exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("There was an error, could not export all textures!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool ImportTextures_CanExecute(object parameter)
        {
            return Textures.Count > 0;
        }
        private void ImportTextures_Execute(object parameter)
        {
            try
            {
                string directory = mainView.FilePath.Replace(".", "_") + "_textures";
                if (Directory.Exists(directory) == true)
                {
                    DdsFile dds;
                    foreach (string filePath in Directory.GetFiles(directory, "*.dds"))
                    {
                        foreach (PssgTextureViewModel texView in Textures)
                        {
                            if (Path.GetFileNameWithoutExtension(filePath) == texView.Texture.GetAttribute("id").ToString())
                            {
                                dds = new DdsFile(File.Open(filePath, FileMode.Open));
                                dds.Write(texView.Texture);
                                if (texView.IsSelected)
                                {
                                    texView.GetPreview();
                                }
                                break;
                            }
                        }
                    }

                    MessageBox.Show("Textures imported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Could not find textures folder!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("There was an error, could not import all textures!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
