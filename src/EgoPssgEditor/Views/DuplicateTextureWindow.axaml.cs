using Avalonia.Controls;
using Avalonia.Interactivity;

namespace EgoPssgEditor.Views
{
    /// <summary>
    /// Interaction logic for DuplicateTextureWindow.xaml
    /// </summary>
    public partial class DuplicateTextureWindow : Window
    {
        public DuplicateTextureWindow()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextureName) || string.IsNullOrWhiteSpace(TextureName))
            {
                Close(null);
            }
            else
            {
                Close(true);
            }
        }
        
        public string TextureName
        {
            get { return textureNameTextBox.Text; }
            set { textureNameTextBox.Text = value; }
        }
    }
}
