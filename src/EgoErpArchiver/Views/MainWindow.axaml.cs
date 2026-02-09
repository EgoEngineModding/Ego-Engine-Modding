using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.Folding;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;

using EgoErpArchiver.Controls;
using EgoErpArchiver.Dialogs.Erp;
using EgoErpArchiver.ViewModels;

namespace EgoErpArchiver.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel? ViewModel => (MainViewModel?)base.DataContext;
        
        public MainWindow()
        {
            InitializeComponent();
            
            FileDialogAvalonia.Register(this);
            MessageBoxAvalonia.Register(this);
            ErpDialogAvalonia.Register(this);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            ViewModel?.ParseCommandLineArguments();
        }

        private void websiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(new ProcessStartInfo("https://petar.page/l/ego-eea-home") { UseShellExecute = true });
        }

        private void sourceCodeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(new ProcessStartInfo("https://petar.page/l/ego-eea-code") { UseShellExecute = true });
        }

        private void moddingDiscordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(new ProcessStartInfo("https://discord.gg/5bCjMqS") { UseShellExecute = true });
        }

        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (resourcesDataGrid is null)
            {
                return;
            }
            
            var selectedItem = resourcesDataGrid.SelectedItem;
            if (mainTabControl.SelectedIndex == 0 && selectedItem != null)
            {
                resourcesDataGrid.Focus();
                resourcesDataGrid.ScrollIntoView(selectedItem, null);
            }
        }

        FoldingManager? _braceFoldingManager;
        readonly BraceFoldingStrategy _braceFoldingStrategy = new();
        private void packagesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_braceFoldingManager != null)
            {
                FoldingManager.Uninstall(_braceFoldingManager);
                _braceFoldingManager = null;
            }

            if (e.AddedItems.Count == 0)
            {
                packagePreviewTextEditor.Text = string.Empty;
                return;
            }
            
            packagePreviewTextEditor.Text = ((ErpPackageViewModel)e.AddedItems[0]).Preview;
            _braceFoldingManager = FoldingManager.Install(packagePreviewTextEditor.TextArea);
            _braceFoldingStrategy.UpdateFoldings(_braceFoldingManager, packagePreviewTextEditor.Document);
        }

        FoldingManager? _xmlFoldingManager;
        readonly XmlFoldingStrategy _xmlFoldingStrategy = new();
        private void xmlFilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_xmlFoldingManager != null)
            {
                FoldingManager.Uninstall(_xmlFoldingManager);
                _xmlFoldingManager = null;
            }

            if (e.AddedItems.Count == 0)
            {
                xmlFilePreviewTextEditor.Text = string.Empty;
                return;
            }
            
            xmlFilePreviewTextEditor.Text = ((ErpXmlFileViewModel)e.AddedItems[0]).Preview;
            _xmlFoldingManager = FoldingManager.Install(xmlFilePreviewTextEditor.TextArea);
            _xmlFoldingStrategy.UpdateFoldings(_xmlFoldingManager, xmlFilePreviewTextEditor.Document);
        }
    }
}
