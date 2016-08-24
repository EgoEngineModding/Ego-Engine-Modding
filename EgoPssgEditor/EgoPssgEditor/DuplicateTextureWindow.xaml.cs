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
    /// Interaction logic for DuplicateTextureWindow.xaml
    /// </summary>
    public partial class DuplicateTextureWindow : Window
    {
        public DuplicateTextureWindow()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextureName) || string.IsNullOrWhiteSpace(TextureName))
            {
                this.DialogResult = null;
            }
            else
            {
                this.DialogResult = true;
            }
        }
        
        public string TextureName
        {
            get { return textureNameTextBox.Text; }
            set { textureNameTextBox.Text = value; }
        }
    }
}
