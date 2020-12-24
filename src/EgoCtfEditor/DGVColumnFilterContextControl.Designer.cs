namespace EgoCtfEditor
{
    partial class DGVColumnFilterContextControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.filterTextBox = new System.Windows.Forms.TextBox();
            this.deleteButton = new System.Windows.Forms.Button();
            this.addButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // filterTextBox
            // 
            this.filterTextBox.Location = new System.Drawing.Point(3, 12);
            this.filterTextBox.Name = "filterTextBox";
            this.filterTextBox.Size = new System.Drawing.Size(180, 20);
            this.filterTextBox.TabIndex = 0;
            this.filterTextBox.TextChanged += new System.EventHandler(this.filterTextBox_TextChanged);
            this.filterTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.filterTextBox_KeyDown);
            // 
            // deleteButton
            // 
            this.deleteButton.Image = global::EgoCtfEditor.Properties.Resources.delete;
            this.deleteButton.Location = new System.Drawing.Point(159, 38);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(24, 23);
            this.deleteButton.TabIndex = 2;
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // addButton
            // 
            this.addButton.Image = global::EgoCtfEditor.Properties.Resources.accept;
            this.addButton.Location = new System.Drawing.Point(3, 38);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(24, 23);
            this.addButton.TabIndex = 1;
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // DGVColumnFilterContextControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.filterTextBox);
            this.Name = "DGVColumnFilterContextControl";
            this.Size = new System.Drawing.Size(186, 70);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox filterTextBox;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button deleteButton;
    }
}
