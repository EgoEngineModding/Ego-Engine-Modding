using Avalonia.Platform.Storage;

namespace EgoEngineLibrary.Frontend.Dialogs.File;

public static class FilePickerTypes
{
    private static readonly FilePickerFileType Bin = new("Bin files")
    {
        Patterns = ["*.bin"],
        MimeTypes = ["application/octet-stream"],
        AppleUniformTypeIdentifiers = ["public.data"]
    };

    private static readonly FilePickerFileType Dds = new("DDS files")
    {
        Patterns = ["*.dds"],
        MimeTypes = ["application/octet-stream"],
        AppleUniformTypeIdentifiers = ["public.image"]
    };

    private static readonly FilePickerFileType Erp = new("ERP files")
    {
        Patterns = ["*.erp"],
        MimeTypes = ["application/octet-stream"],
        AppleUniformTypeIdentifiers = ["public.data"]
    };

    private static readonly FilePickerFileType Gltf = new("Gltf files")
    {
        Patterns = ["*.glb", "*.gltf"],
        MimeTypes = ["model/gltf-binary", "model/gltf+json"],
        AppleUniformTypeIdentifiers = ["public.3d-content"]
    };

    private static readonly FilePickerFileType Mipmaps = new("Mipmaps files")
    {
        Patterns = ["*.mipmaps"],
        MimeTypes = ["application/octet-stream"],
        AppleUniformTypeIdentifiers = ["public.data"]
    };

    private static readonly FilePickerFileType Pssg = new("PSSG files")
    {
        Patterns = ["*.pssg"],
        MimeTypes = ["application/octet-stream"],
        AppleUniformTypeIdentifiers = ["public.data"]
    };

    public static FilePickerFileType ToFilePickerType(this FilePickerType type)
    {
        return type switch
        {
            FilePickerType.All => FilePickerFileTypes.All,
            FilePickerType.Bin => Bin,
            FilePickerType.Dds => Dds,
            FilePickerType.Erp => Erp,
            FilePickerType.Gltf => Gltf,
            FilePickerType.Json => FilePickerFileTypes.Json,
            FilePickerType.Mipmaps => Mipmaps,
            FilePickerType.Pssg => Pssg,
            FilePickerType.Xml => FilePickerFileTypes.Xml,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
