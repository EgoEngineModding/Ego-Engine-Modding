namespace EgoEngineLibrary.Frontend.Dialogs.File;

public record FileSaveOptions : FilePickerOptions
{
    public string? DefaultExtension { get; set; }

    public bool? ShowOverwritePrompt { get; set; } = true;
}
