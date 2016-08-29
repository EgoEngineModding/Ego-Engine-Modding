using EgoEngineLibrary.Archive.Erp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoErpArchiver.ViewModel
{
    public class TreeRootViewModel : TreeNodeViewModel
    {
        readonly ErpFile file;

        public override object Text
        {
            get
            {
                return "Root Node";
            }
        }

        public TreeRootViewModel(ErpFile file)
        {
            this.file = file;
            foreach (ErpResource resource in file.Resources)
            {
                Children.Add(new ErpResourceViewModel(resource));
            }
            LazyLoading = false;
        }

        protected override void LoadChildren()
        {
            //foreach (ErpResource resource in file.Resources)
            //{
            //    Children.Add(new ErpResourceViewModel(resource));
            //}
        }
    }
}
