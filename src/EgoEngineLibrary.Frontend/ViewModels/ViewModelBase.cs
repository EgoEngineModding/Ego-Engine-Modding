using CommunityToolkit.Mvvm.ComponentModel;

namespace EgoErpArchiver.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    /// <summary>
    /// Returns the user-friendly name of this object.
    /// Child classes can set this property to a new value,
    /// or override it to determine the value on-demand.
    /// </summary>
    public virtual string DisplayName { get; protected set; } = string.Empty;
}
