namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public abstract class PssgRenderInstance : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("RENDERINSTANCE", PssgElementType.None)
    {
        CreateElement = null,
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("streamCount", PssgAttributeType.Int),
            new PssgSchemaAttribute("shader", PssgAttributeType.String),
        }
    };

    public byte StreamCount
    {
        get => GetAttributeValue<byte>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public string Shader
    {
        get => GetAttributeValue<string>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    internal PssgRenderInstance(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }

    public PssgShaderInstance GetShaderInstance() => File.GetObject<PssgShaderInstance>(Shader.AsMemory(1));

    public PssgShaderInstance? TryGetShaderInstance() => File.TryGetObject<PssgShaderInstance>(Shader.AsMemory(1));
}