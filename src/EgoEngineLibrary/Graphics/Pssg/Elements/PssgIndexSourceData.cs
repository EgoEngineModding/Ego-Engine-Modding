namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgIndexSourceData : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("INDEXSOURCEDATA", PssgElementType.Byte)
    {
        CreateElement = (s, f, p) => new PssgIndexSourceData(s, f, p),
        ElementsPerRow = 16,
        LinkAttributeName = "^format"
    };

    public PssgIndexSourceData(PssgFile file, PssgElement? parent)
        : base(Schema, file, parent)
    {
    }

    internal PssgIndexSourceData(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}