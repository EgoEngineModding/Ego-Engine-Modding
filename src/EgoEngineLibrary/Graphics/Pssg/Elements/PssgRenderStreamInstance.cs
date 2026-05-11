namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgRenderStreamInstance : PssgRenderInstance
{
    internal static new PssgSchemaElement Schema { get; } = new("RENDERSTREAMINSTANCE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgRenderStreamInstance(s, f, p),
        BaseElement = PssgRenderInstance.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("sourceCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("indices",  PssgAttributeType.String),
        }
    };

    public uint SourceCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public string Indices
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public PssgRenderInstanceSource RenderInstanceSource => ChildElements.OfType<PssgRenderInstanceSource>().Single();

    public PssgRenderStreamInstance(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgRenderStreamInstance(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}