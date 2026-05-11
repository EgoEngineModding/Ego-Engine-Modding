namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgSkinJoint : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("SKINJOINT", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgSkinJoint(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("joint", PssgAttributeType.String),
        }
    };

    public string Joint
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public PssgSkinJoint(PssgFile file, PssgSkinNode parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgSkinJoint(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}