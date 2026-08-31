using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EgoEngineLibrary.Frontend.ViewModels;
using EgoEngineLibrary.Graphics.Pssg;

namespace EgoPssgEditor.ViewModels;

public partial class PssgElementViewModel : ViewModelBase
{
    public PssgElement Element { get; }

    public override string DisplayName
    {
        get { return Element.Name; }
    }
    public string DisplayValue
    {
        get { return Element.DisplayValue; }
    }
    public bool HasAttributes
    {
        get { return Element.Attributes.Count > 0; }
    }
    public bool IsDataElement
    {
        get { return Element.IsDataElement; }
    }
    public PssgElementViewModel? Parent { get; }

    public ObservableCollection<PssgElementViewModel> Children { get; }

    public ObservableCollection<PssgAttributeViewModel> Attributes { get; }

    [ObservableProperty]
    public partial bool IsExpanded
    {
        get;
        set;
    }

    [ObservableProperty]
    public partial bool IsSelected
    {
        get;
        set;
    }

    public PssgElementViewModel(PssgElement element, PssgElementViewModel? parent = null)
    {
        Element = element;
        Parent = parent;
        Attributes = new ObservableCollection<PssgAttributeViewModel>();
        Children =
        [
            .. from child in element.ChildElements
            select new PssgElementViewModel(child, this)
        ];
    }

    public IEnumerable<PssgElementViewModel> GetElements()
    {
        yield return this;

        foreach (PssgElementViewModel child in Children)
        {
            foreach (PssgElementViewModel cc in child.GetElements()) yield return cc;
        }
    }

    partial void OnIsExpandedChanged(bool value)
    {
        // Expand all the way up to the root.
        if (value && Parent is not null)
        {
            Parent.IsExpanded = true;
        }
    }

    partial void OnIsSelectedChanged(bool value)
    {
        if (value)
        {
            Parent?.IsExpanded = true;
            Attributes.Clear();
            foreach (PssgAttribute attr in Element.Attributes)
            {
                Attributes.Add(new PssgAttributeViewModel(attr, this));
            }
        }
        else
        {
            // Don't hold in memory
            Attributes.Clear();
        }
    }
}