namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgVisibleRenderNode : PssgNode
{
    internal static new PssgSchemaElement Schema { get; } = new("VISIBLERENDERNODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgVisibleRenderNode(s, f, p),
        BaseElement = PssgNode.Schema
    };
    
    public virtual IEnumerable<PssgRenderInstance> RenderInstances => ChildElements.OfType<PssgRenderInstance>();

    public PssgVisibleRenderNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgVisibleRenderNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}