namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgNode : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("NODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgNode(s, f, p),
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("stopTraversal", PssgAttributeType.Int),
            new PssgSchemaAttribute("nickname", PssgAttributeType.String),
        }
    };

    public bool StopTraversal
    {
        get => GetAttributeValue<bool>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public string Nickname
    {
        get => GetAttributeValue<string>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public PssgTransform Transform => ChildElements.OfType<PssgTransform>().Single();
    
    public PssgBoundingBox BoundingBox => ChildElements.OfType<PssgBoundingBox>().Single();

    public PssgNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}