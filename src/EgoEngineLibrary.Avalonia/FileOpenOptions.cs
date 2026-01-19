namespace EgoEngineLibrary.Avalonia;

public record FileOpenOptions : FilePickerOptions
{
    public bool AllowMultiple { get; set; }
}
