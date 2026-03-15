namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgCgStream : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("CGSTREAM", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgCgStream(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("cgStreamName", PssgAttributeType.String),
            new PssgSchemaAttribute("cgStreamDataType", PssgAttributeType.String),
            new PssgSchemaAttribute("cgStreamRenderType", PssgAttributeType.String),
        }
    };

    public string StreamName
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public string StreamDataType
    {
        get => GetAttributeValue<string>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public string StreamRenderType
    {
        get => GetAttributeValue<string>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    public PssgCgStream(PssgFile file, PssgShaderProgramCode parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgCgStream(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}