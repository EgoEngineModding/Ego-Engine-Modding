namespace EgoEngineLibrary.Frontend.Dialogs;

/// <summary>
/// A context created for each call to show a dialog.
/// </summary>
public interface IDialogContext
{
    /// <summary>
    /// Closes the dialog with the given result.
    /// </summary>
    /// <param name="result">true for dialog success.</param>
    void Close(bool result = true);
}
