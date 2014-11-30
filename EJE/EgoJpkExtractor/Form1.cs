using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace EgoJpkExtractor
{
    public partial class Form1 : Form
    {
        // TODO: Rewrite code in OOP format within EgoEngineLibrary (EEL)
        List<Entry> entries = new List<Entry>();
        Dictionary<string, int> entriesFill = new Dictionary<string, int>();
        byte[] header = new byte[20];
        byte[] footer = null;
        int bytesToParamEnd = 0;
        string fileName = "";
        string selectedPath = "";

        public Form1(string[] Args)
        {
            InitializeComponent();
            if (Args.Length > 0)
            {
                fileName = Args[0];
                ReadJPK(fileName);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                fileName = openFileDialog.FileName;
                ReadJPK(fileName);
                openFileDialog.Dispose();
            }
        }

        private void ReadJPK(string fileName)
        {
            // Header/Params
            int numEntries = 0;
            Entry info = new Entry();
            // Names
            int nameLength = 0;

            using (BinaryReader b = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                // Header
                header = b.ReadBytes(20);
                bytesToParamEnd = BitConverter.ToInt32(b.ReadBytes(4), 0);
                b.ReadBytes(8);
                numEntries = (bytesToParamEnd / 32) - 1;
                for (int i = 0; i < numEntries; i++)
                {
                    info = new Entry();
                    info.BytesToName = BitConverter.ToInt32(b.ReadBytes(4), 0);
                    info.Size = BitConverter.ToInt32(b.ReadBytes(4), 0);
                    info.BytesToBlockEnd = BitConverter.ToInt32(b.ReadBytes(4), 0);
                    b.ReadBytes(20);
                    listView.Items.Add(new ListViewItem(new string[]{info.Name, info.Size.ToString()}));
                    entries.Add(info);
                }
                // Entries
                for (int i = 0; i < numEntries; i++)
                {
                    // Get Name
                    if (i == numEntries - 1)
                    {
                        b.BaseStream.Position = entries[i].BytesToName;
                        nameLength = entries[0].BytesToBlockEnd - entries[i].BytesToName;
                        entries[i].Name = Encoding.UTF8.GetString(b.ReadBytes(nameLength));
                        entries[i].Name = entries[i].Name.TrimEnd('\0');
                        listView.Items[i].SubItems[0].Text = entries[i].Name;
                    }
                    else
                    {
                        b.BaseStream.Position = entries[i].BytesToName;
                        nameLength = entries[i + 1].BytesToName - entries[i].BytesToName - 1;
                        entries[i].Name = Encoding.UTF8.GetString(b.ReadBytes(nameLength));
                        listView.Items[i].SubItems[0].Text = entries[i].Name;
                    }
                    // Get File
                    b.BaseStream.Position = entries[i].BytesToBlockEnd;
                    entries[i].File = b.ReadBytes(entries[i].Size);
                    if (i > 0)
                    {
                        entriesFill.Add(entries[i].Name, entries[i].BytesToBlockEnd - 
                            (entries[i - 1].BytesToBlockEnd + entries[i - 1].Size) - (16 - (entries[i - 1].Size % 16)));
                    }
                }
                // Footer
                footer = b.ReadBytes(Convert.ToInt32(b.BaseStream.Length - b.BaseStream.Position) - (16 - (entries[entries.Count - 1].Size % 16)));
            }
        }

        private void extractToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = "Select a folder to export the files to:";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                selectedPath = folderBrowserDialog.SelectedPath;
                try
                {
                    foreach (Entry info in entries)
                    {
                        using (BinaryWriter b = new BinaryWriter(File.Open(selectedPath + "\\" + info.Name, FileMode.Create)))
                        {
                            b.Write(info.File);
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Failed Exporting!", "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = fileName;
            if (saveFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                WriteJPK(saveFileDialog.FileName);
                saveFileDialog.Dispose();
            }
        }

        private void WriteJPK(string saveName)
        {
            using (BinaryWriter b = new BinaryWriter(File.Open(saveName, FileMode.Create)))
            {
                // header
                b.Write(header);
                b.Write(bytesToParamEnd);
                for (int i = 0; i < 8; i++)
                {
                    b.Write(new byte());
                }
                // Entry Params
                foreach (Entry info in entries)
                {
                    b.Write(info.BytesToName);
                    b.Write(info.Size);
                    b.Write(info.BytesToBlockEnd);
                    b.Write(info.Size);
                    for (int i = 0; i < 16; i++)
                    {
                        b.Write(new byte());
                    }
                }
                // Entry Name
                foreach (Entry info in entries)
                {
                    b.Write(new byte());
                    b.Write(Encoding.UTF8.GetBytes(info.Name));
                }
                // Fill in gap
                b.Write(new byte[16 - (b.BaseStream.Position % 16)]);
                // Entry files
                foreach (Entry info in entries)
                {
                    if (entriesFill.ContainsKey(info.Name) == true)
                    {
                        if (entriesFill[info.Name] != 0)
                        {
                            b.Write(new byte[entriesFill[info.Name]]);
                        }
                    }
                    b.Write(info.File);
                    b.Write(new byte[16 - (b.BaseStream.Position % 16)]);
                }
                // Footer
                if (footer != null)
                {
                    b.Write(footer);
                }
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = "Select a folder to import the files from:";
            folderBrowserDialog.SelectedPath = selectedPath;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    listView.Items.Clear();
                    // Populate dictionary
                    Dictionary<string, Entry> organizedEntries = new Dictionary<string, Entry>();
                    foreach (Entry entry in entries)
                    {
                        organizedEntries.Add(entry.Name, entry);
                    }
                    // Import
                    foreach (string file in Directory.GetFiles(folderBrowserDialog.SelectedPath))
                    {
                        if (organizedEntries.ContainsKey(Path.GetFileName(file)) == true)
                        {
                            using (BinaryReader b = new BinaryReader(File.Open(file, FileMode.Open)))
                            {
                                organizedEntries[Path.GetFileName(file)].Size = Convert.ToInt32(b.BaseStream.Length);
                                organizedEntries[Path.GetFileName(file)].File = b.ReadBytes(Convert.ToInt32(b.BaseStream.Length));
                                listView.Items.Add(new ListViewItem(new string[] { Path.GetFileName(file), organizedEntries[Path.GetFileName(file)].Size.ToString() }));
                            }
                        }
                    }
                    // Update BytesToBlockEnd
                    entries = new List<Entry>(organizedEntries.Values);
                    for (int i = 1; i < entries.Count; i++)
                    {
                        entries[i].BytesToBlockEnd = entries[i - 1].BytesToBlockEnd + entries[i - 1].Size + (16 - (entries[i - 1].Size % 16));
                        if (entriesFill.ContainsKey(entries[i].Name) == true)
                        {
                            entries[i].BytesToBlockEnd = entries[i].BytesToBlockEnd + entriesFill[entries[i].Name];
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Failed Importing!", "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
