using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace EgoEngineLibrary.Graphics.Pssg;

public static class PssgStringHelper
{
    private const string FloatFormat = "e9";
    internal static CultureInfo Culture => CultureInfo.InvariantCulture;
    internal static Encoding Encoding => Encoding.Latin1;
    internal static StringComparison StringComparison => StringComparison.Ordinal;

    private static string ToString<T>(this ReadOnlySpan<byte> value, int elementsPerRow,
        Func<ReadOnlySpan<byte>, string> toStringFunc)
        where T : unmanaged
    {
        var elementSize = Unsafe.SizeOf<T>();
        var sb = new StringBuilder();
        for (int e = 0; value.Length >= elementSize; e++)
        {
            if (e % elementsPerRow == 0)
            {
                sb.Append('\n');
            }

            sb.Append(toStringFunc(value));
            sb.Append(' ');
            value = value[elementSize..];
        }

        return sb.ToString();
    }
    private static byte[] ToByteArray<T>(this string value, Action<ReadOnlySpan<char>, Span<byte>> writeFunc)
        where T : unmanaged
    {
        var elementSize = Unsafe.SizeOf<T>();
        var valueSpan = value.AsSpan();
        List<byte> result = [];
        foreach (Range range in valueSpan.SplitAny(' ', '\n', '\r'))
        {
            var part = valueSpan[range];
            if (part.IsEmpty)
            {
                continue;
            }

            var i = result.Count;
            CollectionsMarshal.SetCount(result, result.Count + elementSize);
            var resultSpan = CollectionsMarshal.AsSpan(result)[i..];
            writeFunc(valueSpan[range], resultSpan);
        }

        return result.ToArray();
    }

    public static string ToPssgFloatString(this ReadOnlySpan<byte> value, int elementsPerRow)
    {
        return value.ToString<float>(elementsPerRow,
            static v => BinaryPrimitives.ReadSingleBigEndian(v).ToPssgString());
    }

    public static byte[] ToPssgFloatByteArray(this string value)
    {
        return value.ToByteArray<float>(static (v, d) =>
            BinaryPrimitives.WriteSingleBigEndian(d, float.Parse(v, Culture)));
    }

    public static string ToPssgUIntString(this ReadOnlySpan<byte> value, int elementsPerRow)
    {
        return value.ToString<uint>(elementsPerRow,
            static v => BinaryPrimitives.ReadUInt32BigEndian(v).ToString(Culture));
    }

    public static byte[] ToPssgUIntByteArray(this string value)
    {
        return value.ToByteArray<uint>(static (v, d) =>
            BinaryPrimitives.WriteUInt32BigEndian(d, uint.Parse(v, Culture)));
    }

    public static string ToPssgShortString(this ReadOnlySpan<byte> value, int elementsPerRow)
    {
        return value.ToString<short>(elementsPerRow,
            static v => BinaryPrimitives.ReadInt16BigEndian(v).ToString(Culture));
    }

    public static byte[] ToPssgShortByteArray(this string value)
    {
        return value.ToByteArray<short>(static (v, d) =>
            BinaryPrimitives.WriteInt16BigEndian(d, short.Parse(v, Culture)));
    }

    public static string ToPssgUShortString(this ReadOnlySpan<byte> value, int elementsPerRow)
    {
        return value.ToString<ushort>(elementsPerRow,
            static v => BinaryPrimitives.ReadUInt16BigEndian(v).ToString(Culture));
    }

    public static byte[] ToPssgUShortByteArray(this string value)
    {
        return value.ToByteArray<ushort>(static (v, d) =>
            BinaryPrimitives.WriteUInt16BigEndian(d, ushort.Parse(v, Culture)));
    }

    public static string ToPssgIntString(this ReadOnlySpan<byte> value, int elementsPerRow)
    {
        return value.ToString<int>(elementsPerRow,
            static v => BinaryPrimitives.ReadInt32BigEndian(v).ToString(Culture));
    }

    public static byte[] ToPssgIntByteArray(this string value)
    {
        return value.ToByteArray<int>(static (v, d) =>
            BinaryPrimitives.WriteInt32BigEndian(d, int.Parse(v, Culture)));
    }

    public static string ToPssgHalfString(this ReadOnlySpan<byte> value, int elementsPerRow)
    {
        return value.ToString<Half>(elementsPerRow,
            static v => BinaryPrimitives.ReadHalfBigEndian(v).ToString(Culture));
    }

    public static byte[] ToPssgHalfByteArray(this string value)
    {
        return value.ToByteArray<Half>(static (v, d) =>
            BinaryPrimitives.WriteHalfBigEndian(d, Half.Parse(v, Culture)));
    }

    public static string ToPssgString(this float value)
    {
        return value.ToString(FloatFormat, Culture);
    }

    public static string ToPssgString(this Vector2 value)
    {
        return $"{value.X.ToString(FloatFormat, Culture)} {value.Y.ToString(FloatFormat, Culture)}";
    }

    public static Vector2 ToPssgVector2(this string value)
    {
        int i = 0;
        Vector2 ret = new();
        var valueSpan = value.AsSpan();
        foreach (var range in valueSpan.Split(' '))
        {
            var part = valueSpan[range];
            if (part.IsEmpty)
            {
                continue;
            }

            ret[i++] = float.Parse(part, Culture);
        }

        return i != 2 ? throw new FormatException() : ret;
    }

    public static string ToPssgString(this Vector3 value)
    {
        return $"{value.X.ToString(FloatFormat, Culture)} {value.Y.ToString(FloatFormat, Culture)} {value.Z.ToString(FloatFormat, Culture)}";
    }

    public static Vector3 ToPssgVector3(this string value)
    {
        int i = 0;
        Vector3 ret = new();
        var valueSpan = value.AsSpan();
        foreach (var range in valueSpan.Split(' '))
        {
            var part = valueSpan[range];
            if (part.IsEmpty)
            {
                continue;
            }

            ret[i++] = float.Parse(part, Culture);
        }
        
        return i != 3 ? throw new FormatException() : ret;
    }

    public static string ToPssgString(this Vector4 value)
    {
        return $"{value.X.ToString(FloatFormat, Culture)} {value.Y.ToString(FloatFormat, Culture)} {value.Z.ToString(FloatFormat, Culture)} {value.W.ToString(FloatFormat, Culture)}";
    }

    public static Vector4 ToPssgVector4(this string value)
    {
        int i = 0;
        Vector4 ret = new();
        var valueSpan = value.AsSpan();
        foreach (var range in valueSpan.Split(' '))
        {
            var part = valueSpan[range];
            if (part.IsEmpty)
            {
                continue;
            }

            ret[i++] = float.Parse(part, Culture);
        }
        
        return i != 4 ? throw new FormatException() : ret;
    }
}