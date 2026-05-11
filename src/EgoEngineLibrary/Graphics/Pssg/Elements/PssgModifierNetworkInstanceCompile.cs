namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgModifierNetworkInstanceCompile : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("MODIFIERNETWORKINSTANCECOMPILE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgModifierNetworkInstanceCompile(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("uniqueInputCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("packetCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("maxElementCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("memorySizeForProcess", PssgAttributeType.Int),
            new PssgSchemaAttribute("maxPacketOutputSize", PssgAttributeType.Int),
            new PssgSchemaAttribute("maxTemporaryBufferSize", PssgAttributeType.Int),
            new PssgSchemaAttribute("stateBlockBufferSize", PssgAttributeType.Int),
            new PssgSchemaAttribute("totalInputPacketSize", PssgAttributeType.Int),
            new PssgSchemaAttribute("packetSizeCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("packetModifierInputCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("infoPacketSize", PssgAttributeType.Int),
        }
    };

    public uint UniqueInputCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public uint PacketCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public uint MaxElementCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    public uint MemorySizeForProcess
    {
        get => GetAttributeValue<uint>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }

    public uint MaxPacketOutputSize
    {
        get => GetAttributeValue<uint>(Schema.Attributes[4].Name);
        set => AddAttribute(Schema.Attributes[4].Name, value);
    }

    public uint MaxTemporaryBufferSize
    {
        get => GetAttributeValue<uint>(Schema.Attributes[5].Name);
        set => AddAttribute(Schema.Attributes[5].Name, value);
    }

    public uint StateBlockBufferSize
    {
        get => GetAttributeValue<uint>(Schema.Attributes[6].Name);
        set => AddAttribute(Schema.Attributes[6].Name, value);
    }

    public uint TotalInputPacketSize
    {
        get => GetAttributeValue<uint>(Schema.Attributes[7].Name);
        set => AddAttribute(Schema.Attributes[7].Name, value);
    }

    public uint PacketSizeCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[8].Name);
        set => AddAttribute(Schema.Attributes[8].Name, value);
    }

    public uint PacketModifierInputCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[9].Name);
        set => AddAttribute(Schema.Attributes[9].Name, value);
    }

    public uint InfoPacketSize
    {
        get => GetAttributeValue<uint>(Schema.Attributes[10].Name);
        set => AddAttribute(Schema.Attributes[10].Name, value);
    }

    public IEnumerable<PssgModifierNetworkInstanceUniqueModifierInput> UniqueModifierInputs =>
        ChildElements.OfType<PssgModifierNetworkInstanceUniqueModifierInput>();

    public PssgModifierNetworkInstanceCompile(PssgFile file, PssgModifierNetworkInstance parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgModifierNetworkInstanceCompile(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}