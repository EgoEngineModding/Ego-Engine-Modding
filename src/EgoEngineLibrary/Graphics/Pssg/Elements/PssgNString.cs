namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgNString : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("PNSTRING", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgNString(s, f, p),
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("data", PssgAttributeType.String),
            new PssgSchemaAttribute("size", PssgAttributeType.Int),
        }
    };

    public string Data
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public int Size
    {
        get => GetAttributeValue<int>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }
    
    public PssgData DataElement => ChildElements.OfType<PssgData>().Single();

    public PssgNString(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgNString(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}