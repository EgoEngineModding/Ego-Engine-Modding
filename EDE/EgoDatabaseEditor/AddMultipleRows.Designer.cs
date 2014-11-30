namespace EgoDatabaseEditor {
	partial class AddMultipleRows {
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
			this.enterLabel = new System.Windows.Forms.Label();
			this.amountTextBox = new System.Windows.Forms.TextBox();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// enterLabel
			// 
			this.enterLabel.AutoSize = true;
			this.enterLabel.Location = new System.Drawing.Point(13, 13);
			this.enterLabel.Name = "enterLabel";
			this.enterLabel.Size = new System.Drawing.Size(231, 13);
			this.enterLabel.TabIndex = 0;
			this.enterLabel.Text = "Enter the number of rows you would like to add:";
			// 
			// amountTextBox
			// 
			this.amountTextBox.Location = new System.Drawing.Point(16, 29);
			this.amountTextBox.Name = "amountTextBox";
			this.amountTextBox.Size = new System.Drawing.Size(228, 20);
			this.amountTextBox.TabIndex = 1;
			this.amountTextBox.Text = "5";
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(51, 55);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(132, 55);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// AddMultipleRows
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(258, 85);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.amountTextBox);
			this.Controls.Add(this.enterLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddMultipleRows";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Add Rows";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label enterLabel;
		private System.Windows.Forms.TextBox amountTextBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
	}
}