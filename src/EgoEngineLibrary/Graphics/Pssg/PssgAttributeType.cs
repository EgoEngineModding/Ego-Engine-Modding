using System.Numerics;
using System.Runtime.CompilerServices;

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
    
    public static T CastTo<T>(this PssgAttributeType type, object value)
        where T : notnull
    {
        return type switch
        {
            PssgAttributeType.Int => ConvertFromInt((int)value),
            _ => (T)value,
        };

        static T ConvertFromInt(int val)
        {
            T t = default!;
            switch (t)
            {
                case int or uint:
                    return Unsafe.BitCast<int, T>(val);
                case bool:
                    bool b = Convert.ToBoolean(val);
                    return Unsafe.As<bool, T>(ref b);
                case ushort:
                    ushort us = Convert.ToUInt16(val);
                    return Unsafe.As<ushort, T>(ref us);
                case short:
                    short s = Convert.ToInt16(val);
                    return Unsafe.As<short, T>(ref s);
                case byte:
                    byte by = Convert.ToByte(val);
                    return Unsafe.As<byte, T>(ref by);
                case sbyte:
                    sbyte sb = Convert.ToSByte(val);
                    return Unsafe.As<sbyte, T>(ref sb);
                default:
                    throw new InvalidCastException();
            }
        }
    }
    
    public static object CastFrom<T>(this PssgAttributeType type, T value)
        where T : notnull
    {
        return type switch
        {
            PssgAttributeType.Int => ConvertToInt(value),
            _ => value,
        };

        static int ConvertToInt(T val)
        {
            return val switch
            {
                int i => i,
                uint u => unchecked((int)u),
                bool b => Convert.ToInt32(b),
                ushort us => us,
                short s => s,
                byte by => by,
                sbyte sb => sb,
                _ => throw new InvalidCastException()
            };
        }
    }
}