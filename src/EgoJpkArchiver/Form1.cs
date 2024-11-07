using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using BrightIdeasSoftware;

using EgoEngineLibrary.Archive.Jpk;

namespace EgoJpkArchiver
{
    public partial class Form1 : Form
    {
        private const string ManifestTxtFileName = "manifest.jpk.txt";
        JpkFile _file;
        string _fileName = "raceload.jpk";
        string _selectedPath = "";

        public Form1(string[] Args)
        {
            InitializeComponent();
            this.Icon = Properties.Resources.Ryder25;
            this.Text = $"{Properties.Resources.AppTitleLong} {Properties.Resources.AppVersionShort}";

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
                this.ReadJpk(Args[0]);
            }
        }

        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                folderBrowserDialog.Description = "Select a folder to create a new jpk:";
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    CreateJpk(folderBrowserDialog.SelectedPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create!{Environment.NewLine}{ex}", Properties.Resources.AppTitleLong,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog.FileName = _fileName;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.ReadJpk(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open!{Environment.NewLine}{ex}", Properties.Resources.AppTitleLong,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog.FileName = _fileName;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.WriteJpk(saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save!{Environment.NewLine}{ex}", Properties.Resources.AppTitleLong,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateJpk(string folderPath)
        {
            _file = new JpkFile();
            var manifestFilePath = Path.Combine(folderPath, ManifestTxtFileName);
            var files = File.Exists(manifestFilePath)
                ? File.ReadAllLines(manifestFilePath).Select(x => Path.Combine(folderPath, x))
                : Directory.EnumerateFiles(folderPath);
            foreach (string f in files)
            {
                var entry = new JpkEntry(_file) { Name = Path.GetFileName(f) };
                using var fs = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                entry.Import(fs);
                _file.Entries.Add(entry);
            }
            this.listView.SetObjects(_file.Entries);

            SetFileName($"{Path.GetFileName(folderPath)}.jpk");
            SetSelectedPath(folderPath);
            SetTitle();
        }

        private void ReadJpk(string filePath)
        {
            _file = new JpkFile();
            using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _file.Read(fs);
            this.listView.SetObjects(_file.Entries);

            SetFileName(filePath);
            SetSelectedPath(filePath);
            SetTitle();
        }

        private void WriteJpk(string filePath)
        {
            using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            _file.Write(fs);

            SetFileName(filePath);
            SetSelectedPath(filePath);
            SetTitle();
        }

        private void SetFileName(string filePath)
        {
            _fileName = Path.GetFileName(filePath);
        }

        private void SetSelectedPath(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath) ?? "";
            _selectedPath = fileName;
        }

        private void SetTitle()
        {
            this.Text = $"{Properties.Resources.AppTitleShort} {Properties.Resources.AppVersionShort} - {_fileName}";
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = "Select a target folder to export the files:";
            folderBrowserDialog.SelectedPath = _selectedPath;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    foreach (JpkEntry entry in this._file.Entries)
                    {
                        using var fs = File.Open(Path.Combine(folderBrowserDialog.SelectedPath, entry.Name),
                            FileMode.Create, FileAccess.Write, FileShare.Read);
                        entry.Export(fs);
                    }

                    File.WriteAllLines(Path.Combine(folderBrowserDialog.SelectedPath, ManifestTxtFileName),
                        this._file.Entries.Select(x => x.Name));
                    SetSelectedPath(folderBrowserDialog.SelectedPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export!{Environment.NewLine}{ex}", Properties.Resources.AppTitleLong,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = "Select a source folder to import the files:";
            folderBrowserDialog.SelectedPath = _selectedPath;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Import
                    foreach (string f in Directory.EnumerateFiles(folderBrowserDialog.SelectedPath))
                    {
                        var fileName = Path.GetFileName(f);
                        if (this._file.Contains(fileName))
                        {
                            using var fs = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                            this._file[fileName].Import(fs);
                        }
                    }

                    this.listView.SetObjects(this._file.Entries);
                    SetSelectedPath(folderBrowserDialog.SelectedPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to import!{Environment.NewLine}{ex}", Properties.Resources.AppTitleLong,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
