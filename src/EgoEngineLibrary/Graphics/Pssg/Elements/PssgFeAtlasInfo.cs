namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgFeAtlasInfo : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("FEATLASINFO", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgFeAtlasInfo(s, f, p),
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("atlasname", PssgAttributeType.String),
            new PssgSchemaAttribute("numberatlastextures", PssgAttributeType.Int),
        }
    };

    public string AtlasName
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public int NumberAtlasTextures
    {
        get => GetAttributeValue<int>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public IEnumerable<PssgFeAtlasInfoData> Data => ChildElements.OfType<PssgFeAtlasInfoData>();

    public PssgFeAtlasInfo(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgFeAtlasInfo(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}