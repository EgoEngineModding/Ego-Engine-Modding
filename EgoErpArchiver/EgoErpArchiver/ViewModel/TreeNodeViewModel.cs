using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EgoErpArchiver.ViewModel
{
    public abstract class TreeNodeViewModel : ViewModelBase
    {
        public virtual string ResourceType
        {
            get { return null; }
        }

        public virtual ulong? Size
        {
            get { return null; }
        }

        public virtual ulong? PackedSize
        {
            get { return null; }
        }

        public virtual string FullPath
        {
            get { return null; }
        }

        public virtual void UpdateSize()
        {
            OnPropertyChanged("Size");
            OnPropertyChanged("PackedSize");
        }
    }
}
