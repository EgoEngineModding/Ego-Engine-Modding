namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgMatrixPaletteNode : PssgVisibleRenderNode
{
    internal static new PssgSchemaElement Schema { get; } = new("MATRIXPALETTENODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgMatrixPaletteNode(s, f, p),
        BaseElement = PssgVisibleRenderNode.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("jointCount", PssgAttributeType.Int),
        },
    };

    /// <summary>
    /// Used in RD: Grid.
    /// </summary>
    public uint JointCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    /// <summary>
    /// Used in RD: Grid.
    /// </summary>
    public override IEnumerable<PssgRenderStreamInstance> RenderInstances => ChildElements.OfType<PssgRenderStreamInstance>();

    /// <summary>
    /// Used in RD: Grid.
    /// </summary>
    public IEnumerable<PssgMatrixPaletteSkinJoint> SkinJoints => ChildElements.OfType<PssgMatrixPaletteSkinJoint>();

    /// <summary>
    /// Used in Dirt 2 and later.
    /// </summary>
    public IEnumerable<PssgMatrixPaletteRenderInstance> MatrixPaletteRenderInstances => ChildElements.OfType<PssgMatrixPaletteRenderInstance>();

    public PssgMatrixPaletteNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgMatrixPaletteNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}