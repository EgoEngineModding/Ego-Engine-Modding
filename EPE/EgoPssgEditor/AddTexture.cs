using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EgoPssgEditor
{
    public partial class AddTexture : Form
    {
        public string TName
        {
            get { return nameTextBox.Text; }
        }

        public AddTexture(string name)
        {
            InitializeComponent();
            nameTextBox.Text = name;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (TName == "")
            {
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
