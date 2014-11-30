namespace EgoPssgEditor {
	partial class AddBox {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.attributeInfoLabel = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.nodeTabPage = new System.Windows.Forms.TabPage();
            this.nodeInfoComboBox1 = new System.Windows.Forms.ComboBox();
            this.nodeInfoLabel1 = new System.Windows.Forms.Label();
            this.attributeTabPage = new System.Windows.Forms.TabPage();
            this.valueTypeLabel = new System.Windows.Forms.Label();
            this.valueTypeComboBox = new System.Windows.Forms.ComboBox();
            this.valueTextBox = new System.Windows.Forms.TextBox();
            this.valueLabel = new System.Windows.Forms.Label();
            this.attributeInfoComboBox = new System.Windows.Forms.ComboBox();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.nodeTabPage.SuspendLayout();
            this.attributeTabPage.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // attributeInfoLabel
            // 
            this.attributeInfoLabel.AutoSize = true;
            this.attributeInfoLabel.Location = new System.Drawing.Point(8, 11);
            this.attributeInfoLabel.Name = "attributeInfoLabel";
            this.attributeInfoLabel.Size = new System.Drawing.Size(67, 13);
            this.attributeInfoLabel.TabIndex = 2;
            this.attributeInfoLabel.Text = "Attribute Info";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.nodeTabPage);
            this.tabControl.Controls.Add(this.attributeTabPage);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(407, 148);
            this.tabControl.TabIndex = 3;
            this.tabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl_Selecting);
            // 
            // nodeTabPage
            // 
            this.nodeTabPage.Controls.Add(this.nodeInfoComboBox1);
            this.nodeTabPage.Controls.Add(this.nodeInfoLabel1);
            this.nodeTabPage.Location = new System.Drawing.Point(4, 22);
            this.nodeTabPage.Name = "nodeTabPage";
            this.nodeTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.nodeTabPage.Size = new System.Drawing.Size(399, 122);
            this.nodeTabPage.TabIndex = 0;
            this.nodeTabPage.Text = "Add Node";
            this.nodeTabPage.UseVisualStyleBackColor = true;
            // 
            // nodeInfoComboBox1
            // 
            this.nodeInfoComboBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.nodeInfoComboBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.nodeInfoComboBox1.FormattingEnabled = true;
            this.nodeInfoComboBox1.Location = new System.Drawing.Point(8, 29);
            this.nodeInfoComboBox1.Name = "nodeInfoComboBox1";
            this.nodeInfoComboBox1.Size = new System.Drawing.Size(177, 21);
            this.nodeInfoComboBox1.TabIndex = 0;
            // 
            // nodeInfoLabel1
            // 
            this.nodeInfoLabel1.AutoSize = true;
            this.nodeInfoLabel1.Location = new System.Drawing.Point(6, 13);
            this.nodeInfoLabel1.Name = "nodeInfoLabel1";
            this.nodeInfoLabel1.Size = new System.Drawing.Size(57, 13);
            this.nodeInfoLabel1.TabIndex = 1;
            this.nodeInfoLabel1.Text = "Node Info:";
            // 
            // attributeTabPage
            // 
            this.attributeTabPage.Controls.Add(this.valueTypeLabel);
            this.attributeTabPage.Controls.Add(this.valueTypeComboBox);
            this.attributeTabPage.Controls.Add(this.valueTextBox);
            this.attributeTabPage.Controls.Add(this.valueLabel);
            this.attributeTabPage.Controls.Add(this.attributeInfoComboBox);
            this.attributeTabPage.Controls.Add(this.attributeInfoLabel);
            this.attributeTabPage.Location = new System.Drawing.Point(4, 22);
            this.attributeTabPage.Name = "attributeTabPage";
            this.attributeTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.attributeTabPage.Size = new System.Drawing.Size(399, 122);
            this.attributeTabPage.TabIndex = 1;
            this.attributeTabPage.Text = "Add Attribute";
            this.attributeTabPage.UseVisualStyleBackColor = true;
            // 
            // valueTypeLabel
            // 
            this.valueTypeLabel.AutoSize = true;
            this.valueTypeLabel.Location = new System.Drawing.Point(214, 11);
            this.valueTypeLabel.Name = "valueTypeLabel";
            this.valueTypeLabel.Size = new System.Drawing.Size(61, 13);
            this.valueTypeLabel.TabIndex = 7;
            this.valueTypeLabel.Text = "Value Type";
            // 
            // valueTypeComboBox
            // 
            this.valueTypeComboBox.FormattingEnabled = true;
            this.valueTypeComboBox.Location = new System.Drawing.Point(214, 27);
            this.valueTypeComboBox.Name = "valueTypeComboBox";
            this.valueTypeComboBox.Size = new System.Drawing.Size(177, 21);
            this.valueTypeComboBox.TabIndex = 6;
            // 
            // valueTextBox
            // 
            this.valueTextBox.Location = new System.Drawing.Point(8, 76);
            this.valueTextBox.Name = "valueTextBox";
            this.valueTextBox.Size = new System.Drawing.Size(177, 20);
            this.valueTextBox.TabIndex = 5;
            // 
            // valueLabel
            // 
            this.valueLabel.AutoSize = true;
            this.valueLabel.Location = new System.Drawing.Point(8, 60);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Size = new System.Drawing.Size(34, 13);
            this.valueLabel.TabIndex = 4;
            this.valueLabel.Text = "Value";
            // 
            // attributeInfoComboBox
            // 
            this.attributeInfoComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.attributeInfoComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.attributeInfoComboBox.FormattingEnabled = true;
            this.attributeInfoComboBox.Location = new System.Drawing.Point(8, 27);
            this.attributeInfoComboBox.Name = "attributeInfoComboBox";
            this.attributeInfoComboBox.Size = new System.Drawing.Size(177, 21);
            this.attributeInfoComboBox.TabIndex = 3;
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.cancelButton);
            this.buttonPanel.Controls.Add(this.okButton);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(0, 148);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(407, 47);
            this.buttonPanel.TabIndex = 4;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(218, 12);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(114, 12);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // AddBox
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(407, 195);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.buttonPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddBox";
            this.Text = "Add Box";
            this.tabControl.ResumeLayout(false);
            this.nodeTabPage.ResumeLayout(false);
            this.nodeTabPage.PerformLayout();
            this.attributeTabPage.ResumeLayout(false);
            this.attributeTabPage.PerformLayout();
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label attributeInfoLabel;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage nodeTabPage;
		private System.Windows.Forms.ComboBox nodeInfoComboBox1;
		private System.Windows.Forms.Label nodeInfoLabel1;
		private System.Windows.Forms.TabPage attributeTabPage;
		private System.Windows.Forms.Label valueTypeLabel;
		private System.Windows.Forms.ComboBox valueTypeComboBox;
		private System.Windows.Forms.TextBox valueTextBox;
		private System.Windows.Forms.Label valueLabel;
		private System.Windows.Forms.ComboBox attributeInfoComboBox;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
	}
}