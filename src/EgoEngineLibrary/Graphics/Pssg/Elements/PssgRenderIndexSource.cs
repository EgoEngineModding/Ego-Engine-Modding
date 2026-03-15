namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgRenderIndexSource : PssgRenderInterfaceBound
{
    internal static new PssgSchemaElement Schema { get; } = new("RENDERINDEXSOURCE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgRenderIndexSource(s, f, p),
        BaseElement = PssgRenderInterfaceBound.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("primitive", PssgAttributeType.String),
            new PssgSchemaAttribute("minimumIndex", PssgAttributeType.Int),
            new PssgSchemaAttribute("maximumIndex",  PssgAttributeType.Int),
            new PssgSchemaAttribute("format",  PssgAttributeType.String),
            new PssgSchemaAttribute("count",  PssgAttributeType.Int),
        }
    };

    public string Primitive
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public uint MinimumIndex
    {
        get => GetAttributeValue<uint>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public uint MaximumIndex
    {
        get => GetAttributeValue<uint>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    public string Format
    {
        get => GetAttributeValue<string>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }

    public uint Count
    {
        get => GetAttributeValue<uint>(Schema.Attributes[4].Name);
        set => AddAttribute(Schema.Attributes[4].Name, value);
    }

    public PssgRenderIndexSource(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgRenderIndexSource(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}