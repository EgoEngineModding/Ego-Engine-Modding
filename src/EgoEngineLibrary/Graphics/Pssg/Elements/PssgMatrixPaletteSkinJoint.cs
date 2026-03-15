namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgMatrixPaletteSkinJoint : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("MATRIXPALETTESKINJOINT", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgMatrixPaletteSkinJoint(s, f, p),
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

    public PssgMatrixPaletteSkinJoint(PssgFile file, PssgElement parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgMatrixPaletteSkinJoint(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}