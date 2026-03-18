namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgTextureMipMap : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("TEXTUREMIPMAP", PssgElementType.Byte)
    {
        CreateElement = (s, f, p) => new PssgTextureMipMap(s, f, p),
    };

    public PssgTextureMipMap(PssgFile file, PssgTexture? parent)
        : base(Schema, file, parent)
    {
    }

    internal PssgTextureMipMap(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}