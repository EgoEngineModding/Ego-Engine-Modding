using System.Windows.Forms;
using System.Data;
using EgoEngineLibrary.Language;

namespace EgoLngEditor {
	public partial class AddRow : Form {
        LngFile file;
		public string Key {
			get { return textBox1.Text; }
		}
		public string Value {
			get { return richTextBox2.Text; }
		}

		public AddRow(LngFile _file) {
			InitializeComponent();
            file = _file;
            textBox1.SelectAll();
		}

		private void okButton_Click(object sender, System.EventArgs e) {
			//if (DT.Select("LNG_Key = '" + this.Key + "'").Length > 0) {
            //MessageBox.Show(file[this.Key]);
            if (file.ContainsKey(this.Key))
            {
                MessageBox.Show("Please use a unique key, the one you have provided is already taken", "Unique ID", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.DialogResult = DialogResult.None;
                return;
            }
		}
	}
}
