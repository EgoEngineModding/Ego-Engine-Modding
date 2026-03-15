namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgLayer : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("LAYER", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgLayer(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("name", PssgAttributeType.String),
        }
    };

    public string LayerName
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public PssgLayer(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgLayer(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}