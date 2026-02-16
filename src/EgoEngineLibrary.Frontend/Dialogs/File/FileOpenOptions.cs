namespace EgoEngineLibrary.Frontend.Dialogs.File;

public record FileOpenOptions : FilePickerOptions
{
    public bool AllowMultiple { get; set; }
}
