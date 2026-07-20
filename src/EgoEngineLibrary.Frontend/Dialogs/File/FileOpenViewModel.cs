namespace EgoEngineLibrary.Frontend.Dialogs.File;

public class FileOpenViewModel(FileOpenOptions options) : IDialogViewModel
{
    public string Title => "";

    public IDialogContext? DialogContext { get; set; }

    public FileOpenOptions Options { get; } = options;

    public IReadOnlyList<string> Result { get; set; } = [];
}
