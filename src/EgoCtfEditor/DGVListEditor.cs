using System;
using System.Windows.Forms;
using EgoEngineLibrary.Vehicle;

namespace EgoCtfEditor
{
    public partial class DGVListEditor : Form
    {
        private int _rowIndex;

        public DGVListEditor(CtfEditorGamePage page, int rowIndex)
        {
            InitializeComponent();
            _rowIndex = rowIndex;
            // DataGridView Setup
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#E8EDFF");
            // Populate DGV
            for (int i = 0; i < page.files.Count; i++)
            {
                dataGridView.Columns.Add(i.ToString(), page.files[i].name);
                dataGridView.Columns[i].ToolTipText = page.files[i].fileName;
                dataGridView.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView.Columns[i].Tag = page.files[i];
                if (i == 0)
                {
                    dataGridView.TopLeftHeaderCell.Value = "Entry Name";
                    dataGridView.Rows.Add();
                    dataGridView.Rows.Add();
                    dataGridView.Rows[0].HeaderCell.Value = "Item Count";
                    dataGridView.Rows[0].ReadOnly = true;
                    dataGridView.Rows[1].HeaderCell.Value = "Step";
                }
                if (!page.files[i].entry.ContainsKey(rowIndex)) {
                    continue;
                }
                dataGridView.Rows[0].Cells[i].Value = ((FloatList)page.files[i].entry[rowIndex]).count;
                dataGridView.Rows[1].Cells[i].Value = ((FloatList)page.files[i].entry[rowIndex]).step;
                dataGridView.Rows[1].Cells[i].ValueType = typeof(float);
                for (int j = 0; j < ((FloatList)page.files[i].entry[rowIndex]).count; j++)
                {
                    if (dataGridView.Rows.Count - 2 <= j)
                    {
                        dataGridView.Rows.Add();
                    }
                    dataGridView.Rows[j + 2].Cells[i].Value = ((FloatList)page.files[i].entry[rowIndex]).items[j];
                    dataGridView.Rows[j + 2].Cells[i].ValueType = typeof(float);
                    dataGridView.Rows[j + 2].HeaderCell.Value = (j*((FloatList)page.files[i].entry[rowIndex]).step).ToString();
                }
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // Return if its not in one of the data/file columns
            if (e.ColumnIndex < 0)
            {
                return;
            }
            PerformanceFile ctfFile = (PerformanceFile)dataGridView.Columns[e.ColumnIndex].Tag;
            // Cancel if the current value is null
            if (string.IsNullOrEmpty(dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue.ToString()))
            {
                e.Cancel = true;
            }
        }

        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Return if its not in one of the data/file columns OR if this is the first time column is being populated
            if (e.ColumnIndex < 0 || e.RowIndex == 0)
            {
                return;
            }
            PerformanceFile ctfFile = (PerformanceFile)dataGridView.Columns[e.ColumnIndex].Tag;
            //MessageBox.Show(e.RowIndex.ToString());
            // Set the new value in the backend
            FloatList fList = ((FloatList)ctfFile.entry[_rowIndex]);
            if (e.RowIndex == 1)
            {
                if (string.IsNullOrEmpty(dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue.ToString()))
                {
                    dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = fList.step;
                    return;
                }
                fList.step = (float)dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                //MessageBox.Show(fList.unknown.ToString());
            }
            else
            {
                if (string.IsNullOrEmpty(dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue.ToString()))
                {
                    dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = fList.items[e.RowIndex - 2];
                    return;
                }
                fList.items[e.RowIndex - 2] = (float)dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            }
            ctfFile.entry[_rowIndex] = fList;
            ctfFile.hasChanges = true;
        }

        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                DataObject d = ((DataGridView)sender).GetClipboardContent();
                Clipboard.SetDataObject(d);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                DataGridView grid = ((DataGridView)sender);
                char[] rowSplitter = { '\r', '\n' };
                char[] columnSplitter = { '\t' };
                //get the text from clipboard
                IDataObject dataInClipboard = Clipboard.GetDataObject();
                string stringInClipboard = (string)dataInClipboard.GetData(DataFormats.Text);
                if (stringInClipboard == null)
                {
                    e.Handled = true;
                    return;
                }
                //split it into lines
                string[] rowsInClipboard = stringInClipboard.Split(rowSplitter, StringSplitOptions.RemoveEmptyEntries);
                // get the row and column of selected cell in grid
                int r = grid.SelectedCells[0].RowIndex;
                int c = grid.SelectedCells[0].ColumnIndex;
                if (((DataGridView)sender).SelectedCells.Count > 1)
                {
                    foreach (DataGridViewCell cell in ((DataGridView)sender).SelectedCells)
                    {
                        if (cell.RowIndex <= r && cell.RowIndex >= 0)
                        {
                            r = cell.RowIndex;
                            if (cell.ColumnIndex < c && cell.ColumnIndex >= 0)
                            {
                                c = cell.ColumnIndex;
                            }
                        }
                    }
                }
                // loop through the lines, split them into cells and place the values in the corresponding cell.
                for (int iRow = 0; iRow < rowsInClipboard.Length; iRow++)
                {
                    //split row into cell values
                    string[] valuesInRow = rowsInClipboard[iRow].Split(columnSplitter);
                    //cycle through cell values
                    if (grid.RowCount - 1 >= r + iRow)
                    {
                        for (int iCol = 0; iCol < valuesInRow.Length; iCol++)
                        {
                            //assign cell value, only if it within columns of the grid
                            if (grid.ColumnCount - 1 >= c + iCol)
                            {
                                try
                                {
                                    if (grid[c + iCol, r + iRow].ReadOnly == false && !string.IsNullOrEmpty(grid.Rows[r + iRow].Cells[c + iCol].FormattedValue.ToString()))
                                    {
                                        grid[c + iCol, r + iRow].Value = Convert.ChangeType(valuesInRow[iCol], grid[c + iCol, r + iRow].ValueType);
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                grid.CurrentCell.Selected = false;
                grid.CurrentCell = grid.Rows[r].Cells[c];
                grid.CurrentCell.Selected = true;
                e.Handled = true;
            }
        }
    }
}
