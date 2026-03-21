namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgShaderGroupPass : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("SHADERGROUPPASS", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgShaderGroupPass(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("vertexProgram", PssgAttributeType.String),
            new PssgSchemaAttribute("fragmentProgram", PssgAttributeType.String),
            new PssgSchemaAttribute("hullProgram", PssgAttributeType.String),
            new PssgSchemaAttribute("domainProgram", PssgAttributeType.String),
            new PssgSchemaAttribute("blendEnable", PssgAttributeType.Int),
            new PssgSchemaAttribute("blendSource", PssgAttributeType.String),
            new PssgSchemaAttribute("blendDest", PssgAttributeType.String),
            new PssgSchemaAttribute("blendOp", PssgAttributeType.String),
            new PssgSchemaAttribute("separateAlphaBlendEnable", PssgAttributeType.Int),
            new PssgSchemaAttribute("blendSourceAlpha", PssgAttributeType.String),
            new PssgSchemaAttribute("blendDestAlpha", PssgAttributeType.String),
            new PssgSchemaAttribute("blendOpAlpha", PssgAttributeType.String),
            new PssgSchemaAttribute("alphaTestEnable", PssgAttributeType.Int),
            new PssgSchemaAttribute("alphaTestFunc", PssgAttributeType.String),
            new PssgSchemaAttribute("alphaTestRef", PssgAttributeType.Float),
            new PssgSchemaAttribute("alphaToCoverageEnable", PssgAttributeType.Int),
            new PssgSchemaAttribute("alphaToCoverageLevel", PssgAttributeType.Int),
            new PssgSchemaAttribute("depthTestEnable", PssgAttributeType.Int),
            new PssgSchemaAttribute("depthTestFunc", PssgAttributeType.String),
            new PssgSchemaAttribute("depthMaskEnable", PssgAttributeType.Int),
            new PssgSchemaAttribute("cullFaceType", PssgAttributeType.String),
            new PssgSchemaAttribute("polyFillType", PssgAttributeType.String),
            new PssgSchemaAttribute("polyOffsetEnable", PssgAttributeType.Int),
            new PssgSchemaAttribute("polyOffsetFactor", PssgAttributeType.Float),
            new PssgSchemaAttribute("polyOffsetUnits", PssgAttributeType.Float),
            new PssgSchemaAttribute("colorMaskRed", PssgAttributeType.Int),
            new PssgSchemaAttribute("colorMaskGreen", PssgAttributeType.Int),
            new PssgSchemaAttribute("colorMaskBlue", PssgAttributeType.Int),
            new PssgSchemaAttribute("colorMaskAlpha", PssgAttributeType.Int),
            new PssgSchemaAttribute("stencilMode", PssgAttributeType.String),
            new PssgSchemaAttribute("2SidedStencilFrontFunc", PssgAttributeType.String),
            new PssgSchemaAttribute("2SidedStencilFrontRef", PssgAttributeType.Int),
            new PssgSchemaAttribute("2SidedStencilFrontMask", PssgAttributeType.Int),
            new PssgSchemaAttribute("2SidedStencilFrontFailOp", PssgAttributeType.String),
            new PssgSchemaAttribute("2SidedStencilFrontZFailOp", PssgAttributeType.String),
            new PssgSchemaAttribute("2SidedStencilFrontZPassOp", PssgAttributeType.String),
            new PssgSchemaAttribute("2SidedStencilFrontStencilMask", PssgAttributeType.Int),
            new PssgSchemaAttribute("2SidedStencilBackFunc", PssgAttributeType.String),
            new PssgSchemaAttribute("2SidedStencilBackRef", PssgAttributeType.Int),
            new PssgSchemaAttribute("2SidedStencilBackMask", PssgAttributeType.Int),
            new PssgSchemaAttribute("2SidedStencilBackFailOp", PssgAttributeType.String),
            new PssgSchemaAttribute("2SidedStencilBackZFailOp", PssgAttributeType.String),
            new PssgSchemaAttribute("2SidedStencilBackZPassOp", PssgAttributeType.String),
            new PssgSchemaAttribute("2SidedStencilBackStencilMask", PssgAttributeType.Int),
            new PssgSchemaAttribute("normalizeEnable", PssgAttributeType.Int),
            new PssgSchemaAttribute("passConfigMaskLow", PssgAttributeType.Int),
            new PssgSchemaAttribute("passConfigMaskHigh", PssgAttributeType.Int),
            new PssgSchemaAttribute("polySmoothEnable", PssgAttributeType.Int),
            new PssgSchemaAttribute("coupleVertexAndPixelProgram", PssgAttributeType.Int),
        }
    };

    public string VertexProgram
    {
        get => GetAttributeValue<string>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public string FragmentProgram
    {
        get => GetAttributeValue<string>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public string HullProgram
    {
        get => GetAttributeValue<string>(Schema.Attributes[2].Name);
        set => AddAttribute(Schema.Attributes[2].Name, value);
    }

    public string DomainProgram
    {
        get => GetAttributeValue<string>(Schema.Attributes[3].Name);
        set => AddAttribute(Schema.Attributes[3].Name, value);
    }

    public bool BlendEnable
    {
        get => GetAttributeValue<bool>(Schema.Attributes[4].Name);
        set => AddAttribute(Schema.Attributes[4].Name, value);
    }

    public string BlendSource
    {
        get => GetAttributeValue<string>(Schema.Attributes[5].Name);
        set => AddAttribute(Schema.Attributes[5].Name, value);
    }

    public string BlendDest
    {
        get => GetAttributeValue<string>(Schema.Attributes[6].Name);
        set => AddAttribute(Schema.Attributes[6].Name, value);
    }

    public string BlendOp
    {
        get => GetAttributeValue<string>(Schema.Attributes[7].Name);
        set => AddAttribute(Schema.Attributes[7].Name, value);
    }

    public bool SeparateAlphaBlendEnable
    {
        get => GetAttributeValue<bool>(Schema.Attributes[8].Name);
        set => AddAttribute(Schema.Attributes[8].Name, value);
    }

    public string BlendSourceAlpha
    {
        get => GetAttributeValue<string>(Schema.Attributes[9].Name);
        set => AddAttribute(Schema.Attributes[9].Name, value);
    }

    public string BlendDestAlpha
    {
        get => GetAttributeValue<string>(Schema.Attributes[10].Name);
        set => AddAttribute(Schema.Attributes[10].Name, value);
    }

    public string BlendOpAlpha
    {
        get => GetAttributeValue<string>(Schema.Attributes[11].Name);
        set => AddAttribute(Schema.Attributes[11].Name, value);
    }

    public bool AlphaTestEnable
    {
        get => GetAttributeValue<bool>(Schema.Attributes[12].Name);
        set => AddAttribute(Schema.Attributes[12].Name, value);
    }

    public string AlphaTestFunc
    {
        get => GetAttributeValue<string>(Schema.Attributes[13].Name);
        set => AddAttribute(Schema.Attributes[13].Name, value);
    }

    public float AlphaTestRef
    {
        get => GetAttributeValue<float>(Schema.Attributes[14].Name);
        set => AddAttribute(Schema.Attributes[14].Name, value);
    }

    public bool AlphaToCoverageEnable
    {
        get => GetAttributeValue<bool>(Schema.Attributes[15].Name);
        set => AddAttribute(Schema.Attributes[15].Name, value);
    }

    public int AlphaToCoverageLevel
    {
        get => GetAttributeValue<int>(Schema.Attributes[16].Name);
        set => AddAttribute(Schema.Attributes[16].Name, value);
    }

    public bool DepthTestEnable
    {
        get => GetAttributeValue<bool>(Schema.Attributes[17].Name);
        set => AddAttribute(Schema.Attributes[17].Name, value);
    }

    public string DepthTestFunc
    {
        get => GetAttributeValue<string>(Schema.Attributes[18].Name);
        set => AddAttribute(Schema.Attributes[18].Name, value);
    }

    public bool DepthMaskEnable
    {
        get => GetAttributeValue<bool>(Schema.Attributes[19].Name);
        set => AddAttribute(Schema.Attributes[19].Name, value);
    }

    public string CullFaceType
    {
        get => GetAttributeValue<string>(Schema.Attributes[20].Name);
        set => AddAttribute(Schema.Attributes[20].Name, value);
    }

    public string PolyFillType
    {
        get => GetAttributeValue<string>(Schema.Attributes[21].Name);
        set => AddAttribute(Schema.Attributes[21].Name, value);
    }

    public bool PolyOffsetEnable
    {
        get => GetAttributeValue<bool>(Schema.Attributes[22].Name);
        set => AddAttribute(Schema.Attributes[22].Name, value);
    }

    public float PolyOffsetFactor
    {
        get => GetAttributeValue<float>(Schema.Attributes[23].Name);
        set => AddAttribute(Schema.Attributes[23].Name, value);
    }

    public float PolyOffsetUnits
    {
        get => GetAttributeValue<float>(Schema.Attributes[24].Name);
        set => AddAttribute(Schema.Attributes[24].Name, value);
    }

    public bool ColorMaskRed
    {
        get => GetAttributeValue<bool>(Schema.Attributes[25].Name);
        set => AddAttribute(Schema.Attributes[25].Name, value);
    }

    public bool ColorMaskGreen
    {
        get => GetAttributeValue<bool>(Schema.Attributes[26].Name);
        set => AddAttribute(Schema.Attributes[26].Name, value);
    }

    public bool ColorMaskBlue
    {
        get => GetAttributeValue<bool>(Schema.Attributes[27].Name);
        set => AddAttribute(Schema.Attributes[27].Name, value);
    }

    public bool ColorMaskAlpha
    {
        get => GetAttributeValue<bool>(Schema.Attributes[28].Name);
        set => AddAttribute(Schema.Attributes[28].Name, value);
    }

    public string StencilMode
    {
        get => GetAttributeValue<string>(Schema.Attributes[29].Name);
        set => AddAttribute(Schema.Attributes[29].Name, value);
    }

    public string TwoSidedStencilFrontFunc
    {
        get => GetAttributeValue<string>(Schema.Attributes[30].Name);
        set => AddAttribute(Schema.Attributes[30].Name, value);
    }

    public byte TwoSidedStencilFrontRef
    {
        get => GetAttributeValue<byte>(Schema.Attributes[31].Name);
        set => AddAttribute(Schema.Attributes[31].Name, value);
    }

    public byte TwoSidedStencilFrontMask
    {
        get => GetAttributeValue<byte>(Schema.Attributes[32].Name);
        set => AddAttribute(Schema.Attributes[32].Name, value);
    }

    public string TwoSidedStencilFrontFailOp
    {
        get => GetAttributeValue<string>(Schema.Attributes[33].Name);
        set => AddAttribute(Schema.Attributes[33].Name, value);
    }

    public string TwoSidedStencilFrontZFailOp
    {
        get => GetAttributeValue<string>(Schema.Attributes[34].Name);
        set => AddAttribute(Schema.Attributes[34].Name, value);
    }

    public string TwoSidedStencilFrontZPassOp
    {
        get => GetAttributeValue<string>(Schema.Attributes[35].Name);
        set => AddAttribute(Schema.Attributes[35].Name, value);
    }

    public byte TwoSidedStencilFrontStencilMask
    {
        get => GetAttributeValue<byte>(Schema.Attributes[36].Name);
        set => AddAttribute(Schema.Attributes[36].Name, value);
    }

    public string TwoSidedStencilBackFunc
    {
        get => GetAttributeValue<string>(Schema.Attributes[37].Name);
        set => AddAttribute(Schema.Attributes[37].Name, value);
    }

    public byte TwoSidedStencilBackRef
    {
        get => GetAttributeValue<byte>(Schema.Attributes[38].Name);
        set => AddAttribute(Schema.Attributes[38].Name, value);
    }

    public byte TwoSidedStencilBackMask
    {
        get => GetAttributeValue<byte>(Schema.Attributes[39].Name);
        set => AddAttribute(Schema.Attributes[39].Name, value);
    }

    public string TwoSidedStencilBackFailOp
    {
        get => GetAttributeValue<string>(Schema.Attributes[40].Name);
        set => AddAttribute(Schema.Attributes[40].Name, value);
    }

    public string TwoSidedStencilBackZFailOp
    {
        get => GetAttributeValue<string>(Schema.Attributes[41].Name);
        set => AddAttribute(Schema.Attributes[41].Name, value);
    }

    public string TwoSidedStencilBackZPassOp
    {
        get => GetAttributeValue<string>(Schema.Attributes[42].Name);
        set => AddAttribute(Schema.Attributes[42].Name, value);
    }

    public byte TwoSidedStencilBackStencilMask
    {
        get => GetAttributeValue<byte>(Schema.Attributes[43].Name);
        set => AddAttribute(Schema.Attributes[43].Name, value);
    }

    public bool NormalizeEnable
    {
        get => GetAttributeValue<bool>(Schema.Attributes[44].Name);
        set => AddAttribute(Schema.Attributes[44].Name, value);
    }

    public uint PassConfigMaskLow
    {
        get => GetAttributeValue<uint>(Schema.Attributes[45].Name);
        set => AddAttribute(Schema.Attributes[45].Name, value);
    }

    public uint PassConfigMaskHigh
    {
        get => GetAttributeValue<uint>(Schema.Attributes[46].Name);
        set => AddAttribute(Schema.Attributes[46].Name, value);
    }

    public bool PolySmoothEnable
    {
        get => GetAttributeValue<bool>(Schema.Attributes[47].Name);
        set => AddAttribute(Schema.Attributes[47].Name, value);
    }

    public bool CoupleVertexAndPixelProgram
    {
        get => GetAttributeValue<bool>(Schema.Attributes[48].Name);
        set => AddAttribute(Schema.Attributes[48].Name, value);
    }

    public PssgShaderGroupPass(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgShaderGroupPass(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}