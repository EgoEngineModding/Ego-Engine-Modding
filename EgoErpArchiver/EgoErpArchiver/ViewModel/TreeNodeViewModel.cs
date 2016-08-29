using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ICSharpCode.TreeView;

namespace EgoErpArchiver.ViewModel
{
    public abstract class TreeNodeViewModel : ICSharpCode.TreeView.SharpTreeNode
    {
        public virtual string ResourceType
        {
            get { return null; }
        }

        public virtual ulong? Size
        {
            get { return null; }
            set { }
        }

        public virtual ulong? PackedSize
        {
            get { return null; }
            set { }
        }

        public virtual string FullPath
        {
            get { return null; }
        }

        public override bool CanCut(SharpTreeNode[] nodes)
        {
            return false;
        }

        public override bool CanCopy(SharpTreeNode[] nodes)
        {
            return false;
        }

        public override bool CanPaste(IDataObject data)
        {
            return false;
        }

        public override bool CanDelete(SharpTreeNode[] nodes)
        {
            return false;
        }
    }
}
