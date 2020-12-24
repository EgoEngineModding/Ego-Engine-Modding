using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EgoDatabaseEditor {
	public partial class RowEdit : Form {
		public DataRow dr;
		DataSet DDS;
		private int primaryKeyRowIndex;

		public RowEdit(DataSet dds, DataTable dt, int rowIndex) {
			InitializeComponent();
			DDS = dds;
			dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
			dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
			dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
			dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			dataGridView.Columns.Add("values", "Values");
			dataGridView.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
			dataGridView.TopLeftHeaderCell.Value = dt.TableName;
			dataGridView.TopLeftHeaderCell.Tag = dt;
			dr = dt.NewRow();
			DataGridViewRow row;
			DataGridViewCell cell;
			DataColumn primaryKeyColumn = dt.PrimaryKey.Length == 0 ? null : dt.PrimaryKey[0];
			for (int i = 0; i < dt.Columns.Count; i++) {
				row = new DataGridViewRow();
				foreach (DataRelation dataRel in dt.ParentRelations) {
					if (dataRel.ChildColumns[0] == dt.Columns[i]) {
						cell = new DataGridViewComboBoxCell();
						cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
						foreach (DataRow drR in DDS.Tables[dataRel.ParentColumns[0].Table.TableName].Rows) {
							if (drR.RowState != DataRowState.Deleted) {
								((DataGridViewComboBoxCell)cell).Items.Add(drR.ItemArray[dataRel.ParentColumns[0].Ordinal]);
							}
						}
						row.Cells.Add(cell);
						row.Tag = dataRel.ParentColumns[0].Table;
						break;
					}
				}
				if (row.Cells.Count == 0 || rowIndex == -2) {
					row.Cells.Clear();
					if (dt.Columns[i].DataType == typeof(bool)) {
						cell = new DataGridViewCheckBoxCell();
						cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
						row.Cells.Add(cell);
					} else if (dt.Columns[i].DataType == typeof(string)) {
						cell = new DataGridViewTextBoxCell();
						row.Cells.Add(cell);
					} else if (dt.Columns[i].DataType == typeof(int) || dt.Columns[i].DataType == typeof(float)) {
						cell = new DataGridViewTextBoxCell();
						cell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
						row.Cells.Add(cell);
					} else {
						cell = new DataGridViewTextBoxCell();
						row.Cells.Add(cell);
					}
				}
				dataGridView.Rows.Add(row);
				dataGridView.Rows[i].HeaderCell.Value = dt.Columns[i].ColumnName;
				if (rowIndex != -1) {
					if (dataGridView.Rows[i].Cells[0] is DataGridViewComboBoxCell) {
						if (((DataGridViewComboBoxCell)dataGridView.Rows[i].Cells[0]).Items.Count > 0) {
							if (((DataGridViewComboBoxCell)dataGridView.Rows[i].Cells[0]).Items.Contains(dt.DefaultView[rowIndex].Row.ItemArray[i]) == true) {
								((DataGridViewComboBoxCell)dataGridView.Rows[i].Cells[0]).Value = dt.DefaultView[rowIndex].Row.ItemArray[i];
							} else {
								((DataGridViewComboBoxCell)dataGridView.Rows[i].Cells[0]).Items.Add(dt.DefaultView[rowIndex].Row.ItemArray[i]);
								((DataGridViewComboBoxCell)dataGridView.Rows[i].Cells[0]).Value = dt.DefaultView[rowIndex].Row.ItemArray[i];
							}
						} else {
							((DataGridViewComboBoxCell)dataGridView.Rows[i].Cells[0]).Items.Add(dt.DefaultView[rowIndex].Row.ItemArray[i]);
							((DataGridViewComboBoxCell)dataGridView.Rows[i].Cells[0]).Value = dt.DefaultView[rowIndex].Row.ItemArray[i];
						}
					} else {
						dataGridView.Rows[i].Cells[0].Value = dt.DefaultView[rowIndex].Row.ItemArray[i];
					}
				} else {
					if (dataGridView.Rows[i].Cells[0] is DataGridViewComboBoxCell) {
						if (((DataGridViewComboBoxCell)dataGridView.Rows[i].Cells[0]).Items.Count > 0) {
							((DataGridViewComboBoxCell)dataGridView.Rows[i].Cells[0]).Value = ((DataGridViewComboBoxCell)dataGridView.Rows[i].Cells[0]).Items[0];
						} else {
							((DataGridViewComboBoxCell)dataGridView.Rows[i].Cells[0]).Items.Add(dt.Columns[i].DefaultValue);
							((DataGridViewComboBoxCell)dataGridView.Rows[i].Cells[0]).Value = dt.Columns[i].DefaultValue;
						}
					} else {
						dataGridView.Rows[i].Cells[0].Value = dt.Columns[i].DefaultValue;
					}
				}
				dataGridView.Rows[i].Cells[0].ValueType = dt.Columns[i].DataType;
				if (dt.Columns[i].MaxLength >= 0) {
					if (dataGridView.Rows[i].Cells[0] is DataGridViewTextBoxCell) {
						((DataGridViewTextBoxCell)dataGridView.Rows[i].Cells[0]).MaxInputLength = dt.Columns[i].MaxLength;
					}
				}
				// Set Up PrimaryKey Row Values for Validation
				if (dt.Columns[i] == primaryKeyColumn) {
					primaryKeyRowIndex = i;
					dataGridView.Rows[i].Tag = new List<object>();
					int largestIndex = 1;
					foreach (DataRow dRPK in dt.Rows) {
						if (dRPK.RowState != DataRowState.Deleted && !dataGridView.Rows[i].Cells[0].Value.Equals(dRPK.ItemArray[i])) {
							((List<object>)dataGridView.Rows[i].Tag).Add(dRPK.ItemArray[i]);
							if (dRPK.ItemArray[i] is int) {
								largestIndex = Math.Max(largestIndex, (int)dRPK.ItemArray[i]);
							}
						}
					}
					// If a new row, give it next unique index
					if (rowIndex == -1) {
						if (dataGridView.Rows[i].Cells[0].ValueType == typeof(int)) {
							dataGridView.Rows[i].Cells[0].Value = largestIndex + 1;
						}
					}
				}
				// Increment OrderIndex on Add Row (REQUESTED FEATURE)
				/*if (rowIndex == -1) {
					if (dt.Columns[i].ColumnName.Contains("order_index") == true) {
						int largestIndex = 1;
						foreach (DataRow dRPK in dt.Rows) {
							if (dRPK.ItemArray[i] is int) {
								if ((int)dRPK.ItemArray[i] > largestIndex) {
									largestIndex = (int)dRPK.ItemArray[i];
								}
							}
						}
						if (dataGridView.Rows[i].Cells[0].ValueType == typeof(int)) {
							dataGridView.Rows[i].Cells[0].Value = largestIndex + 1;
						}
					}
				}*/
			}
			if (rowIndex != -1) {
				this.Text = "Edit Row";
			}
		}

		private void tabControl_MouseDoubleClick(object sender, MouseEventArgs e) {
			if (tabControl.SelectedIndex <= 0) {
				return;
			}
			Rectangle r = tabControl.GetTabRect(tabControl.SelectedIndex);
			if (r.Contains(e.Location) == true) {
				tabControl.TabPages.RemoveAt(tabControl.SelectedIndex);
			}
		}

		private void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e) {
			e.ThrowException = false;
			MessageBox.Show("Incorrect data was entered into the cell" + Environment.NewLine + Environment.NewLine +
			e.Exception.Message, "Bad Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
			DataGridView dataGridView = ((DataGridView)sender);
			dataGridView.CancelEdit();
		}

		private void buttonOK_Click(object sender, EventArgs e) {
			finishFunction();
		}
		public void finishFunction() {
			if (dataGridView.Rows[primaryKeyRowIndex].Tag is List<object>) {
				foreach (object primaryKey in dataGridView.Rows[primaryKeyRowIndex].Tag as List<object>) {
					if (primaryKey.Equals(dataGridView.Rows[primaryKeyRowIndex].Cells[0].Value) == true) {
						MessageBox.Show("This value is already taken by another row. Please use a unique value for " +
						(string)dataGridView.Rows[primaryKeyRowIndex].HeaderCell.Value + 
						".", "Bad Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
						this.DialogResult = DialogResult.None;
						return;
					}
				}
			}
			List<object> items = new List<object>();
			for (int i = 0; i < dataGridView.Rows.Count; i++) {
				items.Add(dataGridView.Rows[i].Cells[0].Value);
			}
			dr.ItemArray = items.ToArray();
		}

		private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
			// Display New Tab
			if (e.ColumnIndex != -1) {
				return;
			}
			TabPage tp;
			if (e.RowIndex != -1) {
				if (dataGridView.Rows[e.RowIndex].Tag == null || dataGridView.Rows[e.RowIndex].Tag is List<object>) {
					return;
				}
				if (tabControl.TabPages.ContainsKey(((DataTable)dataGridView.Rows[e.RowIndex].Tag).TableName) == true) {
					tabControl.SelectedTab = tabControl.TabPages[((DataTable)dataGridView.Rows[e.RowIndex].Tag).TableName];
					return;
				}
				tp = new TabPage(((DataTable)dataGridView.Rows[e.RowIndex].Tag).TableName + " [" + (string)dataGridView.Rows[e.RowIndex].HeaderCell.Value + "]");
				tp.Name = ((DataTable)dataGridView.Rows[e.RowIndex].Tag).TableName;
			} else {
				if (tabControl.TabPages.ContainsKey(((DataTable)dataGridView.TopLeftHeaderCell.Tag).TableName) == true) {
					tabControl.SelectedTab = tabControl.TabPages[((DataTable)dataGridView.TopLeftHeaderCell.Tag).TableName];
					return;
				}
				tp = new TabPage(((DataTable)dataGridView.TopLeftHeaderCell.Tag).TableName);
				tp.Name = ((DataTable)dataGridView.TopLeftHeaderCell.Tag).TableName;
			}

			DataGridView dgv = new DataGridView();
			dgv.Dock = DockStyle.Fill;
			dgv.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#E8EDFF");//#E0E0E0
			dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
			dgv.AllowUserToAddRows = false;
			dgv.ReadOnly = true;
			if (e.RowIndex != -1) {
				dgv.DataSource = DDS.Tables[((DataTable)dataGridView.Rows[e.RowIndex].Tag).TableName];
			} else {
				dgv.DataSource = DDS.Tables[((DataTable)dataGridView.TopLeftHeaderCell.Tag).TableName];
			}

			tp.Controls.Add(dgv);
			tabControl.TabPages.Add(tp);
			tabControl.SelectedIndex = tabControl.TabPages.Count - 1;
			tabControl.Focus();
		}
	}
}
