using System;
using System.Windows.Forms;
using EgoEngineLibrary.Vehicle;

namespace EgoCtfEditor
{
    public partial class GameSelect : Form
    {
        public int GameID
        {
            get { return gameComboBox.SelectedIndex; }
        }

        public GameSelect()
        {
            InitializeComponent();
            gameComboBox.DataSource = Enum.GetNames(typeof(CtfEditorMainTabs));
            gameComboBox.SelectedIndex = 0;
        }

        private void button_Click(object sender, EventArgs e)
        {
            if (gameComboBox.SelectedItem == null)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.None;
            }
        }
    }
}
