namespace EgoDatabaseEditor
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
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.importAsXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAsXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openPopulatedTablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllTablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.addRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addMultipleRowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.showHideSidebarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openLinkedTablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compareDatabasesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeDatabasesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleConstraintsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tmppppToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.tableListBox = new System.Windows.Forms.ListBox();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.mainMenu.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.advancedToolStripMenuItem,
            this.tmppppToolStripMenuItem});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(632, 24);
            this.mainMenu.TabIndex = 8;
            this.mainMenu.Text = "menuStrip1";
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
            this.openToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.folder_database;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.disk;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(195, 6);
            // 
            // importAsXMLToolStripMenuItem
            // 
            this.importAsXMLToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.lorry_add;
            this.importAsXMLToolStripMenuItem.Name = "importAsXMLToolStripMenuItem";
            this.importAsXMLToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.importAsXMLToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.importAsXMLToolStripMenuItem.Text = "Import As XML";
            this.importAsXMLToolStripMenuItem.Click += new System.EventHandler(this.ImportAsXMLToolStripMenuItem_Click);
            // 
            // exportAsXMLToolStripMenuItem
            // 
            this.exportAsXMLToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.lorry_delete;
            this.exportAsXMLToolStripMenuItem.Name = "exportAsXMLToolStripMenuItem";
            this.exportAsXMLToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.exportAsXMLToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.exportAsXMLToolStripMenuItem.Text = "Export As XML";
            this.exportAsXMLToolStripMenuItem.Click += new System.EventHandler(this.ExportAsXMLToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openTableToolStripMenuItem,
            this.openPopulatedTablesToolStripMenuItem,
            this.closeTabToolStripMenuItem,
            this.closeAllTablesToolStripMenuItem,
            this.toolStripSeparator2,
            this.addRowToolStripMenuItem,
            this.addMultipleRowsToolStripMenuItem,
            this.editRowToolStripMenuItem,
            this.removeRowToolStripMenuItem,
            this.toolStripSeparator3,
            this.showHideSidebarToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // openTableToolStripMenuItem
            // 
            this.openTableToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.table_add;
            this.openTableToolStripMenuItem.Name = "openTableToolStripMenuItem";
            this.openTableToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.openTableToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.openTableToolStripMenuItem.Text = "Open Table";
            this.openTableToolStripMenuItem.Click += new System.EventHandler(this.OpenTableToolStripMenuItem_Click);
            // 
            // openPopulatedTablesToolStripMenuItem
            // 
            this.openPopulatedTablesToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.table_add;
            this.openPopulatedTablesToolStripMenuItem.Name = "openPopulatedTablesToolStripMenuItem";
            this.openPopulatedTablesToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.openPopulatedTablesToolStripMenuItem.Text = "Open Populated Tables";
            this.openPopulatedTablesToolStripMenuItem.Click += new System.EventHandler(this.OpenPopulatedTablesToolStripMenuItem_Click);
            // 
            // closeTabToolStripMenuItem
            // 
            this.closeTabToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.table_delete;
            this.closeTabToolStripMenuItem.Name = "closeTabToolStripMenuItem";
            this.closeTabToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.closeTabToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.closeTabToolStripMenuItem.Text = "Close Table";
            this.closeTabToolStripMenuItem.Click += new System.EventHandler(this.CloseTabToolStripMenuItem_Click);
            // 
            // closeAllTablesToolStripMenuItem
            // 
            this.closeAllTablesToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.table_delete;
            this.closeAllTablesToolStripMenuItem.Name = "closeAllTablesToolStripMenuItem";
            this.closeAllTablesToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.closeAllTablesToolStripMenuItem.Text = "Close All Tables";
            this.closeAllTablesToolStripMenuItem.Click += new System.EventHandler(this.CloseAllTablesToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(194, 6);
            // 
            // addRowToolStripMenuItem
            // 
            this.addRowToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.table_row_insert;
            this.addRowToolStripMenuItem.Name = "addRowToolStripMenuItem";
            this.addRowToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.addRowToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.addRowToolStripMenuItem.Text = "Add Row";
            this.addRowToolStripMenuItem.Click += new System.EventHandler(this.AddRowToolStripMenuItem_Click);
            // 
            // addMultipleRowsToolStripMenuItem
            // 
            this.addMultipleRowsToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.table_row_insert;
            this.addMultipleRowsToolStripMenuItem.Name = "addMultipleRowsToolStripMenuItem";
            this.addMultipleRowsToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.addMultipleRowsToolStripMenuItem.Text = "Add Multiple Rows";
            this.addMultipleRowsToolStripMenuItem.Click += new System.EventHandler(this.AddMultipleRowsToolStripMenuItem_Click);
            // 
            // editRowToolStripMenuItem
            // 
            this.editRowToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.table_edit;
            this.editRowToolStripMenuItem.Name = "editRowToolStripMenuItem";
            this.editRowToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.editRowToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.editRowToolStripMenuItem.Text = "Edit Row";
            this.editRowToolStripMenuItem.Click += new System.EventHandler(this.EditRowToolStripMenuItem_Click);
            // 
            // removeRowToolStripMenuItem
            // 
            this.removeRowToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.table_row_delete;
            this.removeRowToolStripMenuItem.Name = "removeRowToolStripMenuItem";
            this.removeRowToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.removeRowToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.removeRowToolStripMenuItem.Text = "Remove Row(s)";
            this.removeRowToolStripMenuItem.Click += new System.EventHandler(this.RemoveRowToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(194, 6);
            // 
            // showHideSidebarToolStripMenuItem
            // 
            this.showHideSidebarToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.layout_sidebar;
            this.showHideSidebarToolStripMenuItem.Name = "showHideSidebarToolStripMenuItem";
            this.showHideSidebarToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.showHideSidebarToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.showHideSidebarToolStripMenuItem.Text = "Toggle Sidebar";
            this.showHideSidebarToolStripMenuItem.Click += new System.EventHandler(this.ShowHideSidebarToolStripMenuItem_Click);
            // 
            // advancedToolStripMenuItem
            // 
            this.advancedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openLinkedTablesToolStripMenuItem,
            this.compareDatabasesToolStripMenuItem1,
            this.mergeDatabasesToolStripMenuItem,
            this.toggleConstraintsToolStripMenuItem});
            this.advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
            this.advancedToolStripMenuItem.Size = new System.Drawing.Size(72, 20);
            this.advancedToolStripMenuItem.Text = "Advanced";
            // 
            // openLinkedTablesToolStripMenuItem
            // 
            this.openLinkedTablesToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.table_link;
            this.openLinkedTablesToolStripMenuItem.Name = "openLinkedTablesToolStripMenuItem";
            this.openLinkedTablesToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.openLinkedTablesToolStripMenuItem.Text = "Open Linked Tables";
            this.openLinkedTablesToolStripMenuItem.Click += new System.EventHandler(this.OpenLinkedTablesToolStripMenuItem_Click);
            // 
            // compareDatabasesToolStripMenuItem1
            // 
            this.compareDatabasesToolStripMenuItem1.Image = global::EgoDatabaseEditor.Properties.Resources.table_relationship;
            this.compareDatabasesToolStripMenuItem1.Name = "compareDatabasesToolStripMenuItem1";
            this.compareDatabasesToolStripMenuItem1.Size = new System.Drawing.Size(179, 22);
            this.compareDatabasesToolStripMenuItem1.Text = "Compare Databases";
            this.compareDatabasesToolStripMenuItem1.Click += new System.EventHandler(this.CompareDatabasesToolStripMenuItem_Click);
            // 
            // mergeDatabasesToolStripMenuItem
            // 
            this.mergeDatabasesToolStripMenuItem.Image = global::EgoDatabaseEditor.Properties.Resources.table_multiple;
            this.mergeDatabasesToolStripMenuItem.Name = "mergeDatabasesToolStripMenuItem";
            this.mergeDatabasesToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.mergeDatabasesToolStripMenuItem.Text = "Merge Databases";
            this.mergeDatabasesToolStripMenuItem.Click += new System.EventHandler(this.MergeDatabasesToolStripMenuItem_Click);
            // 
            // toggleConstraintsToolStripMenuItem
            // 
            this.toggleConstraintsToolStripMenuItem.Name = "toggleConstraintsToolStripMenuItem";
            this.toggleConstraintsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.toggleConstraintsToolStripMenuItem.Text = "Toggle Constraints";
            this.toggleConstraintsToolStripMenuItem.Click += new System.EventHandler(this.ToggleConstraintsToolStripMenuItem_Click);
            // 
            // tmppppToolStripMenuItem
            // 
            this.tmppppToolStripMenuItem.Name = "tmppppToolStripMenuItem";
            this.tmppppToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.tmppppToolStripMenuItem.Text = "tmpppp";
            this.tmppppToolStripMenuItem.Click += new System.EventHandler(this.TmppppToolStripMenuItem_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "database.bin";
            this.openFileDialog.Filter = "BIN files|*.bin|XML files|*.xml";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileName = "database.bin";
            this.saveFileDialog.Filter = "BIN files|*.bin|XML files|*.xml";
            // 
            // leftPanel
            // 
            this.leftPanel.Controls.Add(this.tableListBox);
            this.leftPanel.Controls.Add(this.searchTextBox);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Location = new System.Drawing.Point(0, 24);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Size = new System.Drawing.Size(200, 509);
            this.leftPanel.TabIndex = 10;
            // 
            // tableListBox
            // 
            this.tableListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableListBox.FormattingEnabled = true;
            this.tableListBox.Location = new System.Drawing.Point(0, 20);
            this.tableListBox.Name = "tableListBox";
            this.tableListBox.Size = new System.Drawing.Size(200, 489);
            this.tableListBox.TabIndex = 1;
            this.tableListBox.DoubleClick += new System.EventHandler(this.TableListBox_DoubleClick);
            this.tableListBox.MouseEnter += new System.EventHandler(this.TableListBox_MouseEnter);
            // 
            // searchTextBox
            // 
            this.searchTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchTextBox.Location = new System.Drawing.Point(0, 0);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(200, 20);
            this.searchTextBox.TabIndex = 2;
            this.searchTextBox.TextChanged += new System.EventHandler(this.SearchTextBox_TextChanged);
            this.searchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchMaskedTextBox_KeyDown);
            // 
            // tabControl
            // 
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(200, 24);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(432, 509);
            this.tabControl.TabIndex = 11;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            this.tabControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TabControl_MouseDoubleClick);
            this.tabControl.MouseEnter += new System.EventHandler(this.TabControl_MouseEnter);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 533);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.mainMenu);
            this.MainMenuStrip = this.mainMenu;
            this.Name = "Form1";
            this.Text = "Ego Database Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.leftPanel.ResumeLayout(false);
            this.leftPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.ListBox tableListBox;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem importAsXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAsXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openTableToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem showHideSidebarToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editRowToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem addRowToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem removeRowToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addMultipleRowsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openPopulatedTablesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem closeAllTablesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openLinkedTablesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem compareDatabasesToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem mergeDatabasesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleConstraintsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tmppppToolStripMenuItem;
    }
}

