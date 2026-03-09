namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgDataBlock : PssgRenderInterfaceBound
{
    internal static new PssgSchemaElement Schema { get; } = new("DATABLOCK", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgDataBlock(s, f, p),
        BaseElement = PssgRenderInterfaceBound.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("streamCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("size",  PssgAttributeType.Int),
            new PssgSchemaAttribute("elementCount",  PssgAttributeType.Int),
        }
    };

    public ushort StreamCount
    {
        get => GetAttributeValue<ushort>(Schema.Attributes[0].Name, 0);
        set => AddAttribute(Schema.Attributes[0].Name, (int)value);
    }

    public uint Size
    {
        get => GetAttributeValue<uint>(Schema.Attributes[1].Name, 0);
        set => AddAttribute(Schema.Attributes[1].Name, (int)value);
    }

    public uint ElementCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[2].Name, 0);
        set => AddAttribute(Schema.Attributes[2].Name, (int)value);
    }

    public PssgDataBlock(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgDataBlock(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}