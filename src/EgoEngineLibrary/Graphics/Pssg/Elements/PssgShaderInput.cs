namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgShaderInput : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("SHADERINPUT", PssgElementType.Byte)
    {
        CreateElement = (s, f, p) => new PssgShaderInput(s, f, p),
        LinkAttributeName = "format",
        Attributes =
        {
            new PssgSchemaAttribute("parameterID", PssgAttributeType.Int),
            new PssgSchemaAttribute("type", PssgAttributeType.String),
            new PssgSchemaAttribute("format", PssgAttributeType.String),
            new PssgSchemaAttribute("custom", PssgAttributeType.String),
            new PssgSchemaAttribute("texture", PssgAttributeType.String),
            new PssgSchemaAttribute("light", PssgAttributeType.String),
            new PssgSchemaAttribute("object", PssgAttributeType.String),
        }
    };

    public uint ParameterId
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
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

    public string Custom
    {
        get => GetAttributeValue<string>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }

    public string Texture
    {
        get => GetAttributeValue<string>(Schema.Attributes[4].Name);
        set => AddAttribute(Schema.Attributes[4].Name, value);
    }

    public string Light
    {
        get => GetAttributeValue<string>(Schema.Attributes[5].Name);
        set => AddAttribute(Schema.Attributes[5].Name, value);
    }

    public string Object
    {
        get => GetAttributeValue<string>(Schema.Attributes[6].Name);
        set => AddAttribute(Schema.Attributes[6].Name, value);
    }

    public PssgShaderInput(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgShaderInput(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }

    public PssgTexture? TryGetTexture() => File.TryGetObject<PssgTexture>(Texture.AsMemory(1));
}