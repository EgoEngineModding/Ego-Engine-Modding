using EgoEngineLibrary.Graphics;

using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using CommunityToolkit.Mvvm.Input;

using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;
using EgoEngineLibrary.Graphics.Pssg;
using EgoPssgEditor.Dialogs.Pssg;

namespace EgoPssgEditor.ViewModels
{
    public partial class NodesWorkspaceViewModel : WorkspaceViewModel
    {
        #region Data
        PssgNodeViewModel rootNode;
        readonly ObservableCollection<PssgNodeViewModel> pssgNodes;

        public override string DisplayName
        {
            get
            {
                return "All Nodes";
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
        #endregion

        #region Presentation Props
        #endregion

        public NodesWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {
            pssgNodes = new ObservableCollection<PssgNodeViewModel>();
        }

        public override void LoadData(object data)
        {
            RootNode = new PssgNodeViewModel(((PssgFile)data).RootNode);
            RootNode.IsExpanded = true;
        }

        public override void ClearData()
        {
            rootNode = null;
            pssgNodes.Clear();
        }

        #region Menu

        private bool Export_CanExecute(object parameter)
        {
            return parameter != null;
        }
        [RelayCommand(CanExecute = nameof(Export_CanExecute))]
        private async Task Export(object parameter)
        {
            PssgNode node = ((PssgNodeViewModel)parameter).Node;
            FileSaveOptions saveOptions = new()
            {
                FileTypeChoices = [FilePickerType.Xml, FilePickerType.All],
                Title = "Select the node's save location and file name",
                DefaultExtension = "xml",
                FileName = "node.xml",
            };

            var result = await FileDialog.ShowSaveFileDialog(saveOptions);
            if (result is not null)
            {
                try
                {
                    XDocument xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
                    xDoc.Add(new XElement("PSSGFILE", new XAttribute("version", "1.0.0.0")));
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Encoding = new UTF8Encoding(false);
                    settings.NewLineChars = "\n";
                    settings.Indent = true;
                    settings.IndentChars = "";
                    settings.CloseOutput = true;

                    XElement pssg = (XElement)xDoc.FirstNode;
                    node.WriteXml(pssg);

                    using (XmlWriter writer = XmlWriter.Create(File.Open(result, FileMode.Create, FileAccess.Write, FileShare.Read), settings))
                    {
                        xDoc.Save(writer);
                    }
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not export the node!" + Environment.NewLine + Environment.NewLine +
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
            PssgNodeViewModel nodeView = (PssgNodeViewModel)parameter;
            FileOpenOptions openOptions = new()
            {
                FileTypeChoices = [FilePickerType.Xml, FilePickerType.All],
                Title = "Select a xml file",
                FileName = "node.xml",
            };

            var result = await FileDialog.ShowOpenFileDialog(openOptions);
            if (result.Count > 0)
            {
                try
                {
                    PssgNode node = nodeView.Node;
                    using (FileStream fileStream = File.Open(result[0], FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        XDocument xDoc = XDocument.Load(fileStream);

                        PssgNode newNode = new PssgNode((XElement)((XElement)xDoc.FirstNode).FirstNode, node.File, node.ParentNode);
                        if (node.ParentNode != null)
                        {
                            node = node.ParentNode.SetChild(node, newNode);
                            int index = nodeView.Parent.Children.IndexOf(nodeView);
                            PssgNodeViewModel newNodeView = new PssgNodeViewModel(node, nodeView.Parent);
                            nodeView.Parent.Children[index] = newNodeView;
                            newNodeView.IsSelected = true;
                        }
                        else
                        {
                            node.File.RootNode = newNode;
                            LoadData(node.File);
                            rootNode.IsSelected = true;
                        }

                        mainView.TexturesWorkspace.ClearData();
                        mainView.TexturesWorkspace.LoadData(RootNode);
                    }
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not import the node!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportData_CanExecute(object parameter)
        {
            if (parameter == null) return false;

            return ((PssgNodeViewModel)parameter).IsDataNode;
        }
        [RelayCommand(CanExecute = nameof(ExportData_CanExecute))]
        private async Task ExportData(object parameter)
        {
            PssgNodeViewModel nodeView = (PssgNodeViewModel)parameter;
            FileSaveOptions saveOptions = new()
            {
                FileTypeChoices = [FilePickerType.Bin, FilePickerType.All],
                Title = "Select the byte data save location and file name",
                DefaultExtension = "bin",
                FileName = "nodeData.bin",
            };

            var result = await FileDialog.ShowSaveFileDialog(saveOptions);
            if (result is not null)
            {
                try
                {
                    PssgNode node = nodeView.Node;
                    using (var fs = File.Open(result, FileMode.Create))
                    using (PssgBinaryWriter writer = new PssgBinaryWriter(EndianBitConverter.Big, fs, false))
                    {
                        writer.WriteObject(node.Value);
                    }
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not export data!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool ImportData_CanExecute(object parameter)
        {
            if (parameter == null) return false;

            return ((PssgNodeViewModel)parameter).IsDataNode;
        }
        [RelayCommand(CanExecute = nameof(ImportData_CanExecute))]
        private async Task ImportData(object parameter)
        {
            PssgNodeViewModel nodeView = (PssgNodeViewModel)parameter;
            FileOpenOptions openOptions = new()
            {
                FileTypeChoices = [FilePickerType.Bin, FilePickerType.All],
                Title = "Select a bin file",
                FileName = "nodeData.bin",
            };

            var result = await FileDialog.ShowOpenFileDialog(openOptions);
            if (result.Count > 0)
            {
                try
                {
                    PssgNode node = nodeView.Node;
                    using (var fs = File.Open(result[0], FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (PssgBinaryReader reader = new PssgBinaryReader(EndianBitConverter.Big, fs, false))
                    {
                        node.Value = reader.ReadNodeValue((int)reader.BaseStream.Length);
                        nodeView.IsSelected = false;
                        nodeView.IsSelected = true;
                    }
                }
                catch (Exception ex)
                {
                    await MessageBox.Show("Could not import data!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool AddNode_CanExecute(object parameter)
        {
            return parameter != null;
        }
        [RelayCommand(CanExecute = nameof(AddNode_CanExecute))]
        private async Task AddNode(object parameter)
        {
            PssgNodeViewModel nodeView = (PssgNodeViewModel)parameter;
            if (nodeView.Node.IsDataNode)
            {
                await MessageBox.Show("Adding sub nodes to a node with data is not allowed!", Properties.Resources.AppTitleShort, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            var nodeName = await PssgDialog.ShowAddNodeDialog();
            if (nodeName is not null)
            {
                PssgNode newNode = nodeView.Node.AppendChild(nodeName);

                if (newNode == null)
                {
                    return;
                }

                PssgNodeViewModel newNodeView = new PssgNodeViewModel(newNode, nodeView);
                nodeView.Children.Add(newNodeView);
            }
        }
        private bool RemoveNode_CanExecute(object parameter)
        {
            return parameter != null && parameter != RootNode;
        }
        [RelayCommand(CanExecute = nameof(RemoveNode_CanExecute))]
        private void RemoveNode(object parameter)
        {
            PssgNodeViewModel nodeView = (PssgNodeViewModel)parameter;
            nodeView.Node.ParentNode.RemoveChild(nodeView.Node);

            nodeView.Parent.Children.Remove(nodeView);
            mainView.TexturesWorkspace.RemoveTexture(nodeView);
        }
        private bool CloneNode_CanExecute(object parameter)
        {
            return parameter != null && parameter != RootNode;
        }
        [RelayCommand(CanExecute = nameof(CloneNode_CanExecute))]
        private async Task CloneNode(object parameter)
        {
            try
            {
                PssgNodeViewModel nodeView = (PssgNodeViewModel)parameter;
                var newNode = nodeView.Node.File.CloneNode(nodeView.Node);

                PssgNodeViewModel newNodeView = new PssgNodeViewModel(newNode, nodeView.Parent);
                var nodeViewIndex = nodeView.Parent.Children.IndexOf(nodeView);
                if (nodeViewIndex < 0 || nodeViewIndex >= nodeView.Parent.Children.Count)
                    nodeView.Parent.Children.Add(newNodeView);
                else
                    nodeView.Parent.Children.Insert(nodeViewIndex + 1, newNodeView);

                mainView.TexturesWorkspace.LoadTextures(newNodeView);
                newNodeView.IsSelected = true;
            }
            catch (Exception ex)
            {
                await MessageBox.Show("Could not clone node!" + Environment.NewLine + Environment.NewLine +
                    ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool AddAttribute_CanExecute(object parameter)
        {
            return parameter != null;
        }
        [RelayCommand(CanExecute = nameof(AddAttribute_CanExecute))]
        private async Task AddAttribute(object parameter)
        {
            PssgNodeViewModel nodeView = (PssgNodeViewModel)parameter;
            var res = await PssgDialog.ShowAddAttributeDialog();
            if (res is not null)
            {
                PssgAttribute attr = nodeView.Node.AddAttribute(res.Name, Convert.ChangeType(res.Value, res.Type));
                if (attr == null)
                {
                    return;
                }

                nodeView.IsSelected = false;
                nodeView.IsSelected = true;
            }
        }
        private bool RemoveAttribute_CanExecute(object parameter)
        {
            return parameter != null;
        }
        [RelayCommand(CanExecute = nameof(RemoveAttribute_CanExecute))]
        private void RemoveAttribute(object parameter)
        {
            PssgAttributeViewModel attrView = (PssgAttributeViewModel)parameter;

            attrView.Attribute.ParentNode.RemoveAttribute(attrView.Attribute.Name);
            attrView.Parent.IsSelected = false;
            attrView.Parent.IsSelected = true;
        }
        #endregion
    }
}
