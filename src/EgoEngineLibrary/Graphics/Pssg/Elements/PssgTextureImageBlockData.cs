namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgTextureImageBlockData : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("TEXTUREIMAGEBLOCKDATA", PssgElementType.Byte)
    {
        CreateElement = (s, f, p) => new PssgTextureImageBlockData(s, f, p),
    };

    public PssgTextureImageBlockData(PssgFile file, PssgElement? parent)
        : base(Schema, file, parent)
    {
    }

    internal PssgTextureImageBlockData(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}