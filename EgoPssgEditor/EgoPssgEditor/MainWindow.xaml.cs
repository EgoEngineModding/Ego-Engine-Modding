using EgoEngineLibrary.Graphics;
using EgoPssgEditor.ViewModel;
using System;
using System.Collections.Generic;
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

        private void texListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            PssgTextureViewModel texView = e.AddedItems[0] as PssgTextureViewModel;
            if (texView != null) SelectNode(view.NodesWorkspace.RootNode, texView.Texture);
        }

        public void SelectNode(PssgNodeViewModel nodeView, PssgNode node)
        {
            if (nodeView.Node == node)
            {
                nodeView.IsSelected = true;
                return;
            }

            foreach (PssgNodeViewModel nvm in nodeView.Children)
            {
                SelectNode(nvm, node);
            }
        }
    }
}
