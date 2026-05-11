namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgShaderProgramCodeBlock : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("SHADERPROGRAMCODEBLOCK", PssgElementType.Byte)
    {
        CreateElement = (s, f, p) => new PssgShaderProgramCodeBlock(s, f, p),
    };

    public PssgShaderProgramCodeBlock(PssgFile file, PssgShaderProgramCode parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgShaderProgramCodeBlock(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}