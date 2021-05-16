using EgoEngineLibrary.Archive.Erp;
using System.Collections.ObjectModel;

namespace EgoErpArchiver.ViewModel
{
    public class ErpResourceViewModel : TreeNodeViewModel
    {
        private readonly ErpResource resource;
        private readonly ResourcesWorkspaceViewModel resourceWorkspace;
        private readonly ObservableCollection<ErpFragmentViewModel> fragments;

        public ErpResource Resource
        {
            get { return resource; }
        }
        public ObservableCollection<ErpFragmentViewModel> Fragments
        {
            get { return fragments; }
        }

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
            get { return ReferenceEquals(this, resourceWorkspace.SelectedItem); }
        }

        public void Select()
        {
            resourceWorkspace.SelectedItem = this;
        }

        public ErpResourceViewModel(ErpResource resource, ResourcesWorkspaceViewModel resourceWorkspace)
        {
            this.resource = resource;
            this.resourceWorkspace = resourceWorkspace;
            fragments = new ObservableCollection<ErpFragmentViewModel>();

            foreach (var fragment in resource.Fragments)
            {
                fragments.Add(new ErpFragmentViewModel(fragment));
            }
        }

        public override void UpdateSize()
        {
            foreach (var child in fragments)
            {
                child.UpdateSize();
            }

            base.UpdateSize();
        }
    }
}
