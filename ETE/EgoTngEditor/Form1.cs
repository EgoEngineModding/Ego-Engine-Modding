using System;
using System.Drawing;
using System.Windows.Forms;
using EgoEngineLibrary.Vehicle;
using System.IO;

namespace EgoTngEditor
{
    public partial class Form1 : Form
    {
        // 1.0 -- First Release
        TngFile file;
        string[] args;

        public Form1(string[] _args)
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // DataGridSetup
            dataGridView.Columns.Add("value", "Value");
            dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            // FileDialogSetup
            openFileDialog.Filter = "Tng files|*.tng|All files|*.*";
            saveFileDialog.Filter = "Tng files|*.tng|All files|*.*";
            args = _args;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (args.Length > 0)
            {
                Open(args[0]);
            }
            args = null;
        }

        #region MainMenu
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Open(openFileDialog.FileName);
            }
        }
        private void Open(string fileName)
        {
            try
            {
                treeView.Nodes.Clear();
                dataGridView.Rows.Clear();
                this.Text = fileName + " - Ego TNG Editor";
                file = new TngFile(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
                file.CreateTreeViewList(treeView);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open the file: " + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Tng Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Text = "Ego TNG Editor";
            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (file == null)
                return;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    file.Write(File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save the file: " + Environment.NewLine + Environment.NewLine + ex.Message,
                        "Tng Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            dataGridView.CellValueChanged -= dataGridView_CellValueChanged;
            TngEntry entry = (TngEntry)e.Node.Tag;
            dataGridView.Rows.Clear();
            entry.CreateDgvTable(dataGridView);
            dataGridView.CellValueChanged += dataGridView_CellValueChanged;
        }

        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            TngEntry entry = (TngEntry)treeView.SelectedNode.Tag;
            entry.Data[(string)dataGridView.Rows[e.RowIndex].HeaderCell.Value] = dataGridView.Rows[e.RowIndex].Cells[0].Value;
        }
    }
}
