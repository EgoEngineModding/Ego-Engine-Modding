using System;

namespace EgoEngineLibrary.Formats.TrackQuadTree.Static;

public struct VcQuadTreeNode : IStaticQuadTreeNode
{
    private const int MaxTriangleListOffset = 0x7FFE;
    private const int MaxChildIndex = 0x7FFF;
    private const int LeafBit = 0x8000;
    private const int LeafWithoutTrianglesId = 0xFFFF;
    public const int MaxNodes = MaxChildIndex + 4;
    
    byte Data0 { get; set; }
    byte Data1 { get; set; }

    private int ChildIndexOrTriangleListOffset
    {
        get => (Data0 << 8) | Data1;
        set
        {
            Data0 = (byte)((value >> 8) & 0xFF);
            Data1 = (byte)(value & 0xFF);
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
