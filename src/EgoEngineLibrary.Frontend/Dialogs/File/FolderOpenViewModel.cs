namespace EgoEngineLibrary.Frontend.Dialogs.File;

public class FolderOpenViewModel(FolderOpenOptions options) : IDialogViewModel
{
    public string Title => "";

    public IDialogContext? DialogContext { get; set; }

    public FolderOpenOptions Options { get; } = options;

    public IReadOnlyList<string> Result { get; set; } = [];
}
