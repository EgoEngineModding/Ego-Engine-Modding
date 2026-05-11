using Avalonia.Controls;
using Avalonia.Interactivity;
using EgoEngineLibrary.Graphics.Pssg;

namespace EgoPssgEditor.Views
{
    /// <summary>
    /// Interaction logic for AddElementWindow.xaml
    /// </summary>
    public partial class AddElementWindow : Window
    {
        public AddElementWindow()
        {
            InitializeComponent();

            NameComboBox.ItemsSource = (PssgSchema.GetElementNames());
            NameComboBox.SelectedIndex = 0;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Close(true);
        }
        
        public string? ElementName
        {
            get { return NameComboBox.Text; }
        }
    }
}
