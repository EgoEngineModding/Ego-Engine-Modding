namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgInverseBindMatrix : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("INVERSEBINDMATRIX", PssgElementType.Float)
    {
        CreateElement = (s, f, p) => new PssgInverseBindMatrix(s, f, p),
    };

    public PssgInverseBindMatrix(PssgFile file, PssgElement? parent)
        : base(Schema, file, parent)
    {
    }

    internal PssgInverseBindMatrix(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}