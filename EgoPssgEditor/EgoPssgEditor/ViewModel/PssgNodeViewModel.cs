using EgoEngineLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoPssgEditor.ViewModel
{
    public class PssgNodeViewModel : ViewModelBase
    {
        #region Data Props
        static readonly PssgNodeViewModel DummyChild = new PssgNodeViewModel();
        readonly PssgNode node;
        PssgNodeViewModel parent;
        readonly ObservableCollection<PssgNodeViewModel> children;
        readonly ObservableCollection<PssgAttributeViewModel> attributes;

        public PssgNode Node
        {
            get { return node; }
        }
        public override string DisplayName
        {
            get { return node.Name; }
        }
        public string DisplayValue
        {
            get { return node.DisplayValue; }
        }
        public bool HasAttributes
        {
            get { return node.HasAttributes; }
        }
        public bool IsDataNode
        {
            get { return node.IsDataNode; }
        }
        public PssgNodeViewModel Parent
        {
            get { return parent; }
        }
        public ObservableCollection<PssgNodeViewModel> Children
        {
            get { return children; }
        }
        public ObservableCollection<PssgAttributeViewModel> Attributes
        {
            get { return attributes; }
        }
        #endregion

        #region Presentation Props
        bool isExpanded;
        bool isSelected;

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (value != isExpanded)
                {
                    isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (isExpanded && parent != null)
                    parent.IsExpanded = true;
            }
        }
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    if (value)
                    {
                        if (parent != null) parent.IsExpanded = true;
                        GetAttributes();
                    }
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        #endregion

        private PssgNodeViewModel()
        { }

        public PssgNodeViewModel(PssgNode node)
            : this(node, null)
        {

        }

        public PssgNodeViewModel(PssgNode node, PssgNodeViewModel parent)
        {
            this.node = node;
            this.parent = parent;

            attributes = new ObservableCollection<PssgAttributeViewModel>();
            //this.attributes = new ObservableCollection<PssgAttributeViewModel>(
            //    from attribute in node.Attributes
            //    select new PssgAttributeViewModel(attribute));

            children = new ObservableCollection<PssgNodeViewModel>(
                from child in node.ChildNodes
                select new PssgNodeViewModel(child, this));
        }

        private bool HasDummyChild
        {
            get { return this.Children.Count == 1 && this.Children[0] == DummyChild; }
        }

        private void GetAttributes()
        {
            attributes.Clear();
            foreach (PssgAttribute attr in node.Attributes)
            {
                attributes.Add(new PssgAttributeViewModel(attr));
            }
        }
    }
}
