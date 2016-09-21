using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using EgoEngineLibrary.Vehicle;
using System.Threading;
using System.Diagnostics;

namespace EgoCtfEditor
{
    public partial class Form1 : Form
    {
        // TODO: Search; VisibleRowSelect
        // 1.1.2012.1005 -- Focus/Highlight Text On Column Filter, Exit CFilter on Enter, Better Operation of FileDialog, 
        // -- Added Dirt Showdown and F1 2012
        // 1.2.2013.0105 -- Added Grid CSV, Improved Error Checks, FloatList Step, CPage DefaultExtension, DragDrop, ActiveControl,
        // -- Updated Schemas
        // 3.1.2013.0107 (1.2.1) -- Fixed Various Bugs With Grid, Cleaned up backend, New Versioning
        // 4.0 -- Added Dirt and Grid 2, Fixed Dirt Showdown, Fix Paste with hidden rows, Simplified UI, Uses EgoEngineLibrary
        // 4.1 -- Close/Save All, Speed up open
        // 4.2 -- Fixed Errors with Non-English Computers
        string[] args;
        private CtfEditorGamePage currentPage
        {
            get
            {
                return mainTabControl.SelectedTab.Tag as CtfEditorGamePage;
            }
        }
        private DataGridView currentDgv
        {
            get
            {
                if (currentPage == null)
                    return null;
                return mainTabControl.SelectedTab.Controls[0] as DataGridView;
            }
        }
        private PerformanceFile currentFile
        {
            get
            {
                if (currentDgv == null || currentDgv.SelectedColumns.Count == 0 || currentDgv.SelectedColumns[0].Index == 0)
                    return null;
                return currentDgv.SelectedColumns[0].Tag as PerformanceFile;
            }
        }

