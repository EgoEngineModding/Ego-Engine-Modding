using EgoEngineLibrary.Graphics;

using Avalonia.Controls;
using Avalonia.Interactivity;
using EgoEngineLibrary.Graphics.Pssg;

namespace EgoPssgEditor.Views
{
    /// <summary>
    /// Interaction logic for AddNodeWindow.xaml
    /// </summary>
    public partial class AddNodeWindow : Window
    {
        public AddNodeWindow()
        {
            InitializeComponent();

            // NodeInfo Combo
            nodeNameComboBox.ItemsSource = (PssgSchema.GetNodeNames());
            // Select
            nodeNameComboBox.SelectedIndex = 0;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Close(true);
        }
        
        public string NodeName
        {
            get { return nodeNameComboBox.Text; }
        }
    }
}
