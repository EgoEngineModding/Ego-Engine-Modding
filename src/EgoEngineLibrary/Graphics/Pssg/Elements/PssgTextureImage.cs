namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgTextureImage : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("TEXTUREIMAGE", PssgElementType.Byte)
    {
        CreateElement = (s, f, p) => new PssgTextureImage(s, f, p),
    };

    public PssgTextureImage(PssgFile file, PssgTexture? parent)
        : base(Schema, file, parent)
    {
    }

    internal PssgTextureImage(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}