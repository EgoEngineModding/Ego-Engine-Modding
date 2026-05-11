namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgTextureImageBlock : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("TEXTUREIMAGEBLOCK", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgTextureImageBlock(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("typename", PssgAttributeType.String),
            new PssgSchemaAttribute("size", PssgAttributeType.Int),
        }
    };

    public string TypeName
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public uint Size
    {
        get => GetAttributeValue<uint>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public PssgTextureImageBlockData ImageData => ChildElements.OfType<PssgTextureImageBlockData>().Single();

    public PssgTextureImageBlock(PssgFile file, PssgTexture parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgTextureImageBlock(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}