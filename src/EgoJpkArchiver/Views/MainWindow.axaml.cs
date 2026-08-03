using Avalonia.Controls;
using Avalonia.Interactivity;
using EgoJpkArchiver.ViewModels;

namespace EgoJpkArchiver.Views;

public partial class MainWindow : Window
{
    public MainViewModel? ViewModel => (MainViewModel?)base.DataContext;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (Design.IsDesignMode)
        {
            return;
        }
            
        ViewModel?.ParseCommandLineArgs();
    }
}