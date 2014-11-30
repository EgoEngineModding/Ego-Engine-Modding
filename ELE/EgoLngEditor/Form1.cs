using System;
using System.Data;
using System.Windows.Forms;
using System.IO;
using DgvFilterPopup;
using EgoEngineLibrary.Language;

namespace EgoLngEditor
{
    public partial class Form1 : Form
    {
        // 3.0 -- Using EgoEngineLibrary, Compare/Merge, QuitSafety, AddRowUIFix
        // 4.0 -- Rewrote backend, Internal Schema, Find Next/Prev, --ReadForRelease
		LngFile lngFile;
		DataSet LNG;
		FindAndReplaceForm fRF;
        string filePath;
        string filePath2 = @"F:\Games\Racing\F1 2010\language\temp.bin";

        public Form1(string[] args)
        {
            InitializeComponent();
			this.Icon = Properties.Resources.Ryder25;
			LNG = new DataSet("language");
            fRF = new FindAndReplaceForm(this.lngDataGridView);
			filePath = openFileDialog.FileName;
			lngDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			lngDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
			lngDataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
			lngDataGridView.AllowUserToAddRows = false;
			lngDataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#E8EDFF");
			DgvFilterManager dgvfm = new DgvFilterManager(lngDataGridView);
			// Open With
			if (args.Length > 0) {
				if (File.Exists(args[0]) == true) {
					filePath = args[0];
					Stream s = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
					lngFile = new LngFile(s);
					lngDataGridView.DataSource = lngFile.GetDataTable();

					this.Text = "Ego Language Editor - " + filePath;
				}
			}
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (lngFile != null && lngFile.hasChanges)
            {
                if (MessageBox.Show("Are you sure you want to quit?", "Ryder Language Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

		#region MainMenu
		private void openToolStripMenuItem_Click(object sender, EventArgs e) {
			openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
			openFileDialog.FileName = Path.GetFileName(filePath).Replace(".xml", ".lng");
			if (openFileDialog.ShowDialog() == DialogResult.OK) {
				// Clean
				lngDataGridView.DataSource = null;
				lngDataGridView.Rows.Clear();
				lngDataGridView.Columns.Clear();
				filePath = openFileDialog.FileName;
				openFileDialog.Dispose();
                Stream s = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				try {
					lngFile = new LngFile(s);
				}
				catch (Exception ex) {
					MessageBox.Show("Conversion Failed!" + Environment.NewLine + ex.Message, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
                }
				lngDataGridView.DataSource = lngFile.GetDataTable();

				this.Text = "EGO Language Editor - " + filePath;
			}
		}
		private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            if (lngFile == null)
                return;

			saveFileDialog.FilterIndex = 1;
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
			saveFileDialog.FileName = Path.GetFileName(filePath).Replace(".xml", ".lng");
			if (saveFileDialog.ShowDialog() == DialogResult.OK) {
				filePath = saveFileDialog.FileName;
				Stream s = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
				lngFile.Write(s);
				saveFileDialog.Dispose();
			}
		}
		private void importAsXMLToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog.FilterIndex = 2;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
			openFileDialog.FileName = filePath.Replace(".lng", ".xml");
			if (openFileDialog.ShowDialog() == DialogResult.OK) {
				// Clean
				lngDataGridView.DataSource = null;
				lngDataGridView.Rows.Clear();
				lngDataGridView.Columns.Clear();
				LNG = new DataSet("language");
				//LNG.ReadXmlSchema(Path.GetFullPath(openFileDialog.FileName).Replace(Path.GetFileName(openFileDialog.FileName), string.Empty) +
						//Path.GetFileNameWithoutExtension(openFileDialog.FileName) + "_schema.xsd");
				try {
                    LNG.ReadXml(File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read), XmlReadMode.ReadSchema);
				}
				catch (Exception ex) {
					MessageBox.Show("Loading XML failed!" + Environment.NewLine + ex.Message, "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				lngFile = new LngFile(LNG);
				lngDataGridView.DataSource = LNG.Tables[1];
				filePath = openFileDialog.FileName;
				this.Text = "EGO Language Editor - " + openFileDialog.FileName;
				openFileDialog.Dispose();
			}
		}
		private void exportAsXMLToolStripMenuItem_Click(object sender, EventArgs e) {
			if (lngFile != null) {
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
				saveFileDialog.FileName = Path.GetFileName(filePath).Replace(".lng", ".xml");
				if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    LNG = lngFile.WriteXml(File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read), (DataTable)lngDataGridView.DataSource);
					saveFileDialog.Dispose();
				}
			} else {
				MessageBox.Show("There must already be an opened language file in order to save it as an xml file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
		}
		private void addRowToolStripMenuItem_Click(object sender, EventArgs e) {
			if (lngFile == null) {
				MessageBox.Show("There must already be an opened language file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				return;
			}
			DataTable DT = (DataTable)lngDataGridView.DataSource;
			AddRow AR = new AddRow(lngFile);
			if (AR.ShowDialog() == DialogResult.OK) {
				if (AR.Key == string.Empty) {
					return;
				}
				DataRow DR = DT.NewRow();
				DR[0] = AR.Key;
				DR[1] = AR.Value;
                DT.Rows.Add(DR);
                lngFile.Add(AR.Key, AR.Value);
			}
		}
		private void deleteRowToolStripMenuItem_Click(object sender, EventArgs e) {
			if (lngDataGridView.SelectedRows.Count == 0) {
				MessageBox.Show("No rows were selected", "No Row Selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			DataTable DT = (DataTable)lngDataGridView.DataSource;
            foreach (DataGridViewRow row in lngDataGridView.SelectedRows)
            {
                lngFile.Remove((string)row.Cells[0].Value);
                DT.DefaultView[row.Index].Delete();
			}
		}
        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lngFile == null)
                return;

            fRF.InitializeForm(this.lngDataGridView);
            fRF.BringToFront();
            fRF.Show();
        }
        private void findNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lngFile == null)
                return;

            fRF.Find(false);
        }
        private void findPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lngFile == null)
                return;

            fRF.Find(true);
        }
        private void compareFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lngFile == null)
                return;

            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
            openFileDialog.FileName = Path.GetFileName(filePath).Replace(".xml", ".lng");
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LngFile two = new LngFile(File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read));
                    openFileDialog.Dispose();
                    saveFileDialog.FilterIndex = 2;
                    saveFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
                    saveFileDialog.FileName = Path.GetFileName(filePath).Replace(".lng", "Differences.xml");
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        LngFile differences = lngFile.GetDifferences(two);
                        differences.WriteXml(File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read));
                        saveFileDialog.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The program failed to compare the files:" + Environment.NewLine + Environment.NewLine + ex.Message, "Ryder Language Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void mergeFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lngFile == null)
                return;

            openFileDialog.FilterIndex = 2;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
            openFileDialog.FileName = Path.GetFileName(filePath).Replace(".lng", "Differences.xml");
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Load diffs
                    DataSet diffs = new DataSet("language");
                    //diffs.ReadXmlSchema(Path.GetFullPath(openFileDialog.FileName).Replace(Path.GetFileName(openFileDialog.FileName), string.Empty) +
                            //Path.GetFileNameWithoutExtension(openFileDialog.FileName) + "_schema.xsd");
                    diffs.ReadXml(File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read), XmlReadMode.ReadSchema);
                    openFileDialog.Dispose();

                    // Get current datatable
                    DataTable current = (DataTable)lngDataGridView.DataSource;

                    // Merge
                    lngFile.MergeDifferences(current, diffs.Tables["entry"]);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The program failed to merge the files:" + Environment.NewLine + Environment.NewLine + ex.Message, "Ryder Database Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
		#endregion

		public void test()
        {
            using (BinaryReader b = new BinaryReader(File.Open(filePath2, FileMode.Open)))
            {
                byte[] temp = b.ReadBytes(4);
                Array.Reverse(temp);
                MessageBox.Show(Convert.ToString(BitConverter.ToSingle(temp, 0)));
            }
        }

		private void lngDataGridView_KeyDown(object sender, KeyEventArgs e) {
			if (e.Control && e.KeyCode == Keys.C) {
				DataObject d = ((DataGridView)sender).GetClipboardContent();
				Clipboard.SetDataObject(d);
				e.Handled = true;
			} else if (e.Control && e.KeyCode == Keys.V) {
				DataGridView grid = ((DataGridView)sender);
				char[] rowSplitter = { '\r', '\n' };
				char[] columnSplitter = { '\t' };
				//get the text from clipboard
				IDataObject dataInClipboard = Clipboard.GetDataObject();
				string stringInClipboard = (string)dataInClipboard.GetData(DataFormats.Text);
				if (stringInClipboard == null) {
					e.Handled = true;
					return;
				}
				//split it into lines
				string[] rowsInClipboard = stringInClipboard.Split(rowSplitter, StringSplitOptions.RemoveEmptyEntries);
				//get the row and column of selected cell in grid
				int r = grid.SelectedCells[0].RowIndex;
				int c = grid.SelectedCells[0].ColumnIndex;
				if (((DataGridView)sender).SelectedCells.Count > 1) {
					foreach (DataGridViewCell cell in ((DataGridView)sender).SelectedCells) {
						if (cell.RowIndex <= r && cell.RowIndex >= 0) {
							r = cell.RowIndex;
							if (cell.ColumnIndex < c && cell.ColumnIndex >= 0) {
								c = cell.ColumnIndex;
							}
						}
					}
				}
				DataTable table = (DataTable)grid.DataSource;
				DataTable tableO = table.Copy();
				System.Collections.Generic.Dictionary<int, string[]> cellIndexes = new System.Collections.Generic.Dictionary<int, string[]>();
				// loop through the lines, split them into cells and place the values in the corresponding cell.
				for (int iRow = 0; iRow < rowsInClipboard.Length; iRow++) {
					//split row into cell values
					string[] valuesInRow = rowsInClipboard[iRow].Split(columnSplitter);
					//cycle through cell values
					if (grid.RowCount - 1 >= r + iRow) {
						for (int iCol = 0; iCol < valuesInRow.Length; iCol++) {
							//assign cell value, only if it within columns of the grid
							if (grid.ColumnCount - 1 >= c + iCol) {
								try {
									//table.Rows.IndexOf(table.DefaultView[r + iRow].Row);
									//MessageBox.Show(r + iRow + " " + table.DefaultView[r + iRow]);
									cellIndexes.Add(table.Rows.IndexOf(table.DefaultView[r + iRow].Row), new string[2] { grid.Columns[c + iCol].Name, valuesInRow[iCol] });
								}
								catch {}
							} else {
								break;
							}
						}
					} else {
						break;
					}
				}
				string sort = table.DefaultView.Sort;
				table.DefaultView.Sort = "";
                foreach (System.Collections.Generic.KeyValuePair<int, string[]> index in cellIndexes)
                {
                    if (index.Value[0] == "LNG_Key")
                    {
                        if (lngFile.ContainsKey(index.Value[1]))
                            continue;
                        lngFile.Remove((string)table.Rows[index.Key][0], false);
                        lngFile.Add(index.Value[1], (string)table.Rows[index.Key][1]);
                    }
                    else
                    {
                        lngFile.Add((string)table.Rows[index.Key][0], index.Value[1]);
                    }
					table.Rows[index.Key].BeginEdit();
					table.Rows[index.Key][index.Value[0]] = index.Value[1];
					table.Rows[index.Key].EndEdit();
				}
				grid.CurrentCell.Selected = false;
				grid.CurrentCell = grid.Rows[r].Cells[c];
				table.DefaultView.Sort = sort;
				e.Handled = true;
			}
		}
		private void lngDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
			// Validate the LNG_Key entry by disallowing same entries.
			DataTable DT = (DataTable)lngDataGridView.DataSource;
			if (lngDataGridView.Columns[e.ColumnIndex].Name == "LNG_Key") {
				if (DT.Select("LNG_Key = '" + e.FormattedValue + "'").Length > 1) {
					//MessageBox.Show("Please use a unique key, \"" + e.FormattedValue + "\" is already taken", "Unique ID", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					//e.Cancel = true;
				}
			}
		}
        private void lngDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            lngFile.hasChanges = true;
            DataTable DT = (DataTable)lngDataGridView.DataSource;
            if (e.ColumnIndex == 0)
            {
                lngFile.Remove((string)((DataRowView)lngDataGridView.Rows[e.RowIndex].DataBoundItem).Row[0, DataRowVersion.Current], false);
            }
            lngFile.Add((string)DT.DefaultView[e.RowIndex][0], (string)DT.DefaultView[e.RowIndex][1]);
            // Does the same thing as above
            //lngFile.Add((string)lngDataGridView[0, e.RowIndex].Value, (string)lngDataGridView[1, e.RowIndex].Value);
        }
	}
}
