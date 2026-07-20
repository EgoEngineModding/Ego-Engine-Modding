namespace EgoEngineLibrary.Frontend.Dialogs;

/// <summary>
/// A service to interact with dialogs from view models without knowing specifics about the UI framework.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Displays a modal dialog based on the given view model.
    /// </summary>
    /// <param name="viewModel">The view model for the dialog.</param>
    /// <returns>A nullable bool that signifies how a dialog was closed.</returns>
    Task<bool?> ShowDialogAsync(IDialogViewModel viewModel);
    
    /// <summary>
    /// Displays a non-modal dialog based on the given view model.
    /// </summary>
    /// <param name="viewModel">The view model for the dialog.</param>
    void Show(IDialogViewModel viewModel);

    /// <summary>
    /// Creates a view model of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the view model to create.</typeparam>
    T CreateViewModel<T>() where T : IDialogViewModel;
}
