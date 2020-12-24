using EgoErpArchiver.ViewModel;
using ICSharpCode.AvalonEdit.Folding;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            OpenBrowser("https://www.ryder25.com/modding/ego-engine/#EgoERPArchiver");
        }

        private void issuesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenBrowser("https://github.com/ptasev/Ego-Engine-Modding/issues");
        }

        private void moddingDiscordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenBrowser("https://discord.gg/5bCjMqS");
        }

        private void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTabControl.SelectedIndex == 0 && resourcesDataGrid.SelectedItem != null)
            {
                resourcesDataGrid.Focus();
                resourcesDataGrid.ScrollIntoView(resourcesDataGrid.SelectedItem);
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
