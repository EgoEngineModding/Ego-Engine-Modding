namespace EgoPssgEditor
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsOpenedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsPssgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsCompressedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsXmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.schemaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadSchemaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSchemaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearSchemaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addAttributeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAttributeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportNodeDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importNodeDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importAllTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.termpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.allTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.treeView = new System.Windows.Forms.TreeView();
            this.idTextBox = new System.Windows.Forms.TextBox();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.texturesTabPage = new System.Windows.Forms.TabPage();
            this.textureSplitContainer = new System.Windows.Forms.SplitContainer();
            this.textureObjectListView = new BrightIdeasSoftware.ObjectListView();
            this.texturesTextBox = new System.Windows.Forms.TextBox();
            this.textureImageLabel = new System.Windows.Forms.Label();
            this.texturePictureBox = new System.Windows.Forms.PictureBox();
            this.cubeMapTabPage = new System.Windows.Forms.TabPage();
            this.cubeMapSplitContainer = new System.Windows.Forms.SplitContainer();
            this.cubeMapTreeView = new System.Windows.Forms.TreeView();
            this.cubeMapImageLabel = new System.Windows.Forms.Label();
            this.cubeMapPictureBox = new System.Windows.Forms.PictureBox();
            this.cubeMapToolStrip = new System.Windows.Forms.ToolStrip();
            this.cubeMapExportToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.cubeMapImportToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.menuStrip.SuspendLayout();
            this.mainTabControl.SuspendLayout();
            this.allTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.texturesTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textureSplitContainer)).BeginInit();
            this.textureSplitContainer.Panel1.SuspendLayout();
            this.textureSplitContainer.Panel2.SuspendLayout();
            this.textureSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textureObjectListView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.texturePictureBox)).BeginInit();
            this.cubeMapTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cubeMapSplitContainer)).BeginInit();
            this.cubeMapSplitContainer.Panel1.SuspendLayout();
            this.cubeMapSplitContainer.Panel2.SuspendLayout();
            this.cubeMapSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cubeMapPictureBox)).BeginInit();
            this.cubeMapToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.schemaToolStripMenuItem,
            this.nodesToolStripMenuItem,
            this.texturesToolStripMenuItem,
            this.termpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(792, 24);
            this.menuStrip.TabIndex = 2;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = global::EgoPssgEditor.Properties.Resources.image_add;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::EgoPssgEditor.Properties.Resources.folder_image;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsOpenedToolStripMenuItem,
            this.saveAsPssgToolStripMenuItem,
            this.saveAsCompressedToolStripMenuItem,
            this.saveAsXmlToolStripMenuItem});
            this.saveToolStripMenuItem.Image = global::EgoPssgEditor.Properties.Resources.disk;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // saveAsOpenedToolStripMenuItem
            // 
            this.saveAsOpenedToolStripMenuItem.Name = "saveAsOpenedToolStripMenuItem";
            this.saveAsOpenedToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveAsOpenedToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.saveAsOpenedToolStripMenuItem.Text = "Save As Opened";
            this.saveAsOpenedToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsPssgToolStripMenuItem
            // 
            this.saveAsPssgToolStripMenuItem.Name = "saveAsPssgToolStripMenuItem";
            this.saveAsPssgToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.saveAsPssgToolStripMenuItem.Text = "Save As Pssg";
            this.saveAsPssgToolStripMenuItem.Click += new System.EventHandler(this.saveAsPssgToolStripMenuItem_Click);
            // 
            // saveAsCompressedToolStripMenuItem
            // 
            this.saveAsCompressedToolStripMenuItem.Name = "saveAsCompressedToolStripMenuItem";
            this.saveAsCompressedToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.saveAsCompressedToolStripMenuItem.Text = "Save As Compressed";
            this.saveAsCompressedToolStripMenuItem.Click += new System.EventHandler(this.saveAsCompressedToolStripMenuItem_Click);
            // 
            // saveAsXmlToolStripMenuItem
            // 
            this.saveAsXmlToolStripMenuItem.Name = "saveAsXmlToolStripMenuItem";
            this.saveAsXmlToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.saveAsXmlToolStripMenuItem.Text = "Save As Xml";
            this.saveAsXmlToolStripMenuItem.Click += new System.EventHandler(this.saveAsXmlToolStripMenuItem_Click);
            // 
            // schemaToolStripMenuItem
            // 
            this.schemaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadSchemaToolStripMenuItem,
            this.saveSchemaToolStripMenuItem,
            this.clearSchemaToolStripMenuItem});
            this.schemaToolStripMenuItem.Name = "schemaToolStripMenuItem";
            this.schemaToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.schemaToolStripMenuItem.Text = "Schema";
            // 
            // loadSchemaToolStripMenuItem
            // 
            this.loadSchemaToolStripMenuItem.Name = "loadSchemaToolStripMenuItem";
            this.loadSchemaToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
            this.loadSchemaToolStripMenuItem.Text = "Load";
            this.loadSchemaToolStripMenuItem.Click += new System.EventHandler(this.loadSchemaToolStripMenuItem_Click);
            // 
            // saveSchemaToolStripMenuItem
            // 
            this.saveSchemaToolStripMenuItem.Name = "saveSchemaToolStripMenuItem";
            this.saveSchemaToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
            this.saveSchemaToolStripMenuItem.Text = "Save";
            this.saveSchemaToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem1_Click);
            // 
            // clearSchemaToolStripMenuItem
            // 
            this.clearSchemaToolStripMenuItem.Name = "clearSchemaToolStripMenuItem";
            this.clearSchemaToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
            this.clearSchemaToolStripMenuItem.Text = "Clear";
            this.clearSchemaToolStripMenuItem.Click += new System.EventHandler(this.clearSchemaToolStripMenuItem_Click);
            // 
            // nodesToolStripMenuItem
            // 
            this.nodesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNodeToolStripMenuItem,
            this.removeNodeToolStripMenuItem,
            this.exportNodeToolStripMenuItem,
            this.importNodeToolStripMenuItem,
            this.addAttributeToolStripMenuItem,
            this.removeAttributeToolStripMenuItem,
            this.exportNodeDataToolStripMenuItem,
            this.importNodeDataToolStripMenuItem});
            this.nodesToolStripMenuItem.Name = "nodesToolStripMenuItem";
            this.nodesToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.nodesToolStripMenuItem.Text = "Nodes";
            // 
            // addNodeToolStripMenuItem
            // 
            this.addNodeToolStripMenuItem.Name = "addNodeToolStripMenuItem";
            this.addNodeToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.addNodeToolStripMenuItem.Text = "Add Node";
            this.addNodeToolStripMenuItem.Click += new System.EventHandler(this.addNodeToolStripMenuItem_Click);
            // 
            // removeNodeToolStripMenuItem
            // 
            this.removeNodeToolStripMenuItem.Name = "removeNodeToolStripMenuItem";
            this.removeNodeToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.removeNodeToolStripMenuItem.Text = "Remove Node";
            this.removeNodeToolStripMenuItem.Click += new System.EventHandler(this.removeNodeToolStripMenuItem_Click);
            // 
            // exportNodeToolStripMenuItem
            // 
            this.exportNodeToolStripMenuItem.Name = "exportNodeToolStripMenuItem";
            this.exportNodeToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.exportNodeToolStripMenuItem.Text = "Export Node";
            this.exportNodeToolStripMenuItem.Click += new System.EventHandler(this.exportNodeToolStripMenuItem_Click);
            // 
            // importNodeToolStripMenuItem
            // 
            this.importNodeToolStripMenuItem.Name = "importNodeToolStripMenuItem";
            this.importNodeToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.importNodeToolStripMenuItem.Text = "Import Node";
            this.importNodeToolStripMenuItem.Click += new System.EventHandler(this.importNodeToolStripMenuItem_Click);
            // 
            // addAttributeToolStripMenuItem
            // 
            this.addAttributeToolStripMenuItem.Name = "addAttributeToolStripMenuItem";
            this.addAttributeToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.addAttributeToolStripMenuItem.Text = "Add Attribute";
            this.addAttributeToolStripMenuItem.Click += new System.EventHandler(this.addAttributeToolStripMenuItem_Click);
            // 
            // removeAttributeToolStripMenuItem
            // 
            this.removeAttributeToolStripMenuItem.Name = "removeAttributeToolStripMenuItem";
            this.removeAttributeToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.removeAttributeToolStripMenuItem.Text = "Remove Attribute";
            this.removeAttributeToolStripMenuItem.Click += new System.EventHandler(this.removeAttributeToolStripMenuItem_Click);
            // 
            // exportNodeDataToolStripMenuItem
            // 
            this.exportNodeDataToolStripMenuItem.Name = "exportNodeDataToolStripMenuItem";
            this.exportNodeDataToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.exportNodeDataToolStripMenuItem.Text = "Export Node Data";
            this.exportNodeDataToolStripMenuItem.Click += new System.EventHandler(this.exportNodeDataToolStripMenuItem_Click);
            // 
            // importNodeDataToolStripMenuItem
            // 
            this.importNodeDataToolStripMenuItem.Name = "importNodeDataToolStripMenuItem";
            this.importNodeDataToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.importNodeDataToolStripMenuItem.Text = "Import Node Data";
            this.importNodeDataToolStripMenuItem.Click += new System.EventHandler(this.importNodeDataToolStripMenuItem_Click);
            // 
            // texturesToolStripMenuItem
            // 
            this.texturesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTextureToolStripMenuItem,
            this.removeTextureToolStripMenuItem,
            this.exportTextureToolStripMenuItem,
            this.importTextureToolStripMenuItem,
            this.exportAllTexturesToolStripMenuItem,
            this.importAllTexturesToolStripMenuItem});
            this.texturesToolStripMenuItem.Name = "texturesToolStripMenuItem";
            this.texturesToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.texturesToolStripMenuItem.Text = "Textures";
            // 
            // addTextureToolStripMenuItem
            // 
            this.addTextureToolStripMenuItem.Name = "addTextureToolStripMenuItem";
            this.addTextureToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.addTextureToolStripMenuItem.Text = "Add Texture";
            this.addTextureToolStripMenuItem.Click += new System.EventHandler(this.addTextureToolStripMenuItem_Click);
            // 
            // removeTextureToolStripMenuItem
            // 
            this.removeTextureToolStripMenuItem.Name = "removeTextureToolStripMenuItem";
            this.removeTextureToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.removeTextureToolStripMenuItem.Text = "Remove Texture";
            this.removeTextureToolStripMenuItem.Click += new System.EventHandler(this.removeTextureToolStripMenuItem_Click);
            // 
            // exportTextureToolStripMenuItem
            // 
            this.exportTextureToolStripMenuItem.Name = "exportTextureToolStripMenuItem";
            this.exportTextureToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.exportTextureToolStripMenuItem.Text = "Export Texture";
            this.exportTextureToolStripMenuItem.Click += new System.EventHandler(this.exportTextureToolStripMenuItem_Click);
            // 
            // importTextureToolStripMenuItem
            // 
            this.importTextureToolStripMenuItem.Name = "importTextureToolStripMenuItem";
            this.importTextureToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.importTextureToolStripMenuItem.Text = "Import Texture";
            this.importTextureToolStripMenuItem.Click += new System.EventHandler(this.importTextureToolStripMenuItem_Click);
            // 
            // exportAllTexturesToolStripMenuItem
            // 
            this.exportAllTexturesToolStripMenuItem.Name = "exportAllTexturesToolStripMenuItem";
            this.exportAllTexturesToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.exportAllTexturesToolStripMenuItem.Text = "Export All Textures";
            this.exportAllTexturesToolStripMenuItem.Click += new System.EventHandler(this.exportAllTexturesToolStripMenuItem_Click);
            // 
            // importAllTexturesToolStripMenuItem
            // 
            this.importAllTexturesToolStripMenuItem.Name = "importAllTexturesToolStripMenuItem";
            this.importAllTexturesToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.importAllTexturesToolStripMenuItem.Text = "Import All Textures";
            this.importAllTexturesToolStripMenuItem.Click += new System.EventHandler(this.importAllTexturesToolStripMenuItem_Click);
            // 
            // termpToolStripMenuItem
            // 
            this.termpToolStripMenuItem.Name = "termpToolStripMenuItem";
            this.termpToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.termpToolStripMenuItem.Text = "Termp";
            this.termpToolStripMenuItem.Click += new System.EventHandler(this.termpToolStripMenuItem_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "PSSG files|*.pssg|DDS files|*.dds|Xml files|*.xml|All files|*.*";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "PSSG files|*.pssg|DDS files|*.dds|Xml files|*.xml|All files|*.*";
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.allTabPage);
            this.mainTabControl.Controls.Add(this.texturesTabPage);
            this.mainTabControl.Controls.Add(this.cubeMapTabPage);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(0, 24);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(792, 542);
            this.mainTabControl.TabIndex = 6;
            // 
            // allTabPage
            // 
            this.allTabPage.Controls.Add(this.splitContainer);
            this.allTabPage.Location = new System.Drawing.Point(4, 22);
            this.allTabPage.Name = "allTabPage";
            this.allTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.allTabPage.Size = new System.Drawing.Size(784, 516);
            this.allTabPage.TabIndex = 2;
            this.allTabPage.Text = "All Sections";
            this.allTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(3, 3);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.treeView);
            this.splitContainer.Panel1.Controls.Add(this.idTextBox);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.dataGridView);
            this.splitContainer.Panel2.Controls.Add(this.richTextBox1);
            this.splitContainer.Size = new System.Drawing.Size(778, 510);
            this.splitContainer.SplitterDistance = 278;
            this.splitContainer.SplitterWidth = 1;
            this.splitContainer.TabIndex = 8;
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.HideSelection = false;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(278, 490);
            this.treeView.TabIndex = 6;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            this.treeView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyUp);
            this.treeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseDown);
            // 
            // idTextBox
            // 
            this.idTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.idTextBox.Location = new System.Drawing.Point(0, 490);
            this.idTextBox.Name = "idTextBox";
            this.idTextBox.Size = new System.Drawing.Size(278, 20);
            this.idTextBox.TabIndex = 7;
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.Location = new System.Drawing.Point(0, 0);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.Size = new System.Drawing.Size(499, 510);
            this.dataGridView.TabIndex = 4;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.HighlightText;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(499, 510);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // texturesTabPage
            // 
            this.texturesTabPage.Controls.Add(this.textureSplitContainer);
            this.texturesTabPage.Location = new System.Drawing.Point(4, 22);
            this.texturesTabPage.Name = "texturesTabPage";
            this.texturesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.texturesTabPage.Size = new System.Drawing.Size(784, 516);
            this.texturesTabPage.TabIndex = 1;
            this.texturesTabPage.Text = "Textures";
            this.texturesTabPage.UseVisualStyleBackColor = true;
            // 
            // textureSplitContainer
            // 
            this.textureSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textureSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.textureSplitContainer.Name = "textureSplitContainer";
            // 
            // textureSplitContainer.Panel1
            // 
            this.textureSplitContainer.Panel1.Controls.Add(this.textureObjectListView);
            this.textureSplitContainer.Panel1.Controls.Add(this.texturesTextBox);
            // 
            // textureSplitContainer.Panel2
            // 
            this.textureSplitContainer.Panel2.Controls.Add(this.textureImageLabel);
            this.textureSplitContainer.Panel2.Controls.Add(this.texturePictureBox);
            this.textureSplitContainer.Size = new System.Drawing.Size(778, 510);
            this.textureSplitContainer.SplitterDistance = 278;
            this.textureSplitContainer.SplitterWidth = 1;
            this.textureSplitContainer.TabIndex = 6;
            // 
            // textureObjectListView
            // 
            this.textureObjectListView.CellEditUseWholeCell = false;
            this.textureObjectListView.Cursor = System.Windows.Forms.Cursors.Default;
            this.textureObjectListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textureObjectListView.HighlightBackgroundColor = System.Drawing.Color.Empty;
            this.textureObjectListView.HighlightForegroundColor = System.Drawing.Color.Empty;
            this.textureObjectListView.Location = new System.Drawing.Point(0, 20);
            this.textureObjectListView.Name = "textureObjectListView";
            this.textureObjectListView.Size = new System.Drawing.Size(278, 490);
            this.textureObjectListView.TabIndex = 7;
            this.textureObjectListView.UseCompatibleStateImageBehavior = false;
            this.textureObjectListView.View = System.Windows.Forms.View.Details;
            this.textureObjectListView.SelectionChanged += new System.EventHandler(this.textureObjectListView_SelectionChanged);
            // 
            // texturesTextBox
            // 
            this.texturesTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.texturesTextBox.Location = new System.Drawing.Point(0, 0);
            this.texturesTextBox.Name = "texturesTextBox";
            this.texturesTextBox.Size = new System.Drawing.Size(278, 20);
            this.texturesTextBox.TabIndex = 7;
            this.texturesTextBox.TextChanged += new System.EventHandler(this.texturesTextBox_TextChanged);
            // 
            // textureImageLabel
            // 
            this.textureImageLabel.AutoSize = true;
            this.textureImageLabel.Location = new System.Drawing.Point(14, 20);
            this.textureImageLabel.Name = "textureImageLabel";
            this.textureImageLabel.Size = new System.Drawing.Size(35, 13);
            this.textureImageLabel.TabIndex = 6;
            this.textureImageLabel.Text = "label1";
            // 
            // texturePictureBox
            // 
            this.texturePictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.texturePictureBox.Location = new System.Drawing.Point(0, 0);
            this.texturePictureBox.Name = "texturePictureBox";
            this.texturePictureBox.Size = new System.Drawing.Size(499, 510);
            this.texturePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.texturePictureBox.TabIndex = 5;
            this.texturePictureBox.TabStop = false;
            // 
            // cubeMapTabPage
            // 
            this.cubeMapTabPage.Controls.Add(this.cubeMapSplitContainer);
            this.cubeMapTabPage.Controls.Add(this.cubeMapToolStrip);
            this.cubeMapTabPage.Location = new System.Drawing.Point(4, 22);
            this.cubeMapTabPage.Name = "cubeMapTabPage";
            this.cubeMapTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.cubeMapTabPage.Size = new System.Drawing.Size(784, 516);
            this.cubeMapTabPage.TabIndex = 3;
            this.cubeMapTabPage.Text = "CubeMaps";
            this.cubeMapTabPage.UseVisualStyleBackColor = true;
            // 
            // cubeMapSplitContainer
            // 
            this.cubeMapSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cubeMapSplitContainer.Location = new System.Drawing.Point(3, 3);
            this.cubeMapSplitContainer.Name = "cubeMapSplitContainer";
            // 
            // cubeMapSplitContainer.Panel1
            // 
            this.cubeMapSplitContainer.Panel1.Controls.Add(this.cubeMapTreeView);
            // 
            // cubeMapSplitContainer.Panel2
            // 
            this.cubeMapSplitContainer.Panel2.Controls.Add(this.cubeMapImageLabel);
            this.cubeMapSplitContainer.Panel2.Controls.Add(this.cubeMapPictureBox);
            this.cubeMapSplitContainer.Size = new System.Drawing.Size(778, 485);
            this.cubeMapSplitContainer.SplitterDistance = 278;
            this.cubeMapSplitContainer.SplitterWidth = 1;
            this.cubeMapSplitContainer.TabIndex = 7;
            // 
            // cubeMapTreeView
            // 
            this.cubeMapTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cubeMapTreeView.HideSelection = false;
            this.cubeMapTreeView.Location = new System.Drawing.Point(0, 0);
            this.cubeMapTreeView.Name = "cubeMapTreeView";
            this.cubeMapTreeView.Size = new System.Drawing.Size(278, 485);
            this.cubeMapTreeView.TabIndex = 6;
            this.cubeMapTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.cubeMapTreeView_AfterSelect);
            // 
            // cubeMapImageLabel
            // 
            this.cubeMapImageLabel.AutoSize = true;
            this.cubeMapImageLabel.Location = new System.Drawing.Point(14, 20);
            this.cubeMapImageLabel.Name = "cubeMapImageLabel";
            this.cubeMapImageLabel.Size = new System.Drawing.Size(35, 13);
            this.cubeMapImageLabel.TabIndex = 7;
            this.cubeMapImageLabel.Text = "label1";
            // 
            // cubeMapPictureBox
            // 
            this.cubeMapPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cubeMapPictureBox.Location = new System.Drawing.Point(0, 0);
            this.cubeMapPictureBox.Name = "cubeMapPictureBox";
            this.cubeMapPictureBox.Size = new System.Drawing.Size(499, 485);
            this.cubeMapPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.cubeMapPictureBox.TabIndex = 6;
            this.cubeMapPictureBox.TabStop = false;
            this.cubeMapPictureBox.Click += new System.EventHandler(this.cubeMapPictureBox_Click);
            // 
            // cubeMapToolStrip
            // 
            this.cubeMapToolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cubeMapToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.cubeMapToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cubeMapExportToolStripButton,
            this.cubeMapImportToolStripButton});
            this.cubeMapToolStrip.Location = new System.Drawing.Point(3, 488);
            this.cubeMapToolStrip.Name = "cubeMapToolStrip";
            this.cubeMapToolStrip.Size = new System.Drawing.Size(778, 25);
            this.cubeMapToolStrip.TabIndex = 8;
            this.cubeMapToolStrip.Text = "CubeMap Tool Box";
            // 
            // cubeMapExportToolStripButton
            // 
            this.cubeMapExportToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cubeMapExportToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("cubeMapExportToolStripButton.Image")));
            this.cubeMapExportToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cubeMapExportToolStripButton.Name = "cubeMapExportToolStripButton";
            this.cubeMapExportToolStripButton.Size = new System.Drawing.Size(44, 22);
            this.cubeMapExportToolStripButton.Text = "Export";
            this.cubeMapExportToolStripButton.Click += new System.EventHandler(this.cubeMapExportToolStripButton_Click);
            // 
            // cubeMapImportToolStripButton
            // 
            this.cubeMapImportToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cubeMapImportToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("cubeMapImportToolStripButton.Image")));
            this.cubeMapImportToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cubeMapImportToolStripButton.Name = "cubeMapImportToolStripButton";
            this.cubeMapImportToolStripButton.Size = new System.Drawing.Size(47, 22);
            this.cubeMapImportToolStripButton.Text = "Import";
            this.cubeMapImportToolStripButton.Click += new System.EventHandler(this.cubeMapImportToolStripButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 566);
            this.Controls.Add(this.mainTabControl);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "Form1";
            this.Text = "Ego PSSG Editor";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.mainTabControl.ResumeLayout(false);
            this.allTabPage.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.texturesTabPage.ResumeLayout(false);
            this.textureSplitContainer.Panel1.ResumeLayout(false);
            this.textureSplitContainer.Panel1.PerformLayout();
            this.textureSplitContainer.Panel2.ResumeLayout(false);
            this.textureSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textureSplitContainer)).EndInit();
            this.textureSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.textureObjectListView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.texturePictureBox)).EndInit();
            this.cubeMapTabPage.ResumeLayout(false);
            this.cubeMapTabPage.PerformLayout();
            this.cubeMapSplitContainer.Panel1.ResumeLayout(false);
            this.cubeMapSplitContainer.Panel2.ResumeLayout(false);
            this.cubeMapSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cubeMapSplitContainer)).EndInit();
            this.cubeMapSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cubeMapPictureBox)).EndInit();
            this.cubeMapToolStrip.ResumeLayout(false);
            this.cubeMapToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage texturesTabPage;
        private System.Windows.Forms.PictureBox texturePictureBox;
        private System.Windows.Forms.TabPage allTabPage;
		private System.Windows.Forms.TabPage cubeMapTabPage;
		private System.Windows.Forms.PictureBox cubeMapPictureBox;
		private System.Windows.Forms.TreeView cubeMapTreeView;
        private System.Windows.Forms.SplitContainer textureSplitContainer;
		private System.Windows.Forms.SplitContainer cubeMapSplitContainer;
		private System.Windows.Forms.ToolStrip cubeMapToolStrip;
		private System.Windows.Forms.ToolStripButton cubeMapExportToolStripButton;
        private System.Windows.Forms.ToolStripButton cubeMapImportToolStripButton;
        private System.Windows.Forms.TextBox texturesTextBox;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.Label textureImageLabel;
        private System.Windows.Forms.Label cubeMapImageLabel;
        private System.Windows.Forms.ToolStripMenuItem schemaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSchemaToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.TextBox idTextBox;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ToolStripMenuItem nodesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem termpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadSchemaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearSchemaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem texturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addAttributeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAttributeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportNodeDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importNodeDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importAllTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsPssgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsCompressedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsXmlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsOpenedToolStripMenuItem;
        private BrightIdeasSoftware.ObjectListView textureObjectListView;
    }
}

