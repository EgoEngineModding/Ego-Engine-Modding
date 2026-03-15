namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgRenderInstanceSource : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("RENDERINSTANCESOURCE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgRenderInstanceSource(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("source",  PssgAttributeType.String),
        }
    };

    public string Source
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public PssgRenderInstanceSource(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgRenderInstanceSource(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}