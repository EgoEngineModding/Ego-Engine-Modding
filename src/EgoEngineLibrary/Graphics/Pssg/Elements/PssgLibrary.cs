namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgLibrary : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("LIBRARY", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgLibrary(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("type", PssgAttributeType.String),
        }
    };

    public string Type
    {
        get => GetAttributeValue(Schema.Attributes[0].Name, string.Empty);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public PssgLibrary(PssgFile file, PssgDatabase parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgLibrary(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}