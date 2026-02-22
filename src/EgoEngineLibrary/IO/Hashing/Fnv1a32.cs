using System.Buffers.Binary;
using System.IO.Hashing;

namespace EgoEngineLibrary.IO.Hashing;

public sealed class Fnv1a32 : NonCryptographicHashAlgorithm
{
    private uint _hash;

    private const uint FnvDefaultPrime = 0x01000193;
    private const uint FnvDefaultOffsetBasis = 0x811C9DC5;

    public Fnv1a32(int hashLengthInBytes) : base(hashLengthInBytes)
    {
        Reset();
    }

    public override void Append(ReadOnlySpan<byte> source)
    {
        _hash = Append(_hash, source);
    }

    public override void Reset()
    {
        _hash = FnvDefaultOffsetBasis;
    }

    protected override void GetCurrentHashCore(Span<byte> destination)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(destination, _hash);
    }

    public static uint HashToUInt32(ReadOnlySpan<byte> source) =>
        Append(FnvDefaultOffsetBasis, source);

    public uint GetCurrentHashAsUInt32() => _hash;

    private static uint Append(uint hash, ReadOnlySpan<byte> source)
    {
        for (var i = 0; i < source.Length; ++i)
        {
            hash ^= source[i];
            hash *= FnvDefaultPrime;
        }

        return hash;
    }
}