namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgShaderProgramCode : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("SHADERPROGRAMCODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgShaderProgramCode(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("codeSize", PssgAttributeType.Int),
            new PssgSchemaAttribute("codeType", PssgAttributeType.String),
            new PssgSchemaAttribute("profileType", PssgAttributeType.Int),
            new PssgSchemaAttribute("profile", PssgAttributeType.Int),
            new PssgSchemaAttribute("codeEntry", PssgAttributeType.String),
            new PssgSchemaAttribute("parameterCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("streamCount", PssgAttributeType.Int),
        }
    };

    public uint CodeSize
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public string CodeType
    {
        get => GetAttributeValue<string>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public int ProfileType
    {
        get => GetAttributeValue<int>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    public uint Profile
    {
        get => GetAttributeValue<uint>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }

    public string CodeEntry
    {
        get => GetAttributeValue<string>(Schema.Attributes[4].Name);
        set => AddAttribute(Schema.Attributes[4].Name, value);
    }

    public ushort ParameterCount
    {
        get => GetAttributeValue<ushort>(Schema.Attributes[5].Name);
        set => AddAttribute(Schema.Attributes[5].Name, value);
    }

    public ushort StreamCount
    {
        get => GetAttributeValue<ushort>(Schema.Attributes[6].Name);
        set => AddAttribute(Schema.Attributes[6].Name, value);
    }

    public PssgShaderProgramCodeBlock Code => ChildElements.OfType<PssgShaderProgramCodeBlock>().Single();
    
    public IEnumerable<PssgCgStream> StreamDefinitions => ChildElements.OfType<PssgCgStream>();
    
    public IEnumerable<PssgShaderInputDefinition> ParameterDefinitions => ChildElements.OfType<PssgShaderInputDefinition>();

    public PssgShaderProgramCode(PssgFile file, PssgShaderProgram parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgShaderProgramCode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}