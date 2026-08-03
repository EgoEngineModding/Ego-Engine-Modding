namespace EgoEngineLibrary.Frontend.Dialogs.File;

public class FileSaveViewModel(FileSaveOptions options) : IDialogViewModel
{
    public string Title => "";

    public IDialogContext? DialogContext { get; set; }

    public FileSaveOptions Options { get; } = options;

    public string? Result { get; set; }
}
