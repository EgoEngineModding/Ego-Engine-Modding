namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgMatrixPaletteJointRenderInstance : PssgRenderStreamInstance
{
    internal static new PssgSchemaElement Schema { get; } = new("MATRIXPALETTEJOINTRENDERINSTANCE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgMatrixPaletteJointRenderInstance(s, f, p),
        BaseElement = PssgRenderStreamInstance.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("streamOffset", PssgAttributeType.Int),
            new PssgSchemaAttribute("elementCountFromOffset", PssgAttributeType.Int),
            new PssgSchemaAttribute("indexOffset", PssgAttributeType.Int),
            new PssgSchemaAttribute("indicesCountFromOffset", PssgAttributeType.Int),
            new PssgSchemaAttribute("jointID", PssgAttributeType.Int),
        }
    };

    /// <summary>
    /// 20 bits.
    /// </summary>
    public uint StreamOffset
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    /// <summary>
    /// 20 bits.
    /// </summary>
    public uint ElementCountFromOffset
    {
        get => GetAttributeValue<uint>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    /// <summary>
    /// 20 bits.
    /// </summary>
    public uint IndexOffset
    {
        get => GetAttributeValue<uint>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    /// <summary>
    /// 20 bits.
    /// </summary>
    public uint IndicesCountFromOffset
    {
        get => GetAttributeValue<uint>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }

    public ushort JointId
    {
        get => GetAttributeValue<ushort>(Schema.Attributes[4].Name);
        set => AddAttribute(Schema.Attributes[4].Name, value);
    }

    public PssgMatrixPaletteJointRenderInstance(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgMatrixPaletteJointRenderInstance(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}