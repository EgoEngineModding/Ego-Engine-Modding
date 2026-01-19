using Avalonia.Platform.Storage;

namespace EgoEngineLibrary.Avalonia;

public static class FilePickerTypes
{
    public static readonly FilePickerFileType Pssg = new("PSSG files")
    {
        Patterns = ["*.pssg"],
        MimeTypes = ["application/octet-stream"],
        AppleUniformTypeIdentifiers = ["public.data"]
    };
    
    public static readonly FilePickerFileType Dds = new("DDS files")
    {
        Patterns = ["*.dds"],
        MimeTypes = ["application/octet-stream"],
        AppleUniformTypeIdentifiers = ["public.image"]
    };

    public static readonly FilePickerFileType Gltf = new("Gltf files")
    {
        Patterns = ["*.glb", "*.gltf"],
        MimeTypes = ["model/gltf-binary", "model/gltf+json"],
        AppleUniformTypeIdentifiers = ["public.3d-content"]
    };

    public static readonly FilePickerFileType Bin = new("Bin files")
    {
        Patterns = ["*.bin"],
        MimeTypes = ["application/octet-stream"],
        AppleUniformTypeIdentifiers = ["public.data"]
    };
}
