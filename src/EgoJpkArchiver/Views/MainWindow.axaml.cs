using Avalonia.Controls;
using Avalonia.Interactivity;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;
using EgoJpkArchiver.ViewModels;

namespace EgoJpkArchiver.Views;

public partial class MainWindow : Window
{
    public MainViewModel? ViewModel => (MainViewModel?)base.DataContext;

    public MainWindow()
    {
        InitializeComponent();
        FileDialogAvalonia.Register(this);
        MessageBoxAvalonia.Register(this);
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