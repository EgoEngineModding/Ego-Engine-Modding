using System;

namespace EgoEngineLibrary.Formats.TrackQuadTree.Static;

public struct CQuadTreeNode : IStaticQuadTreeNode
{
    private const int MaxTriangleListOffset = 0x7FFFFE;
    private const int MaxChildIndex = 0x7FFFFF;
    private const int LeafBit = 0x800000;
    private const int LeafWithoutTrianglesId = 0xFFFFFF;
    public const int MaxNodes = MaxChildIndex + 4;

    private byte _data0;
    private byte _data1;
    private byte _data2;

    private int ChildIndexOrTriangleListOffset
    {
        get => (_data0 << 16) | (_data1 << 8) | _data2;
        set
        {
            _data0 = (byte)(value >> 16);
            _data1 = (byte)(value >> 8);
            _data2 = (byte)value;
        }
    }

    public int TriangleListOffset
    {
        get => ChildIndexOrTriangleListOffset & (~LeafBit);
        set
        {
            if (value == -1)
            {
                ChildIndexOrTriangleListOffset = LeafWithoutTrianglesId;
                return;
            }
            
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxTriangleListOffset, nameof(TriangleListOffset));
            ChildIndexOrTriangleListOffset = value | LeafBit;
        }
    }

    public int ChildIndex
    {
        get => ChildIndexOrTriangleListOffset;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxChildIndex, nameof(ChildIndex));

            ChildIndexOrTriangleListOffset = value;
        }
    }
    
    public bool IsLeaf => (ChildIndexOrTriangleListOffset & LeafBit) != 0;

    public bool HasTriangles => IsLeaf && ChildIndexOrTriangleListOffset != LeafWithoutTrianglesId;
}
