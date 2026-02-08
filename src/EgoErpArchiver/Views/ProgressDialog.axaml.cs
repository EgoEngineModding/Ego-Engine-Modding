using Avalonia.Controls;

namespace EgoErpArchiver.Views
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        private void statusTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            scrollViewer.ScrollToEnd();
        }
    }
}
