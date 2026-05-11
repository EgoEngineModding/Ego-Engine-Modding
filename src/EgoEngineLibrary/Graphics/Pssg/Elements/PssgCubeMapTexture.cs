namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgCubeMapTexture : PssgTexture
{
    internal static new PssgSchemaElement Schema { get; } = new("CUBEMAPTEXTURE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgCubeMapTexture(s, f, p),
        BaseElement = PssgTexture.Schema,
    };

    public PssgCubeMapTexture(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgCubeMapTexture(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}