namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgSkinNode : PssgRenderNode
{
    internal static new PssgSchemaElement Schema { get; } = new("SKINNODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgSkinNode(s, f, p),
        BaseElement = PssgRenderNode.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("jointCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("skeleton", PssgAttributeType.String),
            new PssgSchemaAttribute("updateBounds", PssgAttributeType.Int),
        }
    };

    public uint JointCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public string Skeleton
    {
        get => GetAttributeValue<string>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public bool UpdateBounds
    {
        get => GetAttributeValue<bool>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public IEnumerable<PssgSkinJoint> Joints => ChildElements.OfType<PssgSkinJoint>();

    public PssgSkinNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgSkinNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}