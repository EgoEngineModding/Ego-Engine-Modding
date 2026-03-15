namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgMatrixPaletteBundleNode : PssgNode
{
    internal static new PssgSchemaElement Schema { get; } = new("MATRIXPALETTEBUNDLENODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgMatrixPaletteBundleNode(s, f, p),
        BaseElement = PssgNode.Schema
    };

    public IEnumerable<PssgMatrixPaletteJointNode> JointNodes => ChildElements.OfType<PssgMatrixPaletteJointNode>();
    
    public PssgMatrixPaletteNode MatrixPaletteNode => ChildElements.OfType<PssgMatrixPaletteNode>().Single();

    public PssgMatrixPaletteBundleNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgMatrixPaletteBundleNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}