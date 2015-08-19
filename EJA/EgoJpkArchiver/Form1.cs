namespace EgoJpkExtractor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.IO;
    using EgoEngineLibrary.Archive.Jpk;
    using BrightIdeasSoftware;

    public partial class Form1 : Form
    {
        // 2.0 -- Rewrote entire code, Supports latest games, Uses EEL
        JpkFile file;
        string fileName = "raceload.jpk";
        string selectedPath = "";

        public Form1(string[] Args)
        {
            InitializeComponent();
            this.Icon = EgoJpkArchiver.Properties.Resources.Ryder25;
            this.Text = EgoJpkArchiver.Properties.Resources.AppTitleLong + " " + EgoJpkArchiver.Properties.Resources.AppVersionShort;

            this.listView.ShowGroups = false;
            OLVColumn nameCol = new OLVColumn("Name", "Name");
            nameCol.Width = 300;
            nameCol.IsEditable = false;
            this.listView.Columns.Add(nameCol);

            OLVColumn sizeCol = new OLVColumn("Size", "Size");
            sizeCol.Width = 100;
            sizeCol.IsEditable = false;
            this.listView.Columns.Add(sizeCol);

            if (Args.Length > 0)
            {
                fileName = Args[0];
                this.ReadJpk();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = Path.GetFileName(fileName);
            if (openFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                fileName = openFileDialog.FileName;
                this.ReadJpk();
                openFileDialog.Dispose();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = Path.GetFileName(fileName);
            if (saveFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                this.WriteJpk(saveFileDialog.FileName);
                saveFileDialog.Dispose();
            }
        }

        private void ReadJpk()
        {
            file = new JpkFile();
            file.Read(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
            this.listView.SetObjects(file.Entries);

            this.Text = EgoJpkArchiver.Properties.Resources.AppTitleShort + " " +
                EgoJpkArchiver.Properties.Resources.AppVersionShort + " - " +
                Path.GetFileName(fileName);
        }

        private void WriteJpk(string fName)
        {
            file.Write(File.Open(fName, FileMode.Create, FileAccess.Write, FileShare.Read));

            this.Text = EgoJpkArchiver.Properties.Resources.AppTitleShort + " " +
                EgoJpkArchiver.Properties.Resources.AppVersionShort + " - " +
                Path.GetFileName(fName);
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = "Select a folder to export the files to:";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                selectedPath = folderBrowserDialog.SelectedPath;
                try
                {
                    foreach (JpkEntry entry in this.file.Entries)
                    {
                        entry.Export(File.Open(selectedPath + "\\" + entry.Name, FileMode.Create, FileAccess.Write, FileShare.Read));
                    }
                }
                catch
                {
                    MessageBox.Show("Failed Exporting!", "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    // Import
                    foreach (string f in Directory.GetFiles(folderBrowserDialog.SelectedPath))
                    {
                        if (this.file.Contains(Path.GetFileName(f)) == true)
                        {
                            this.file[Path.GetFileName(f)].Import(File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read));
                        }
                    }
                    this.listView.SetObjects(this.file.Entries);
                }
                catch
                {
                    MessageBox.Show("Failed Importing!", "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
