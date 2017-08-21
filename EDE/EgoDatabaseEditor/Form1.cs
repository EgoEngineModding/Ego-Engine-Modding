using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using DGVColumnSelector;
using DgvFilterPopup;
using System.Data;
using System.Collections.Generic;
using EgoEngineLibrary.Data;
using System.Linq;

namespace EgoDatabaseEditor
{
    public partial class Form1 : Form
    {
		// 2.2 -- Open Populated Tables, Close All Tables, Open Linked Tables, Merge Databases, Compare Databases, Code Overhaul, Open LinkedTable of Column on Cell by MiddleMouseClick, BeginEdit on RightClick
        // 2.2.1 -- Fixed Edit Row Form, Fixed Error When Pasting with Hidden Columns, Added F1 2012 support, Improve Open/SaveDlg
        // 11.0 -- Fixed Compare, F1 2013 Support, Minor UI Improvements, Pasting Change, Xml Internal Schema/Predict Schema, Dirt Import
        // ToDo -- Disable Constraints Button
        List<string> schemaPaths = new List<string>();
		DatabaseFile dbFile;
        string fileName = "";

        public Form1(string[] Args)
        {
            InitializeComponent();
            tmppppToolStripMenuItem.Visible = false;
            this.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
			dbFile = new DatabaseFile();
            // Load Schema List
            foreach (string schemaPath in Directory.GetFiles(Application.StartupPath, "schema*.xml", SearchOption.TopDirectoryOnly))
            {
                schemaPaths.Add(schemaPath);
            }
            if (Args.Length > 0)
            {
                if (File.Exists(Args[0]) == true)
                {
                    openController(Args[0], 0, 0);
                    openFileDialog.FileName = fileName;
                }
            }
        }
		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			if (dbFile.HasChanges() == true) {
				if (MessageBox.Show("The data in the database has changed." + Environment.NewLine + Environment.NewLine + "Are you sure you want to quit?", "Ryder Database Editor", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel) {
					e.Cancel = true;
				}
			}
		}

