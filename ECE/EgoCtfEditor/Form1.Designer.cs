namespace EgoCtfEditor
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
            this.saveAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showHideDifferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.thanksToMiekForFiguringOutTheSchemaWithoutWhichThisWouldNotBePossibleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.infoToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.dirtTabPage = new System.Windows.Forms.TabPage();
            this.gridTabPage = new System.Windows.Forms.TabPage();
            this.f12012TabPage = new System.Windows.Forms.TabPage();
            this.grid2TabPage = new System.Windows.Forms.TabPage();
            this.mainMenuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.mainTabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.thanksToMiekForFiguringOutTheSchemaWithoutWhichThisWouldNotBePossibleToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(757, 24);
            this.mainMenuStrip.TabIndex = 0;
            this.mainMenuStrip.Text = "Main Menu";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAllToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.closeAllToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::EgoCtfEditor.Properties.Resources.car_add;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = global::EgoCtfEditor.Properties.Resources.disk;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAllToolStripMenuItem
            // 
            this.saveAllToolStripMenuItem.Name = "saveAllToolStripMenuItem";
            this.saveAllToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.saveAllToolStripMenuItem.Text = "Save All";
            this.saveAllToolStripMenuItem.Click += new System.EventHandler(this.saveAllToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Image = global::EgoCtfEditor.Properties.Resources.car_delete;
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // closeAllToolStripMenuItem
            // 
            this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
            this.closeAllToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.closeAllToolStripMenuItem.Text = "Close All";
            this.closeAllToolStripMenuItem.Click += new System.EventHandler(this.closeAllToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showHideDifferencesToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // showHideDifferencesToolStripMenuItem
            // 
            this.showHideDifferencesToolStripMenuItem.Image = global::EgoCtfEditor.Properties.Resources.script_code_red;
            this.showHideDifferencesToolStripMenuItem.Name = "showHideDifferencesToolStripMenuItem";
            this.showHideDifferencesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.showHideDifferencesToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.showHideDifferencesToolStripMenuItem.Text = "Toggle Differences";
            this.showHideDifferencesToolStripMenuItem.Click += new System.EventHandler(this.showHideDifferencesToolStripMenuItem_Click);
            // 
            // thanksToMiekForFiguringOutTheSchemaWithoutWhichThisWouldNotBePossibleToolStripMenuItem
            // 
            this.thanksToMiekForFiguringOutTheSchemaWithoutWhichThisWouldNotBePossibleToolStripMenuItem.Enabled = false;
            this.thanksToMiekForFiguringOutTheSchemaWithoutWhichThisWouldNotBePossibleToolStripMenuItem.Name = "thanksToMiekForFiguringOutTheSchemaWithoutWhichThisWouldNotBePossibleToolStripMen" +
    "uItem";
            this.thanksToMiekForFiguringOutTheSchemaWithoutWhichThisWouldNotBePossibleToolStripMenuItem.Size = new System.Drawing.Size(284, 20);
            this.thanksToMiekForFiguringOutTheSchemaWithoutWhichThisWouldNotBePossibleToolStripMenuItem.Text = "Special Thanks to Miek for making all this possible";
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.infoToolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 600);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(757, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "Status";
            // 
            // infoToolStripStatusLabel
            // 
            this.infoToolStripStatusLabel.Name = "infoToolStripStatusLabel";
            this.infoToolStripStatusLabel.Size = new System.Drawing.Size(39, 17);
            this.infoToolStripStatusLabel.Text = "Ready";
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.dirtTabPage);
            this.mainTabControl.Controls.Add(this.gridTabPage);
            this.mainTabControl.Controls.Add(this.f12012TabPage);
            this.mainTabControl.Controls.Add(this.grid2TabPage);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(0, 24);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(757, 576);
            this.mainTabControl.TabIndex = 2;
            this.mainTabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.mainTabControl_Selecting);
            this.mainTabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.mainTabControl_Selected);
            // 
            // dirtTabPage
            // 
            this.dirtTabPage.Location = new System.Drawing.Point(4, 22);
            this.dirtTabPage.Name = "dirtTabPage";
            this.dirtTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.dirtTabPage.Size = new System.Drawing.Size(749, 550);
            this.dirtTabPage.TabIndex = 7;
            this.dirtTabPage.Text = "Dirt";
            this.dirtTabPage.UseVisualStyleBackColor = true;
            // 
            // gridTabPage
            // 
            this.gridTabPage.Location = new System.Drawing.Point(4, 22);
            this.gridTabPage.Name = "gridTabPage";
            this.gridTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.gridTabPage.Size = new System.Drawing.Size(749, 550);
            this.gridTabPage.TabIndex = 6;
            this.gridTabPage.Text = "Grid";
            this.gridTabPage.UseVisualStyleBackColor = true;
            // 
            // f12012TabPage
            // 
            this.f12012TabPage.Location = new System.Drawing.Point(4, 22);
            this.f12012TabPage.Name = "f12012TabPage";
            this.f12012TabPage.Padding = new System.Windows.Forms.Padding(3);
            this.f12012TabPage.Size = new System.Drawing.Size(749, 550);
            this.f12012TabPage.TabIndex = 5;
            this.f12012TabPage.Text = "Formula 1";
            this.f12012TabPage.UseVisualStyleBackColor = true;
            // 
            // grid2TabPage
            // 
            this.grid2TabPage.Location = new System.Drawing.Point(4, 22);
            this.grid2TabPage.Name = "grid2TabPage";
            this.grid2TabPage.Padding = new System.Windows.Forms.Padding(3);
            this.grid2TabPage.Size = new System.Drawing.Size(749, 550);
            this.grid2TabPage.TabIndex = 8;
            this.grid2TabPage.Text = "Other";
            this.grid2TabPage.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(757, 622);
            this.Controls.Add(this.mainTabControl);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.mainMenuStrip);
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "Form1";
            this.Text = "Ego CTF Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.mainTabControl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel infoToolStripStatusLabel;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem thanksToMiekForFiguringOutTheSchemaWithoutWhichThisWouldNotBePossibleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showHideDifferencesToolStripMenuItem;
        private System.Windows.Forms.TabPage f12012TabPage;
        private System.Windows.Forms.TabPage gridTabPage;
        private System.Windows.Forms.TabPage dirtTabPage;
        private System.Windows.Forms.TabPage grid2TabPage;
        private System.Windows.Forms.ToolStripMenuItem saveAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllToolStripMenuItem;
    }
}

