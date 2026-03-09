namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgDataBlockStream : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("DATABLOCKSTREAM", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgDataBlockStream(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("renderType", PssgAttributeType.String),
            new PssgSchemaAttribute("dataType", PssgAttributeType.String),
            new PssgSchemaAttribute("offset", PssgAttributeType.Int),
            new PssgSchemaAttribute("stride", PssgAttributeType.Int),
        }
    };

    public string RenderType
    {
        get => GetAttributeValue(Schema.Attributes[0].Name, string.Empty);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public string DataType
    {
        get => GetAttributeValue(Schema.Attributes[1].Name, string.Empty);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public uint Offset
    {
        get => GetAttributeValue<uint>(Schema.Attributes[2].Name, 0);
        set => AddAttribute(Schema.Attributes[2].Name, (int)value);
    }

    public uint Stride
    {
        get => GetAttributeValue<uint>(Schema.Attributes[3].Name, 0);
        set => AddAttribute(Schema.Attributes[3].Name, (int)value);
    }

    public PssgDataBlockStream(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgDataBlockStream(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}