using EgoPssgEditor.ViewModel;
using System;
using System.Diagnostics;
using System.Windows;

namespace EgoPssgEditor
{
    // Copy/Paste; Search; ModelViewer;
    // 1.7.2 - Quick Float4 Fix To Make work with all textures
    // 1.8 - Brand New Base PSSG Class from Miek, Export/Import CubeMaps, Remove Textures, Improved UI, Import All Textures, Better Image Preview, Brand New DDS Class
    // 2.0 - Support for texelformats "ui8x4" and "u8", Auto-Update "All Sections", Export/Import Data Nodes, Add/Remove Attributes to Nodes, Hogs Less Resources, PSSG Backend, textures search bar
    // 2.0.1 - Fixed Tag Errors on Save, Improved Open/SaveFileDialog Usability, Cleaned Up Some Code, Reduced Size from Icon
    // 10.0 -- Use EgoEngineLibrary, Added Dirt support, MainMenus, Export/Import Xml, Schema System, Move Nodes, Supports Compressed Pssg
    // 10.1 -- Added two texelTypes for Dirt Rally, Fixed bug when copying node (a new node info was created even if the same name already existed)
    // 10.2 -- Changed DDS saving to use linear size instead of pitch to make it work with Gimp 2.8
    // 10.3 -- Properly closes the save stream, Changed byte data font to Consolas, Hex byte now always 2 chars and caps, 64bit, opens/saves really large files, OLV textures
    // 11.0 -- Preview for u8 textures, Rewrote UI in WPF, Dropped CubeMap support, Added BC7 support
    // 11.1 -- Support for BC1, BC3, and srgb variants

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly MainViewModel view;
        public MainWindow()
        {
            InitializeComponent();
            view = this.DataContext as MainViewModel;
        }

        private void websiteMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://petar.page/l/ego-epe-home") { UseShellExecute = true });
        }

        private void sourceCodeMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://petar.page/l/ego-epe-code") { UseShellExecute = true });
        }
    }
}
