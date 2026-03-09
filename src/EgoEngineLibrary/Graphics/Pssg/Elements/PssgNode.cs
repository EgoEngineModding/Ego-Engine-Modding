using System.Numerics;
using System.Runtime.CompilerServices;

namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgNode : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("NODE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgNode(s, f, p),
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("stopTraversal", PssgAttributeType.Int),
            new PssgSchemaAttribute("nickname", PssgAttributeType.String),
        }
    };

    public bool StopTraversal
    {
        get => Convert.ToBoolean(GetAttributeValue(Schema.Attributes[0].Name, 0));
        set => AddAttribute(Schema.Attributes[0].Name, Convert.ToInt32(value));
    }

    public Matrix4x4 Transform
    {
        get => ChildElements.OfType<PssgTransform>().Single().Transform;
        set => ChildElements.OfType<PssgTransform>().Single().Transform = value;
    }

    public Vector3 BoundsMin
    {
        get => ChildElements.OfType<PssgBoundingBox>().Single().BoundsMin;
        set => ChildElements.OfType<PssgBoundingBox>().Single().BoundsMin = value;
    }

    public Vector3 BoundsMax
    {
        get => ChildElements.OfType<PssgBoundingBox>().Single().BoundsMax;
        set => ChildElements.OfType<PssgBoundingBox>().Single().BoundsMax = value;
    }

    public PssgNode(PssgFile file, PssgElement? parent)
        : this(Schema, file, parent)
    {
    }
    
    internal PssgNode(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}