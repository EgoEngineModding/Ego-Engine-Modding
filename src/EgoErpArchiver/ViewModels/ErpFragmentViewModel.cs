using EgoEngineLibrary.Archive.Erp;

namespace EgoErpArchiver.ViewModels
{
    public class ErpFragmentViewModel : TreeNodeViewModel
    {
        readonly ErpFragment fragment;

        public override string DisplayName
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
        }

        public override ulong? PackedSize
        {
            get
            {
                return Fragment.PackedSize;
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
