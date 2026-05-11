namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgLodSkinNode : PssgSkinNode
{
    internal static new PssgSchemaElement Schema { get; } = new("LODSKINNODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgLodSkinNode(s, f, p),
        BaseElement = PssgSkinNode.Schema,
    };

    public PssgLodSkinNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgLodSkinNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}