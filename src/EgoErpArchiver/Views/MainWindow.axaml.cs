using System.Diagnostics;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using AvaloniaEdit.Folding;

using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;

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

        private async void setDirectoryF12016MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
            {
                return;
            }

            var options = new FolderPickerOpenOptions
            {
                Title = "Select the location of your game:",
                AllowMultiple = false,
            };

            var res = await topLevel.StorageProvider.OpenFolderPickerAsync(options);
            if (res.Count <= 0)
            {
                return;
            }

            Properties.Settings.Default.F12016Dir = res[0].Path.LocalPath + Path.DirectorySeparatorChar;
            Properties.Settings.Default.Save();
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.StartingTab == mainTabControl.Items.Count - 1)
                return;

            ++Properties.Settings.Default.StartingTab;
            Properties.Settings.Default.Save();
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (((int)Properties.Settings.Default["StartingTab"]) == 0)
                return;

            --Properties.Settings.Default.StartingTab;
            Properties.Settings.Default.Save();
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

        private void packagesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                packagePreviewTextEditor.Text = string.Empty;
                return;
            }
            
            packagePreviewTextEditor.Text = ((ErpPackageViewModel)e.AddedItems[0]).Preview;
        }

        FoldingManager foldingManager;
        XmlFoldingStrategy foldingStrategy = new XmlFoldingStrategy();
        private void xmlFilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (foldingManager != null)
            {
                FoldingManager.Uninstall(foldingManager);
                foldingManager = null;
            }

            if (e.AddedItems.Count == 0)
            {
                xmlFilePreviewTextEditor.Text = string.Empty;
                return;
            }

            xmlFilePreviewTextEditor.Text = ((ErpXmlFileViewModel)e.AddedItems[0]).Preview;
            foldingManager = FoldingManager.Install(xmlFilePreviewTextEditor.TextArea);
            foldingStrategy.UpdateFoldings(foldingManager, xmlFilePreviewTextEditor.Document);
        }
    }
}
