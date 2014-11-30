using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EgoCtfEditor
{
    public partial class DGVColumnFilterContextControl : UserControl
    {
        private int colIndex;
        private DataGridView dgv;
        private ToolStripDropDown popup;

        public DGVColumnFilterContextControl(DataGridView dgv, int colIndex)
        {
            InitializeComponent();
            this.colIndex = colIndex;
            this.dgv = dgv;
            popup = new ToolStripDropDown();
            popup.Margin = Padding.Empty;
            popup.Padding = Padding.Empty;
            ToolStripControlHost host = new ToolStripControlHost(this);
            host.Margin = Padding.Empty;
            host.Padding = Padding.Empty;
            popup.Items.Add(host);
            // Setup
            LostFocus += new EventHandler(DGVColumnFilterContextControl_LostFocus);
            dgv.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(dgv_ColumnHeaderMouseClick);
        }

        void DGVColumnFilterContextControl_LostFocus(object sender, EventArgs e)
        {
            popup.Hide();
        }

        void dgv_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == colIndex)
            {
                int xPos = dgv.TopLeftHeaderCell.Size.Width;
                for (int i = 0; i <= colIndex; i++)
                {
                    xPos += dgv.Columns[i].HeaderCell.Size.Width;
                }
                popup.Show(dgv, xPos, dgv.Columns[e.ColumnIndex].HeaderCell.ContentBounds.Top);
                filterTextBox.SelectAll();
                filterTextBox.Focus();
            }
        }

        private void filterTextBox_TextChanged(object sender, EventArgs e)
        {
            // Unsubscribe from event while filter is working
            //filterTextBox.TextChanged -= filterTextBox_TextChanged;
            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in dgv.Rows)
            {
                rows.Add(row);
            }

            dgv.SuspendDrawing();
            dgv.Rows.Clear();

            foreach (DataGridViewRow row in rows)
            {
                row.Visible = false;
                if (((string)row.Cells[colIndex].Value).StartsWith(filterTextBox.Text, StringComparison.CurrentCultureIgnoreCase) ||
                    ((string)row.Cells[colIndex].Value).Contains(filterTextBox.Text.ToLower()))
                {
                    row.Visible = true;
                }
            }

            dgv.Rows.AddRange(rows.ToArray());
            dgv.ResumeDrawing();
            //filterTextBox.TextChanged +=new EventHandler(filterTextBox_TextChanged);
        }

        private void filterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                popup.Hide();
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            popup.Hide();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            dgv.ColumnHeaderMouseClick -= dgv_ColumnHeaderMouseClick;
            filterTextBox.Text = "";
            dgv.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(dgv_ColumnHeaderMouseClick);

            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in dgv.Rows)
            {
                rows.Add(row);
            }

            dgv.SuspendDrawing();
            dgv.Rows.Clear();

            foreach (DataGridViewRow row in rows)
                row.Visible = true;

            dgv.Rows.AddRange(rows.ToArray());
            dgv.ResumeDrawing();

            popup.Hide();
        }
    }
}
