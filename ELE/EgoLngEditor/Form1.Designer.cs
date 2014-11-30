namespace EgoLngEditor
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
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.importAsXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAsXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findNextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findPreviousToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.compareFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.specialThanksToMiekForFiguringOutTheHashFunctionsToAllowNewValuesToBeInsertedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.lngDataGridView = new System.Windows.Forms.DataGridView();
            this.mainMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lngDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.specialThanksToMiekForFiguringOutTheHashFunctionsToAllowNewValuesToBeInsertedToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(784, 24);
            this.mainMenuStrip.TabIndex = 0;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.toolStripSeparator1,
            this.importAsXMLToolStripMenuItem,
            this.exportAsXMLToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::EgoLngEditor.Properties.Resources.folder;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = global::EgoLngEditor.Properties.Resources.disk;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(150, 6);
            // 
            // importAsXMLToolStripMenuItem
            // 
            this.importAsXMLToolStripMenuItem.Image = global::EgoLngEditor.Properties.Resources.lorry_add;
            this.importAsXMLToolStripMenuItem.Name = "importAsXMLToolStripMenuItem";
            this.importAsXMLToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.importAsXMLToolStripMenuItem.Text = "Import As XML";
            this.importAsXMLToolStripMenuItem.Click += new System.EventHandler(this.importAsXMLToolStripMenuItem_Click);
            // 
            // exportAsXMLToolStripMenuItem
            // 
            this.exportAsXMLToolStripMenuItem.Image = global::EgoLngEditor.Properties.Resources.lorry_delete;
            this.exportAsXMLToolStripMenuItem.Name = "exportAsXMLToolStripMenuItem";
            this.exportAsXMLToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.exportAsXMLToolStripMenuItem.Text = "Export As XML";
            this.exportAsXMLToolStripMenuItem.Click += new System.EventHandler(this.exportAsXMLToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addRowToolStripMenuItem,
            this.deleteRowToolStripMenuItem,
            this.toolStripSeparator3,
            this.searchToolStripMenuItem,
            this.findNextToolStripMenuItem,
            this.findPreviousToolStripMenuItem,
            this.toolStripSeparator2,
            this.compareFilesToolStripMenuItem,
            this.mergeFilesToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // addRowToolStripMenuItem
            // 
            this.addRowToolStripMenuItem.Image = global::EgoLngEditor.Properties.Resources.table_row_insert;
            this.addRowToolStripMenuItem.Name = "addRowToolStripMenuItem";
            this.addRowToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.addRowToolStripMenuItem.Text = "Add Row...";
            this.addRowToolStripMenuItem.Click += new System.EventHandler(this.addRowToolStripMenuItem_Click);
            // 
            // deleteRowToolStripMenuItem
            // 
            this.deleteRowToolStripMenuItem.Image = global::EgoLngEditor.Properties.Resources.table_row_delete;
            this.deleteRowToolStripMenuItem.Name = "deleteRowToolStripMenuItem";
            this.deleteRowToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.deleteRowToolStripMenuItem.Text = "Remove Row";
            this.deleteRowToolStripMenuItem.Click += new System.EventHandler(this.deleteRowToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(188, 6);
            // 
            // searchToolStripMenuItem
            // 
            this.searchToolStripMenuItem.Image = global::EgoLngEditor.Properties.Resources.find;
            this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            this.searchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.searchToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.searchToolStripMenuItem.Text = "Find...";
            this.searchToolStripMenuItem.Click += new System.EventHandler(this.searchToolStripMenuItem_Click);
            // 
            // findNextToolStripMenuItem
            // 
            this.findNextToolStripMenuItem.Name = "findNextToolStripMenuItem";
            this.findNextToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.findNextToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.findNextToolStripMenuItem.Text = "Find Next";
            this.findNextToolStripMenuItem.Click += new System.EventHandler(this.findNextToolStripMenuItem_Click);
            // 
            // findPreviousToolStripMenuItem
            // 
            this.findPreviousToolStripMenuItem.Name = "findPreviousToolStripMenuItem";
            this.findPreviousToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F3)));
            this.findPreviousToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.findPreviousToolStripMenuItem.Text = "Find Previous";
            this.findPreviousToolStripMenuItem.Click += new System.EventHandler(this.findPreviousToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(188, 6);
            // 
            // compareFilesToolStripMenuItem
            // 
            this.compareFilesToolStripMenuItem.Image = global::EgoLngEditor.Properties.Resources.table_relationship;
            this.compareFilesToolStripMenuItem.Name = "compareFilesToolStripMenuItem";
            this.compareFilesToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.compareFilesToolStripMenuItem.Text = "Compare Files";
            this.compareFilesToolStripMenuItem.Click += new System.EventHandler(this.compareFilesToolStripMenuItem_Click);
            // 
            // mergeFilesToolStripMenuItem
            // 
            this.mergeFilesToolStripMenuItem.Image = global::EgoLngEditor.Properties.Resources.table_multiple;
            this.mergeFilesToolStripMenuItem.Name = "mergeFilesToolStripMenuItem";
            this.mergeFilesToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.mergeFilesToolStripMenuItem.Text = "Merge Files";
            this.mergeFilesToolStripMenuItem.Click += new System.EventHandler(this.mergeFilesToolStripMenuItem_Click);
            // 
            // specialThanksToMiekForFiguringOutTheHashFunctionsToAllowNewValuesToBeInsertedToolStripMenuItem
            // 
            this.specialThanksToMiekForFiguringOutTheHashFunctionsToAllowNewValuesToBeInsertedToolStripMenuItem.Enabled = false;
            this.specialThanksToMiekForFiguringOutTheHashFunctionsToAllowNewValuesToBeInsertedToolStripMenuItem.Name = "specialThanksToMiekForFiguringOutTheHashFunctionsToAllowNewValuesToBeInsertedTool" +
    "StripMenuItem";
            this.specialThanksToMiekForFiguringOutTheHashFunctionsToAllowNewValuesToBeInsertedToolStripMenuItem.Size = new System.Drawing.Size(140, 20);
            this.specialThanksToMiekForFiguringOutTheHashFunctionsToAllowNewValuesToBeInsertedToolStripMenuItem.Text = "Special Thanks to Miek";
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "language_xxx.lng";
            this.openFileDialog.Filter = "LNG files|*.lng|XML files|*.xml|All files|*.*";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileName = "language_xxx.lng";
            this.saveFileDialog.Filter = "LNG files|*.lng|XML files|*.xml|All files|*.*";
            // 
            // lngDataGridView
            // 
            this.lngDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.lngDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lngDataGridView.Location = new System.Drawing.Point(0, 24);
            this.lngDataGridView.Name = "lngDataGridView";
            this.lngDataGridView.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            this.lngDataGridView.Size = new System.Drawing.Size(784, 538);
            this.lngDataGridView.TabIndex = 4;
            this.lngDataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.lngDataGridView_CellValidating);
            this.lngDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.lngDataGridView_CellValueChanged);
            this.lngDataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lngDataGridView_KeyDown);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.lngDataGridView);
            this.Controls.Add(this.mainMenuStrip);
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "Form1";
            this.Text = "Ego Language Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lngDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.DataGridView lngDataGridView;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem importAsXMLToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportAsXMLToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem specialThanksToMiekForFiguringOutTheHashFunctionsToAllowNewValuesToBeInsertedToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addRowToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteRowToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem compareFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem findPreviousToolStripMenuItem;
    }
}

