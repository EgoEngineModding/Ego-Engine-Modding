using Avalonia.Platform.Storage;

namespace EgoEngineLibrary.Avalonia;

public record FilePickerOptions
{
    public string? Title { get; set; }

    public string? InitialDirectory { get; set; }

    public string? FileName { get; set; }

    public IReadOnlyList<FilePickerFileType>? FileTypeChoices { get; set; }

    public FilePickerFileType? SuggestedFileType { get; set; }
}
