﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public interface IQuadTreeTypeInfo
{
    int GetSheetInfo(ref string material);

    string GetMaterial(string material, int sheetInfo);
    
    int GetTriangleIndexOffset(int minIndex, int index);
    
    bool ShouldSplit(QuadTreeMeshData data);
}

public class VcQuadTreeTypeInfo : IQuadTreeTypeInfo
{
    private static readonly Dictionary<VcQuadTreeType, VcQuadTreeTypeInfo> Infos;

    public VcQuadTreeType Type { get; private init; }

    public bool NegativeMaterials { get; private init; }

    public int MaxMaterials { get; private init; }

    public bool ForceMaxMaterials { get; private init; }
    
    public bool SupportsSheetMaterials { get; private init; }

    private const int MaxVert0 = 1023;
    private const int MaxOffset = byte.MaxValue;

    static VcQuadTreeTypeInfo()
    {
        // Max Stats Info
        // Entries, Level, Triangles, Vertices, Materials, Nodes, NodeTriangles
        // Grid1 E532 L9 T1246 V819 M9 N1581 NT170
        // Dirt2 E772 L10 T1574 V864 M14 N1733 NT189
        // Dirt3 E1022 L12 T1565 V929 M16 N937 NT479
        // DirtS E187 L8 T1541 V847 M8 N297 NT165
        // Grid2 E2034 L14 T1591 V856 M8 N645 NT313
        // GridA E689 L11 T1772 V874 M8 N581 NT296
        // DirtR E4590 L14 T1564 V839 M8 N1353 NT504

        // F10   E475 L9 T1688 V942 M15 N341 NT246
        // F11   E486 L9 T1520 V868 M16 N333 NT190
        // F12   E438 L8 T1528 V877 M16 N285 NT109
        // F13   E438 L8 T1528 V886 M16 N285 NT173
        // F14   E438 L8 T1529 V867 M16 N253 NT173
        Infos = new Dictionary<VcQuadTreeType, VcQuadTreeTypeInfo>
        {
            [VcQuadTreeType.RaceDriverGrid] = new()
            {
                Type = VcQuadTreeType.RaceDriverGrid,
                NegativeMaterials = true,
                MaxMaterials = 16,
                ForceMaxMaterials = false,
                SupportsSheetMaterials = true,
            },
            [VcQuadTreeType.Dirt3] = new()
            {
                Type = VcQuadTreeType.Dirt3,
                NegativeMaterials = false,
                MaxMaterials = 16,
                ForceMaxMaterials = true,
                SupportsSheetMaterials = true,
            },
            [VcQuadTreeType.DirtShowdown] = new()
            {
                Type = VcQuadTreeType.DirtShowdown,
                NegativeMaterials = false,
                MaxMaterials = 8,
                ForceMaxMaterials = true,
                SupportsSheetMaterials = false,
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

    public int GetSheetInfo(ref string material)
    {
        var isStarMat = (material[3] == '*');
        // Not sure how to calculate sheet info. Just set to 1 if isStarMat
        if (SupportsSheetMaterials || !isStarMat)
        {
            return 0;
        }

        material = material[..^1] + "+";
        return 1;
    }

    public string GetMaterial(string material, int sheetInfo)
    {
        var isStarMat = (material[3] == '+' && (sheetInfo & 0x1) != 0);
        if (SupportsSheetMaterials || !isStarMat)
        {
            return material;
        }

        return material[..^1] + "*";
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

    public bool ShouldSplit(QuadTreeMeshData data)
    {
        return data.Triangles.Count > 2048 ||
               data.Vertices.Count > (MaxVert0 + 1) ||
               data.Materials.Count > MaxMaterials;
    }

    public override string ToString()
    {
        return Type.ToString();
    }
}
