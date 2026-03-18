namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgRenderNode : PssgVisibleRenderNode
{
    internal static new PssgSchemaElement Schema { get; } = new("RENDERNODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgRenderNode(s, f, p),
        BaseElement = PssgVisibleRenderNode.Schema
    };
    
    public override IEnumerable<PssgRenderStreamInstance> RenderInstances => ChildElements.OfType<PssgRenderStreamInstance>();

    public PssgRenderNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgRenderNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}