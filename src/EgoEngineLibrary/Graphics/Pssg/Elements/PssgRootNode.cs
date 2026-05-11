namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgRootNode : PssgNode
{
    internal static new PssgSchemaElement Schema { get; } = new("ROOTNODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgRootNode(s, f, p),
        BaseElement = PssgNode.Schema
    };

    public PssgRootNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgRootNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}