using EgoEngineLibrary.Archive.Erp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        readonly ResourcesWorkspaceViewModel resourceWorkspace;
        readonly ObservableCollection<ErpFragmentViewModel> fragments;

        public ErpResource Resource
        {
            get { return resource; }
        }
        public ObservableCollection<ErpFragmentViewModel> Fragments
        {
            get { return fragments; }
        }
        #endregion


        #region Presentation Props
        public override string DisplayName
        {
            get
            {
                return resource.FileName;;
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
        }

        public override ulong? PackedSize
        {
            get
            {
                return resource.PackedSize;
            }
        }

        public override string FullPath
        {
            get
            {
                return resource.Identifier;
            }
        }
        
        public bool IsSelected
        {
            get { return object.ReferenceEquals(this, resourceWorkspace.SelectedItem); }
        }
        public void Select()
        {
            resourceWorkspace.SelectedItem = this;
        }
        #endregion

        public ErpResourceViewModel(ErpResource resource, ResourcesWorkspaceViewModel resourceWorkspace)
        {
            this.resource = resource;
            this.resourceWorkspace = resourceWorkspace;
            fragments = new ObservableCollection<ErpFragmentViewModel>();

            foreach (ErpFragment fragment in resource.Fragments)
            {
                fragments.Add(new ErpFragmentViewModel(fragment));
            }
        }

        public override void UpdateSize()
        {
            foreach (ErpFragmentViewModel child in fragments)
            {
                child.UpdateSize();
            }

            base.UpdateSize();
        }
    }
}
