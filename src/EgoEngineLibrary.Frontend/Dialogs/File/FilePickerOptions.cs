namespace EgoEngineLibrary.Frontend.Dialogs.File;

public record FilePickerOptions
{
    public string? Title { get; set; }

    public string? InitialDirectory { get; set; }

    public string? FileName { get; set; }

    public IReadOnlyList<FilePickerType>? FileTypeChoices { get; set; }

    public FilePickerType? SuggestedFileType { get; set; }
}
