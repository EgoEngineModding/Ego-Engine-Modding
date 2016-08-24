using EgoEngineLibrary.Graphics;
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
using System.Windows.Shapes;

namespace EgoPssgEditor
{
    /// <summary>
    /// Interaction logic for AddNodeWindow.xaml
    /// </summary>
    public partial class AddNodeWindow : Window
    {
        public AddNodeWindow()
        {
            InitializeComponent();

            // NodeInfo Combo
            nodeNameComboBox.ItemsSource = (PssgSchema.GetNodeNames());
            // Select
            nodeNameComboBox.SelectedIndex = 0;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        
        public string NodeName
        {
            get { return nodeNameComboBox.Text; }
        }
    }
}
