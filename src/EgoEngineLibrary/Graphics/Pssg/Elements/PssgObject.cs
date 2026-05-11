namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgObject : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("XXX", PssgElementType.None)
    {
        CreateElement = null,
        Attributes =
        {
            new PssgSchemaAttribute("id", PssgAttributeType.String),
        }
    };

    public string Id
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public IEnumerable<PssgUserData> UserData => ChildElements.OfType<PssgUserData>();

    internal PssgObject(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}