        #region MainMenu
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.FilterIndex = 1;
            if (!string.IsNullOrEmpty(fileName))
            {
                openFileDialog.FileName = Path.GetFileNameWithoutExtension(fileName);
                openFileDialog.InitialDirectory = Path.GetDirectoryName(fileName);
            }
			//openFileDialog.FileName = fileName;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                openController(openFileDialog.FileName, 0, 0);
				//List<string[]> errors = new List<string[]>();
                //convertToXML(openFileDialog.FileName, schemaPaths[3], errors);
                openFileDialog.Dispose();
            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(fileName);
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(fileName);
            //saveFileDialog.FileName = fileName.Replace(".xml", ".bin");
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                openController(saveFileDialog.FileName, 1, 0);
                saveFileDialog.Dispose();
            }
        }
        private void importAsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.FilterIndex = 2;
            if (!string.IsNullOrEmpty(fileName))
            {
                openFileDialog.FileName = Path.GetFileNameWithoutExtension(fileName);
                openFileDialog.InitialDirectory = Path.GetDirectoryName(fileName);
            }
			//openFileDialog.FileName = fileName.Replace(".bin", ".xml");
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
				openController(openFileDialog.FileName, 2, 0);
                openFileDialog.Dispose();
            }
        }
        private void exportAsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableListBox.Items.Count != 0)
            {
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(fileName);
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(fileName);
                //saveFileDialog.FileName = fileName.Replace(".bin", ".xml");
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
					openController(saveFileDialog.FileName, 3, 0);
                    saveFileDialog.Dispose();
                }
            }
            else
            {
                MessageBox.Show("There must already be an opened database in order to save it as an xml file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
        private void openController(string path, int conversionType, int i)
        {
            try
            {
                if (i == schemaPaths.Count)
                {
                    MessageBox.Show("The program failed to convert!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
					switch (conversionType) {
						case 0:
							ClearInterface();
							dbFile = new DatabaseFile(path, schemaPaths[i]);
							WriteErrorLog(dbFile.LoadErrors);
							for (int j = 0; j < dbFile.Tables.Count; j++) {
								tableListBox.Items.Add(dbFile.Tables[j].TableName);
							}
							fileName = path;
							this.Text = "Ego Database Editor - " + path;
							break;
						case 1:
							dbFile.Write(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read));
                            this.Text = "Ego Database Editor - " + path;
							break;
						case 2:
							ClearInterface();
							dbFile = new DatabaseFile(path);
							WriteErrorLog(dbFile.LoadErrors);
							for (int j = 0; j < dbFile.Tables.Count; j++) {
								tableListBox.Items.Add(dbFile.Tables[j].TableName);
							}
							fileName = path;
                            this.Text = "Ego Database Editor - " + path;
							break;
						case 3:
							dbFile.WriteXML(path);
                            this.Text = "Ego Database Editor - " + path;
							break;
						default:
							MessageBox.Show("Incorrect Conversion Type! 0 - binToXml, 1 - xmlToBin", "Incorrect Conversion Type", MessageBoxButtons.OK, MessageBoxIcon.Error);
							break;
					}
                }
            }
            catch
            {
				if (conversionType == 0) {
					openController(path, conversionType, i + 1);
				} else {
					openController(string.Empty, -1, schemaPaths.Count);
				}
            }
        }
		private void WriteErrorLog(List<string[]> errors) {
			if (errors.Count == 0) {
				return;
			}
			TabPage tp = new TabPage("Error Log");
			tp.Name = "errorLog";
			DataGridView dgv = new DataGridView();
			dgv.Visible = false;
			TextBox tb = new TextBox();
			tb.Multiline = true;
			tb.ReadOnly = true;
			tb.Dock = DockStyle.Fill;
			tb.Text = "The following errors may or may not break the game. You can continue to edit the database regularly but if you don't fix the following errors you may get unwanted results. NOTE: The program only searches for errors when opening/importing a file." + Environment.NewLine + Environment.NewLine;
			foreach (string[] error in errors) {
				foreach (string line in error) {
					tb.Text += line;
					tb.Text += Environment.NewLine;
				}
				tb.Text += Environment.NewLine;
			}
			tp.Controls.Add(dgv);
			tp.Controls.Add(tb);
			tabControl.TabPages.Add(tp);
			tabControl.SelectedIndex = tabControl.TabPages.Count - 1;
			tabControl.Focus();
			/*using (StreamWriter sw = new StreamWriter(File.Open(Application.StartupPath + "\\errorLog.txt", FileMode.Create, FileAccess.Write, FileShare.Read))) {
				foreach (string[] error in errors) {
					foreach (string line in error) {
						sw.Write(line);
						sw.Write(Environment.NewLine);
					}
					sw.Write(Environment.NewLine);
				}
			}*/
			//MessageBox.Show("The file opened successfully but has some data warnings/errors", "Data Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		private void openTableToolStripMenuItem_Click(object sender, EventArgs e) {
			// Display New Tab
			OpenTable((string)tableListBox.SelectedItem);
		}
		private void openPopulatedTablesToolStripMenuItem_Click(object sender, EventArgs e) {
			foreach (DataTable t in dbFile.Tables) {
				if (t.Rows.Count > 0) {
					OpenTable(t.TableName);
				}
			}
		}
		private void closeTabToolStripMenuItem_Click(object sender, EventArgs e) {
			if (tabControl.SelectedIndex == -1) {
				return;
			}
			((DataGridView)tabControl.TabPages[tabControl.SelectedIndex].Controls[0]).DataBindings.Clear();
			((DataGridView)tabControl.TabPages[tabControl.SelectedIndex].Controls[0]).DataSource = null;
			tabControl.TabPages.RemoveAt(tabControl.SelectedIndex);
			/*if (tabControl.SelectedIndex == -1)
			{
				return;
			}
			DialogResult saveChanges = MessageBox.Show("Would you like to save changes to this table before closing?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (saveChanges == DialogResult.Yes)
			{
				// Get table node index
				int tableIndex = 0;
				for (int i = 0; i < tableListBox.Items.Count; i++)
				{
					if ((string)tableListBox.Items[i] == tabControl.TabPages[tabControl.SelectedIndex].Text)
					{
						tableIndex = i;
						break;
					}
				}
				// Remove all child nodes of the table so they can be recreated based on dgv
				DXML.DocumentElement.ChildNodes[tableIndex].RemoveAll();
				DataGridView dgv = (DataGridView)tabControl.TabPages[tabControl.SelectedIndex].Controls[0];
				DXML.DocumentElement.ChildNodes[tableIndex].Attributes.Append(DXML.CreateAttribute("name"));
				DXML.DocumentElement.ChildNodes[tableIndex].Attributes["name"].InnerText = tabControl.TabPages[tabControl.SelectedIndex].Text;
				DXML.DocumentElement.ChildNodes[tableIndex].Attributes.Append(DXML.CreateAttribute("id"));
				DXML.DocumentElement.ChildNodes[tableIndex].Attributes["id"].InnerText = tableIndex.ToString();
				DXML.DocumentElement.ChildNodes[tableIndex].Attributes.Append(DXML.CreateAttribute("num"));
				DXML.DocumentElement.ChildNodes[tableIndex].Attributes["num"].InnerText = (dgv.Rows.Count - 1).ToString();
				foreach (DataGridViewRow item in dgv.Rows)
				{
					if (item == dgv.Rows[dgv.Rows.Count - 1])
					{
						continue;
					}
					DXML.DocumentElement.ChildNodes[tableIndex].AppendChild(DXML.CreateElement(tabControl.TabPages[tabControl.SelectedIndex].Text));
					for (int i = 0; i < dgv.Columns.Count; i++)
					{
						XmlTextReader xmlReader = new XmlTextReader(new StringReader(dgv.Columns[i].ToolTipText));
						XmlNode field = DXML.ReadNode(xmlReader);
						field.InnerText = Convert.ToString(item.Cells[i].Value);
						DXML.DocumentElement.ChildNodes[tableIndex].LastChild.AppendChild(field);
					}
				}
				// Close the Tab
				tabControl.TabPages.RemoveAt(tabControl.SelectedIndex);
			}
			else if (saveChanges == DialogResult.No)
			{
				// Close the Tab
				tabControl.TabPages.RemoveAt(tabControl.SelectedIndex);
			}*/
		}
		private void closeAllTablesToolStripMenuItem_Click(object sender, EventArgs e) {
			foreach (TabPage tP in tabControl.TabPages) {
				((DataGridView)tP.Controls[0]).DataBindings.Clear();
				((DataGridView)tP.Controls[0]).DataSource = null;
				tabControl.TabPages.Remove(tP);
			}
		}
		private void addRowToolStripMenuItem_Click(object sender, EventArgs e) {
			if (tabControl.SelectedIndex == -1) {
				return;
			}
			DataGridView dgv = ((DataGridView)tabControl.TabPages[tabControl.SelectedIndex].Controls[0]);
			if (dgv.Columns.Count == 0) {
				return;
			}
			DataTable dT = (DataTable)dgv.DataSource;
			RowEdit rE = new RowEdit(dbFile, dT, -1);
			if (rE.ShowDialog() == DialogResult.OK) {
				dT.Rows.Add(rE.dr);
			}
		}
		private void addMultipleRowsToolStripMenuItem_Click(object sender, EventArgs e) {
			if (tabControl.SelectedIndex == -1) {
				return;
			}
			DataGridView dgv = ((DataGridView)tabControl.TabPages[tabControl.SelectedIndex].Controls[0]);
			if (dgv.Columns.Count == 0) {
				return;
			}
			DataTable dT = (DataTable)dgv.DataSource;
			AddMultipleRows aMR = new AddMultipleRows();
			if (aMR.ShowDialog() == DialogResult.OK) {
				if (aMR.Amount != -1) {
					for (int i = 0; i < aMR.Amount; i++) {
						dT.Rows.Add(CreateNewRow(dT));
					}
				}
			}
		}
		private void editRowToolStripMenuItem_Click(object sender, EventArgs e) {
			if (tabControl.SelectedIndex == -1) {
				return;
			}
			DataGridView dgv = ((DataGridView)tabControl.TabPages[tabControl.SelectedIndex].Controls[0]);
			DataTable dT = (DataTable)dgv.DataSource;
			if (dgv.SelectedRows.Count == 0) {
				MessageBox.Show("No rows were selected", "No Row Selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			RowEdit rE = new RowEdit(dbFile, dT, dgv.SelectedRows[0].Index);
			if (rE.ShowDialog() == DialogResult.OK) {
				dT.DefaultView[dgv.SelectedRows[0].Index].Row.ItemArray = rE.dr.ItemArray;
			}
		}
		private void removeRowToolStripMenuItem_Click(object sender, EventArgs e) {
			if (tabControl.SelectedIndex == -1) {
				return;
			}
			DataGridView dgv = ((DataGridView)tabControl.TabPages[tabControl.SelectedIndex].Controls[0]);
			DataTable dT = (DataTable)dgv.DataSource;
			if (dgv.SelectedRows.Count == 0) {
				MessageBox.Show("No rows were selected", "No Row Selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			foreach (DataGridViewRow row in dgv.SelectedRows) {
				dT.DefaultView[row.Index].Delete();
			}
		}
		private void showHideSidebarToolStripMenuItem_Click(object sender, EventArgs e) {
			leftPanel.Visible = !leftPanel.Visible;
		}

		private void openLinkedTablesToolStripMenuItem_Click(object sender, EventArgs e) {
			if (tabControl.SelectedIndex < 0) {
				return;
			}

			DataGridView dgv = ((DataGridView)tabControl.TabPages[tabControl.SelectedIndex].Controls[0]);
			DataTable child = (DataTable)dgv.DataSource;
			if (child.ExtendedProperties.Count == 0) {
				return;
			}
			foreach (DataColumn col in child.Columns) {
				if (child.ExtendedProperties.ContainsKey(col.ColumnName) == true) {
					OpenTable((string)child.ExtendedProperties[col.ColumnName]);
				}
			}
		}
		private void compareDatabasesToolStripMenuItem_Click(object sender, EventArgs e) {
			openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
				try
                {
                    DatabaseFile two = new DatabaseFile(openFileDialog.FileName, Path.Combine(Application.StartupPath, dbFile.Namespace));
                    openFileDialog.Dispose();
                    saveFileDialog.FilterIndex = 2;
                    saveFileDialog.FileName = fileName.Replace(".bin", "Differences.xml");
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        dbFile.GetDifferences(two).WriteXML(saveFileDialog.FileName);
                        saveFileDialog.Dispose();
                    }
                }
				catch (Exception ex) {
					MessageBox.Show("The program failed to compare the files:" + Environment.NewLine + Environment.NewLine + ex.Message, "Ryder Database Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
		private void mergeDatabasesToolStripMenuItem_Click(object sender, EventArgs e) {
			openFileDialog.FilterIndex = 2;
			openFileDialog.FileName = fileName.Replace(".bin", "Differences.xml");
			if (openFileDialog.ShowDialog() == DialogResult.OK) {
				try {
					DatabaseFile two = new DatabaseFile(openFileDialog.FileName);
					openFileDialog.Dispose();
					dbFile.Merge(two);
				}
				catch (Exception ex) {
					MessageBox.Show("The program failed to merge the files:" + Environment.NewLine + Environment.NewLine + ex.Message, "Ryder Database Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
        #endregion

        public void ClearInterface()
        {
            this.Text = "Ego Database Editor";
			dbFile = new DatabaseFile();
            tabControl.TabPages.Clear();
            tableListBox.Items.Clear();
        }

        private void searchMaskedTextBox_KeyDown(object sender, KeyEventArgs e)
        {//OLD
            if (e.KeyCode == Keys.Enter)
            {
                for (int i = 0; i < tableListBox.Items.Count; i++)
                {
                    if ((string)tableListBox.Items[i] == searchTextBox.Text)
                    {
                        tableListBox.SelectedIndex = i;
                        tableListBox.Focus();
                        return;
                    }
                }
            }
        }
		private void searchTextBox_TextChanged(object sender, EventArgs e) {
			tableListBox.Items.Clear();

			foreach (DataTable dt in dbFile.Tables) {
				if (dt.TableName.StartsWith(searchTextBox.Text, StringComparison.CurrentCultureIgnoreCase) ||
					dt.TableName.Contains(searchTextBox.Text.ToLower())) {
					tableListBox.Items.Add(dt.TableName);
				}
			}
		}

        private void tableListBox_DoubleClick(object sender, EventArgs e)
        {
            // Display New Tab
			OpenTable((string)tableListBox.SelectedItem);
        }
		void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
			// Display New Tab
			if (e.ColumnIndex < 0 || e.RowIndex < 0 || ((DataGridView)sender).Columns.Count == 0) {
				return;
			}
			DataTable child = (DataTable)((DataGridView)sender).DataSource;
			if (child.ExtendedProperties.Count == 0) {
				return;
			}
			if (child.ExtendedProperties.ContainsKey(((DataGridView)sender).Columns[e.ColumnIndex].Name) == true) {
				OpenTable((string)child.ExtendedProperties[((DataGridView)sender).Columns[e.ColumnIndex].Name]);
			}
		}
		void dgv_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e) {
			if (e.ColumnIndex < 0 || e.RowIndex < 0 || ((DataGridView)sender).Columns.Count == 0) {
				return;
			}
			if (e.Button == System.Windows.Forms.MouseButtons.Middle) {
				DataTable child = (DataTable)((DataGridView)sender).DataSource;
				if (child.ExtendedProperties.Count == 0) {
					return;
				}
				if (child.ExtendedProperties.ContainsKey(((DataGridView)sender).Columns[e.ColumnIndex].Name) == true) {
					OpenTable((string)child.ExtendedProperties[((DataGridView)sender).Columns[e.ColumnIndex].Name]);
				}
			} else if (e.Button == System.Windows.Forms.MouseButtons.Right) {
				((DataGridView)sender).CurrentCell = ((DataGridView)sender)[e.ColumnIndex, e.RowIndex];
				((DataGridView)sender).BeginEdit(true);
			}
		}
		private void OpenTable(string tableName) {
			// Display New Tab
			if (tabControl.TabPages.ContainsKey(tableName) == true || tableListBox.SelectedIndex == -1) {
				tabControl.SelectedTab = tabControl.TabPages[tableName];
				return;
			}
			TabPage tp = new TabPage(tableName);
			tp.Name = tableName;

			DataGridView dgv = new DataGridView();
			dgv.Dock = DockStyle.Fill;
			dgv.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#E8EDFF");//#E0E0E0
			dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
			dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
			dgv.AllowUserToAddRows = false;
			dgv.KeyDown += new KeyEventHandler(dgv_KeyDown);
			dgv.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(dgv_DataBindingComplete);
			dgv.DataError += new DataGridViewDataErrorEventHandler(dgv_DataError);
			//dgv.CellDoubleClick += new DataGridViewCellEventHandler(dgv_CellDoubleClick);
			dgv.CellMouseDown += new DataGridViewCellMouseEventHandler(dgv_CellMouseDown);
			dgv.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(dgv_ColumnHeaderMouseClick);
            dgv.MouseEnter += dgv_MouseEnter;
			dgv.DataSource = dbFile.Tables[tableName];
			DataGridViewColumnSelector dgvcs = new DataGridViewColumnSelector(dgv);
			DgvFilterManager dgvfm = new DgvFilterManager(dgv);

			tp.Controls.Add(dgv);
			tabControl.TabPages.Add(tp);
			foreach (DataColumn col in dbFile.Tables[tableName].Columns) {
				if (col.DataType == typeof(string)) {
					if (dgv.Columns[col.ColumnName] is DataGridViewTextBoxColumn && col.MaxLength >= 0) {
						((DataGridViewTextBoxColumn)dgv.Columns[col.ColumnName]).MaxInputLength = col.MaxLength;
					}
				}
			}
			tabControl.SelectedIndex = tabControl.TabPages.Count - 1;
			//tabControl.Focus();
		}

		private void dgv_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
			if (e.Button == System.Windows.Forms.MouseButtons.Right) {
				return;
			}
			DataGridView dgv = (DataGridView)sender;
			DataTable dt = (DataTable)dgv.DataSource;
			if (dt.ExtendedProperties.ContainsKey("stopSort") == false) {
				dt.ExtendedProperties.Add("stopSort", new object[2] { false, -1 });
			}
			bool stopSort = (bool)((object[])dt.ExtendedProperties["stopSort"])[0];
			int indexSort = (int)((object[])dt.ExtendedProperties["stopSort"])[1];
			if (indexSort == e.ColumnIndex) {
				if (dgv.SortedColumn.HeaderCell.SortGlyphDirection == SortOrder.Descending) {
					stopSort = true;
					indexSort = dgv.SortedColumn.DisplayIndex;
				} else if (stopSort == true) {
					dgv.SortedColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
					dt.DefaultView.Sort = "";
					stopSort = false;
					indexSort = -1;
				}
			} else {
				stopSort = false;
				indexSort = dgv.SortedColumn.DisplayIndex;
			}
			((object[])dt.ExtendedProperties["stopSort"])[0] = stopSort;
			((object[])dt.ExtendedProperties["stopSort"])[1] = indexSort;
		}
        private void dgv_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
			MessageBox.Show("Incorrect data was entered into the cell!" + Environment.NewLine + Environment.NewLine +
			e.Exception.Message, "Bad Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
			DataGridView dataGridView = ((DataGridView)sender);
			dataGridView.CancelEdit();
			/*if (e.Context == DataGridViewDataErrorContexts.Commit) {

				dataGridView = ((DataGridView)sender);
				string valType = "";
				if (dataGridView.Columns[e.ColumnIndex].ValueType == typeof(float)) {
					valType = "a float (ex: 0.6).";
				} else if (dataGridView.Columns[e.ColumnIndex].ValueType == typeof(int)) {
					valType = "an int (ex: 5).";
				} else if (dataGridView.Columns[e.ColumnIndex].ValueType == typeof(string)) {
					valType = "a string (ex: this_is text).";
				} else if (dataGridView.Columns[e.ColumnIndex].ValueType == typeof(bool)) {
					valType = "a bool (ex: True/False).";
				} else {
					valType = "a proper value.";
				}
				//dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "The value entered was wrong. The original value is reset";

				//MessageBox.Show("Incorrect value type, you must enter " + valType, "Bad Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}*/
        }
        private void dgv_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
        }
        private void dgv_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                DataObject d = ((DataGridView)sender).GetClipboardContent();
                Clipboard.SetDataObject(d);
                e.Handled = true;
            }
            else if (e.Shift && e.KeyCode == Keys.D6)
            {
                //MessageBox.Show(DDS.HasChanges().ToString());
                ((DataTable)((DataGridView)sender).DataSource).DefaultView.Sort = "";
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                DataGridView grid = ((DataGridView)sender);
                char[] rowSplitter = { '\r', '\n' };
                char[] columnSplitter = { '\t' };

                // Get the text from clipboard
                IDataObject dataInClipboard = Clipboard.GetDataObject();
                string stringInClipboard = (string)dataInClipboard.GetData(DataFormats.Text);
                if (stringInClipboard == null)
                {
                    e.Handled = true;
                    return;
                }

                // Split it into lines
                string[] rowsInClipboard = stringInClipboard.Split(rowSplitter, StringSplitOptions.RemoveEmptyEntries);

                // Get the row and column of selected cell in grid
                if (grid.SelectedCells.Count == 0)
                {
                    e.Handled = true;
                    return;
                }
                int r = grid.SelectedCells[0].RowIndex;
                int c = grid.SelectedCells[0].ColumnIndex;
                if (grid.SelectedCells.Count > 1)
                {
                    foreach (DataGridViewCell cell in grid.SelectedCells)
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

                // Make a backup of the current table
                DataTable table = (DataTable)grid.DataSource;
                DataTable tableC = table.GetChanges();
                if (tableC != null)
                    tableC.AcceptChanges();

                // Add rows if necessary
                DialogResult addRow = DialogResult.None;
                if (rowsInClipboard.Length + r > grid.RowCount)
                {
                    addRow = MessageBox.Show("The data in the clipboard has more rows than the datagrid. You can choose to add these rows or ignore them." + Environment.NewLine + Environment.NewLine + "Do you want to add these rows?", "Add Rows?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (addRow == DialogResult.Yes)
                    {
                        int startRowCount = grid.RowCount;
                        for (int i = 0; i < (rowsInClipboard.Length + r - startRowCount); i++)
                        {
                            table.Rows.Add(CreateNewRow(table));
                        }
                    }
                }

                // Get Visible Columns
                List<string> visibleColumnNames = new List<string>();
                for (int i = c; i < grid.Columns.Count; i++)
                {
                    if (grid.Columns[i].Visible)
                    {
                        visibleColumnNames.Add(grid.Columns[i].Name);
                    }
                }

                // Loop through the lines, split them into cells and place the values in the corresponding cell.
                Dictionary<string[], string> cellIndexes = new Dictionary<string[], string>();
                for (int iRow = 0; iRow < rowsInClipboard.Length; iRow++)
                {
                    if (grid.RowCount - 1 >= r + iRow)
                    {
                        //split row into cell values
                        string[] valuesInRow = rowsInClipboard[iRow].Split(columnSplitter);
                        //cycle through cell values
                        for (int iCol = 0; iCol < valuesInRow.Length; iCol++)
                        {
                            //assign cell value, only if it within columns of the grid
                            if (visibleColumnNames.Count - 1 >= iCol)
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(valuesInRow[iCol]) && table.Columns[visibleColumnNames[iCol]].DataType != typeof(string))
                                    {
                                        continue;
                                    }
                                    cellIndexes.Add(new string[2] { table.Rows.IndexOf(table.DefaultView[r + iRow].Row).ToString(), visibleColumnNames[iCol] }, valuesInRow[iCol]);
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

                // Assign the values and check for errors
                bool error = false;
                string eMessage = "";
                try
                {
                    string sort = table.DefaultView.Sort;
                    table.DefaultView.Sort = "";
                    foreach (KeyValuePair<string[], string> index in cellIndexes)
                    {
                        table.Rows[Convert.ToInt32(index.Key[0])].BeginEdit();
                        table.Rows[Convert.ToInt32(index.Key[0])][index.Key[1]] = Convert.ChangeType(index.Value, table.Columns[index.Key[1]].DataType);
                        table.Rows[Convert.ToInt32(index.Key[0])].EndEdit();
                    }
                    grid.CurrentCell.Selected = false;
                    grid.CurrentCell = grid.Rows[r].Cells[c];
                    table.DefaultView.Sort = sort;
                }
                catch (Exception ex)
                {
                    error = true;
                    eMessage += ex.Message + Environment.NewLine;
                }
                if (error == true)
                {
                    table.RejectChanges();
                    if (tableC != null)
                    {
                        table.Merge(tableC, false);
                        // Hack to ask to save on quit
                        try { table.Rows[0].BeginEdit(); table.Rows[0][0] = table.Rows[0][0]; table.Rows[0].EndEdit(); }
                        catch { }
                    }
                    MessageBox.Show("Pasting data failed, the table values were reset! Errors:" + Environment.NewLine + Environment.NewLine +
                    eMessage, "Bad Paste", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                e.Handled = true;
            }
        }
        private void dgv_MouseEnter(object sender, EventArgs e)
        {
            this.ActiveControl = (DataGridView)sender;
        }
		private DataRow CreateNewRow(DataTable dt) {
			DataRow dr;
			List<object> items = new List<object>();
			dr = dt.NewRow();
			DataGridViewRow row;
			DataGridViewCell cell;
			DataColumn primaryKeyColumn = dt.PrimaryKey.Length == 0 ? null : dt.PrimaryKey[0];
			for (int i = 0; i < dt.Columns.Count; i++) {
				row = new DataGridViewRow();
				foreach (DataRelation dataRel in dt.ParentRelations) {
					if (dataRel.ChildColumns[0] == dt.Columns[i]) {
						cell = new DataGridViewComboBoxCell();
						foreach (DataRow drR in dbFile.Tables[dataRel.ParentColumns[0].Table.TableName].Rows) {
							if (drR.RowState != DataRowState.Deleted) {
								((DataGridViewComboBoxCell)cell).Items.Add(drR.ItemArray[dataRel.ParentColumns[0].Ordinal]);
							}
							if (((DataGridViewComboBoxCell)cell).Items.Count > 0) {
								break;
							}
						}
						row.Cells.Add(cell);
						break;
					}
				}
				if (row.Cells.Count == 0) {
					row.Cells.Clear();
					if (dt.Columns[i].DataType == typeof(bool)) {
						cell = new DataGridViewCheckBoxCell();
						row.Cells.Add(cell);
					} else if (dt.Columns[i].DataType == typeof(string)) {
						cell = new DataGridViewTextBoxCell();
						row.Cells.Add(cell);
					} else if (dt.Columns[i].DataType == typeof(int) || dt.Columns[i].DataType == typeof(float)) {
						cell = new DataGridViewTextBoxCell();
						row.Cells.Add(cell);
					} else {
						cell = new DataGridViewTextBoxCell();
						row.Cells.Add(cell);
					}
				}
				if (row.Cells[0] is DataGridViewComboBoxCell) {
					if (((DataGridViewComboBoxCell)row.Cells[0]).Items.Count > 0) {
						((DataGridViewComboBoxCell)row.Cells[0]).Value = ((DataGridViewComboBoxCell)row.Cells[0]).Items[0];
					} else {
						((DataGridViewComboBoxCell)row.Cells[0]).Items.Add(dt.Columns[i].DefaultValue);
						((DataGridViewComboBoxCell)row.Cells[0]).Value = dt.Columns[i].DefaultValue;
					}
				} else {
					row.Cells[0].Value = dt.Columns[i].DefaultValue;
				}
				row.Cells[0].ValueType = dt.Columns[i].DataType;
				// Set Up PrimaryKey Row Values for Validation
				if (dt.Columns[i] == primaryKeyColumn) {
					int largestIndex = 1;
					foreach (DataRow dRPK in dt.Rows) {
						if (dRPK.RowState != DataRowState.Deleted) {
							if (dRPK.ItemArray[i] is int) {
								largestIndex = Math.Max(largestIndex, (int)dRPK.ItemArray[i]);
							}
						}
					}
					if (row.Cells[0].ValueType == typeof(int)) {
						row.Cells[0].Value = largestIndex + 1;
					}
				}
				items.Add(row.Cells[0].Value);
			}
			// Finish
			dr.ItemArray = items.ToArray();
			return dr;
		}

		private void tabControl_MouseDoubleClick(object sender, MouseEventArgs e) {
			if (tabControl.SelectedIndex < 0) {
				return;
			}
			Rectangle r = tabControl.GetTabRect(tabControl.SelectedIndex);
			if (r.Contains(e.Location) == true) {
				((DataGridView)tabControl.TabPages[tabControl.SelectedIndex].Controls[0]).DataBindings.Clear();
				((DataGridView)tabControl.TabPages[tabControl.SelectedIndex].Controls[0]).DataSource = null;
				tabControl.TabPages.RemoveAt(tabControl.SelectedIndex);
			}
		}
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
                this.ActiveControl = tabControl.SelectedTab.Controls[0];
        }
        private void tabControl_MouseEnter(object sender, EventArgs e)
        {
            this.ActiveControl = tabControl;
        }

        private void tableListBox_MouseEnter(object sender, EventArgs e)
        {
            this.ActiveControl = tableListBox;
            //if (!tableListBox.Focused)
                //tableListBox.Focus();
        }

        private void toggleConstraintsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dbFile.EnforceConstraints = false;//!dbFile.EnforceConstraints;
        }

        private void tmppppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int[] count = new int[703];
            List<int> teamId = new List<int>();
            List<DataRow> dataRow = new List<DataRow>();

            foreach (DataRow dR in dbFile.Tables["driver"].Rows)
            {
                count[(int)dR.ItemArray[15]]++;
                if ((int)dR.ItemArray[15] == 1)
                {
                    //dataRow.Add(dR);
                }
            }
            for (int i = 0; i < count.Length; i++)
            {
                if (count[i] <= 2 && count[i] >= 1)
                {
                    teamId.Add(i);
                }
            }

            // Unused Teams
            HashSet<int> usedTeam = new HashSet<int>();
            string output = string.Empty;
            foreach (DataRow dR in dbFile.Tables["event_driver"].Rows)
            {
                int driverId = (int)dR.ItemArray[0];
                DataRow[] drivers = dbFile.Tables["driver"].Select("id = " + driverId);
                if (drivers.Length < 1) continue;
                int teamId1 = (int)drivers[0].ItemArray[15];
                if (!usedTeam.Contains(teamId1))
                {
                    usedTeam.Add(teamId1);
                }
            }
            foreach (DataRow dR in dbFile.Tables["lemans_restrictions"].Rows)
            {
                int driverId = (int)dR.ItemArray[0];
                DataRow[] drivers = dbFile.Tables["driver"].Select("id = " + driverId);
                if (drivers.Length < 1) continue;
                int teamId1 = (int)drivers[0].ItemArray[15];
                if (!usedTeam.Contains(teamId1))
                {
                    usedTeam.Add(teamId1);
                }
            }
            foreach (DataRow dR in dbFile.Tables["team"].Rows)
            {
                int teamId1 = (int)dR.ItemArray[0];
                if (!usedTeam.Contains(teamId1))
                {
                    output += teamId1 + Environment.NewLine;
                }
            }

            // Unused Livery
            HashSet<int> usedLivery = new HashSet<int>();
            string output2 = string.Empty;
            foreach (DataRow dR in dbFile.Tables["event_driver"].Rows)
            {
                int liveryId = (int)dR.ItemArray[3];
                DataRow[] liveries = dbFile.Tables["vehicle_livery"].Select("id = " + liveryId);
                if (liveries.Length < 1) continue;
                if (!usedLivery.Contains(liveryId))
                {
                    usedLivery.Add(liveryId);
                }
            }
            foreach (DataRow dR in dbFile.Tables["lemans_restrictions"].Rows)
            {
                int liveryId = (int)dR.ItemArray[1];
                DataRow[] liveries = dbFile.Tables["vehicle_livery"].Select("id = " + liveryId);
                if (liveries.Length < 1) continue;
                if (!usedLivery.Contains(liveryId))
                {
                    usedLivery.Add(liveryId);
                }
            }
            foreach (DataRow dR in dbFile.Tables["vehicle_livery"].Rows)
            {
                int livId = (int)dR.ItemArray[2];
                if (!usedLivery.Contains(livId))
                {
                    output2 += livId + " " + dR.ItemArray[1] + " " + dR.ItemArray[5] + Environment.NewLine;
                }
            }
            Clipboard.SetText(output2);

            MessageBox.Show(output);
            MessageBox.Show(output2);
            MessageBox.Show(teamId[3].ToString());
            MessageBox.Show(dataRow.Count.ToString());
        }
    }
}

/* OLD PASTE Oct 6 2013
				dbFile.EnforceConstraints = false;
				DataGridView grid = ((DataGridView)sender);
				char[] rowSplitter = { '\r', '\n' };
				char[] columnSplitter = { '\t' };

				// Get the text from clipboard
				IDataObject dataInClipboard = Clipboard.GetDataObject();
				string stringInClipboard = (string)dataInClipboard.GetData(DataFormats.Text);
				if (stringInClipboard == null) {
					e.Handled = true;
					return;
				}

				// Split it into lines
				string[] rowsInClipboard = stringInClipboard.Split(rowSplitter, StringSplitOptions.RemoveEmptyEntries);

				// Get the row and column of selected cell in grid
				if (grid.SelectedCells.Count == 0) {
					e.Handled = true;
					return;
				}
				int r = grid.SelectedCells[0].RowIndex;
				int c = grid.SelectedCells[0].ColumnIndex;
				if (grid.SelectedCells.Count > 1) {
					foreach (DataGridViewCell cell in grid.SelectedCells) {
						if (cell.RowIndex <= r && cell.RowIndex >= 0) {
							r = cell.RowIndex;
							if (cell.ColumnIndex < c && cell.ColumnIndex >= 0) {
								c = cell.ColumnIndex;
							}
						}
					}
				}

                // Make a backup of the current table
				DataTable table = (DataTable)grid.DataSource;
				DataTable tableC = table.Copy();
				tableC.AcceptChanges();

				// Add rows if necessary
				DialogResult addRow = DialogResult.None;
				if (rowsInClipboard.Length + r > grid.RowCount) {
					addRow = MessageBox.Show("The data in the clipboard has more rows than the datagrid. You can choose to add these rows or ignore them." + Environment.NewLine + Environment.NewLine + "Do you want to add these rows?", "Add Rows?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (addRow == DialogResult.Yes) {
						int startRowCount = grid.RowCount;
						for (int i = 0; i < (rowsInClipboard.Length + r - startRowCount); i++) {
							table.Rows.Add(CreateNewRow(table));
						}
					}
				}

                // Get Visible Columns
                List<string> visibleColumnNames = new List<string>();
                for (int i = c; i < grid.Columns.Count; i++)
                {
                    if (grid.Columns[i].Visible)
                    {
                        visibleColumnNames.Add(grid.Columns[i].Name);
                    }
                }

                // Loop through the lines, split them into cells and place the values in the corresponding cell.
				Dictionary<string[], string> cellIndexes = new Dictionary<string[], string>();
				for (int iRow = 0; iRow < rowsInClipboard.Length; iRow++) {
					if (grid.RowCount - 1 >= r + iRow) {
						//split row into cell values
						string[] valuesInRow = rowsInClipboard[iRow].Split(columnSplitter);
						//cycle through cell values
						for (int iCol = 0; iCol < valuesInRow.Length; iCol++) {
							//assign cell value, only if it within columns of the grid
							if (visibleColumnNames.Count - 1 >= iCol) {
								try {
                                    if (string.IsNullOrEmpty(valuesInRow[iCol]) && table.Columns[visibleColumnNames[iCol]].DataType != typeof(string))
                                    {
										continue;
									}
                                    if (table.Columns[visibleColumnNames[iCol]].DataType == typeof(string) 
                                        && table.Columns[visibleColumnNames[iCol]].MaxLength < valuesInRow[iCol].Length
                                        && table.Columns[visibleColumnNames[iCol]].MaxLength >= 0)
                                    {
                                        //e.Handled = true;
                                        //return;
                                    }
									cellIndexes.Add(new string[2] { table.Rows.IndexOf(table.DefaultView[r + iRow].Row).ToString(), visibleColumnNames[iCol] }, valuesInRow[iCol]);
								}
								catch { }
							} else {
								break;
							}
						}
					} else {
						break;
					}
				}

                // Assign the values and check for errors
				bool error = false;
				string eMessage = "";
				try {
					string sort = table.DefaultView.Sort;
					table.DefaultView.Sort = "";
					foreach (KeyValuePair<string[], string> index in cellIndexes) {
                        //MessageBox.Show(index.Key[1]);
						table.Rows[Convert.ToInt32(index.Key[0])].BeginEdit();
						table.Rows[Convert.ToInt32(index.Key[0])][index.Key[1]] = Convert.ChangeType(index.Value, table.Columns[index.Key[1]].DataType);
						table.Rows[Convert.ToInt32(index.Key[0])].EndEdit();
					}
					grid.CurrentCell.Selected = false;
					grid.CurrentCell = grid.Rows[r].Cells[c];
					table.DefaultView.Sort = sort;
				}
				catch (Exception ex) {
					error = true;
					eMessage += ex.Message + Environment.NewLine;
				}
				try {
					dbFile.EnforceConstraints = true;
				}
				catch (Exception ex) {
					error = true;
					eMessage += ex.Message + Environment.NewLine;
				}
				if (error == true) {
					grid.DataSource = null;
					grid.Rows.Clear();
					grid.Columns.Clear();
					table = tableC;
					grid.DataSource = table;
					MessageBox.Show("Pasting data failed, the table values were reset! Errors:" + Environment.NewLine + Environment.NewLine +
					eMessage, "Bad Paste", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				e.Handled = true;
*/