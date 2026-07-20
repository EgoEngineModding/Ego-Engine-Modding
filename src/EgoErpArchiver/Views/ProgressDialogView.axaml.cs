using Avalonia.Controls;

namespace EgoErpArchiver.Views;

public partial class ProgressDialogView : UserControl
{
    public ProgressDialogView()
    {
        InitializeComponent();
    }

    private void statusTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ScrollViewer.ScrollToEnd();
    }
}