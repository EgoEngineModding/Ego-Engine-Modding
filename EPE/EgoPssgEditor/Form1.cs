using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using FreeImageAPI;
using System.Drawing;
using System.Linq;
using MiscUtil.Conversion;
using EgoEngineLibrary.Graphics;
using System.Xml.Linq;
using System.Xml;
using System.Text;
using System.IO.Compression;
using BrightIdeasSoftware;

namespace EgoPssgEditor
{
    public partial class Form1 : Form
    {
        // Copy/Paste; Search; ModelViewer;
        // 1.7.2 - Quick Float4 Fix To Make work with all textures
        // 1.8 - Brand New Base PSSG Class from Miek, Export/Import CubeMaps, Remove Textures, Improved UI, Import All Textures, Better Image Preview, Brand New DDS Class
        // 2.0 - Support for texelformats "ui8x4" and "u8", Auto-Update "All Sections", Export/Import Data Nodes, Add/Remove Attributes to Nodes, Hogs Less Resources, PSSG Backend, textures search bar
        // 2.0.1 - Fixed Tag Errors on Save, Improved Open/SaveFileDialog Usability, Cleaned Up Some Code, Reduced Size from Icon
        // 10.0 -- Use EgoEngineLibrary, Added Dirt support, MainMenus, Export/Import Xml, Schema System, Move Nodes, Supports Compressed Pssg
        // 10.1 -- Added two texelTypes for Dirt Rally, Fixed bug when copying node (a new node info was created even if the same name already existed)
        // 10.2 -- Changed DDS saving to use linear size instead of pitch to make it work with Gimp 2.8
        // 10.3 -- Properly closes the save stream, Changed byte data font to Consolas, Hex byte now always 2 chars and caps, 64bit, opens/saves really large files, OLV textures
        string schemaPath = Application.StartupPath + "\\schema.xml";
        PssgFile pssg;
        string filePath = @"C:\";
        string[] args;

        public Form1(string[] args)
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.Text = Properties.Resources.AppTitleLong;
            texturePictureBox.BackColor = ColorTranslator.FromHtml("#BFBFBF");
            cubeMapPictureBox.BackColor = ColorTranslator.FromHtml("#BFBFBF");
            // DataGridSetup
            dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView.CellEndEdit += new DataGridViewCellEventHandler(dataGridView_CellValueChanged);
            // MainTabControl
            mainTabControl.SelectedIndexChanged += new EventHandler(mainTabControl_SelectedIndexChanged);
            mainTabControl.Selecting += new TabControlCancelEventHandler(mainTabControl_Selecting);
            textureImageLabel.Text = "";
            cubeMapImageLabel.Text = "";
            // MainMenu
            nodesToolStripMenuItem.DropDown.Opening += nodesToolStripMenuItemDropDown_Opening;
            termpToolStripMenuItem.Visible = false;
            // TreeView
            treeView.ItemDrag += treeView_ItemDrag;
            treeView.DragEnter += treeView_DragEnter;
            treeView.DragDrop += treeView_DragDrop;
            treeView.AllowDrop = true;
            //textureOLV
            textureObjectListView.UseFiltering = true;
            textureObjectListView.ShowGroups = false;
            OLVColumn nameCol = new OLVColumn("Name", "");
            nameCol.UseFiltering = true;
            nameCol.FillsFreeSpace = true;
            nameCol.AspectGetter = delegate(object x)
            {
                return ((PssgNode)x).GetAttribute("id").Value;
            };
            textureObjectListView.Columns.Add(nameCol);

