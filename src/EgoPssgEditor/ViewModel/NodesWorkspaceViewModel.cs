using EgoEngineLibrary.Graphics;
using Microsoft.Win32;
using MiscUtil.Conversion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace EgoPssgEditor.ViewModel
{
    public class NodesWorkspaceViewModel : WorkspaceViewModel
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

            export = new RelayCommand(Export_Execute, Export_CanExecute);
            import = new RelayCommand(Import_Execute, Import_CanExecute);
            exportData = new RelayCommand(ExportData_Execute, ExportData_CanExecute);
            importData = new RelayCommand(ImportData_Execute, ImportData_CanExecute);

            addNode = new RelayCommand(AddNode_Execute, AddNode_CanExecute);
            removeNode = new RelayCommand(RemoveNode_Execute, RemoveNode_CanExecute);
            CloneNode = new RelayCommand(CloneNode_Execute, CloneNode_CanExecute);

            addAttribute = new RelayCommand(AddAttribute_Execute, AddAttribute_CanExecute);
            removeAttribute = new RelayCommand(RemoveAttribute_Execute, RemoveAttribute_CanExecute);
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
        readonly RelayCommand export;
        readonly RelayCommand import;
        readonly RelayCommand exportData;
        readonly RelayCommand importData;

        readonly RelayCommand addNode;
        readonly RelayCommand removeNode;
        readonly RelayCommand addAttribute;
        readonly RelayCommand removeAttribute;

        public RelayCommand Export
        {
            get { return export; }
        }
        public RelayCommand Import
        {
            get { return import; }
        }
        public RelayCommand ExportData
        {
            get { return exportData; }
        }
        public RelayCommand ImportData
        {
            get { return importData; }
        }

        public RelayCommand AddNode
        {
            get { return addNode; }
        }
        public RelayCommand RemoveNode
        {
            get { return removeNode; }
        }
        public RelayCommand CloneNode { get; }

        public RelayCommand AddAttribute
        {
            get { return addAttribute; }
        }
        public RelayCommand RemoveAttribute
        {
            get { return removeAttribute; }
        }

        private bool Export_CanExecute(object parameter)
        {
            return parameter != null;
        }
        private void Export_Execute(object parameter)
        {
            PssgNode node = ((PssgNodeViewModel)parameter).Node;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Xml files|*.xml|All files|*.*";
            dialog.Title = "Select the node's save location and file name";
            dialog.DefaultExt = "xml";
            dialog.FileName = "node.xml";
            if (dialog.ShowDialog() == true)
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

                    using (XmlWriter writer = XmlWriter.Create(File.Open(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read), settings))
                    {
                        xDoc.Save(writer);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export the node!" + Environment.NewLine + Environment.NewLine +
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
            PssgNodeViewModel nodeView = (PssgNodeViewModel)parameter;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Xml files|*.xml|All files|*.*";
            dialog.Title = "Select a xml file";
            dialog.FileName = "node.xml";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    PssgNode node = nodeView.Node;
                    using (FileStream fileStream = File.Open(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
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
                    MessageBox.Show("Could not import the node!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ExportData_CanExecute(object parameter)
        {
            if (parameter == null) return false;

            return ((PssgNodeViewModel)parameter).IsDataNode;
        }
        private void ExportData_Execute(object parameter)
        {
            PssgNodeViewModel nodeView = (PssgNodeViewModel)parameter;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Bin files|*.bin|All files|*.*";
            dialog.Title = "Select the byte data save location and file name";
            dialog.DefaultExt = "bin";
            dialog.FileName = "nodeData.bin";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    PssgNode node = nodeView.Node;
                    using (PssgBinaryWriter writer = new PssgBinaryWriter(new BigEndianBitConverter(), File.Open(dialog.FileName, FileMode.Create)))
                    {
                        writer.WriteObject(node.Value);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export data!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool ImportData_CanExecute(object parameter)
        {
            if (parameter == null) return false;

            return ((PssgNodeViewModel)parameter).IsDataNode;
        }
        private void ImportData_Execute(object parameter)
        {
            PssgNodeViewModel nodeView = (PssgNodeViewModel)parameter;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Bin files|*.bin|All files|*.*";
            dialog.Title = "Select a bin file";
            dialog.FileName = "nodeData.bin";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    PssgNode node = nodeView.Node;
                    using (PssgBinaryReader reader = new PssgBinaryReader(new BigEndianBitConverter(), File.Open(dialog.FileName, FileMode.Open, FileAccess.Read)))
                    {
                        node.Value = reader.ReadNodeValue((int)reader.BaseStream.Length);
                        nodeView.IsSelected = false;
                        nodeView.IsSelected = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import data!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool AddNode_CanExecute(object parameter)
        {
            return parameter != null;
        }
        private void AddNode_Execute(object parameter)
        {
            PssgNodeViewModel nodeView = (PssgNodeViewModel)parameter;
            if (nodeView.Node.IsDataNode)
            {
                MessageBox.Show("Adding sub nodes to a node with data is not allowed!", Properties.Resources.AppTitleShort, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            AddNodeWindow aaw = new AddNodeWindow();
            if (aaw.ShowDialog() == true)
            {
                PssgNode newNode = nodeView.Node.AppendChild(aaw.NodeName);

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
        private void RemoveNode_Execute(object parameter)
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
        private void CloneNode_Execute(object parameter)
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
                MessageBox.Show("Could not clone node!" + Environment.NewLine + Environment.NewLine +
                    ex.Message, Properties.Resources.AppTitleLong, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool AddAttribute_CanExecute(object parameter)
        {
            return parameter != null;
        }
        private void AddAttribute_Execute(object parameter)
        {
            PssgNodeViewModel nodeView = (PssgNodeViewModel)parameter;
            AddAttributeWindow aaw = new AddAttributeWindow();

            if (aaw.ShowDialog() == true)
            {
                PssgAttribute attr = nodeView.Node.AddAttribute(aaw.AttributeName, Convert.ChangeType(aaw.Value, aaw.AttributeValueType));
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
        private void RemoveAttribute_Execute(object parameter)
        {
            PssgAttributeViewModel attrView = (PssgAttributeViewModel)parameter;

            attrView.Attribute.ParentNode.RemoveAttribute(attrView.Attribute.Name);
            attrView.Parent.IsSelected = false;
            attrView.Parent.IsSelected = true;
        }
        #endregion
    }
}
