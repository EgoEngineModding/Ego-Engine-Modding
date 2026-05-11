namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgDataBlockData : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("DATABLOCKDATA", PssgElementType.Byte)
    {
        CreateElement = (s, f, p) => new PssgDataBlockData(s, f, p),
    };

    public PssgDataBlockData(PssgFile file, PssgElement? parent)
        : base(Schema, file, parent)
    {
    }

    internal PssgDataBlockData(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}