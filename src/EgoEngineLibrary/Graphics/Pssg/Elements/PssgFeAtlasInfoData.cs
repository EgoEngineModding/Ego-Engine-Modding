namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgFeAtlasInfoData : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("FEATLASINFODATA", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgFeAtlasInfoData(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("texturename", PssgAttributeType.String),
            new PssgSchemaAttribute("u0", PssgAttributeType.Float),
            new PssgSchemaAttribute("v0", PssgAttributeType.Float),
            new PssgSchemaAttribute("u1", PssgAttributeType.Float),
            new PssgSchemaAttribute("v1", PssgAttributeType.Float),
        }
    };

    public string TextureName
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public float U0
    {
        get => GetAttributeValue<float>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public float V0
    {
        get => GetAttributeValue<float>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    public float U1
    {
        get => GetAttributeValue<float>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }

    public float V1
    {
        get => GetAttributeValue<float>(Schema.Attributes[4].Name);
        set => AddAttribute(Schema.Attributes[4].Name, value);
    }

    public PssgFeAtlasInfoData(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgFeAtlasInfoData(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}