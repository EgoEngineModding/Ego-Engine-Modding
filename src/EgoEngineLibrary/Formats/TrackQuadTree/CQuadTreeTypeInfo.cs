using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public class CQuadTreeTypeInfo : IQuadTreeTypeInfo
{
    private const int MaxVert0 = (1 << 24) - 1;
    private const int MaxOffset = (1 << 12) - 1;

    private static readonly Dictionary<CQuadTreeType, CQuadTreeTypeInfo> Infos;

    public CQuadTreeType Type { get; private init; }

    public bool NegativeTriangles => false;

    public bool NegativeVertices => false;

    public bool NegativeMaterials => false;

    public int MaxMaterials => 8;

    static CQuadTreeTypeInfo()
    {
        // Max Stats Info
        // Level, Triangles, Vertices, Materials, Nodes, NodeTriangles, NodeVertices, NodeMaterials
        // Dirt1 L11 T133847 V69660 M8 N69977 NT46 NV45 NM5
        // Grid1 L9 T9706 V9709 M2 N2997 NT16 NV24 NM2
        // Dirt2 L12 T9214 V9770 M4 N3661 NT94 NV66 NM4
        // Dirt3 L11 T59463 V59665 M7 N32741 NT82 NV82 NM5
        // DirtS L9 T2777 V2808 M6 N677 NT19 NV30 NM5
        // Grid2 L12 T20538 V18971 M7 N9953 NT148 NV148 NM5
        // GridA L11 T8580 V8587 M7 N2305 NT30 NV35 NM6
        // DirtR L11 T4394 V4395 M2 N1537 NT16 NV22 NM1 (resetlines new file type)
        // F10   L8 T2424 V2427 M2 N701 NT16 NV24 NM2
        // F11   L11 T2392 V2393 M2 N801 NT17 NV24 NM2
        // F12   L11 T2392 V2393 M2 N869 NT44 NV36 NM2
        // F13   L11 T2392 V2393 M2 N869 NT44 NV36 NM2
        // F14   L11 T4914 V4919 M2 N1377 NT44 NV36 NM2
        Infos = new Dictionary<CQuadTreeType, CQuadTreeTypeInfo>
        {
            [CQuadTreeType.Dirt] = new()
            {
                Type = CQuadTreeType.Dirt,
            },
        };
    }

    public static CQuadTreeTypeInfo Get(CQuadTreeType type)
    {
        if (!Infos.TryGetValue(type, out var info))
        {
            throw new ArgumentOutOfRangeException(nameof(type), $"Unknown CQuadTreeType: {type}");
        }
        
        return info;
    }

    private CQuadTreeTypeInfo()
    {
    }

    public int GetSheetInfo(ref string material)
    {
        return 0;
    }

    public string GetMaterial(string material, int sheetInfo)
    {
        return material;
    }

    public int GetTriangleIndexOffset(int minIndex, int index)
    {
        const int Padding = MaxOffset / 5;
        Debug.Assert(minIndex <= index);
        var offset = index - minIndex;
        return offset switch
        {
            > MaxOffset => MaxOffset - Padding,
            _ => 0
        };
    }

    public bool ShouldSplit(QuadTreeMeshData data)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return Type.ToString();
    }
}
