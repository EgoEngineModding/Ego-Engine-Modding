namespace EgoEngineLibrary.Avalonia;

public record FileSaveOptions : FilePickerOptions
{
    public string? DefaultExtension { get; set; }

    public bool? ShowOverwritePrompt { get; set; } = true;
}
