using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EgoLngEditor
{
    public partial class FindAndReplaceForm : Form
    {
        private DataGridView m_DataGridView;
        private int m_SearchStartRow;
        private int m_SearchStartColumn;
        private int m_SearchSelectionIndex;
        private List<DataGridViewCell> m_SelectedCells;

        public FindAndReplaceForm( DataGridView datagridview )
        {
            m_SelectedCells = new List<DataGridViewCell>();
            InitializeComponent();            
            InitializeForm(datagridview);
        }

        public void InitializeForm( DataGridView datagridview )
        {
            if ( m_DataGridView != datagridview )
            {
                if (m_DataGridView != null)
                {
                    m_DataGridView.MouseClick -= DataGridView_MouseClick;
                }
                m_DataGridView = datagridview;
                m_DataGridView.MouseClick += new MouseEventHandler(DataGridView_MouseClick);
                m_DataGridView.SelectionChanged += new EventHandler(DataGridView_SelectionChanged);
            }
            if (m_DataGridView.SelectedCells.Count > 1)
            {
                this.LookInComboBox1.SelectedIndex = 1;
            }
            else
            {
                this.LookInComboBox1.SelectedIndex = 0;
            }
            if (m_DataGridView.CurrentCell != null)
            {
                m_SearchStartRow = m_DataGridView.CurrentCell.RowIndex;
                m_SearchStartColumn = m_DataGridView.CurrentCell.ColumnIndex;
                if (m_DataGridView.CurrentCell.Value != null)
                {
                    this.FindWhatTextBox1.Text = m_DataGridView.CurrentCell.Value.ToString();
                }
            }
            SelectionCellsChanged();
        }

        void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
			SelectionCellsChanged();
        }

        void SelectionCellsChanged()
        {
            m_SearchSelectionIndex = 0;
            m_SelectedCells.Clear();
            foreach (DataGridViewCell cell in m_DataGridView.SelectedCells)
            {
                m_SelectedCells.Add(cell);
            }
        }

        void DataGridView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DataGridView.HitTestInfo hitTest = m_DataGridView.HitTest(e.X, e.Y);
                if (hitTest.Type == DataGridViewHitTestType.Cell)
                {
                    m_SearchStartRow = hitTest.RowIndex;
                    m_SearchStartColumn = hitTest.ColumnIndex;
                }
            }
        }

        void FindWhatTextBox1_TextChanged(object sender, System.EventArgs e)
        {
            this.FindWhatTextBox2.Text = this.FindWhatTextBox1.Text;
        }

        void FindWhatTextBox2_TextChanged(object sender, System.EventArgs e)
        {
            this.FindWhatTextBox1.Text = this.FindWhatTextBox2.Text;
        }

        void LookInComboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            this.LookInComboBox2.SelectedIndex = this.LookInComboBox1.SelectedIndex;
        }

        void LookInComboBox2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            this.LookInComboBox1.SelectedIndex = this.LookInComboBox2.SelectedIndex;
        }

        void MatchCaseCheckBox1_CheckedChanged(object sender, System.EventArgs e)
        {
            this.MatchCaseCheckBox2.Checked = this.MatchCaseCheckBox1.Checked;
        }

        void MatchCaseCheckBox2_CheckedChanged(object sender, System.EventArgs e)
        {
            this.MatchCaseCheckBox1.Checked = this.MatchCaseCheckBox2.Checked;
        }

        void MatchCellCheckBox1_CheckedChanged(object sender, System.EventArgs e)
        {
            this.MatchCellCheckBox2.Checked = this.MatchCellCheckBox1.Checked;
        }

        void MatchCellCheckBox2_CheckedChanged(object sender, System.EventArgs e)
        {
            this.MatchCellCheckBox1.Checked = this.MatchCellCheckBox2.Checked;
        }

        void SearchUpCheckBox1_CheckedChanged(object sender, System.EventArgs e)
        {
            this.SearchUpCheckBox2.Checked = this.SearchUpCheckBox1.Checked;
        }

        void SearchUpCheckBox2_CheckedChanged(object sender, System.EventArgs e)
        {
            this.SearchUpCheckBox1.Checked = this.SearchUpCheckBox2.Checked;
        }

        void UseCheckBox1_CheckedChanged(object sender, System.EventArgs e)
        {
            this.UseCheckBox2.Checked = this.UseCheckBox1.Checked;
            if (this.UseCheckBox1.Checked)
            {
                this.UseComboBox1.Enabled = true;
            }
            else
            {
                this.UseComboBox1.Enabled = false;
                this.UseComboBox1.SelectedItem = null;
            }
        }

        void UseCheckBox2_CheckedChanged(object sender, System.EventArgs e)
        {
            this.UseCheckBox1.Checked = this.UseCheckBox2.Checked;
            if (this.UseCheckBox2.Checked)
            {
                this.UseComboBox2.Enabled = true;
            }
            else
            {
                this.UseComboBox2.Enabled = false;
                this.UseComboBox2.SelectedItem = null;
            }
        }

        void UseComboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            this.UseComboBox2.SelectedIndex = this.UseComboBox1.SelectedIndex;
        }

        void UseComboBox2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            this.UseComboBox1.SelectedIndex = this.UseComboBox2.SelectedIndex;
        }

        void FindButton2_Click(object sender, System.EventArgs e)
        {
            FindButton1_Click(sender, e);
        }

        void ReplaceAllButton_Click(object sender, System.EventArgs e)
        {
            DataGridViewCell FindCell = null;
            if (this.LookInComboBox1.SelectedIndex == 0)
            {
                FindCell = FindAndReplaceInTable(true, false, this.ReplaceWithTextBox.Text);
            }
            else if (this.LookInComboBox1.SelectedIndex == 1)
            {
                FindCell = FindAndReplaceInSelection(true, false, this.ReplaceWithTextBox.Text);
            } else if (this.LookInComboBox1.SelectedIndex == 2) {
				FindCell = FindAndReplaceInColumn(true, false, this.ReplaceWithTextBox.Text);
			}
            if (FindCell != null)
            {
				DataGridViewCell[] cells = m_SelectedCells.ToArray(); ;
				int iSearchIndex = m_SearchSelectionIndex;
				m_DataGridView.CurrentCell = FindCell;
				// restore cached selection variables that was changed by setting CurrentCell
				m_SearchSelectionIndex = iSearchIndex;
				m_SelectedCells.Clear();
				m_SelectedCells.AddRange(cells);                
            }
        }

        void ReplaceButton_Click(object sender, System.EventArgs e)
        {
            DataGridViewCell FindCell = null;
            if (this.LookInComboBox1.SelectedIndex == 0)
            {
                FindCell = FindAndReplaceInTable(true, true, this.ReplaceWithTextBox.Text);
            }
            else if (this.LookInComboBox1.SelectedIndex == 1)
            {
                FindCell = FindAndReplaceInSelection(true, true, this.ReplaceWithTextBox.Text);
            } else if (this.LookInComboBox1.SelectedIndex == 2) {
				FindCell = FindAndReplaceInColumn(true, true, this.ReplaceWithTextBox.Text);
			}
            if (FindCell != null)
            {
				DataGridViewCell[] cells = m_SelectedCells.ToArray(); ;
				int iSearchIndex = m_SearchSelectionIndex;
				m_DataGridView.CurrentCell = FindCell;
				// restore cached selection variables that was changed by setting CurrentCell
				m_SearchSelectionIndex = iSearchIndex;
				m_SelectedCells.Clear();
				m_SelectedCells.AddRange(cells);
            }
        }

        void FindButton1_Click(object sender, System.EventArgs e)
        {
            Find(this.SearchUpCheckBox1.Checked);
        }
        public void Find(bool searchUp)
        {
            this.SearchUpCheckBox1.Checked = searchUp;

            DataGridViewCell FindCell = null;
            if (this.LookInComboBox1.SelectedIndex == 0)
            {
                FindCell = FindAndReplaceInTable(false, true, null);
            }
            else if (this.LookInComboBox1.SelectedIndex == 1)
            {
                FindCell = FindAndReplaceInSelection(false, true, null);
            }
            else if (this.LookInComboBox1.SelectedIndex == 2)
            {
                FindCell = FindAndReplaceInColumn(false, true, null);
            }
            if (FindCell != null)
            {
                DataGridViewCell[] cells = m_SelectedCells.ToArray(); ;
                int iSearchIndex = m_SearchSelectionIndex;
                m_DataGridView.CurrentCell = FindCell;
                // restore cached selection variables that was changed by setting CurrentCell
                m_SearchSelectionIndex = iSearchIndex;
                m_SelectedCells.Clear();
                m_SelectedCells.AddRange(cells);
            }
        }

        DataGridViewCell FindAndReplaceInSelection(bool bReplace, bool bStopOnFind, String replaceString)
        {
            // Search criterions
            String sFindWhat = this.FindWhatTextBox1.Text;
            bool bMatchCase = this.MatchCaseCheckBox1.Checked;
            bool bMatchCell = this.MatchCellCheckBox1.Checked;
            bool bSearchUp = this.SearchUpCheckBox1.Checked;
            int iSearchMethod = -1; // No regular repression or wildcard
            if (this.UseCheckBox1.Checked)
            {
                iSearchMethod = this.UseComboBox1.SelectedIndex;
            }

            // Start of search            
            int iSearchIndex = m_SearchSelectionIndex;
            if (bSearchUp)
            {
                iSearchIndex = m_SelectedCells.Count - m_SearchSelectionIndex - 1;
            }

            while (m_SearchSelectionIndex < m_SelectedCells.Count)
            {
                m_SearchSelectionIndex++;
                // Search end of search
                DataGridViewCell FindCell = null;
                if (FindAndReplaceString(bReplace, m_SelectedCells[iSearchIndex], sFindWhat, replaceString, bMatchCase, bMatchCell, iSearchMethod))
                {
                    FindCell = m_SelectedCells[iSearchIndex];
                }
                if (bStopOnFind && FindCell != null)
                {
                    if (m_SearchSelectionIndex >= m_SelectedCells.Count)
                    {
                        m_SearchSelectionIndex = 0;
                    }
                    return FindCell;
                }                
                if (bSearchUp)
                {
                    iSearchIndex = m_SelectedCells.Count - m_SearchSelectionIndex - 1;
                }
                else
                {
                    iSearchIndex = m_SearchSelectionIndex;
                }
            }
            if (m_SearchSelectionIndex >= m_SelectedCells.Count)
            {
                m_SearchSelectionIndex = 0;
            }
            return null;
        }

        DataGridViewCell FindAndReplaceInTable(bool bReplace, bool bStopOnFind, String replaceString)
        {
            if (m_DataGridView.CurrentCell == null)
            {
                return null;
            }

            // Search criterions
            String sFindWhat = this.FindWhatTextBox1.Text;
            bool bMatchCase = this.MatchCaseCheckBox1.Checked;
            bool bMatchCell = this.MatchCellCheckBox1.Checked;
            bool bSearchUp = this.SearchUpCheckBox1.Checked;
            int iSearchMethod = -1; // No regular repression or wildcard
            if (this.UseCheckBox1.Checked)
            {
                iSearchMethod = this.UseComboBox1.SelectedIndex;
            }

            // Start of search            
            int iSearchStartRow = m_DataGridView.CurrentCell.RowIndex;
            int iSearchStartColumn = m_DataGridView.CurrentCell.ColumnIndex;
            int iRowIndex = m_DataGridView.CurrentCell.RowIndex;
            int iColIndex = m_DataGridView.CurrentCell.ColumnIndex;
            if (bSearchUp)
            {
                iColIndex = iColIndex - 1;
                if (iColIndex < 0)
                {
                    iColIndex = m_DataGridView.ColumnCount - 1;
                    iRowIndex--;
                }
            }
            else
            {
                iColIndex = iColIndex + 1;
                if (iColIndex >= m_DataGridView.ColumnCount)
                {
                    iColIndex = 0;
                    iRowIndex++;
                }
            }
            if (iRowIndex >= m_DataGridView.RowCount)
            {
                iRowIndex = 0;
            }
            else if (iRowIndex < 0)
            {
                iRowIndex = m_DataGridView.RowCount - 1;
            }
            while (!(iRowIndex == iSearchStartRow && iColIndex == iSearchStartColumn))
            {				
                // Search end of search
                DataGridViewCell FindCell = null;
                if (FindAndReplaceString(bReplace, m_DataGridView[iColIndex, iRowIndex], sFindWhat, replaceString, bMatchCase, bMatchCell, iSearchMethod))
                {
                    FindCell = m_DataGridView[iColIndex, iRowIndex];
                }              
                if (bStopOnFind && FindCell != null)
                {
                    return FindCell;
                }
                if (bSearchUp)
                {
                    iColIndex--;
                }
                else
                {
                    iColIndex++;
                }
                if (iColIndex >= m_DataGridView.ColumnCount)
                {
                    iColIndex = 0;
                    iRowIndex++;
                }
                else if (iColIndex < 0)
                {
                    iColIndex = m_DataGridView.ColumnCount - 1;
                    iRowIndex--;
                }
                if (iRowIndex >= m_DataGridView.RowCount)
                {
                    iRowIndex = 0;
                }
                else if (iRowIndex < 0)
                {
                    iRowIndex = m_DataGridView.RowCount - 1;
                }
            }
            if (FindAndReplaceString(bReplace, m_DataGridView[iColIndex, iRowIndex], sFindWhat, replaceString, bMatchCase, bMatchCell, iSearchMethod))
            {
                return m_DataGridView[iColIndex, iRowIndex];
            }
            return null;
        }

		DataGridViewCell FindAndReplaceInColumn(bool bReplace, bool bStopOnFind, String replaceString) {
			if (m_DataGridView.CurrentCell == null) {
				return null;
			}
			
			// Search criterions
			String sFindWhat = this.FindWhatTextBox1.Text;
			bool bMatchCase = this.MatchCaseCheckBox1.Checked;
			bool bMatchCell = this.MatchCellCheckBox1.Checked;
			bool bSearchUp = this.SearchUpCheckBox1.Checked;
			int iSearchMethod = -1; // No regular repression or wildcard
			if (this.UseCheckBox1.Checked) {
				iSearchMethod = this.UseComboBox1.SelectedIndex;
			}

			// Start of search            
			int iSearchStartRow = m_DataGridView.CurrentCell.RowIndex;
			int iSearchStartColumn = m_DataGridView.CurrentCell.ColumnIndex;
			int iRowIndex = m_DataGridView.CurrentCell.RowIndex;
			iRowIndex++;
			if (iRowIndex >= m_DataGridView.RowCount) {
				iRowIndex = 0;
			} else if (iRowIndex < 0) {
				iRowIndex = m_DataGridView.RowCount - 1;
			}
			while (!(iRowIndex == iSearchStartRow)) {
				// Search end of search
				
				DataGridViewCell FindCell = null;
				if (FindAndReplaceString(bReplace, m_DataGridView[iSearchStartColumn, iRowIndex], sFindWhat, replaceString, bMatchCase, bMatchCell, iSearchMethod)) {
					FindCell = m_DataGridView[iSearchStartColumn, iRowIndex];
				}
				if (bStopOnFind && FindCell != null) {
					return FindCell;
				}
				if (bSearchUp) {
					iRowIndex--;
				} else {
					iRowIndex++;
				}
				if (iRowIndex >= m_DataGridView.RowCount) {
					iRowIndex = 0;
				} else if (iRowIndex < 0) {
					iRowIndex = m_DataGridView.RowCount - 1;
				}
			}
			if (FindAndReplaceString(bReplace, m_DataGridView[iSearchStartColumn, iRowIndex], sFindWhat, replaceString, bMatchCase, bMatchCell, iSearchMethod)) {
				return m_DataGridView[iSearchStartColumn, iRowIndex];
			}
			return null;
		}

        DataGridViewCell FindAndReplaceInColumnHeader()
        {
            // Search parameters
            String sFindWhat = this.FindWhatTextBox1.Text;
            bool bMatchCase = this.MatchCaseCheckBox1.Checked;
            bool bMatchCell = this.MatchCellCheckBox1.Checked;
            bool bSearchUp = this.SearchUpCheckBox1.Checked;
            int iSearchMethod = -1; // No regular repression or wildcard
            if (this.UseCheckBox1.Checked)
            {
                iSearchMethod = this.UseComboBox1.SelectedIndex;
            }
            // Start of search
            int iSearchStartColumn = m_DataGridView.CurrentCell.ColumnIndex;
            int iColIndex = m_DataGridView.CurrentCell.ColumnIndex;
            // search one cell back
            if (bSearchUp)
            {
                iColIndex = iColIndex - 1;
                if (iColIndex < 0)
                {
                    iColIndex = m_DataGridView.ColumnCount - 1;
                }
            }
            else
            {
                iColIndex = iColIndex + 1;
                if (iColIndex >= m_DataGridView.ColumnCount)
                {
                    iColIndex = 0;
                }
            }
            while (!(iColIndex == iSearchStartColumn))
            {
                // Search end of search
                DataGridViewCell FindCell = null;
                if (FindString(m_DataGridView.Columns[iColIndex].Name, sFindWhat, bMatchCase, bMatchCell, iSearchMethod))
                {
                    FindCell = m_DataGridView[iColIndex, 0];
                    return FindCell;
                }
                if (bSearchUp)
                {
                    iColIndex--;
                }
                else
                {
                    iColIndex++;
                }
                // Search down
                if (iColIndex >= m_DataGridView.ColumnCount)
                {
                    iColIndex = 0;
                }
                // Search up
                else if (iColIndex < 0)
                {
                    iColIndex = m_DataGridView.ColumnCount - 1;
                }
            }
            return null;
        }
        bool FindString(String SearchString, String FindString, bool bMatchCase, bool bMatchCell, int iSearchMethod)
        {
            // Regular string search
            if (iSearchMethod == -1)
            {
                // Match Cell
                if (bMatchCell)
                {
                    if (!bMatchCase)
                    {
                        if (SearchString.ToLowerInvariant() == FindString.ToLowerInvariant())
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (SearchString == FindString)
                        {
                            return true;
                        }
                    }
                }
                // No Match Cell
                else
                {
                    bool bFound = false;
                    StringComparison strCompare = StringComparison.InvariantCulture;
                    if (!bMatchCase)
                    {
                        strCompare = StringComparison.InvariantCultureIgnoreCase;
                    }
                    bFound = SearchString.IndexOf(FindString, 0, strCompare) != -1;
                    return bFound;
                }
            }
            else
            {
                // Regular Expression
                String RegexPattern = FindString;
                // Wildcards
                if (iSearchMethod == 1)
                {
                    // Convert wildcard to regex:
                    RegexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(FindString).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                }
                System.Text.RegularExpressions.RegexOptions strCompare = System.Text.RegularExpressions.RegexOptions.None;
                if (!bMatchCase)
                {
                    strCompare = System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                }
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(RegexPattern, strCompare);
                if (regex.IsMatch(SearchString))
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        bool FindAndReplaceString(bool bReplace, DataGridViewCell SearchCell, String FindString, String ReplaceString, bool bMatchCase, bool bMatchCell, int iSearchMethod )
        {
            String SearchString = SearchCell.FormattedValue.ToString();
            // Regular string search
            if (iSearchMethod == -1)
            {
                // Match Cell
                if ( bMatchCell )
                {
                    if ( !bMatchCase )
                    {
                        if ( SearchString.ToLowerInvariant() == FindString.ToLowerInvariant() )
                        {
                            if ( bReplace )
                            {
                                SearchCell.Value = Convert.ChangeType(ReplaceString, SearchCell.ValueType);
                            }
                            return true;
                        }
                    }
                    else
                    {
                        if ( SearchString == FindString )
                        {
                            if ( bReplace )
                            {
                                SearchCell.Value = Convert.ChangeType(ReplaceString, SearchCell.ValueType);
                            }
                            return true;
                        }
                    }
                }
                // No Match Cell
                else
                {
                    bool bFound = false;
                    StringComparison strCompare = StringComparison.InvariantCulture;
                    if (!bMatchCase)
                    {
                        strCompare = StringComparison.InvariantCultureIgnoreCase;
                    }
                    if (bReplace)
                    {
                        String NewString = null;
                        int strIndex = 0;
                        while (strIndex != -1)
                        {
                            int nextStrIndex = SearchString.IndexOf(FindString, strIndex, strCompare);
                            if (nextStrIndex != -1)
                            {
                                bFound = true;
                                NewString += SearchString.Substring(strIndex, nextStrIndex - strIndex);
                                NewString += ReplaceString;
                                nextStrIndex = nextStrIndex + FindString.Length;
                            }
                            else
                            {
                                NewString += SearchString.Substring(strIndex);
                            }
                            strIndex = nextStrIndex;
                        }
                        if ( bFound )
                        {
                            SearchCell.Value = Convert.ChangeType(NewString, SearchCell.ValueType);
                        }
                    }
                    else
                    {
                        bFound = SearchString.IndexOf(FindString, 0, strCompare) != -1;
                    }
                    return bFound;  
                }                
            }            
            else
            {
                // Regular Expression
                String RegexPattern = FindString;
                // Wildcards
                if (iSearchMethod == 1)
                {
                    // Convert wildcard to regex:
                    RegexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(FindString).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                }
                System.Text.RegularExpressions.RegexOptions strCompare = System.Text.RegularExpressions.RegexOptions.None;
                if (!bMatchCase)
                {
                    strCompare = System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                }
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(RegexPattern, strCompare);
                if ( regex.IsMatch( SearchString ) )
                {
                    if ( bReplace )
                    {
                        String NewString = regex.Replace(SearchString, ReplaceString );
                        SearchCell.Value = Convert.ChangeType(NewString, SearchCell.ValueType);
                    }
                    return true;
                }
                return false;
            }
            return false;
        }

        private void FindAndReplaceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}