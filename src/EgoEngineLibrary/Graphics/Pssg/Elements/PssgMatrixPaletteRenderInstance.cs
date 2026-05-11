namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgMatrixPaletteRenderInstance : PssgMatrixPaletteRenderStreamInstance
{
    internal static new PssgSchemaElement Schema { get; } = new("MATRIXPALETTERENDERINSTANCE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgMatrixPaletteRenderInstance(s, f, p),
        BaseElement = PssgRenderStreamInstance.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("streamOffset", PssgAttributeType.Int),
            new PssgSchemaAttribute("elementCountFromOffset", PssgAttributeType.Int),
            new PssgSchemaAttribute("indexOffset", PssgAttributeType.Int),
            new PssgSchemaAttribute("indicesCountFromOffset", PssgAttributeType.Int),
            new PssgSchemaAttribute("jointCount", PssgAttributeType.Int),
        }
    };

    /// <summary>
    /// Used in RD: Grid.
    /// </summary>
    public uint StreamOffset
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    /// <summary>
    /// Used in RD: Grid.
    /// </summary>
    public uint ElementCountFromOffset
    {
        get => GetAttributeValue<uint>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    /// <summary>
    /// Used in RD: Grid.
    /// </summary>
    public override uint IndexOffset
    {
        get => GetAttributeValue<uint>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    /// <summary>
    /// Used in RD: Grid.
    /// </summary>
    public override uint IndicesCountFromOffset
    {
        get => GetAttributeValue<uint>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }

    /// <summary>
    /// Used in Dirt 2 and later.
    /// </summary>
    public uint JointCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[4].Name);
        set => AddAttribute(Schema.Attributes[4].Name, value);
    }

    /// <summary>
    /// Used in Dirt 2 and later.
    /// </summary>
    public IEnumerable<PssgMatrixPaletteSkinJoint> SkinJoints => ChildElements.OfType<PssgMatrixPaletteSkinJoint>();

    public PssgMatrixPaletteRenderInstance(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgMatrixPaletteRenderInstance(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}