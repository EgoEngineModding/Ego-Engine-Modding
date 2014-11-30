namespace EgoLngEditor
{
    partial class FindAndReplaceForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.FindPage = new System.Windows.Forms.TabPage();
            this.FindButton1 = new System.Windows.Forms.Button();
            this.FindOptionGroupBox1 = new System.Windows.Forms.GroupBox();
            this.UseComboBox1 = new System.Windows.Forms.ComboBox();
            this.UseCheckBox1 = new System.Windows.Forms.CheckBox();
            this.SearchUpCheckBox1 = new System.Windows.Forms.CheckBox();
            this.MatchCellCheckBox1 = new System.Windows.Forms.CheckBox();
            this.MatchCaseCheckBox1 = new System.Windows.Forms.CheckBox();
            this.LookInComboBox1 = new System.Windows.Forms.ComboBox();
            this.LookInLabel1 = new System.Windows.Forms.Label();
            this.FindWhatTextBox1 = new System.Windows.Forms.TextBox();
            this.FindLabel1 = new System.Windows.Forms.Label();
            this.ReplacePage = new System.Windows.Forms.TabPage();
            this.ReplaceButton = new System.Windows.Forms.Button();
            this.ReplaceAllButton = new System.Windows.Forms.Button();
            this.FindButton2 = new System.Windows.Forms.Button();
            this.FindOptionGroup2 = new System.Windows.Forms.GroupBox();
            this.UseComboBox2 = new System.Windows.Forms.ComboBox();
            this.UseCheckBox2 = new System.Windows.Forms.CheckBox();
            this.SearchUpCheckBox2 = new System.Windows.Forms.CheckBox();
            this.MatchCellCheckBox2 = new System.Windows.Forms.CheckBox();
            this.MatchCaseCheckBox2 = new System.Windows.Forms.CheckBox();
            this.LookInComboBox2 = new System.Windows.Forms.ComboBox();
            this.LookInLabel2 = new System.Windows.Forms.Label();
            this.ReplaceWithTextBox = new System.Windows.Forms.TextBox();
            this.ReplaceLabel = new System.Windows.Forms.Label();
            this.FindWhatTextBox2 = new System.Windows.Forms.TextBox();
            this.FindLabel2 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.FindPage.SuspendLayout();
            this.FindOptionGroupBox1.SuspendLayout();
            this.ReplacePage.SuspendLayout();
            this.FindOptionGroup2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.FindPage);
            this.tabControl1.Controls.Add(this.ReplacePage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(356, 298);
            this.tabControl1.TabIndex = 0;
            // 
            // FindPage
            // 
            this.FindPage.BackColor = System.Drawing.Color.White;
            this.FindPage.Controls.Add(this.FindButton1);
            this.FindPage.Controls.Add(this.FindOptionGroupBox1);
            this.FindPage.Controls.Add(this.LookInComboBox1);
            this.FindPage.Controls.Add(this.LookInLabel1);
            this.FindPage.Controls.Add(this.FindWhatTextBox1);
            this.FindPage.Controls.Add(this.FindLabel1);
            this.FindPage.Location = new System.Drawing.Point(4, 22);
            this.FindPage.Name = "FindPage";
            this.FindPage.Padding = new System.Windows.Forms.Padding(3);
            this.FindPage.Size = new System.Drawing.Size(348, 272);
            this.FindPage.TabIndex = 0;
            this.FindPage.Text = "Find";
            // 
            // FindButton1
            // 
            this.FindButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FindButton1.Location = new System.Drawing.Point(265, 213);
            this.FindButton1.Name = "FindButton1";
            this.FindButton1.Size = new System.Drawing.Size(75, 23);
            this.FindButton1.TabIndex = 11;
            this.FindButton1.Text = "Find Next";
            this.FindButton1.UseVisualStyleBackColor = true;
            this.FindButton1.Click += new System.EventHandler(this.FindButton1_Click);
            // 
            // FindOptionGroupBox1
            // 
            this.FindOptionGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FindOptionGroupBox1.Controls.Add(this.UseComboBox1);
            this.FindOptionGroupBox1.Controls.Add(this.UseCheckBox1);
            this.FindOptionGroupBox1.Controls.Add(this.SearchUpCheckBox1);
            this.FindOptionGroupBox1.Controls.Add(this.MatchCellCheckBox1);
            this.FindOptionGroupBox1.Controls.Add(this.MatchCaseCheckBox1);
            this.FindOptionGroupBox1.Location = new System.Drawing.Point(6, 86);
            this.FindOptionGroupBox1.Name = "FindOptionGroupBox1";
            this.FindOptionGroupBox1.Size = new System.Drawing.Size(334, 122);
            this.FindOptionGroupBox1.TabIndex = 10;
            this.FindOptionGroupBox1.TabStop = false;
            this.FindOptionGroupBox1.Text = "Find options";
            // 
            // UseComboBox1
            // 
            this.UseComboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UseComboBox1.Enabled = false;
            this.UseComboBox1.FormattingEnabled = true;
            this.UseComboBox1.Items.AddRange(new object[] {
            "Regular expressions",
            "Wildcards"});
            this.UseComboBox1.Location = new System.Drawing.Point(35, 91);
            this.UseComboBox1.Name = "UseComboBox1";
            this.UseComboBox1.Size = new System.Drawing.Size(293, 21);
            this.UseComboBox1.TabIndex = 4;
            this.UseComboBox1.SelectedIndexChanged += new System.EventHandler(this.UseComboBox1_SelectedIndexChanged);
            // 
            // UseCheckBox1
            // 
            this.UseCheckBox1.AutoSize = true;
            this.UseCheckBox1.Location = new System.Drawing.Point(16, 75);
            this.UseCheckBox1.Name = "UseCheckBox1";
            this.UseCheckBox1.Size = new System.Drawing.Size(48, 17);
            this.UseCheckBox1.TabIndex = 3;
            this.UseCheckBox1.Text = "Use:";
            this.UseCheckBox1.UseVisualStyleBackColor = true;
            this.UseCheckBox1.CheckedChanged += new System.EventHandler(this.UseCheckBox1_CheckedChanged);
            // 
            // SearchUpCheckBox1
            // 
            this.SearchUpCheckBox1.AutoSize = true;
            this.SearchUpCheckBox1.Location = new System.Drawing.Point(16, 56);
            this.SearchUpCheckBox1.Name = "SearchUpCheckBox1";
            this.SearchUpCheckBox1.Size = new System.Drawing.Size(75, 17);
            this.SearchUpCheckBox1.TabIndex = 2;
            this.SearchUpCheckBox1.Text = "Search up";
            this.SearchUpCheckBox1.UseVisualStyleBackColor = true;
            this.SearchUpCheckBox1.CheckedChanged += new System.EventHandler(this.SearchUpCheckBox1_CheckedChanged);
            // 
            // MatchCellCheckBox1
            // 
            this.MatchCellCheckBox1.AutoSize = true;
            this.MatchCellCheckBox1.Location = new System.Drawing.Point(16, 38);
            this.MatchCellCheckBox1.Name = "MatchCellCheckBox1";
            this.MatchCellCheckBox1.Size = new System.Drawing.Size(106, 17);
            this.MatchCellCheckBox1.TabIndex = 1;
            this.MatchCellCheckBox1.Text = "Match whole cell";
            this.MatchCellCheckBox1.UseVisualStyleBackColor = true;
            this.MatchCellCheckBox1.CheckedChanged += new System.EventHandler(this.MatchCellCheckBox1_CheckedChanged);
            // 
            // MatchCaseCheckBox1
            // 
            this.MatchCaseCheckBox1.AutoSize = true;
            this.MatchCaseCheckBox1.Location = new System.Drawing.Point(16, 19);
            this.MatchCaseCheckBox1.Name = "MatchCaseCheckBox1";
            this.MatchCaseCheckBox1.Size = new System.Drawing.Size(82, 17);
            this.MatchCaseCheckBox1.TabIndex = 0;
            this.MatchCaseCheckBox1.Text = "Match case";
            this.MatchCaseCheckBox1.UseVisualStyleBackColor = true;
            this.MatchCaseCheckBox1.CheckedChanged += new System.EventHandler(this.MatchCaseCheckBox1_CheckedChanged);
            // 
            // LookInComboBox1
            // 
            this.LookInComboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LookInComboBox1.FormattingEnabled = true;
            this.LookInComboBox1.Items.AddRange(new object[] {
            "Current Table",
            "Selection",
            "Current Column"});
            this.LookInComboBox1.Location = new System.Drawing.Point(83, 58);
            this.LookInComboBox1.Name = "LookInComboBox1";
            this.LookInComboBox1.Size = new System.Drawing.Size(257, 21);
            this.LookInComboBox1.TabIndex = 9;
            this.LookInComboBox1.SelectedIndexChanged += new System.EventHandler(this.LookInComboBox1_SelectedIndexChanged);
            // 
            // LookInLabel1
            // 
            this.LookInLabel1.AutoSize = true;
            this.LookInLabel1.Location = new System.Drawing.Point(2, 60);
            this.LookInLabel1.Name = "LookInLabel1";
            this.LookInLabel1.Size = new System.Drawing.Size(45, 13);
            this.LookInLabel1.TabIndex = 8;
            this.LookInLabel1.Text = "Look in:";
            // 
            // FindWhatTextBox1
            // 
            this.FindWhatTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FindWhatTextBox1.Location = new System.Drawing.Point(83, 6);
            this.FindWhatTextBox1.Name = "FindWhatTextBox1";
            this.FindWhatTextBox1.Size = new System.Drawing.Size(257, 20);
            this.FindWhatTextBox1.TabIndex = 5;
            this.FindWhatTextBox1.TextChanged += new System.EventHandler(this.FindWhatTextBox1_TextChanged);
            // 
            // FindLabel1
            // 
            this.FindLabel1.AutoSize = true;
            this.FindLabel1.Location = new System.Drawing.Point(2, 10);
            this.FindLabel1.Name = "FindLabel1";
            this.FindLabel1.Size = new System.Drawing.Size(59, 13);
            this.FindLabel1.TabIndex = 4;
            this.FindLabel1.Text = "Find What:";
            // 
            // ReplacePage
            // 
            this.ReplacePage.Controls.Add(this.ReplaceButton);
            this.ReplacePage.Controls.Add(this.ReplaceAllButton);
            this.ReplacePage.Controls.Add(this.FindButton2);
            this.ReplacePage.Controls.Add(this.FindOptionGroup2);
            this.ReplacePage.Controls.Add(this.LookInComboBox2);
            this.ReplacePage.Controls.Add(this.LookInLabel2);
            this.ReplacePage.Controls.Add(this.ReplaceWithTextBox);
            this.ReplacePage.Controls.Add(this.ReplaceLabel);
            this.ReplacePage.Controls.Add(this.FindWhatTextBox2);
            this.ReplacePage.Controls.Add(this.FindLabel2);
            this.ReplacePage.Location = new System.Drawing.Point(4, 22);
            this.ReplacePage.Name = "ReplacePage";
            this.ReplacePage.Padding = new System.Windows.Forms.Padding(3);
            this.ReplacePage.Size = new System.Drawing.Size(348, 272);
            this.ReplacePage.TabIndex = 1;
            this.ReplacePage.Text = "Replace";
            this.ReplacePage.UseVisualStyleBackColor = true;
            // 
            // ReplaceButton
            // 
            this.ReplaceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ReplaceButton.Location = new System.Drawing.Point(184, 241);
            this.ReplaceButton.Name = "ReplaceButton";
            this.ReplaceButton.Size = new System.Drawing.Size(75, 23);
            this.ReplaceButton.TabIndex = 14;
            this.ReplaceButton.Text = "Replace";
            this.ReplaceButton.UseVisualStyleBackColor = true;
            this.ReplaceButton.Click += new System.EventHandler(this.ReplaceButton_Click);
            // 
            // ReplaceAllButton
            // 
            this.ReplaceAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ReplaceAllButton.Location = new System.Drawing.Point(265, 241);
            this.ReplaceAllButton.Name = "ReplaceAllButton";
            this.ReplaceAllButton.Size = new System.Drawing.Size(75, 23);
            this.ReplaceAllButton.TabIndex = 13;
            this.ReplaceAllButton.Text = "Replace All";
            this.ReplaceAllButton.UseVisualStyleBackColor = true;
            this.ReplaceAllButton.Click += new System.EventHandler(this.ReplaceAllButton_Click);
            // 
            // FindButton2
            // 
            this.FindButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FindButton2.Location = new System.Drawing.Point(265, 213);
            this.FindButton2.Name = "FindButton2";
            this.FindButton2.Size = new System.Drawing.Size(75, 23);
            this.FindButton2.TabIndex = 12;
            this.FindButton2.Text = "Find Next";
            this.FindButton2.UseVisualStyleBackColor = true;
            this.FindButton2.Click += new System.EventHandler(this.FindButton2_Click);
            // 
            // FindOptionGroup2
            // 
            this.FindOptionGroup2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FindOptionGroup2.Controls.Add(this.UseComboBox2);
            this.FindOptionGroup2.Controls.Add(this.UseCheckBox2);
            this.FindOptionGroup2.Controls.Add(this.SearchUpCheckBox2);
            this.FindOptionGroup2.Controls.Add(this.MatchCellCheckBox2);
            this.FindOptionGroup2.Controls.Add(this.MatchCaseCheckBox2);
            this.FindOptionGroup2.Location = new System.Drawing.Point(6, 86);
            this.FindOptionGroup2.Name = "FindOptionGroup2";
            this.FindOptionGroup2.Size = new System.Drawing.Size(334, 122);
            this.FindOptionGroup2.TabIndex = 8;
            this.FindOptionGroup2.TabStop = false;
            this.FindOptionGroup2.Text = "Find options";
            // 
            // UseComboBox2
            // 
            this.UseComboBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UseComboBox2.Enabled = false;
            this.UseComboBox2.FormattingEnabled = true;
            this.UseComboBox2.Items.AddRange(new object[] {
            "Regular expressions",
            "Wildcards"});
            this.UseComboBox2.Location = new System.Drawing.Point(35, 91);
            this.UseComboBox2.Name = "UseComboBox2";
            this.UseComboBox2.Size = new System.Drawing.Size(293, 21);
            this.UseComboBox2.TabIndex = 4;
            this.UseComboBox2.SelectedIndexChanged += new System.EventHandler(this.UseComboBox2_SelectedIndexChanged);
            // 
            // UseCheckBox2
            // 
            this.UseCheckBox2.AutoSize = true;
            this.UseCheckBox2.Location = new System.Drawing.Point(16, 75);
            this.UseCheckBox2.Name = "UseCheckBox2";
            this.UseCheckBox2.Size = new System.Drawing.Size(48, 17);
            this.UseCheckBox2.TabIndex = 3;
            this.UseCheckBox2.Text = "Use:";
            this.UseCheckBox2.UseVisualStyleBackColor = true;
            this.UseCheckBox2.CheckedChanged += new System.EventHandler(this.UseCheckBox2_CheckedChanged);
            // 
            // SearchUpCheckBox2
            // 
            this.SearchUpCheckBox2.AutoSize = true;
            this.SearchUpCheckBox2.Location = new System.Drawing.Point(16, 56);
            this.SearchUpCheckBox2.Name = "SearchUpCheckBox2";
            this.SearchUpCheckBox2.Size = new System.Drawing.Size(75, 17);
            this.SearchUpCheckBox2.TabIndex = 2;
            this.SearchUpCheckBox2.Text = "Search up";
            this.SearchUpCheckBox2.UseVisualStyleBackColor = true;
            this.SearchUpCheckBox2.CheckedChanged += new System.EventHandler(this.SearchUpCheckBox2_CheckedChanged);
            // 
            // MatchCellCheckBox2
            // 
            this.MatchCellCheckBox2.AutoSize = true;
            this.MatchCellCheckBox2.Location = new System.Drawing.Point(16, 38);
            this.MatchCellCheckBox2.Name = "MatchCellCheckBox2";
            this.MatchCellCheckBox2.Size = new System.Drawing.Size(106, 17);
            this.MatchCellCheckBox2.TabIndex = 1;
            this.MatchCellCheckBox2.Text = "Match whole cell";
            this.MatchCellCheckBox2.UseVisualStyleBackColor = true;
            this.MatchCellCheckBox2.CheckedChanged += new System.EventHandler(this.MatchCellCheckBox2_CheckedChanged);
            // 
            // MatchCaseCheckBox2
            // 
            this.MatchCaseCheckBox2.AutoSize = true;
            this.MatchCaseCheckBox2.Location = new System.Drawing.Point(16, 19);
            this.MatchCaseCheckBox2.Name = "MatchCaseCheckBox2";
            this.MatchCaseCheckBox2.Size = new System.Drawing.Size(82, 17);
            this.MatchCaseCheckBox2.TabIndex = 0;
            this.MatchCaseCheckBox2.Text = "Match case";
            this.MatchCaseCheckBox2.UseVisualStyleBackColor = true;
            this.MatchCaseCheckBox2.CheckedChanged += new System.EventHandler(this.MatchCaseCheckBox2_CheckedChanged);
            // 
            // LookInComboBox2
            // 
            this.LookInComboBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LookInComboBox2.FormattingEnabled = true;
            this.LookInComboBox2.Items.AddRange(new object[] {
            "Current Table",
            "Selection",
            "Current Column"});
            this.LookInComboBox2.Location = new System.Drawing.Point(83, 58);
            this.LookInComboBox2.Name = "LookInComboBox2";
            this.LookInComboBox2.Size = new System.Drawing.Size(257, 21);
            this.LookInComboBox2.TabIndex = 7;
            this.LookInComboBox2.SelectedIndexChanged += new System.EventHandler(this.LookInComboBox2_SelectedIndexChanged);
            // 
            // LookInLabel2
            // 
            this.LookInLabel2.AutoSize = true;
            this.LookInLabel2.Location = new System.Drawing.Point(2, 60);
            this.LookInLabel2.Name = "LookInLabel2";
            this.LookInLabel2.Size = new System.Drawing.Size(45, 13);
            this.LookInLabel2.TabIndex = 6;
            this.LookInLabel2.Text = "Look in:";
            // 
            // ReplaceWithTextBox
            // 
            this.ReplaceWithTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReplaceWithTextBox.Location = new System.Drawing.Point(83, 32);
            this.ReplaceWithTextBox.Name = "ReplaceWithTextBox";
            this.ReplaceWithTextBox.Size = new System.Drawing.Size(257, 20);
            this.ReplaceWithTextBox.TabIndex = 5;
            // 
            // ReplaceLabel
            // 
            this.ReplaceLabel.AutoSize = true;
            this.ReplaceLabel.Location = new System.Drawing.Point(2, 36);
            this.ReplaceLabel.Name = "ReplaceLabel";
            this.ReplaceLabel.Size = new System.Drawing.Size(75, 13);
            this.ReplaceLabel.TabIndex = 4;
            this.ReplaceLabel.Text = "Replace With:";
            // 
            // FindWhatTextBox2
            // 
            this.FindWhatTextBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FindWhatTextBox2.Location = new System.Drawing.Point(83, 6);
            this.FindWhatTextBox2.Name = "FindWhatTextBox2";
            this.FindWhatTextBox2.Size = new System.Drawing.Size(257, 20);
            this.FindWhatTextBox2.TabIndex = 3;
            this.FindWhatTextBox2.TextChanged += new System.EventHandler(this.FindWhatTextBox2_TextChanged);
            // 
            // FindLabel2
            // 
            this.FindLabel2.AutoSize = true;
            this.FindLabel2.Location = new System.Drawing.Point(2, 10);
            this.FindLabel2.Name = "FindLabel2";
            this.FindLabel2.Size = new System.Drawing.Size(59, 13);
            this.FindLabel2.TabIndex = 2;
            this.FindLabel2.Text = "Find What:";
            // 
            // FindAndReplaceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 298);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindAndReplaceForm";
            this.ShowIcon = false;
            this.Text = " Find and Replace";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FindAndReplaceForm_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.FindPage.ResumeLayout(false);
            this.FindPage.PerformLayout();
            this.FindOptionGroupBox1.ResumeLayout(false);
            this.FindOptionGroupBox1.PerformLayout();
            this.ReplacePage.ResumeLayout(false);
            this.ReplacePage.PerformLayout();
            this.FindOptionGroup2.ResumeLayout(false);
            this.FindOptionGroup2.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage FindPage;
        private System.Windows.Forms.TabPage ReplacePage;
        private System.Windows.Forms.TextBox FindWhatTextBox1;
        private System.Windows.Forms.Label FindLabel1;
        private System.Windows.Forms.ComboBox LookInComboBox1;
        private System.Windows.Forms.Label LookInLabel1;
        private System.Windows.Forms.GroupBox FindOptionGroup2;
        private System.Windows.Forms.CheckBox MatchCellCheckBox2;
        private System.Windows.Forms.CheckBox MatchCaseCheckBox2;
        private System.Windows.Forms.ComboBox LookInComboBox2;
        private System.Windows.Forms.Label LookInLabel2;
        private System.Windows.Forms.TextBox ReplaceWithTextBox;
        private System.Windows.Forms.Label ReplaceLabel;
        private System.Windows.Forms.TextBox FindWhatTextBox2;
        private System.Windows.Forms.Label FindLabel2;
        private System.Windows.Forms.CheckBox SearchUpCheckBox2;
        private System.Windows.Forms.ComboBox UseComboBox2;
        private System.Windows.Forms.CheckBox UseCheckBox2;
        private System.Windows.Forms.GroupBox FindOptionGroupBox1;
        private System.Windows.Forms.ComboBox UseComboBox1;
        private System.Windows.Forms.CheckBox UseCheckBox1;
        private System.Windows.Forms.CheckBox SearchUpCheckBox1;
        private System.Windows.Forms.CheckBox MatchCellCheckBox1;
        private System.Windows.Forms.CheckBox MatchCaseCheckBox1;
        private System.Windows.Forms.Button FindButton1;
        private System.Windows.Forms.Button FindButton2;
        private System.Windows.Forms.Button ReplaceButton;
        private System.Windows.Forms.Button ReplaceAllButton;
    }
}