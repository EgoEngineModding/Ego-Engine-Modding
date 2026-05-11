namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgRenderDataSource : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("RENDERDATASOURCE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgRenderDataSource(s, f, p),
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("streamCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("packetCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("packetListCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("primitive", PssgAttributeType.String),
        }
    };

    public uint StreamCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public uint PacketCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public uint PacketListCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    public string Primitive
    {
        get => GetAttributeValue<string>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }
    
    public PssgRenderIndexSource? IndexSource => ChildElements.OfType<PssgRenderIndexSource>().FirstOrDefault();
    
    public IEnumerable<PssgRenderStream> Streams => ChildElements.OfType<PssgRenderStream>();

    public PssgRenderDataSource(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgRenderDataSource(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}