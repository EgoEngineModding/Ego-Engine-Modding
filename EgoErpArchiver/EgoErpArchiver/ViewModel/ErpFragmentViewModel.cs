using EgoEngineLibrary.Archive.Erp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoErpArchiver.ViewModel
{
    public class ErpFragmentViewModel : TreeNodeViewModel
    {
        readonly ErpFragment fragment;

        public override object Text
        {
            get
            {
                return Fragment.Name;
            }
        }

        public override ulong? Size
        {
            get
            {
                return Fragment.Size;
            }
            set
            {
                RaisePropertyChanged("Size");
            }
        }

        public override ulong? PackedSize
        {
            get
            {
                return Fragment.PackedSize;
            }
            set
            {
                RaisePropertyChanged("PackedSize");
            }
        }

        public ErpFragment Fragment
        {
            get { return fragment; }
        }

        public ErpFragmentViewModel(ErpFragment fragment)
        {
            this.fragment = fragment;
        }
    }
}
