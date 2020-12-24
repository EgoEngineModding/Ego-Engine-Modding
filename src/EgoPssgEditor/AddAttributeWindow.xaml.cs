using EgoEngineLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EgoPssgEditor
{
    /// <summary>
    /// Interaction logic for AddAttributeWindow.xaml
    /// </summary>
    public partial class AddAttributeWindow : Window
    {
        public AddAttributeWindow()
        {
            InitializeComponent();
            // AttributeInfo Combo
            attributeNameComboBox.ItemsSource = (PssgSchema.GetAttributeNames());
            // ValueType Combo
            attributeValueTypeComboBox.Items.Add(typeof(System.UInt16).ToString());
            attributeValueTypeComboBox.Items.Add(typeof(System.UInt32).ToString());
            attributeValueTypeComboBox.Items.Add(typeof(System.Int16).ToString());
            attributeValueTypeComboBox.Items.Add(typeof(System.Int32).ToString());
            attributeValueTypeComboBox.Items.Add(typeof(System.Single).ToString());
            //attributeValueTypeComboBox.Items.Add(typeof(System.Boolean).ToString());
            attributeValueTypeComboBox.Items.Add(typeof(System.String).ToString());
            // Select
            if (attributeNameComboBox.Items.Count > 0)
            {
                attributeNameComboBox.SelectedIndex = 0;
            }
            attributeValueTypeComboBox.SelectedIndex = 5;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Convert.ChangeType(Value, AttributeValueType);
                this.DialogResult = true;
            }
            catch
            {
                this.DialogResult = null;
            }
        }

        public string AttributeName
        {
            get { return attributeNameComboBox.Text; }
        }
        public Type AttributeValueType
        {
            get { return Type.GetType((string)attributeValueTypeComboBox.SelectedItem); }
        }
        public string Value
        {
            get { return attributeValueTextBox.Text; }
        }
    }
}
