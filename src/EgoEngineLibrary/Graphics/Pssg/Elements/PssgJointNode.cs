namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgJointNode : PssgNode
{
    internal static new PssgSchemaElement Schema { get; } = new("JOINTNODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgJointNode(s, f, p),
        BaseElement = PssgNode.Schema
    };

    public PssgJointNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgJointNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}