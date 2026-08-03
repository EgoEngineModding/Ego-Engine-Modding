namespace EgoEngineLibrary.Frontend.Dialogs;

/// <summary>
/// Represents a view model for a dialog.
/// </summary>
public interface IDialogViewModel
{
    /// <summary>
    /// The title of the dialog.
    /// </summary>
    string Title { get; }
    
    /// <summary>
    /// The dialog context used to control the dialog from the view model.
    /// </summary>
    IDialogContext? DialogContext { get; set; }
}
