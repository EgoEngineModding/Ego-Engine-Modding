using System.Numerics;

namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgTransform : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("TRANSFORM", PssgElementType.Float)
    {
        CreateElement = (s, f, p) => new PssgTransform(s, f, p),
        ElementsPerRow = 8,
    };

    public Matrix4x4 Transform
    {
        get => Value.GetMatrix4();
        set => Value.SetMatrix4(value);
    }

    public PssgTransform(PssgNode parent)
        : base(Schema, parent.File, parent)
    {
    }

    internal PssgTransform(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}