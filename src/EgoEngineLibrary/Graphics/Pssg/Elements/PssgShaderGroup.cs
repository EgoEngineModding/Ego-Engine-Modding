namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgShaderGroup : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("SHADERGROUP", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgShaderGroup(s, f, p),
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("parameterCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("parameterSavedCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("parameterStreamCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("instancesRequireSorting", PssgAttributeType.Int),
            new PssgSchemaAttribute("defaultRenderSortPriority", PssgAttributeType.Int),
            new PssgSchemaAttribute("passCount", PssgAttributeType.Int),
        }
    };

    public ushort ParameterCount
    {
        get => GetAttributeValue<ushort>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public uint ParameterSavedCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public ushort ParameterStreamCount
    {
        get => GetAttributeValue<ushort>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    public bool InstancesRequireSorting
    {
        get => GetAttributeValue<bool>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }

    public uint DefaultRenderSortPriority
    {
        get => GetAttributeValue<uint>(Schema.Attributes[4].Name);
        set => AddAttribute(Schema.Attributes[4].Name, value);
    }

    public ushort PassCount
    {
        get => GetAttributeValue<ushort>(Schema.Attributes[5].Name);
        set => AddAttribute(Schema.Attributes[5].Name, value);
    }

    public IEnumerable<PssgShaderInputDefinition> InputDefinitions => ChildElements.OfType<PssgShaderInputDefinition>();

    public PssgShaderGroup(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgShaderGroup(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}