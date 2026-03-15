namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgShaderInstance : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("SHADERINSTANCE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgShaderInstance(s, f, p),
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("shaderGroup", PssgAttributeType.String),
            new PssgSchemaAttribute("parameterCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("parameterSavedCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("renderSortPriority", PssgAttributeType.Int),
        }
    };

    public string ShaderGroup
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public ushort ParameterCount
    {
        get => GetAttributeValue<ushort>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public uint ParameterSavedCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    public uint RenderSortPriority
    {
        get => GetAttributeValue<uint>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }
    
    public IEnumerable<PssgShaderInput> Inputs => ChildElements.OfType<PssgShaderInput>();

    public PssgShaderInstance(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgShaderInstance(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}