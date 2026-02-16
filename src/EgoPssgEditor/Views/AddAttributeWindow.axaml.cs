using EgoEngineLibrary.Graphics;

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace EgoPssgEditor.Views
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
                Close(true);
            }
            catch
            {
                Close(null);
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
