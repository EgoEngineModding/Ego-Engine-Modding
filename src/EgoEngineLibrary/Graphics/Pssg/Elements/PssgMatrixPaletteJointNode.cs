namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgMatrixPaletteJointNode : PssgJointNode
{
    internal static new PssgSchemaElement Schema { get; } = new("MATRIXPALETTEJOINTNODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgMatrixPaletteJointNode(s, f, p),
        BaseElement = PssgJointNode.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("matrixPalette", PssgAttributeType.String),
            new PssgSchemaAttribute("jointID", PssgAttributeType.Int),
        },
    };

    public string MatrixPalette
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    /// <summary>
    /// Used in RD: Grid.
    /// </summary>
    public uint JointId
    {
        get => GetAttributeValue<uint>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }
    
    /// <summary>
    /// Used in RD: Grid.
    /// </summary>
    public IEnumerable<PssgMatrixPaletteRenderInstance> RenderInstances => ChildElements.OfType<PssgMatrixPaletteRenderInstance>();

    /// <summary>
    /// Used in Dirt 2 and later.
    /// </summary>
    public IEnumerable<PssgMatrixPaletteJointRenderInstance> JointRenderInstances => ChildElements.OfType<PssgMatrixPaletteJointRenderInstance>();

    public PssgMatrixPaletteJointNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgMatrixPaletteJointNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}