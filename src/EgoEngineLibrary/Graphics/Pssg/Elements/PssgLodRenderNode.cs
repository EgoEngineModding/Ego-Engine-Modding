namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgLodRenderNode : PssgRenderNode
{
    internal static new PssgSchemaElement Schema { get; } = new("LODRENDERNODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgLodRenderNode(s, f, p),
        BaseElement = PssgRenderNode.Schema
    };

    public PssgLodRenderNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgLodRenderNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}