            this.args = args;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PssgSchema.LoadSchema(File.Open(schemaPath, FileMode.Open, FileAccess.Read, FileShare.Read));
            // File Association Handler (if arg passed, try to open it)
            //args = new List<string>() { @"C:\Games\Steam\steamapps\common\f1 2011\cars\fe1\livery_main\textures_high\temo.pssg" }.ToArray();
            if (args.Length > 0)
            {
                filePath = args[0];
                clearVars(true);
                try
                {
                    pssg = PssgFile.Open(File.Open(filePath, FileMode.Open, FileAccess.Read));
                    setupEditor(MainTabs.Auto);
                }
                catch (Exception excp)
                {
                    // Fail
                    this.Text = Properties.Resources.AppTitleLong;
                    MessageBox.Show("The program could not open this file!" + Environment.NewLine + Environment.NewLine + excp.Message, "Could Not Open", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            args = null;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Silently delete the temp texture preview dds file
            try
            {
                File.Delete(Application.StartupPath + "\\temp.dds");
            }
            catch { }
        }

        #region MainMenu
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clearVars(true);
            pssg = new PssgFile(PssgFileType.Pssg);
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.FilterIndex = 1;
            if (!string.IsNullOrEmpty(filePath))
            {
                openFileDialog.FileName = Path.GetFileNameWithoutExtension(filePath);
                openFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
            }

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    filePath = openFileDialog.FileName;
                    openFileDialog.Dispose();
                    clearVars(true);
                    pssg = PssgFile.Open(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                    setupEditor(MainTabs.Auto);
                }
                catch (Exception excp)
                {
                    // Fail
                    this.Text = Properties.Resources.AppTitleLong;
                    MessageBox.Show("The program could not open this file!" + Environment.NewLine + Environment.NewLine + excp.Message, "Could Not Open", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SavePssg(0);
        }
        private void saveAsPssgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SavePssg(1);
        }
        private void saveAsCompressedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SavePssg(2);
        }
        private void saveAsXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SavePssg(3);
        }
        private void SavePssg(int type)
        {
            if (pssg == null)
            {
                return;
            }
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

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                tag();
                saveFileDialog.Dispose();
                try
                {
                    FileStream fileStream = File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                    if (type == 0)
                    {
                        pssg.Save(fileStream); // Auto
                    }
                    else if (type == 1)
                    {
                        pssg.FileType = PssgFileType.Pssg;
                        pssg.Save(fileStream); // Pssg
                    }
                    else if (type == 2)
                    {
                        pssg.FileType = PssgFileType.CompressedPssg;
                        pssg.Save(fileStream);
                    }
                    else
                    {
                        pssg.FileType = PssgFileType.Xml;
                        pssg.Save(fileStream);
                    }
                    filePath = saveFileDialog.FileName;
                    this.Text = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The program could not save this file! The error is displayed below:" + Environment.NewLine + Environment.NewLine + ex.Message, "Could Not Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void loadSchemaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PssgSchema.LoadSchema(File.Open(schemaPath, FileMode.Open, FileAccess.Read, FileShare.Read));
        }
        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PssgSchema.SaveSchema(File.Open(schemaPath, FileMode.Create, FileAccess.Write, FileShare.Read));
        }
        private void clearSchemaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PssgSchema.ClearSchema();
        }

