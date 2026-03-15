namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgTexture : PssgRenderInterfaceBound
{
    internal static new PssgSchemaElement Schema { get; } = new("TEXTURE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgTexture(s, f, p),
        BaseElement = PssgRenderInterfaceBound.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("width", PssgAttributeType.Int),
            new PssgSchemaAttribute("height", PssgAttributeType.Int),
            new PssgSchemaAttribute("depth", PssgAttributeType.Int),
            new PssgSchemaAttribute("texelFormat", PssgAttributeType.String),
            new PssgSchemaAttribute("transient", PssgAttributeType.Int),
            new PssgSchemaAttribute("wrapS", PssgAttributeType.Int),
            new PssgSchemaAttribute("wrapT", PssgAttributeType.Int),
            new PssgSchemaAttribute("wrapR", PssgAttributeType.Int),
            new PssgSchemaAttribute("minFilter", PssgAttributeType.Int),
            new PssgSchemaAttribute("magFilter", PssgAttributeType.Int),
            new PssgSchemaAttribute("automipmap", PssgAttributeType.Int),
            new PssgSchemaAttribute("numberMipMapLevels", PssgAttributeType.Int),
            new PssgSchemaAttribute("msaaType", PssgAttributeType.Int),
            new PssgSchemaAttribute("gammaRemapR", PssgAttributeType.Int),
            new PssgSchemaAttribute("gammaRemapG", PssgAttributeType.Int),
            new PssgSchemaAttribute("gammaRemapB", PssgAttributeType.Int),
            new PssgSchemaAttribute("gammaRemapA", PssgAttributeType.Int),
            new PssgSchemaAttribute("enableCompare", PssgAttributeType.Int),
            new PssgSchemaAttribute("maxAnisotropy", PssgAttributeType.Float),
            new PssgSchemaAttribute("lodBias", PssgAttributeType.Float),
            new PssgSchemaAttribute("enableVertexTexture", PssgAttributeType.Int),
            new PssgSchemaAttribute("borderColor", PssgAttributeType.Int),
            new PssgSchemaAttribute("imageBlockCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("mipZeroAbsent", PssgAttributeType.Int),
        }
    };

    public uint Width
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public uint Height
    {
        get => GetAttributeValue<uint>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public uint Depth
    {
        get => GetAttributeValue<uint>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    public string TexelFormat
    {
        get => GetAttributeValue<string>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }

    public bool Transient
    {
        get => GetAttributeValue<bool>(Schema.Attributes[4].Name);
        set => AddAttribute(Schema.Attributes[4].Name, value);
    }

    public int WrapS
    {
        get => GetAttributeValue<int>(Schema.Attributes[5].Name);
        set => AddAttribute(Schema.Attributes[5].Name, value);
    }

    public int WrapT
    {
        get => GetAttributeValue<int>(Schema.Attributes[6].Name);
        set => AddAttribute(Schema.Attributes[6].Name, value);
    }

    public int WrapR
    {
        get => GetAttributeValue<int>(Schema.Attributes[7].Name);
        set => AddAttribute(Schema.Attributes[7].Name, value);
    }

    public int MinFilter
    {
        get => GetAttributeValue<int>(Schema.Attributes[8].Name);
        set => AddAttribute(Schema.Attributes[8].Name, value);
    }

    public int MagFilter
    {
        get => GetAttributeValue<int>(Schema.Attributes[9].Name);
        set => AddAttribute(Schema.Attributes[9].Name, value);
    }

    public bool AutoMipMap
    {
        get => GetAttributeValue<bool>(Schema.Attributes[10].Name);
        set => AddAttribute(Schema.Attributes[10].Name, value);
    }

    public uint MipMapCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[11].Name);
        set => AddAttribute(Schema.Attributes[11].Name, value);
    }

    public int MsaaType
    {
        get => GetAttributeValue<int>(Schema.Attributes[12].Name);
        set => AddAttribute(Schema.Attributes[12].Name, value);
    }

    public bool GammaRemapR
    {
        get => GetAttributeValue<bool>(Schema.Attributes[13].Name);
        set => AddAttribute(Schema.Attributes[13].Name, value);
    }

    public bool GammaRemapG
    {
        get => GetAttributeValue<bool>(Schema.Attributes[14].Name);
        set => AddAttribute(Schema.Attributes[14].Name, value);
    }

    public bool GammaRemapB
    {
        get => GetAttributeValue<bool>(Schema.Attributes[15].Name);
        set => AddAttribute(Schema.Attributes[15].Name, value);
    }

    public bool GammaRemapA
    {
        get => GetAttributeValue<bool>(Schema.Attributes[16].Name);
        set => AddAttribute(Schema.Attributes[16].Name, value);
    }

    public bool EnableCompare
    {
        get => GetAttributeValue<bool>(Schema.Attributes[17].Name);
        set => AddAttribute(Schema.Attributes[17].Name, value);
    }

    public float MaxAnisotropy
    {
        get => GetAttributeValue<float>(Schema.Attributes[18].Name);
        set => AddAttribute(Schema.Attributes[18].Name, value);
    }

    public float LodBias
    {
        get => GetAttributeValue<float>(Schema.Attributes[19].Name);
        set => AddAttribute(Schema.Attributes[19].Name, value);
    }

    public bool EnableVertexTexture
    {
        get => GetAttributeValue<bool>(Schema.Attributes[20].Name);
        set => AddAttribute(Schema.Attributes[20].Name, value);
    }

    public uint BorderColor
    {
        get => GetAttributeValue<uint>(Schema.Attributes[21].Name);
        set => AddAttribute(Schema.Attributes[21].Name, value);
    }

    public uint ImageBlockCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[22].Name);
        set => AddAttribute(Schema.Attributes[22].Name, value);
    }

    public uint MipZeroAbsent
    {
        get => GetAttributeValue<uint>(Schema.Attributes[23].Name);
        set => AddAttribute(Schema.Attributes[23].Name, value);
    }

    public PssgTexture(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    public IEnumerable<PssgTextureImageBlock> ImageBlocks => ChildElements.OfType<PssgTextureImageBlock>();

    internal PssgTexture(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}