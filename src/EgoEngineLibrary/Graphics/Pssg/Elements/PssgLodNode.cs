namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgLodNode : PssgNode
{
    internal static new PssgSchemaElement Schema { get; } = new("LODNODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgLodNode(s, f, p),
        BaseElement = PssgNode.Schema
    };

    public PssgLodNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgLodNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}