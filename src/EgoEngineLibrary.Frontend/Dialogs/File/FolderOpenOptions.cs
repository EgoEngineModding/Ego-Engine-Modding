namespace EgoEngineLibrary.Frontend.Dialogs.File;

public record FolderOpenOptions
{
    public string? Title { get; set; }

    public string? InitialDirectory { get; set; }

    public string? FileName { get; set; }

    public bool AllowMultiple { get; set; }
}
