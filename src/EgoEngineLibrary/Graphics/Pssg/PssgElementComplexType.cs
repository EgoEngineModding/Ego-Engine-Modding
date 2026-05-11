using System.Collections.Frozen;

namespace EgoEngineLibrary.Graphics.Pssg;

public enum PssgElementComplexType
{
    Float,
    Float2,
    Float3,
    Float4,
    UInt,
    UInt2,
    UInt3,
    UInt4,
    Short,
    Short2,
    Short3,
    Short4,
    UShort,
    UShort2,
    UShort3,
    UShort4,
    UChar,
    UChar2,
    UChar3,
    UChar4,
    Int,
    Int2,
    Int3,
    Int4,
    Half,
    Half2,
    Half3,
    Half4,
    Char,
    Float3x4,
    Float4x4,
    Char4n,
    /// <summary>
    /// 11-bit 11-bit 10-bit vector, normalized by dividing by (1023.0f, 1023.0f, 511.0f)
    /// </summary>
    HenD3n,
    UIntColor,
    /// <summary>
    /// unsigned 11-bit 11-bit 10-bit vector
    /// </summary>
    UHenD3,
    UChar4N
}

public static class PssgElementComplexTypeExtensions
{
    private static readonly FrozenDictionary<string, PssgElementComplexType> _complexTypes =
        Enum.GetValues<PssgElementComplexType>().ToFrozenDictionary(x => x.ToPssgString());

    extension(Enum)
    {
        public static bool TryParsePssgElementComplexType(string value, out PssgElementComplexType result)
        {
            return _complexTypes.TryGetValue(value, out result);
        }
    }

    extension(PssgElementComplexType complexType)
    {
        public string ToPssgString()
        {
            if (!Enum.IsDefined(complexType))
            {
                throw new ArgumentOutOfRangeException(nameof(complexType), complexType, null);
            }
        
            return complexType switch
            {
                PssgElementComplexType.UIntColor => "uint_color_argb",
                _ => Enum.GetName(complexType)!.ToLowerInvariant(),
            };
        }

        public PssgElementType ToSimpleType()
        {
            return complexType switch
            {
                PssgElementComplexType.Float => PssgElementType.Float,
                PssgElementComplexType.Float2 => PssgElementType.Float,
                PssgElementComplexType.Float3 => PssgElementType.Float,
                PssgElementComplexType.Float4 => PssgElementType.Float,
                PssgElementComplexType.UInt => PssgElementType.UInt,
                PssgElementComplexType.UInt2 => PssgElementType.UInt,
                PssgElementComplexType.UInt3 => PssgElementType.UInt,
                PssgElementComplexType.UInt4 => PssgElementType.UInt,
                PssgElementComplexType.Short => PssgElementType.Short,
                PssgElementComplexType.Short2 => PssgElementType.Short,
                PssgElementComplexType.Short3 => PssgElementType.Short,
                PssgElementComplexType.Short4 => PssgElementType.Short,
                PssgElementComplexType.UShort => PssgElementType.UShort,
                PssgElementComplexType.UShort2 => PssgElementType.UShort,
                PssgElementComplexType.UShort3 => PssgElementType.UShort,
                PssgElementComplexType.UShort4 => PssgElementType.UShort,
                PssgElementComplexType.UChar => PssgElementType.Byte,
                PssgElementComplexType.UChar2 => PssgElementType.Byte,
                PssgElementComplexType.UChar3 => PssgElementType.Byte,
                PssgElementComplexType.UChar4 => PssgElementType.Byte,
                PssgElementComplexType.Int => PssgElementType.Int,
                PssgElementComplexType.Int2 => PssgElementType.Int,
                PssgElementComplexType.Int3 => PssgElementType.Int,
                PssgElementComplexType.Int4 => PssgElementType.Int,
                PssgElementComplexType.Half => PssgElementType.Half,
                PssgElementComplexType.Half2 => PssgElementType.Half,
                PssgElementComplexType.Half3 => PssgElementType.Half,
                PssgElementComplexType.Half4 => PssgElementType.Half,
                PssgElementComplexType.Char => PssgElementType.Byte,
                PssgElementComplexType.Float3x4 => PssgElementType.Float,
                PssgElementComplexType.Float4x4 => PssgElementType.Float,
                PssgElementComplexType.Char4n => PssgElementType.Byte,
                PssgElementComplexType.HenD3n => PssgElementType.Byte,
                PssgElementComplexType.UIntColor => PssgElementType.Byte,
                PssgElementComplexType.UHenD3 => PssgElementType.Byte,
                PssgElementComplexType.UChar4N => PssgElementType.Byte,
                _ => throw new ArgumentOutOfRangeException(nameof(complexType), complexType, null),
            };
        }
    }
}