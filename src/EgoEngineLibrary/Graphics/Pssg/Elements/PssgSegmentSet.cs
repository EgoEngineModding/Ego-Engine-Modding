namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgSegmentSet : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("SEGMENTSET", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgSegmentSet(s, f, p),
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("segmentCount", PssgAttributeType.Int),
        }
    };

    public uint SegmentCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }
    
    public IEnumerable<PssgRenderDataSource> Segments => ChildElements.OfType<PssgRenderDataSource>();

    public PssgSegmentSet(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgSegmentSet(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}