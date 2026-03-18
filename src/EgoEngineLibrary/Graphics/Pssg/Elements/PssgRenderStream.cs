namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgRenderStream : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("RENDERSTREAM", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgRenderStream(s, f, p),
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("dataBlock", PssgAttributeType.String),
            new PssgSchemaAttribute("subStream", PssgAttributeType.Int),
        }
    };

    public string DataBlock
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public uint SubStream
    {
        get => GetAttributeValue<uint>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public PssgRenderStream(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgRenderStream(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }

    public PssgDataBlock GetDataBlock() => File.GetObject<PssgDataBlock>(DataBlock.AsMemory(1));
}