        public Form1(string[] args)
        {
            InitializeComponent();
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.Text = Properties.Resources.AppTitleLong;
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            // Select Latest Supported Game
            mainTabControl.SelectedIndex = (int)CtfEditorMainTabs.Other;
            this.args = args;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // OpenWith Args
            if (args.Length > 0)
            {
                if (Convert.ToInt32(args[args.Length - 1]) == -1)
                {
                    return;
                }
                mainTabControl.SelectedIndex = Convert.ToInt32(args[args.Length - 1]);
                if (!loadPage())
                    return;
                loadFile(args[0]);
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (int tab in Enum.GetValues(typeof(CtfEditorMainTabs)))
            {
                if (mainTabControl.TabPages[tab].Tag == null)
                {
                    continue;
                }
                foreach (PerformanceFile file in ((CtfEditorGamePage)mainTabControl.TabPages[tab].Tag).files)
                {
                    if (file.hasChanges && MessageBox.Show("The data in one of the CTF files has changed." + Environment.NewLine + Environment.NewLine +
                        "Are you sure you want to close the editor without saving?", Properties.Resources.AppTitleLong, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Effect == DragDropEffects.Copy)
            {
                if (!loadPage())
                    return;
                loadFile(e.Data.GetData(DataFormats.FileDrop) as string[]);
            }
        }

        #region MainMenu
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!loadPage())
                return;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.AddExtension = false;
            //dialog.DefaultExt = "ctf";
            dialog.Filter = "CTF file|*.ctf|CSV file|*.csv|All files|*.*";
            dialog.Multiselect = true;
            dialog.FilterIndex = (int)currentPage.FilterIndex;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                dialog.Dispose();
                loadFile(dialog.FileNames);
            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFile == null)
            {
                MessageBox.Show("A file column is not selected to be saved!", Properties.Resources.AppTitleLong, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = false;
            //dialog.DefaultExt = "ctf";
            dialog.Filter = "CTF file|*.ctf|CSV file|*.csv|All files|*.*";
            dialog.FileName = Path.GetFileName(currentFile.fileName);
            dialog.InitialDirectory = Path.GetDirectoryName(currentFile.fileName);
            dialog.FilterIndex = (int)currentPage.FilterIndex;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                dialog.Dispose();
                try
                {
                    currentFile.Write(dialog.FileName, File.Open(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The file could not be saved!" + Environment.NewLine + Environment.NewLine +
                    ex.Message, Properties.Resources.AppTitleLong, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                currentDgv.SelectedColumns[0].HeaderText = currentFile.name;
            }
        }
        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < currentDgv.Columns.Count; i++)
            {
                CtfFile f = currentDgv.Columns[i].Tag as CtfFile;
                if (f != null)
                {
                    f.Write(f.fileName, File.Open(f.fileName, FileMode.Create, FileAccess.Write, FileShare.Read));
                    currentDgv.Columns[i].HeaderText = f.name;
                }
            }
        }
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFile == null)
            {
                MessageBox.Show("A file column is not selected to be closed!", Properties.Resources.AppTitleLong, MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                return;
            }
            if (currentFile.hasChanges && 
                MessageBox.Show("The data in this CTF file has changed." + Environment.NewLine + Environment.NewLine + 
                "Are you sure you want to close it without saving?", Properties.Resources.AppTitleLong, MessageBoxButtons.OKCancel, 
                MessageBoxIcon.Warning) == DialogResult.Cancel)
            {
                return;
            }
            currentPage.files.Remove(currentFile);
            currentDgv.Columns.RemoveAt(currentDgv.SelectedColumns[0].Index);
        }
        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentDgv.SuspendDrawing();
            for (int i = 1; i < currentDgv.Columns.Count;)
            {
                currentDgv.Columns.RemoveAt(i);
            }
            currentDgv.ResumeDrawing();
            currentPage.files.Clear();
        }
        private void showHideDifferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentDgv == null || currentPage.files.Count <= 1)
            {
                return;
            }
            // Add/Delete Difference Filter
            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in currentDgv.Rows)
            {
                rows.Add(row);
            }
            currentDgv.SuspendDrawing();
            if (currentDgv.Rows.GetRowCount(DataGridViewElementStates.Visible) == currentPage.ctfEntryInfo.Length)
            {
                currentDgv.Rows.Clear();
                foreach (DataGridViewRow row in rows)
                {
                    row.Visible = false;
                    object valueToCompare = row.Cells[1].Value;
                    for (int i = 2; i < row.Cells.Count; i++)
                    {
                        if (valueToCompare == null && row.Cells[i].Value == null)
                        {
                            valueToCompare = row.Cells[i].Value;
                            continue;
                        }
                        else if (valueToCompare == null)
                        {
                            row.Visible = true;
                            break;
                        }
                        if (!valueToCompare.Equals(row.Cells[i].Value))
                        {
                            row.Visible = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                currentDgv.Rows.Clear();
                foreach (DataGridViewRow row in rows)
                    row.Visible = true;
            }
            currentDgv.Rows.AddRange(rows.ToArray());
            currentDgv.ResumeDrawing();
        }

        private void autoSelectSchema(int index, string fileName)
        {
            try
            {
                mainTabControl.SelectedIndex = index;
                setupPage();
                setupFileColumn(fileName);
            }
            catch (Exception ex)
            {
                if (index + 1 == mainTabControl.TabPages.Count)
                {
                    throw ex;
                }
                else
                {
                    // Clear previous page and try next page
                    clearPage();
                    autoSelectSchema(index++, fileName);
                }
            }
        }
        private bool loadPage()
        {
            try
            {
                setupPage();
                return true;
            }
            catch (Exception ex)
            {
                clearPage();
                MessageBox.Show("An error occured while creating the page!" + Environment.NewLine + Environment.NewLine +
                    ex.Message, Properties.Resources.AppTitleLong, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private void setupPage() {
            // Back out if the page has already been SetUp
            if (currentDgv != null)
            {
                ((DataGridView)mainTabControl.SelectedTab.Controls[0]).SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                return;
            }
            CtfEditorGamePage page;
            // Setup Page
            if (mainTabControl.SelectedIndex == (int)CtfEditorMainTabs.Formula1)
            {
                page = new CtfEditorGamePage(File.Open(System.Windows.Forms.Application.StartupPath + "\\ctfSchemaF12012.xml", FileMode.Open),
                    CtfEditorMainTabs.Formula1);
            }
            else if (mainTabControl.SelectedIndex == (int)CtfEditorMainTabs.Dirt)
            {
                page = new CtfEditorGamePage(File.Open(System.Windows.Forms.Application.StartupPath + "\\ctfSchemaDirt.xml", FileMode.Open),
                    CtfEditorMainTabs.Dirt);
            }
            else if (mainTabControl.SelectedIndex == (int)CtfEditorMainTabs.Other)
            {
                page = new CtfEditorGamePage(File.Open(System.Windows.Forms.Application.StartupPath + "\\ctfSchemaGrid2.xml", FileMode.Open),
                    CtfEditorMainTabs.Other);
            }
            else if (mainTabControl.SelectedIndex == (int)CtfEditorMainTabs.Grid)
            {
                page = new CtfEditorGamePage(File.Open(System.Windows.Forms.Application.StartupPath + "\\ctfSchemaGrid.xml", FileMode.Open),
                    CtfEditorMainTabs.Grid);
            }
            else
            {
                // SHOULD NOT HAPPEN, just doing it to satisfy compiler
                return;
            }
            // Setup DGV
            DataGridView dgv;
            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            //dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            //dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#E8EDFF");
            dgv.TopLeftHeaderCell.Value = "ID";
            dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
            dgv.CellValueChanged += new DataGridViewCellEventHandler(dgv_CellValueChanged);
            dgv.CellBeginEdit += new DataGridViewCellCancelEventHandler(dgv_CellBeginEdit);
            dgv.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(dgv_ColumnHeaderMouseClick);
            dgv.CellMouseDown += new DataGridViewCellMouseEventHandler(dgv_CellMouseDown);
            dgv.DataError += new DataGridViewDataErrorEventHandler(dgv_DataError);
            dgv.CellEnter += new DataGridViewCellEventHandler(dgv_CellEnter);
            dgv.KeyDown += new KeyEventHandler(dgv_KeyDown);
            dgv.ColumnHeaderMouseDoubleClick += new DataGridViewCellMouseEventHandler(dgv_ColumnHeaderMouseDoubleClick);
            // Load Page Contents
            mainTabControl.SelectedTab.Tag = page;
            dgv.Columns.Add("entryName", "Entry Name");
            dgv.Columns[0].MinimumWidth = 150;
            dgv.Columns[0].ValueType = typeof(string);
            dgv.Columns[0].ReadOnly = true;
            dgv.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            DGVColumnFilterContextControl colFilter = new DGVColumnFilterContextControl(dgv, 0);
            dgv.SuspendDrawing();
            foreach (CtfEntryInfo entryInfo in page.ctfEntryInfo)
            {
                dgv.Rows.Add(entryInfo.name);
                dgv.Rows[entryInfo.id].HeaderCell.Value = Convert.ToString(entryInfo.id);
                //entryInfo.id == 0 || entryInfo.id == 1 || entryInfo.refID >= 0 || 
                if (entryInfo.readOnly)
                {
                    dgv.Rows[entryInfo.id].ReadOnly = true;
                }
            }
            dgv.ResumeDrawing();
            mainTabControl.SelectedTab.Controls.Add(dgv);
            ActiveControl = currentDgv;
        }
        private void clearPage()
        {
            mainTabControl.SelectedTab.Controls.Clear();
            CtfEditorGamePage p = currentPage;
            p = null;
            mainTabControl.SelectedTab.Tag = null;
        }
        private void loadFile(params string[] fileNames)
        {
            int i = 0;
            try
            {
                currentDgv.SuspendLayout();
                for (; i < fileNames.Length; i++)
                {
                    setupFileColumn(fileNames[i]);
                }
                currentDgv.ResumeLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The file could not be opened!" + Environment.NewLine + Environment.NewLine +
                    fileNames[i] + Environment.NewLine + Environment.NewLine +
                    ex.Message, Properties.Resources.AppTitleLong, MessageBoxButtons.OK, MessageBoxIcon.Error);
                List<string> fNames = fileNames.ToList<string>();
                fNames.RemoveRange(0, i + 1);
                loadFile(fNames.ToArray());
            }
            finally
            {
                currentDgv.ResumeLayout();
            }
        }

        private void setupFileColumn(object fName)
        {
            string fileName = (string)fName;
            // Return if file already opened
            if (currentPage.ContainsFile(fileName))
            {
                MessageBox.Show("This file has already been opened!" + Environment.NewLine + Environment.NewLine +
                    fileName, Properties.Resources.AppTitleLong, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            PerformanceFile ctfFile;
            if (currentPage.FilterIndex == CtfEditorFilterIndex.CSV)
            {
                ctfFile = new CsvFile(fileName, File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read), (CtfEditorGamePage)mainTabControl.SelectedTab.Tag);
            }
            else
            {
                ctfFile = new CtfFile(fileName, File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read), (CtfEditorGamePage)mainTabControl.SelectedTab.Tag);
            }
            int columnCount = currentDgv.Columns.Count;
            currentDgv.Columns.Add(Path.GetTempFileName(), ctfFile.name);
            currentDgv.Columns[columnCount].MinimumWidth = 100;
            currentDgv.Columns[columnCount].ToolTipText = ctfFile.fileName;
            currentDgv.Columns[columnCount].SortMode = DataGridViewColumnSortMode.NotSortable;
            Stopwatch sw = new Stopwatch();
            foreach (KeyValuePair<int, object> entry in ctfFile.entry)
            {
                if (entry.Value.GetType() == typeof(bool))
                {
                    currentDgv.Rows[entry.Key].Cells[columnCount] = new DataGridViewCheckBoxCell();
                    currentDgv.Rows[entry.Key].Cells[columnCount].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                else if (ctfFile.parentPage.ctfEntryInfo[entry.Key].restrictedValues.Length > 0)
                {
                    currentDgv.Rows[entry.Key].Cells[columnCount] = new DataGridViewComboBoxCell();
                    ((DataGridViewComboBoxCell)currentDgv.Rows[entry.Key].Cells[columnCount]).DataSource = ctfFile.parentPage.ctfEntryInfo[entry.Key].restrictedValues;
                }
                currentDgv.Rows[entry.Key].Cells[columnCount].Value = entry.Value;
                currentDgv.Rows[entry.Key].Cells[columnCount].ValueType = currentPage.ctfEntryInfo[entry.Key].realType;//entry.Value.GetType();
            }
            currentDgv.Columns[columnCount].Tag = ctfFile;
            ActiveControl = currentDgv;
        }
        #endregion

        #region DGVEvents
        private void dgv_KeyDown(object sender, KeyEventArgs e)
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
                for (int iRow = 0, rowCount = 0; rowCount < rowsInClipboard.Length; iRow++)
                {
                    // iRow is for current Grid row, rowCount is for current rowsInClipboard row
                    if (!grid.Rows[r + iRow].Visible)
                        continue;

                    //split row into cell values
                    string[] valuesInRow = rowsInClipboard[rowCount].Split(columnSplitter);
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
                                    if (!grid[c + iCol, r + iRow].ReadOnly && !string.IsNullOrEmpty(grid.Rows[r + iRow].Cells[c + iCol].FormattedValue.ToString()))
                                    {
                                        if (grid[c + iCol, r + iRow] is DataGridViewComboBoxCell)
                                        {
                                            if (!(currentPage.ctfEntryInfo[r + iRow].restrictedValues.Contains(Convert.ChangeType(valuesInRow[iCol], grid[c + iCol, r + iRow].ValueType))))
                                            {
                                                continue;
                                            }
                                        }
                                        grid[c + iCol, r + iRow].Value = Convert.ChangeType(valuesInRow[iCol], grid[c + iCol, r + iRow].ValueType);
                                    }
                                }
                                catch {  }
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

                    rowCount++;
                }
                grid.CurrentCell.Selected = false;
                grid.CurrentCell = grid.Rows[r].Cells[c];
                grid.CurrentCell.Selected = true;
                e.Handled = true;
            }
        }
        private void dgv_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show("Input data was not in the correct format!" + Environment.NewLine + Environment.NewLine +
                "Press Esc to restore the original value or type in data of the proper format.", Properties.Resources.AppTitleLong, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //((DataGridView)sender).CancelEdit();
        }
        private void dgv_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (((DataGridView)sender).Rows.Count == 0)
            {
                return;
            }
            infoToolStripStatusLabel.Text = currentPage.ctfEntryInfo[e.RowIndex].description;
        }
        private void dgv_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            ((DataGridView)sender).SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
            // Return if the cell isn't in a data column or if the row is the column headers
            if (e.ColumnIndex < 1 || e.RowIndex < 0)
            {
                return;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ((DataGridView)sender).CurrentCell = ((DataGridView)sender)[e.ColumnIndex, e.RowIndex];
                ((DataGridView)sender).BeginEdit(true);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                ((DataGridView)sender).CurrentCell = null;
                ((DataGridView)sender).Rows[e.RowIndex].Selected = true;
            }
        }
        private void dgv_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            // Return if its not in one of the data/file columns OR if this is the first time column is being populated
            if (e.ColumnIndex < 1 || dgv.Columns[e.ColumnIndex].Tag == null)
            {
                return;
            }
            PerformanceFile ctfFile = (PerformanceFile)dgv.Columns[e.ColumnIndex].Tag;
            // Cancel and Return if the entry is not used
            if (!ctfFile.entry.ContainsKey(e.RowIndex))
            {
                e.Cancel = true;
                return;
            }
            // Bring out special editor for FloatList values
            if (dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].ValueType == typeof(FloatList))
            {
                using (DGVListEditor editor = new DGVListEditor((CtfEditorGamePage)mainTabControl.SelectedTab.Tag, e.RowIndex))
                {
                    editor.ShowDialog();
                }
                e.Cancel = true;
            }
        }
        private void dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            // Return if its not in one of the data/file columns OR if this is the first time column is being populated
            if (e.ColumnIndex < 1 || e.RowIndex < 0 || dgv.Columns[e.ColumnIndex].Tag == null)
            {
                return;
            }
            PerformanceFile ctfFile = (PerformanceFile)dgv.Columns[e.ColumnIndex].Tag;
            // Do Nothing if Same Value
            if (ctfFile.entry[e.RowIndex] == dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value)
            {
                return;
            }
            // Set to Default(Old) Value if the new value is null, Not with Grid/Dirt
            if (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue.ToString())
                && currentPage.FilterIndex != CtfEditorFilterIndex.CSV) // currentPage.parentTab != CtfEditorMainTabs.Grid
            {
                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = currentPage.ctfEntryInfo[e.RowIndex].defaultValue;
                //dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ctfFile.entry[e.RowIndex];
                return;
            }
            //MessageBox.Show(e.RowIndex.ToString());
            // Set the new value in the backend
            ctfFile.entry[e.RowIndex] = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (!ctfFile.hasChanges)
                dgv.Columns[e.ColumnIndex].HeaderText += "*";
            ctfFile.hasChanges = true;
        }
        private void dgv_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 1)
            {
                return;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                DataGridView dgv = (DataGridView)sender;
                dgv.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
                dgv.Columns[e.ColumnIndex].Selected = true;
            }
        }
        private void dgv_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 1)
            {
                return;
            }
            if (e.Clicks == 2 && e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                DataGridView dgv = (DataGridView)sender;
                PerformanceFile ctfFile = (PerformanceFile)dgv.Columns[e.ColumnIndex].Tag;
                if (ctfFile.hasChanges && MessageBox.Show("The data in this CTF file has changed." + Environment.NewLine + Environment.NewLine +
                    "Are you sure you want to close it without saving?", Properties.Resources.AppTitleLong, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                {
                    return;
                }
                currentPage.files.Remove(ctfFile);
                dgv.Columns.RemoveAt(e.ColumnIndex);
            }
        }
        #endregion

        private void mainTabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPageIndex == (int)CtfEditorMainTabs.Dirt)
            {
                //e.Cancel = true;
                //MessageBox.Show("This operation is not implemented!", Properties.Resources.AppTitleLong, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void mainTabControl_Selected(object sender, TabControlEventArgs e)
        {
            ActiveControl = currentDgv;
        }
    }

    public static class ControlHelper
    {
        #region Redraw Suspend/Resume
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessageA", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        private const int WM_SETREDRAW = 0xB;

        public static void SuspendDrawing(this Control target)
        {
            SendMessage(target.Handle, WM_SETREDRAW, 0, 0);
        }

        public static void ResumeDrawing(this Control target) { ResumeDrawing(target, true); }
        public static void ResumeDrawing(this Control target, bool redraw)
        {
            SendMessage(target.Handle, WM_SETREDRAW, 1, 0);

            if (redraw)
            {
                target.Refresh();
            }
        }
        #endregion
    }
}
