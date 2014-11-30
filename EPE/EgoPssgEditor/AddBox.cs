using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EgoEngineLibrary.Graphics;

namespace EgoPssgEditor
{
    public partial class AddBox : Form
    {
        PssgFile pssgFile;
        public int AddType
        {
            get { return tabControl.SelectedIndex; }
        }
        public string NodeName
        {
            get { return nodeInfoComboBox1.Text; }
        }
        public string AttributeName
        {
            get { return attributeInfoComboBox.Text; }
        }
        public Type ValueType
        {
            get { return Type.GetType((string)valueTypeComboBox.SelectedItem); }
        }
        public string Value
        {
            get { return valueTextBox.Text; }
        }

        public AddBox(PssgFile file, int tabIndex)
        {
            InitializeComponent();
            pssgFile = file;
            // NodeInfo Combo
            nodeInfoComboBox1.BeginUpdate();
            nodeInfoComboBox1.Items.AddRange(PssgSchema.GetNodeNames());
            nodeInfoComboBox1.EndUpdate();
            // AttributeInfo Combo
            attributeInfoComboBox.BeginUpdate();
            attributeInfoComboBox.Items.AddRange(PssgSchema.GetAttributeNames());
            attributeInfoComboBox.EndUpdate();
            // ValueType Combo
            valueTypeComboBox.Items.Add(typeof(System.UInt16).ToString());
            valueTypeComboBox.Items.Add(typeof(System.UInt32).ToString());
            valueTypeComboBox.Items.Add(typeof(System.Int16).ToString());
            valueTypeComboBox.Items.Add(typeof(System.Int32).ToString());
            valueTypeComboBox.Items.Add(typeof(System.Single).ToString());
            //valueTypeComboBox.Items.Add(typeof(System.Boolean).ToString());
            valueTypeComboBox.Items.Add(typeof(System.String).ToString());
            // Select
            nodeInfoComboBox1.SelectedIndex = 0;
            if (attributeInfoComboBox.Items.Count > 0)
            {
                attributeInfoComboBox.SelectedIndex = 0;
            }
            valueTypeComboBox.SelectedIndex = 5;

            tabControl.SelectedIndex = tabIndex;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (AddType == 0)
            {
                if (nodeInfoComboBox1.SelectedIndex == -1)
                {
                    //this.DialogResult = DialogResult.None;
                }
            }
            else
            {
                if (attributeInfoComboBox.SelectedIndex == -1 || valueTypeComboBox.SelectedIndex == -1 || valueTextBox.Text == "")
                {
                    //this.DialogResult = DialogResult.None;
                }

                try
                {
                    Convert.ChangeType(Value, ValueType);
                }
                catch
                {
                    this.DialogResult = DialogResult.None;
                }
            }
        }

        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            e.Cancel = true;
            if (e.TabPage.Name == "attributeTabPage" && attributeInfoComboBox.Items.Count == 0)
            {
                e.Cancel = true;
            }
        }
    }
}
