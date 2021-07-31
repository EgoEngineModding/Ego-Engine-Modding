using ICSharpCode.AvalonEdit.Folding;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace EgoErpArchiver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void setDirectoryF12016MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select the location of your F1 game:";
            dlg.IsFolderPicker = true;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Properties.Settings.Default.F12016Dir = dlg.FileName + "\\";
                Properties.Settings.Default.Save();
            }
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
            var selectedItem = resourcesDataGrid.SelectedItem;
            if (mainTabControl.SelectedIndex == 0 && selectedItem != null)
            {
                resourcesDataGrid.Focus();
                resourcesDataGrid.ScrollIntoView(selectedItem);
            }
        }

        private void packagesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                packagePreviewTextEditor.Text = string.Empty;
                return;
            }
            
            packagePreviewTextEditor.Text = ((ViewModel.ErpPackageViewModel)e.AddedItems[0]).Preview;
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

            xmlFilePreviewTextEditor.Text = ((ViewModel.ErpXmlFileViewModel)e.AddedItems[0]).Preview;
            foldingManager = FoldingManager.Install(xmlFilePreviewTextEditor.TextArea);
            foldingStrategy.UpdateFoldings(foldingManager, xmlFilePreviewTextEditor.Document);
        }
    }
}
