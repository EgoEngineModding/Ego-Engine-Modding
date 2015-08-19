using BrightIdeasSoftware;
using EgoEngineLibrary.Archive;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EgoErpArchiver
{
    public partial class Form1 : Form
    {
        ErpFile file = new ErpFile();

        public Form1()
        {
            InitializeComponent();
            this.openFileDialog.Filter = "Erp files|*.erp";
            this.openFileDialog.FileName = "ferrari_paint.tga.erp";

            this.TreeListView.ShowGroups = false;
            this.TreeListView.CellEditActivation = ObjectListView.CellEditActivateMode.DoubleClick;
            this.TreeListView.CanExpandGetter = delegate(object rowObject)
            {
                if (rowObject is ErpEntry)
                {
                    return ((ErpEntry)rowObject).Resources.Count > 0;
                }

                return false;
            };
            this.TreeListView.ChildrenGetter = delegate(object rowObject)
            {
                if (rowObject is ErpEntry)
                {
                    return ((ErpEntry)rowObject).Resources;
                }

                return null;
            };

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.file.Read(File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read));

                OLVColumn nameCol = new OLVColumn("Name", string.Empty);
                nameCol.Width = 325;
                nameCol.IsEditable = false;
                nameCol.AspectGetter = delegate(object rowObject)
                {
                    if (rowObject is ErpEntry)
                    {
                        string str = ((ErpEntry)rowObject).FileName.Substring(7).Replace("/", "\\");
                        return Path.GetFileName(((ErpEntry)rowObject).FileName);//.Substring(7));
                    }
                    else if (rowObject is ErpResource)
                    {
                        return ((ErpResource)rowObject).Name;
                    }

                    return string.Empty;
                };
                this.TreeListView.Columns.Add(nameCol);

                OLVColumn fileTypeCol = new OLVColumn("File Type", "EntryType");
                fileTypeCol.Width = 150;
                this.TreeListView.Columns.Add(fileTypeCol);

                OLVColumn sizeCol = new OLVColumn("Size", "Size");
                sizeCol.Width = 100;
                sizeCol.IsEditable = false;
                this.TreeListView.Columns.Add(sizeCol);

                OLVColumn pSizeCol = new OLVColumn("Packed Size", "PackedSize");
                pSizeCol.Width = 100;
                pSizeCol.IsEditable = false;
                this.TreeListView.Columns.Add(pSizeCol);

                this.TreeListView.SetObjects(this.file.Entries);
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (ErpEntry entry in this.file.Entries)
                {
                    entry.Export(folderBrowserDialog.SelectedPath);
                }
            }
        }
    }
}
