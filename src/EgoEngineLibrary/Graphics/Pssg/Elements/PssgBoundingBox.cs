using System.Numerics;

namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgBoundingBox : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("BOUNDINGBOX", PssgElementType.Float)
    {
        CreateElement = (s, f, p) => new PssgBoundingBox(s, f, p),
    };

    public Vector3 BoundsMin
    {
        get => Value.GetVector3();
        set => Value.SetVector3(value);
    }

    public Vector3 BoundsMax
    {
        get => Value.AsSpan(12).GetVector3();
        set => Value.AsSpan(12).SetVector3(value);
    }

    public PssgBoundingBox(PssgFile file, PssgElement? parent)
        : base(Schema, file, parent)
    {
    }

    internal PssgBoundingBox(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}