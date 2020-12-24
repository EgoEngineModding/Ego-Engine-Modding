using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoPssgEditor.ViewModel
{
    public class CubeMapWorkspaceViewModel : WorkspaceViewModel
    {
        public CubeMapWorkspaceViewModel(MainViewModel mainView)
            : base(mainView)
        {

        }

        public override void ClearData()
        {
            throw new NotImplementedException();
        }

        public override void LoadData(object data)
        {
            throw new NotImplementedException();
        }

        #region CubeMaps
        //private void cubeMapTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        //{
        //    treeView.SelectedNode = ((PssgNode)cubeMapTreeView.SelectedNode.Tag).TreeNode;
        //    cubeMapPictureBox.Tag = 0;
        //    CubeMapCreatePreview(((PssgNode)cubeMapTreeView.SelectedNode.Tag), 0);
        //}
        //private void cubeMapPictureBox_Click(object sender, EventArgs e)
        //{
        //    CubeMapCreatePreview(((PssgNode)cubeMapTreeView.SelectedNode.Tag), (int)cubeMapPictureBox.Tag + 1);
        //}
        //private void CubeMapCreatePreview(PssgNode node, int targetCount)
        //{
        //    // Make Preview
        //    try
        //    {
        //        cubeMapImageLabel.Text = "";
        //        int height = 0; int width = 0;
        //        cubeMapPictureBox.Dock = DockStyle.Fill;
        //        height = cubeMapPictureBox.Height;
        //        width = cubeMapPictureBox.Width;
        //        FREE_IMAGE_FORMAT format = FREE_IMAGE_FORMAT.FIF_DDS;
        //        System.Drawing.Bitmap image = null;
        //        if (targetCount > 5)
        //        {
        //            targetCount = 0;
        //            cubeMapPictureBox.Tag = 0;
        //        }
        //        else
        //        {
        //            cubeMapPictureBox.Tag = targetCount;
        //        }
        //        DdsFile dds = new DdsFile(node, false);
        //        dds.Write(File.Open(Application.StartupPath + "\\temp.dds", FileMode.Create), targetCount);
        //        image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp.dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
        //        if (cubeMapPictureBox.Image != null)
        //        {
        //            cubeMapPictureBox.Image.Dispose();
        //            cubeMapPictureBox.Image = null;
        //        }
        //        /*foreach (CNode sub in node.subNodes) {
        //            if (targetCount == 0 && sub.attributes["typename"].ToString() == "Raw") {
        //                CubeMapWriteDDS(Application.StartupPath + "\\temp" + "Raw" + ".dds", node, targetCount);
        //                image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp" + "Raw" + ".dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
        //            } else if (targetCount == 1 && sub.attributes["typename"].ToString() == "RawNegativeX") {
        //                CubeMapWriteDDS(Application.StartupPath + "\\temp" + "RawNegativeX" + ".dds", node, targetCount);
        //                image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp" + "RawNegativeX" + ".dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
        //            } else if (targetCount == 2 && sub.attributes["typename"].ToString() == "RawPositiveY") {
        //                CubeMapWriteDDS(Application.StartupPath + "\\temp" + "RawPositiveY" + ".dds", node, targetCount);
        //                image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp" + "RawPositiveY" + ".dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
        //            } else if (targetCount == 3 && sub.attributes["typename"].ToString() == "RawNegativeY") {
        //                CubeMapWriteDDS(Application.StartupPath + "\\temp" + "RawNegativeY" + ".dds", node, targetCount);
        //                image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp" + "RawNegativeY" + ".dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
        //            } else if (targetCount == 4 && sub.attributes["typename"].ToString() == "RawPositiveZ") {
        //                CubeMapWriteDDS(Application.StartupPath + "\\temp" + "RawPositiveZ" + ".dds", node, targetCount);
        //                image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp" + "RawPositiveZ" + ".dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
        //            } else if (targetCount == 5 && sub.attributes["typename"].ToString() == "RawNegativeZ") {
        //                CubeMapWriteDDS(Application.StartupPath + "\\temp" + "RawNegativeZ" + ".dds", node, targetCount);
        //                image = FreeImage.LoadBitmap(Application.StartupPath + "\\temp" + "RawNegativeZ" + ".dds", FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref format);
        //            }
        //        }*/
        //        if (image.Height <= height && image.Width <= width)
        //        {
        //            cubeMapPictureBox.Dock = DockStyle.None;
        //            cubeMapPictureBox.Width = image.Width;
        //            cubeMapPictureBox.Height = image.Height;
        //        }
        //        cubeMapPictureBox.Image = image;
        //    }
        //    catch
        //    {
        //        if (cubeMapPictureBox.Image != null)
        //        {
        //            cubeMapPictureBox.Image.Dispose();
        //            cubeMapPictureBox.Image = null;
        //        }
        //        cubeMapImageLabel.Text = "Could not create preview!";
        //        //MessageBox.Show("Could not create preview!", "No Preview", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}

        //private void cubeMapExportToolStripButton_Click(object sender, EventArgs e)
        //{
        //    if (cubeMapTreeView.Nodes.Count == 0 || cubeMapTreeView.SelectedNode.Index == -1)
        //    {
        //        return;
        //    }
        //    PssgNode node = ((PssgNode)cubeMapTreeView.SelectedNode.Tag);
        //    SaveFileDialog dialog = new SaveFileDialog();
        //    dialog.Filter = "DDS files|*.dds|All files|*.*";
        //    dialog.Title = "Select the dds save location and file name";
        //    dialog.FileName = node.Attributes["id"].ToString() + ".dds";
        //    if (dialog.ShowDialog() == DialogResult.OK)
        //    {
        //        try
        //        {
        //            DdsFile dds = new DdsFile(node, false);
        //            dds.Write(File.Open(dialog.FileName, FileMode.Create), -1);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("Could not export cubemap!" + Environment.NewLine + Environment.NewLine +
        //                ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //}

        //private void cubeMapImportToolStripButton_Click(object sender, EventArgs e)
        //{
        //    if (cubeMapTreeView.Nodes.Count == 0 || cubeMapTreeView.SelectedNode.Index == -1)
        //    {
        //        return;
        //    }
        //    PssgNode node = ((PssgNode)cubeMapTreeView.SelectedNode.Tag);
        //    OpenFileDialog dialog = new OpenFileDialog();
        //    dialog.Filter = "DDS files|*.dds|All files|*.*";
        //    dialog.Title = "Select a cubemap dds file";
        //    dialog.FileName = node.Attributes["id"].ToString() + ".dds";
        //    dialog.Multiselect = true;
        //    if (dialog.ShowDialog() == DialogResult.OK)
        //    {
        //        try
        //        {
        //            DdsFile dds = new DdsFile(File.Open(dialog.FileName, FileMode.Open));
        //            dds.Write(node);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("Could not import cubemap!" + Environment.NewLine + Environment.NewLine +
        //                ex.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //        cubeMapPictureBox.Tag = 0;
        //        CubeMapCreatePreview(node, 0);
        //    }
        //}
        #endregion
    }
}
