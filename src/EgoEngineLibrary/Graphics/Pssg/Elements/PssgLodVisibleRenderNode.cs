namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgLodVisibleRenderNode : PssgVisibleRenderNode
{
    internal static new PssgSchemaElement Schema { get; } = new("LODVISIBLERENDERNODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgLodVisibleRenderNode(s, f, p),
        BaseElement = PssgVisibleRenderNode.Schema
    };
    
    public PssgLodRenderInstances LodRenderInstances => ChildElements.OfType<PssgLodRenderInstances>().Single();

    public PssgLodVisibleRenderNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgLodVisibleRenderNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}