        private void websiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.petartasev.com/modding/ego-engine/pssg-editor/");
        }

        private void tag()
        {
            PssgNode node;
            if (pssg.RootNode == null)
            {
                node = new PssgNode("PSSGDATABASE", pssg, null);
                pssg.RootNode = node;
                setupEditor(MainTabs.All);
            }
            else
            {
                node = pssg.RootNode;
            }

            PssgAttribute attribute = node.AddAttribute("creatorApplication", Properties.Resources.AppTitleLong);
        }
        private void clearVars(bool clearPSSG)
        {
            if (pssg == null) return;

            // All tab
            mainTabControl.SelectedTab = mainTabControl.TabPages["allTabPage"];
            treeView.Nodes.Clear();
            idTextBox.Text = "";
            richTextBox1.Text = "";
            dataGridView.Tag = null;
            dataGridView.Rows.Clear();
            dataGridView.Columns.Clear();
            dataGridView.BringToFront();

            // Textures tab
            textureImageLabel.Text = "";
            textureObjectListView.ClearObjects();
            if (texturePictureBox.Image != null)
            {
                texturePictureBox.Image.Dispose();
                texturePictureBox.Image = null;
            }

            // CubeMap Tab
            cubeMapImageLabel.Text = "";
            cubeMapTreeView.Nodes.Clear();
            if (cubeMapPictureBox.Image != null)
            {
                cubeMapPictureBox.Image.Dispose();
                cubeMapPictureBox.Image = null;
            }

            // BackEnd Tab
            mainTabControl.BringToFront();

            this.Text = Properties.Resources.AppTitleLong;
            if (clearPSSG == true)
            {
                pssg = null;
            }
        }
        private void setupEditor(MainTabs tabToSelect)
        {
            dataGridView.Columns.Add("valueColumn", "Value");
            dataGridView.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            if (pssg.RootNode != null)
            {
                treeView.Nodes.Add(pssg.CreateTreeViewNode(pssg.RootNode));
                textureObjectListView.SetObjects(pssg.FindNodes("TEXTURE", "id"));
                pssg.CreateSpecificTreeViewNode(cubeMapTreeView, "CUBEMAPTEXTURE");
            }
            // Select Starting Tab
            if (tabToSelect != MainTabs.Auto)
            {
                mainTabControl.SelectedTab = mainTabControl.TabPages[(int)tabToSelect];
            }
            else
            {
                mainTabControl.SelectedTab = mainTabControl.TabPages["allTabPage"];
                mainTabControl.SelectedTab = mainTabControl.TabPages["cubeMapTabPage"];
                mainTabControl.SelectedTab = mainTabControl.TabPages["texturesTabPage"];
            }
            this.Text = Properties.Resources.AppTitleShort + " - " + Path.GetFileName(filePath);
        }
        public enum MainTabs
        {
            All, // allTabPage
            Textures, // texturesTabPage
            CubeMaps, // cubeMapTabPage
            Auto
        }
        #endregion

        #region All
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode == null && textureObjectListView.SelectedObject == null)
            {
                MessageBox.Show("Tree node not selected!", "Select a Node", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            PssgNode node = ((PssgNode)treeView.SelectedNode.Tag);
            if (node.HasAttribute("id") == true)
            {
                idTextBox.Text = node.Attributes["id"].Value.ToString();
            }
            createView(node);
        }
        private void treeView_KeyUp(object sender, KeyEventArgs e)
        {
            if (pssg == null)
                return;

            if (e.Control && e.Shift && e.KeyCode == Keys.A)
            {
                if (pssg.RootNode != null && treeView.SelectedNode == null)
                    return;
                //allTvAddNode();
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.D)
            {
                if (treeView.SelectedNode == null)
                    return;
                //allTvRemoveNode();
            }
        }
        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (pssg == null)
                    return;

                TreeNode retrieve = treeView.GetNodeAt(e.Location);
                if (retrieve != null)
                {
                    treeView.SelectedNode = retrieve;
                }

                //nodesToolStripMenuItem.DropDown.Show(treeView, e.Location);
            }
        }
        private void nodesToolStripMenuItemDropDown_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (pssg == null)
            {
                addNodeToolStripMenuItem.Enabled = false;
            }
            else
            {
                addNodeToolStripMenuItem.Enabled = true;
            }

            if (treeView.SelectedNode == null)
            {
                removeNodeToolStripMenuItem.Enabled = false;
                exportNodeToolStripMenuItem.Enabled = false;
                importNodeToolStripMenuItem.Enabled = false;
                addAttributeToolStripMenuItem.Enabled = false;
                removeAttributeToolStripMenuItem.Enabled = false;
                exportNodeDataToolStripMenuItem.Enabled = false;
                importNodeDataToolStripMenuItem.Enabled = false;
            }
            else
            {
                removeNodeToolStripMenuItem.Enabled = true;
                exportNodeToolStripMenuItem.Enabled = true;
                importNodeToolStripMenuItem.Enabled = true;
                addAttributeToolStripMenuItem.Enabled = true;
                if (dataGridView.SelectedRows.Count == 0)
                {
                    removeAttributeToolStripMenuItem.Enabled = false;
                }
                else
                {
                    removeAttributeToolStripMenuItem.Enabled = true;
                }
                if (((PssgNode)treeView.SelectedNode.Tag).ChildNodes.Count > 0)
                {
                    exportNodeDataToolStripMenuItem.Enabled = false;
                    importNodeDataToolStripMenuItem.Enabled = false;
                }
                else
                {
                    exportNodeDataToolStripMenuItem.Enabled = true;
                    importNodeDataToolStripMenuItem.Enabled = true;
                }
            }
        }
        private void createView(PssgNode node)
        {
            // Determine if we need a DataGridView or a RichTextBox based on data to be displayed
            dataGridView.Rows.Clear();
            dataGridView.TopLeftHeaderCell.Value = node.Name;
            int i = 0;
            foreach (PssgAttribute pair in node.Attributes)
            {
                dataGridView.Rows.Add(pair.Value);
                dataGridView.Rows[i].HeaderCell.Value = pair.Name;
                dataGridView.Rows[i].Cells[0].ValueType = pair.ValueType;
                dataGridView.Rows[i].Tag = pair;
                i++;
            }
            dataGridView.Tag = node;
            dataGridView.BringToFront();
            if (node.IsDataNode)
            {
                richTextBox1.Text = node.ToString();//EndianBitConverter.ToString(node.data);
                richTextBox1.Visible = true;
                if (node.Attributes.Count == 0)
                {
                    richTextBox1.Dock = DockStyle.Fill;
                    richTextBox1.BringToFront();
                }
                else
                {
                    richTextBox1.Dock = DockStyle.Bottom;
                    richTextBox1.Size = new System.Drawing.Size(richTextBox1.Size.Width, 214);
                    dataGridView.BringToFront();
                }
            }
            else
            {
                richTextBox1.Visible = false;
            }
        }
        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            PssgNode node = ((PssgNode)dataGridView.Tag);
            string attrName = (string)dataGridView.Rows[e.RowIndex].HeaderCell.Value;
            PssgAttribute attr = node.Attributes[(string)dataGridView.Rows[e.RowIndex].HeaderCell.Value];
            if (attr.Value == dataGridView.Rows[e.RowIndex].Cells[0].Value)
            {
                return;
            }
            attr.Value = dataGridView.Rows[e.RowIndex].Cells[0].Value;
        }

        #region DragDrop
        void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }
        void treeView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        void treeView_DragDrop(object sender, DragEventArgs e)
        {
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = treeView.GetNodeAt(targetPoint);
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (!draggedNode.Equals(targetNode) && targetNode != null && draggedNode.Parent != null && draggedNode.Parent != targetNode)
            {
                pssg.MoveNode((PssgNode)draggedNode.Tag, (PssgNode)targetNode.Tag);
                draggedNode.Remove();
                targetNode.Nodes.Add(draggedNode);
                targetNode.Expand();
                treeView.SelectedNode = draggedNode;
            }
        }
        #endregion

        private void addNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allTvAddNode();
        }
        private void removeNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allTvRemoveNode();
        }
        private void exportNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Xml files|*.xml|All files|*.*";
            dialog.Title = "Select the node's save location and file name";
            dialog.DefaultExt = "xml";
            dialog.FileName = "node.xml";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    PssgNode node = ((PssgNode)treeView.SelectedNode.Tag);
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
                        ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void importNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Xml files|*.xml|All files|*.*";
            dialog.Title = "Select a xml file";
            dialog.FileName = "node.xml";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    PssgNode node = ((PssgNode)treeView.SelectedNode.Tag);
                    using (FileStream fileStream = File.Open(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        XDocument xDoc = XDocument.Load(fileStream);

                        PssgNode newNode = new PssgNode((XElement)((XElement)xDoc.FirstNode).FirstNode, pssg, node.ParentNode);
                        if (node.ParentNode != null)
                        {
                            node = node.ParentNode.SetChild(node, newNode);
                        }
                        else
                        {
                            node.File.RootNode = newNode;
                        }
                    }

                    clearVars(false);
                    setupEditor(MainTabs.All);
                    treeView.SelectedNode = node.TreeNode;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import the node!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void addAttributeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PssgNode parentNode = ((PssgNode)treeView.SelectedNode.Tag);
            using (AddBox aBox = new AddBox(pssg, 1))
            {
                if (aBox.ShowDialog() == DialogResult.OK)
                {
                    if (parentNode.AddAttribute(aBox.AttributeName, Convert.ChangeType(aBox.Value, aBox.ValueType)) == null)
                    {
                        return;
                    }
                    createView(parentNode);
                }
            }
        }
        private void removeAttributeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PssgNode node = ((PssgNode)treeView.SelectedNode.Tag);
            PssgAttribute attr = (PssgAttribute)dataGridView.SelectedRows[0].Tag;
            node.RemoveAttribute(attr.Name);
            createView(node);
        }
        private void exportNodeDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Bin files|*.bin|All files|*.*";
            dialog.Title = "Select the byte data save location and file name";
            dialog.DefaultExt = "bin";
            dialog.FileName = "nodeData.bin";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    PssgNode node = ((PssgNode)treeView.SelectedNode.Tag);
                    using (PssgBinaryWriter writer = new PssgBinaryWriter(new BigEndianBitConverter(), File.Open(dialog.FileName, FileMode.Create)))
                    {
                        writer.WriteObject(node.Value);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export data!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void importNodeDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Bin files|*.bin|All files|*.*";
            dialog.Title = "Select a bin file";
            dialog.FileName = "nodeData.bin";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    PssgNode node = ((PssgNode)treeView.SelectedNode.Tag);
                    using (PssgBinaryReader reader = new PssgBinaryReader(new BigEndianBitConverter(), File.Open(dialog.FileName, FileMode.Open, FileAccess.Read)))
                    {
                        node.Value = reader.ReadNodeValue(node.ValueType, (int)reader.BaseStream.Length);
                        createView(node);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import data!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void allTvAddNode()
        {
            PssgNode parentNode = pssg.RootNode == null ? null : ((PssgNode)treeView.SelectedNode.Tag);
            using (AddBox aBox = new AddBox(pssg, 0))
            {
                if (aBox.ShowDialog() == DialogResult.OK)
                {
                    PssgNode newNode;
                    if (pssg.RootNode == null)
                    {
                        newNode = new PssgNode(aBox.NodeName, pssg, null);
                        pssg.RootNode = newNode;
                    }
                    else
                    {
                        newNode = parentNode.AppendChild(aBox.NodeName);
                    }
                    if (newNode == null)
                    {
                        return;
                    }
                    TreeNode newTreeNode = pssg.CreateTreeViewNode(newNode);
                    if (parentNode == null)
                        treeView.Nodes.Add(newTreeNode);
                    else
                        treeView.SelectedNode.Nodes.Add(newTreeNode);
                    treeView.SelectedNode = newTreeNode;
                }
            }
        }
        private void allTvRemoveNode()
        {
            PssgNode node = ((PssgNode)treeView.SelectedNode.Tag);
            if (node.ParentNode != null)
            {
                node.ParentNode.RemoveChild(node);
            }
            else
            {
                node.File.RootNode = null;
            }
            treeView.SelectedNode.Remove();
        }
        #endregion

        #region Textures
        private void textureObjectListView_SelectionChanged(object sender, EventArgs e)
        {
            if (textureObjectListView.SelectedObject == null)
            {
                return;
            }

            treeView.SelectedNode = ((PssgNode)textureObjectListView.SelectedObject).TreeNode;
            //SelectCorrespondingNode(treeView.Nodes[0], ((CNode)textureTreeView.SelectedNode.Tag).attributes["id"].ToString());
            // Create Preview
            createPreview(((PssgNode)textureObjectListView.SelectedObject));
        }
        private bool SelectCorrespondingNode(TreeNode tNode, string id)
        {
            PssgNode tag = (PssgNode)tNode.Tag;

            if (tag.HasAttribute("id") && tag.GetAttribute("id").ToString() == id)
            {
                treeView.SelectedNode = tNode;
                return true;
            }
            else
            {
                foreach (TreeNode sub in tNode.Nodes)
                {
                    bool result = SelectCorrespondingNode(sub, id);
                    if (result == true)
                    {
                        return result;
                    }
                }
            }

            return false;
        }
        private void createPreview(PssgNode node)
        {
            // Make Preview
            try
            {
                textureImageLabel.Text = "";
                int height = 0; int width = 0;
                texturePictureBox.Dock = DockStyle.Fill;
                height = texturePictureBox.Height;
                width = texturePictureBox.Width;
                DdsFile dds = new DdsFile(node, false);
                dds.Write(File.Open(Application.StartupPath + "\\temp.dds", FileMode.Create, FileAccess.ReadWrite, FileShare.Read), -1);
                // Dispose of Old Images
                if (texturePictureBox.Image != null)
                {
                    texturePictureBox.Image.Dispose();
                    texturePictureBox.Image = null;
                }
                // Setup New Image
                System.Drawing.Bitmap image = FreeImage.GetBitmap(FreeImage.Load(FREE_IMAGE_FORMAT.FIF_DDS, Application.StartupPath + "\\temp.dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT));
                if (image.Height <= height && image.Width <= width)
                {
                    texturePictureBox.Dock = DockStyle.None;
                    texturePictureBox.Width = image.Width;
                    texturePictureBox.Height = image.Height;
                }
                texturePictureBox.Image = image;
            }
            catch (Exception ex)
            {
                if (texturePictureBox.Image != null)
                {
                    texturePictureBox.Image.Dispose();
                    texturePictureBox.Image = null;
                }
                textureImageLabel.Text = "Could not create preview! Export/Import may still work in certain circumstances." + Environment.NewLine + Environment.NewLine + ex.Message;
                //MessageBox.Show("Could not create preview! Export/Import may still work in certain circumstances." + Environment.NewLine + Environment.NewLine 
                //	+ ex.Message, "No Preview", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void texturesTextBox_TextChanged(object sender, EventArgs e)
        {
            textureObjectListView.BeginUpdate();
            textureObjectListView.ModelFilter = new ModelFilter(delegate(object x)
            {
                string name = (string)((PssgNode)x).GetAttribute("id").ToString();
                return name.StartsWith(texturesTextBox.Text, StringComparison.CurrentCultureIgnoreCase) ||
                    name.Contains(texturesTextBox.Text.ToLower());
            });
            textureObjectListView.EndUpdate();

            if (textureObjectListView.GetItemCount() > 0)
            {
                textureObjectListView.SelectedIndex = 0;
            }
        }

        private void addTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textureObjectListView.SelectedObject == null)
            {
                MessageBox.Show("Select a texture to copy first!", "Select a Texture", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AddTexture ATForm = new AddTexture(idTextBox.Text + "_2");
            if (ATForm.ShowDialog() == DialogResult.OK)
            {
                // Copy and Edit Name
                PssgNode nodeToCopy = (PssgNode)textureObjectListView.SelectedObject;
                PssgNode newTexture = new PssgNode(nodeToCopy);
                newTexture.Attributes["id"].Value = ATForm.TName;
                // Add to Library
                if (nodeToCopy.ParentNode != null)
                {
                    nodeToCopy.ParentNode.AppendChild(newTexture);
                }
                else
                {
                    nodeToCopy.AppendChild(newTexture);
                }
                // Populate treeViews
                clearVars(false);
                setupEditor(MainTabs.Textures);
                textureObjectListView.SelectedIndex = textureObjectListView.GetItemCount() - 1;
            }
        }
        private void removeTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textureObjectListView.SelectedObject == null)
            {
                return;
            }

            // Remove from Parent Node
            PssgNode textureNode = (PssgNode)textureObjectListView.SelectedObject;
            PssgNode parentNode = textureNode.ParentNode;
            parentNode.RemoveChild(textureNode);
            textureNode = null;
            clearVars(false);
            setupEditor(MainTabs.Textures);
        }
        private void exportTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textureObjectListView.SelectedObject == null)
            {
                return;
            }

            PssgNode node = ((PssgNode)textureObjectListView.SelectedObject);
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Dds files|*.dds|All files|*.*";
            dialog.Title = "Select the dds save location and file name";
            dialog.FileName = node.Attributes["id"].ToString() + ".dds";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DdsFile dds = new DdsFile(node, false);
                    dds.Write(File.Open(dialog.FileName, FileMode.Create), -1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void importTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textureObjectListView.SelectedObject == null)
            {
                return;
            }

            PssgNode node = ((PssgNode)textureObjectListView.SelectedObject);
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Dds files|*.dds|All files|*.*";
            dialog.Title = "Select a dds file";
            dialog.FileName = node.Attributes["id"].ToString() + ".dds";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DdsFile dds = new DdsFile(File.Open(dialog.FileName, FileMode.Open));
                    dds.Write(node);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import texture!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                createPreview(node);
            }
        }
        private void exportAllTexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.HasTextures())
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(filePath.Replace(".", "_") + "_textures");
                DdsFile dds;
                foreach (PssgNode tex in textureObjectListView.Objects)
                {
                    dds = new DdsFile(tex, false);
                    dds.Write(File.Open(filePath.Replace(".", "_") + "_textures" + "\\" + tex.GetAttribute("id").ToString() + ".dds", FileMode.Create), -1);
                }
                MessageBox.Show("Textures exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("There was an error, could not export all textures!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void importAllTexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.HasTextures())
            {
                return;
            }

            try
            {
                string directory = filePath.Replace(".", "_") + "_textures";
                if (Directory.Exists(directory) == true)
                {
                    DdsFile dds;
                    foreach (string fileName in Directory.GetFiles(directory, "*.dds"))
                    {
                        foreach (PssgNode tex in textureObjectListView.Objects)
                        {
                            if (Path.GetFileNameWithoutExtension(fileName) == tex.GetAttribute("id").ToString())
                            {
                                dds = new DdsFile(File.Open(fileName, FileMode.Open));
                                dds.Write(tex);
                                break;
                            }
                        }
                    }

                    if (textureObjectListView.SelectedObject != null)
                    {
                        createPreview(((PssgNode)textureObjectListView.SelectedObject));
                    }
                    MessageBox.Show("Textures imported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Could not find textures folder!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch
            {
                MessageBox.Show("There was an error, could not import all textures!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool HasTextures()
        {
            if (textureObjectListView.Objects != null)
            {
                System.Collections.IEnumerator objects = textureObjectListView.Objects.GetEnumerator();
                return objects.MoveNext();
            }

            return false;
        }
        #endregion

        #region CubeMaps
        private void cubeMapTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeView.SelectedNode = ((PssgNode)cubeMapTreeView.SelectedNode.Tag).TreeNode;
            cubeMapPictureBox.Tag = 0;
            CubeMapCreatePreview(((PssgNode)cubeMapTreeView.SelectedNode.Tag), 0);
        }
        private void cubeMapPictureBox_Click(object sender, EventArgs e)
        {
            CubeMapCreatePreview(((PssgNode)cubeMapTreeView.SelectedNode.Tag), (int)cubeMapPictureBox.Tag + 1);
        }
        private void CubeMapCreatePreview(PssgNode node, int targetCount)
        {
            // Make Preview
            try
            {
                cubeMapImageLabel.Text = "";
                int height = 0; int width = 0;
                cubeMapPictureBox.Dock = DockStyle.Fill;
                height = cubeMapPictureBox.Height;
                width = cubeMapPictureBox.Width;
                FREE_IMAGE_FORMAT format = FREE_IMAGE_FORMAT.FIF_DDS;
                System.Drawing.Bitmap image = null;
                if (targetCount > 5)
                {
                    targetCount = 0;
                    cubeMapPictureBox.Tag = 0;
                }
                else
                {
                    cubeMapPictureBox.Tag = targetCount;
                }
                DdsFile dds = new DdsFile(node, false);
                dds.Write(File.Open(Application.StartupPath + "\\temp.dds", FileMode.Create), targetCount);
                image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp.dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
                if (cubeMapPictureBox.Image != null)
                {
                    cubeMapPictureBox.Image.Dispose();
                    cubeMapPictureBox.Image = null;
                }
                /*foreach (CNode sub in node.subNodes) {
                    if (targetCount == 0 && sub.attributes["typename"].ToString() == "Raw") {
                        CubeMapWriteDDS(Application.StartupPath + "\\temp" + "Raw" + ".dds", node, targetCount);
                        image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp" + "Raw" + ".dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
                    } else if (targetCount == 1 && sub.attributes["typename"].ToString() == "RawNegativeX") {
                        CubeMapWriteDDS(Application.StartupPath + "\\temp" + "RawNegativeX" + ".dds", node, targetCount);
                        image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp" + "RawNegativeX" + ".dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
                    } else if (targetCount == 2 && sub.attributes["typename"].ToString() == "RawPositiveY") {
                        CubeMapWriteDDS(Application.StartupPath + "\\temp" + "RawPositiveY" + ".dds", node, targetCount);
                        image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp" + "RawPositiveY" + ".dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
                    } else if (targetCount == 3 && sub.attributes["typename"].ToString() == "RawNegativeY") {
                        CubeMapWriteDDS(Application.StartupPath + "\\temp" + "RawNegativeY" + ".dds", node, targetCount);
                        image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp" + "RawNegativeY" + ".dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
                    } else if (targetCount == 4 && sub.attributes["typename"].ToString() == "RawPositiveZ") {
                        CubeMapWriteDDS(Application.StartupPath + "\\temp" + "RawPositiveZ" + ".dds", node, targetCount);
                        image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp" + "RawPositiveZ" + ".dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
                    } else if (targetCount == 5 && sub.attributes["typename"].ToString() == "RawNegativeZ") {
                        CubeMapWriteDDS(Application.StartupPath + "\\temp" + "RawNegativeZ" + ".dds", node, targetCount);
                        image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp" + "RawNegativeZ" + ".dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
                    }
                }*/
                if (image.Height <= height && image.Width <= width)
                {
                    cubeMapPictureBox.Dock = DockStyle.None;
                    cubeMapPictureBox.Width = image.Width;
                    cubeMapPictureBox.Height = image.Height;
                }
                cubeMapPictureBox.Image = image;
            }
            catch
            {
                if (cubeMapPictureBox.Image != null)
                {
                    cubeMapPictureBox.Image.Dispose();
                    cubeMapPictureBox.Image = null;
                }
                cubeMapImageLabel.Text = "Could not create preview!";
                //MessageBox.Show("Could not create preview!", "No Preview", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cubeMapExportToolStripButton_Click(object sender, EventArgs e)
        {
            if (cubeMapTreeView.Nodes.Count == 0 || cubeMapTreeView.SelectedNode.Index == -1)
            {
                return;
            }
            PssgNode node = ((PssgNode)cubeMapTreeView.SelectedNode.Tag);
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "DDS files|*.dds|All files|*.*";
            dialog.Title = "Select the dds save location and file name";
            dialog.FileName = node.Attributes["id"].ToString() + ".dds";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DdsFile dds = new DdsFile(node, false);
                    dds.Write(File.Open(dialog.FileName, FileMode.Create), -1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export cubemap!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cubeMapImportToolStripButton_Click(object sender, EventArgs e)
        {
            if (cubeMapTreeView.Nodes.Count == 0 || cubeMapTreeView.SelectedNode.Index == -1)
            {
                return;
            }
            PssgNode node = ((PssgNode)cubeMapTreeView.SelectedNode.Tag);
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "DDS files|*.dds|All files|*.*";
            dialog.Title = "Select a cubemap dds file";
            dialog.FileName = node.Attributes["id"].ToString() + ".dds";
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DdsFile dds = new DdsFile(File.Open(dialog.FileName, FileMode.Open));
                    dds.Write(node);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import cubemap!" + Environment.NewLine + Environment.NewLine +
                        ex.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                cubeMapPictureBox.Tag = 0;
                CubeMapCreatePreview(node, 0);
            }
        }
        #endregion

        private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mainTabControl.SelectedTab.Name == "texturesTabPage")
            {
                textureObjectListView.Focus();
            }
            else if (mainTabControl.SelectedTab.Name == "cubeMapTabPage")
            {
                cubeMapTreeView.Focus();
            }
            else
            {
                treeView.Focus();
            }
        }

        private void mainTabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            // If No Textures Don't Select
            if (e.TabPage.Name == "texturesTabPage")
            {
                if (!this.HasTextures())
                {
                    texturesTextBox.Text = "";
                    e.Cancel = true;
                }
                else
                {
                    int selected = textureObjectListView.SelectedIndex;
                    if (selected < 0)
                    {
                        textureObjectListView.SelectedIndex = 0;
                    }
                    else
                    {
                        textureObjectListView.SelectedIndex = -1;
                        textureObjectListView.SelectedIndex = selected;
                    }
                }
            }
            else if (e.TabPage.Name == "cubeMapTabPage")
            {
                if (cubeMapTreeView.Nodes.Count == 0)
                {
                    e.Cancel = true;
                }
                else
                {
                    TreeNode selected = cubeMapTreeView.SelectedNode;
                    if (selected == null)
                    {
                        cubeMapTreeView.SelectedNode = cubeMapTreeView.Nodes[0];
                    }
                    else
                    {
                        cubeMapTreeView.SelectedNode = null;
                        cubeMapTreeView.SelectedNode = selected;
                    }
                }
            }
            else
            {
                if (treeView.Nodes.Count == 0)
                {
                    e.Cancel = true;
                }
                else
                {
                    TreeNode selected = treeView.SelectedNode;
                    if (selected == null && treeView.Nodes.Count > 0)
                    {
                        treeView.SelectedNode = treeView.Nodes[0];
                    }
                    else
                    {
                        treeView.SelectedNode = null;
                        treeView.SelectedNode = selected;
                    }
                }
            }
        }

        private void termpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Dirt 2 to Grid
            foreach (PssgNode mpjn in pssg.FindNodes("MATRIXPALETTEJOINTNODE"))
            {
                PssgAttribute jointId = new PssgAttribute(null, null, null, null);
                foreach (PssgNode mpjri in mpjn.FindNodes("MATRIXPALETTEJOINTRENDERINSTANCE"))
                {
                    mpjri.Rename("MATRIXPALETTERENDERINSTANCE");
                    jointId = mpjri.GetAttribute("jointID");
                    mpjri.RemoveAttribute(jointId.Name);
                }
                mpjn.AddAttribute(jointId.Name, jointId.Value);
            }

            foreach (PssgNode mpn in pssg.FindNodes("MATRIXPALETTENODE"))
            {
                PssgAttribute jointCount = new PssgAttribute(null, null, null, null);
                UInt32 jointCountNum = 0;
                List<PssgNode> mpriNodes = mpn.FindNodes("MATRIXPALETTERENDERINSTANCE");
                foreach (PssgNode mpri in mpriNodes)
                {
                    mpri.Rename("RENDERSTREAMINSTANCE");
                    jointCountNum += (UInt32)mpri.GetAttribute("jointCount").Value;
                    mpri.RemoveAttribute("jointCount");
                    foreach (PssgNode mpsj in mpri.FindNodes("MATRIXPALETTESKINJOINT"))
                    {
                        pssg.MoveNode(mpsj, mpn);
                    }
                }
                mpn.AddAttribute("jointCount", jointCountNum);
            }

            // Stuff below may not be needed
            foreach (PssgNode rds in pssg.FindNodes("RENDERDATASOURCE"))
            {
                rds[3].GetAttribute("subStream").Value = 3u;
                rds[3].GetAttribute("dataBlock").Value = rds[4].GetAttribute("dataBlock").Value;
                PssgNode dBlock1 = pssg.FindNodes("DATABLOCK", "id", rds[2].GetAttribute("dataBlock").ToString().Substring(1))[0];
                PssgNode dBlock2 = pssg.FindNodes("DATABLOCK", "id", rds[4].GetAttribute("dataBlock").ToString().Substring(1))[0];

                dBlock1[0].GetAttribute("stride").Value = 16u;
                dBlock1[1].GetAttribute("stride").Value = 16u;

                dBlock1[2].GetAttribute("offset").Value = dBlock2[0].GetAttribute("stride").Value;
                dBlock2[0].GetAttribute("stride").Value = (UInt32)dBlock2[0].GetAttribute("stride").Value + 12u;
                dBlock2[1].GetAttribute("stride").Value = (UInt32)dBlock2[1].GetAttribute("stride").Value + 12u;
                dBlock2[2].GetAttribute("stride").Value = (UInt32)dBlock2[2].GetAttribute("stride").Value + 12u;
                dBlock1[2].GetAttribute("stride").Value = dBlock2[0].GetAttribute("stride").Value;

                pssg.MoveNode(dBlock1[2], dBlock2);
            }

            clearVars(false);
            setupEditor(MainTabs.Auto);
        }
    }

    public static class ControlHelper
    {
        #region Redraw Suspend/Resume
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessageA", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        private const int WM_SETREDRAW = 0xB;

        public static void SuspendDrawing(this Control target)
        {
            SendMessage(target.Handle, WM_SETREDRAW, 0, 0);
        }

        public static void ResumeDrawing(this Control target) { ResumeDrawing(target, true); }
        public static void ResumeDrawing(this Control target, bool redraw)
        {
            SendMessage(target.Handle, WM_SETREDRAW, 1, 0);

            if (redraw)
            {
                target.Refresh();
            }
        }
        #endregion
    }
}