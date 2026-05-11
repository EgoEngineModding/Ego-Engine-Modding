namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgBundleNode : PssgNode
{
    internal static new PssgSchemaElement Schema { get; } = new("BUNDLENODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgBundleNode(s, f, p),
        BaseElement = PssgNode.Schema
    };

    public PssgBundleNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgBundleNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}