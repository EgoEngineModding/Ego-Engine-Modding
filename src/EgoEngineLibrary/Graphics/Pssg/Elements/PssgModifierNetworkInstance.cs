namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgModifierNetworkInstance : PssgRenderStreamInstance
{
    internal static new PssgSchemaElement Schema { get; } = new("MODIFIERNETWORKINSTANCE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgModifierNetworkInstance(s, f, p),
        BaseElement = PssgRenderStreamInstance.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("dynamicStreamCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("modifierInputCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("parameterCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("modifierCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("packetModifierCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("network", PssgAttributeType.String),
        }
    };

    public uint DynamicStreamCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public uint ModifierInputCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public uint ParameterCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    public uint ModifierCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }

    public uint PacketModifierCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[4].Name);
        set => AddAttribute(Schema.Attributes[4].Name, value);
    }

    public string Network
    {
        get => GetAttributeValue<string>(Schema.Attributes[5].Name);
        set => AddAttribute(Schema.Attributes[5].Name, value);
    }

    public IEnumerable<PssgModifierNetworkInstanceModifierInput> ModifierInputs => ChildElements.OfType<PssgModifierNetworkInstanceModifierInput>();

    public PssgModifierNetworkInstanceCompile? CompileElement => ChildElements.OfType<PssgModifierNetworkInstanceCompile>().SingleOrDefault();

    public PssgModifierNetworkInstance(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgModifierNetworkInstance(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}