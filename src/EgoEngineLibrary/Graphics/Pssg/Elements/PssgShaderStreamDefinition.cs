namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgShaderStreamDefinition : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("SHADERSTREAMDEFINITION", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgShaderStreamDefinition(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("renderTypeName", PssgAttributeType.String),
            new PssgSchemaAttribute("name", PssgAttributeType.String),
        }
    };

    public string RenderType
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public string DefinitionName
    {
        get => GetAttributeValue<string>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public PssgShaderStreamDefinition(PssgFile file, PssgShaderGroup parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgShaderStreamDefinition(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}