namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgShaderInputDefinition : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("SHADERINPUTDEFINITION", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgShaderInputDefinition(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("name", PssgAttributeType.String),
            new PssgSchemaAttribute("type", PssgAttributeType.String),
            new PssgSchemaAttribute("format", PssgAttributeType.String),
        }
    };

    public string InputName
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public string Type
    {
        get => GetAttributeValue<string>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public string Format
    {
        get => GetAttributeValue<string>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    public PssgShaderInputDefinition(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgShaderInputDefinition(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}