namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgShaderProgram : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("SHADERPROGRAM", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgShaderProgram(s, f, p),
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("codeCount", PssgAttributeType.Int),
        }
    };

    public uint CodeCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }
    
    public IEnumerable<PssgShaderProgramCode> Codes => ChildElements.OfType<PssgShaderProgramCode>();

    public PssgShaderProgram(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgShaderProgram(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}