namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgData : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("DATA", PssgElementType.Byte)
    {
        CreateElement = (s, f, p) => new PssgData(s, f, p),
    };

    public PssgData(PssgFile file, PssgNString parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgData(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}