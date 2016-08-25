using EgoEngineLibrary.Graphics;
using EgoPssgEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    // 11.0 -- Preview for u8 textures, Rewrote UI in WPF, Dropped CubeMap support

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

        private void websiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.petartasev.com/modding/ego-engine/pssg-editor/");
        }

        //private void termpToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    // Dirt 2 to Grid
        //    foreach (PssgNode mpjn in pssg.FindNodes("MATRIXPALETTEJOINTNODE"))
        //    {
        //        PssgAttribute jointId = new PssgAttribute(null, null, null, null);
        //        foreach (PssgNode mpjri in mpjn.FindNodes("MATRIXPALETTEJOINTRENDERINSTANCE"))
        //        {
        //            mpjri.Rename("MATRIXPALETTERENDERINSTANCE");
        //            jointId = mpjri.GetAttribute("jointID");
        //            mpjri.RemoveAttribute(jointId.Name);
        //        }
        //        mpjn.AddAttribute(jointId.Name, jointId.Value);
        //    }

        //    foreach (PssgNode mpn in pssg.FindNodes("MATRIXPALETTENODE"))
        //    {
        //        PssgAttribute jointCount = new PssgAttribute(null, null, null, null);
        //        UInt32 jointCountNum = 0;
        //        List<PssgNode> mpriNodes = mpn.FindNodes("MATRIXPALETTERENDERINSTANCE");
        //        foreach (PssgNode mpri in mpriNodes)
        //        {
        //            mpri.Rename("RENDERSTREAMINSTANCE");
        //            jointCountNum += (UInt32)mpri.GetAttribute("jointCount").Value;
        //            mpri.RemoveAttribute("jointCount");
        //            foreach (PssgNode mpsj in mpri.FindNodes("MATRIXPALETTESKINJOINT"))
        //            {
        //                pssg.MoveNode(mpsj, mpn);
        //            }
        //        }
        //        mpn.AddAttribute("jointCount", jointCountNum);
        //    }

        //    // Stuff below may not be needed
        //    foreach (PssgNode rds in pssg.FindNodes("RENDERDATASOURCE"))
        //    {
        //        rds[3].GetAttribute("subStream").Value = 3u;
        //        rds[3].GetAttribute("dataBlock").Value = rds[4].GetAttribute("dataBlock").Value;
        //        PssgNode dBlock1 = pssg.FindNodes("DATABLOCK", "id", rds[2].GetAttribute("dataBlock").ToString().Substring(1))[0];
        //        PssgNode dBlock2 = pssg.FindNodes("DATABLOCK", "id", rds[4].GetAttribute("dataBlock").ToString().Substring(1))[0];

        //        dBlock1[0].GetAttribute("stride").Value = 16u;
        //        dBlock1[1].GetAttribute("stride").Value = 16u;

        //        dBlock1[2].GetAttribute("offset").Value = dBlock2[0].GetAttribute("stride").Value;
        //        dBlock2[0].GetAttribute("stride").Value = (UInt32)dBlock2[0].GetAttribute("stride").Value + 12u;
        //        dBlock2[1].GetAttribute("stride").Value = (UInt32)dBlock2[1].GetAttribute("stride").Value + 12u;
        //        dBlock2[2].GetAttribute("stride").Value = (UInt32)dBlock2[2].GetAttribute("stride").Value + 12u;
        //        dBlock1[2].GetAttribute("stride").Value = dBlock2[0].GetAttribute("stride").Value;

        //        pssg.MoveNode(dBlock1[2], dBlock2);
        //    }

        //    clearVars(false);
        //    setupEditor(MainTabs.Auto);
        //}
    }
}
