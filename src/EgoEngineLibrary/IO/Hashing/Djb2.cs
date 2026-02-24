using System.Buffers.Binary;
using System.IO.Hashing;

namespace EgoEngineLibrary.IO.Hashing;

public sealed class Djb2 : NonCryptographicHashAlgorithm
{
    private uint _hash;

    private const uint DefaultSeed = 5381;

    public Djb2() : base(4)
    {
        Reset();
    }

    public override void Append(ReadOnlySpan<byte> source)
    {
        _hash = Append(_hash, source);
    }

    public override void Reset()
    {
        _hash = DefaultSeed;
    }

    protected override void GetCurrentHashCore(Span<byte> destination)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(destination, _hash);
    }

    public static uint HashToUInt32(ReadOnlySpan<byte> source) =>
        Append(DefaultSeed, source);

    public uint GetCurrentHashAsUInt32() => _hash;

    private static uint Append(uint hash, ReadOnlySpan<byte> source)
    {
        for (var i = 0; i < source.Length; ++i)
        {
            // (hash * 33) + b
            hash = ((hash << 5) + hash) + source[i];
        }

        return hash;
    }
}