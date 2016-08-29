using EgoEngineLibrary.Archive.Erp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoErpArchiver.ViewModel
{
    public class ErpResourceViewModel : TreeNodeViewModel
    {
        #region Data Props
        readonly ErpResource resource;

        public ErpResource Resource
        {
            get { return resource; }
        }

        public override object Text
        {
            get
            {
                Uri uri = new Uri(resource.FileName);
                return Path.GetFileName(uri.LocalPath) + uri.Query;
            }
        }

        public override string ResourceType
        {
            get
            {
                return resource.ResourceType;
            }
        }

        public override ulong? Size
        {
            get
            {
                return resource.Size;
            }
            set
            {
                foreach (ErpFragmentViewModel child in Children)
                {
                    child.Size = child.Size;
                    child.PackedSize = child.PackedSize;
                }
                RaisePropertyChanged("Size");
            }
        }

        public override ulong? PackedSize
        {
            get
            {
                return resource.PackedSize;
            }
            set
            {
                RaisePropertyChanged("PackedSize");
            }
        }

        public override string FullPath
        {
            get
            {
                return resource.FileName;
            }
        }
        #endregion

        public ErpResourceViewModel(ErpResource resource)
        {
            this.resource = resource;
            LazyLoading = true;
        }

        protected override void LoadChildren()
        {
            foreach (ErpFragment fragment in Resource.Fragments)
            {
                Children.Add(new ErpFragmentViewModel(fragment));
            }
        }
    }
}
