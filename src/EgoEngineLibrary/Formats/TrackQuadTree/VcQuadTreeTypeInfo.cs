using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public interface IQuadTreeTypeInfo
{
    int GetTriangleIndexOffset(int minIndex, int index);
    
    bool ValidateTriangle(QuadTreeTriangle triangle);
}

public class VcQuadTreeTypeInfo : IQuadTreeTypeInfo
{
    private static readonly Dictionary<VcQuadTreeType, VcQuadTreeTypeInfo> Infos;

    public VcQuadTreeType Type { get; private init; }

    public bool NegativeMaterials { get; private init; }

    private const int MaxVert0 = 1023;
    private const int MaxOffset = byte.MaxValue;
    private const int MaxVert1 = MaxVert0 + MaxOffset;

    static VcQuadTreeTypeInfo()
    {
        Infos = new Dictionary<VcQuadTreeType, VcQuadTreeTypeInfo>
        {
            [VcQuadTreeType.RaceDriverGrid] = new()
            {
                Type = VcQuadTreeType.RaceDriverGrid,
                NegativeMaterials = true
            },
            [VcQuadTreeType.Dirt3] = new()
            {
                Type = VcQuadTreeType.Dirt3,
                NegativeMaterials = false
            },
            [VcQuadTreeType.GridAutosport] = new()
            {
                Type = VcQuadTreeType.GridAutosport,
                NegativeMaterials = false
            }
        };
    }
    
    public static VcQuadTreeTypeInfo Get(VcQuadTreeType type)
    {
        if (!Infos.TryGetValue(type, out var info))
        {
            throw new ArgumentOutOfRangeException(nameof(type), $"Unknown VcQuadTreeType: {type}");
        }
        
        return info;
    }

    private VcQuadTreeTypeInfo()
    {
    }

    public int GetTriangleIndexOffset(int minIndex, int index)
    {
        const int Padding = 30;
        Debug.Assert(minIndex <= index);
        var offset = index - minIndex;
        return offset switch
        {
            > MaxOffset => MaxOffset - Padding,
            _ => 0
        };
    }

    public bool ValidateTriangle(QuadTreeTriangle triangle)
    {
        // TODO: some games only go up to 8 mats
        triangle.EnsureFirstIndexLowest();
        return triangle is { A: <= MaxVert0, B: <= MaxVert1, C: <= MaxVert1, MaterialIndex: <= 15 };
    }
}
