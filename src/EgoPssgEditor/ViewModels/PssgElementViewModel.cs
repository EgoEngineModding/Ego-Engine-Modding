using System.Collections.ObjectModel;
using EgoEngineLibrary.Graphics.Pssg;

namespace EgoPssgEditor.ViewModels
{
    public class PssgElementViewModel : ViewModelBase
    {
        #region Data Props
        static readonly PssgElementViewModel DummyChild = new PssgElementViewModel();
        readonly PssgElement _element;
        PssgElementViewModel parent;
        readonly ObservableCollection<PssgElementViewModel> children;
        readonly ObservableCollection<PssgAttributeViewModel> attributes;

        public PssgElement Element
        {
            get { return _element; }
        }
        public override string DisplayName
        {
            get { return _element?.Name; }
        }
        public string DisplayValue
        {
            get { return _element.DisplayValue; }
        }
        public bool HasAttributes
        {
            get { return _element.Attributes.Count > 0; }
        }
        public bool IsDataElement
        {
            get { return _element.IsDataElement; }
        }
        public PssgElementViewModel Parent
        {
            get { return parent; }
        }
        public ObservableCollection<PssgElementViewModel> Children
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
                    if (value)
                    {
                        if (parent != null) parent.IsExpanded = true;
                        GetAttributes();
                    }
                    isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        #endregion

        private PssgElementViewModel()
        { }

        public PssgElementViewModel(PssgElement element)
            : this(element, null)
        {

        }

        public PssgElementViewModel(PssgElement element, PssgElementViewModel parent)
        {
            this._element = element;
            this.parent = parent;

            attributes = new ObservableCollection<PssgAttributeViewModel>();

            children = new ObservableCollection<PssgElementViewModel>(
                from child in element.ChildElements
                select new PssgElementViewModel(child, this));
        }

        private bool HasDummyChild
        {
            get { return this.Children.Count == 1 && this.Children[0] == DummyChild; }
        }

        private void GetAttributes()
        {
            attributes.Clear();
            foreach (PssgAttribute attr in _element.Attributes)
            {
                attributes.Add(new PssgAttributeViewModel(attr, this));
            }
        }

        public IEnumerable<PssgElementViewModel> GetElements()
        {
            yield return this;

            foreach (PssgElementViewModel child in Children)
            {
                foreach (PssgElementViewModel cc in child.GetElements()) yield return cc;
            }
        }
    }
}
