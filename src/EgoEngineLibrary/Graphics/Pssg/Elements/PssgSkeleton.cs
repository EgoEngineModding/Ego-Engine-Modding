namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgSkeleton : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("SKELETON", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgSkeleton(s, f, p),
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("matrixCount", PssgAttributeType.Int),
        }
    };

    public uint MatrixCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }
    
    public IEnumerable<PssgInverseBindMatrix> InverseBindMatrices => ChildElements.OfType<PssgInverseBindMatrix>();

    public PssgSkeleton(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgSkeleton(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}