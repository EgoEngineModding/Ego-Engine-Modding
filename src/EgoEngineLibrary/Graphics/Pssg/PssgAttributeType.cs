using System.Numerics;

namespace EgoEngineLibrary.Graphics.Pssg;

public enum PssgAttributeType
{
    Unknown = 0,
    Int = 1,
    String = 2,
    Float = 3,
    Float2 = 4,
    Float3 = 5,
    Float4 = 6,
    //ObjectLink = 7,
}

public static class PssgAttributeTypeExtensions
{
    public static object GetDefaultValue(this PssgAttributeType type)
    {
        return type switch
        {
            PssgAttributeType.Unknown => Array.Empty<byte>(),
            PssgAttributeType.Int => 0,
            PssgAttributeType.String => string.Empty,
            PssgAttributeType.Float => 0f,
            PssgAttributeType.Float2 => Vector2.Zero,
            PssgAttributeType.Float3 => Vector3.Zero,
            PssgAttributeType.Float4 => Vector4.Zero,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}