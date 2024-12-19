using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

using EgoEngineLibrary.Formats.TrackQuadTree.Static;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public partial class TrackGround
{
    // Grid subdivided into cells, then cells further subdivided to make collection of nodes
    private const int GridWidthBits = 16;
    private const int GridSubdivisions = 5;
    private const int CellWidthBits = GridWidthBits - GridSubdivisions;

    private const int GridWidth = 1 << GridWidthBits;
    private const int CellWidth = 1 << CellWidthBits;
    private const int NumCells = 1 << GridSubdivisions;
    private const int TotalCells = NumCells * NumCells;
    private static readonly Vector3 ScaleFactor = new(1.0f / GridWidth, 1, 1.0f / GridWidth);

    private readonly Vector3 _scale;
    private readonly VcQuadTreeFile?[] _workspace;
    private readonly CellData[] _grid;

    public Vector3 BoundsMin { get; }

    public Vector3 BoundsMax { get; }

    private TrackGround(Vector3 boundsMin, Vector3 boundsMax, int[] cellSubdivisions)
    {
        BoundsMin = boundsMin;
        BoundsMax = boundsMax;
        _scale = (boundsMax - boundsMin) * ScaleFactor;
        if (cellSubdivisions.Length != TotalCells)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(cellSubdivisions.Length, TotalCells, nameof(cellSubdivisions));
        }

        var totalNodes = 0;
        for (var i = 0; i < cellSubdivisions.Length; ++i)
        {
            totalNodes += 1 << (2 * cellSubdivisions[i]);
        }
        
        _grid = new CellData[TotalCells];
        _workspace = new VcQuadTreeFile?[totalNodes];
        var workspaceIndex = 0;
        for (var i = 0; i < TotalCells; ++i)
        {
            var subdivisions = cellSubdivisions[i];
            var cellData = new CellData(workspaceIndex, subdivisions);
            _grid[i] = cellData;
            workspaceIndex += cellData.NodeCount;
        }
    }
    
    public static TrackGround Create(TrackGroundQuadTree quadTree)
    {
        var offset = quadTree.BoundsMin;
        var scale = quadTree.BoundsMax - quadTree.BoundsMin;
        var maxSubdivisions = new int[TotalCells];
        foreach (var node in quadTree.Traverse())
        {
            if (!node.IsLeaf || node.Elements.Count == 0)
            {
                continue;
            }

            var gridIndex = GetGridXz(node.BoundsMin, offset, scale);
            var topLevelCell = GetCellIndex(gridIndex.X, gridIndex.Z);
            var subSubDivisions = node.Level - GridSubdivisions;
            if (maxSubdivisions[topLevelCell] < subSubDivisions)
            {
                maxSubdivisions[topLevelCell] = subSubDivisions;
            }
        }
        
        var ground = new TrackGround(quadTree.BoundsMin, quadTree.BoundsMax, maxSubdivisions);
        foreach (var node in quadTree.Traverse())
        {
            if (!node.IsLeaf || node.Elements.Count == 0)
            {
                continue;
            }

            (int x, int z) = GetGridXz(node.BoundsMin, offset, scale);
            var nodeWidth = GridWidth >> node.Level;
            var vcQuadTree = node.BuildVcQuadTree();
            ground.Set(vcQuadTree, x, z, x + nodeWidth - 1, z + nodeWidth - 1);
            Debug.Assert(vcQuadTree == ground.Get(x, z));
        }

        Debug.Assert(ground._workspace.Distinct().Count() ==
                     ground.TraverseGrid().Count() + (Array.IndexOf(ground._workspace, null) != -1 ? 1 : 0));
        Debug.Assert(ground.TraverseGrid().Count() == quadTree.Traverse().Count(x => x is { IsLeaf: true, Elements.Count: > 0 }));

        return ground;
        static (int X, int Z) GetGridXz(Vector3 nodeMin, Vector3 offset, Vector3 scale)
        {
            var x = nodeMin.X;
            var gridX = (int)(((x - offset.X) * GridWidth) / scale.X + 0.5f);
            gridX = Math.Clamp(gridX, 0, GridWidth - 1);

            var z = nodeMin.Z;
            var gridZ = (int)(((z - offset.Z) * GridWidth) / scale.Z + 0.5f);
            gridZ = Math.Clamp(gridZ, 0, GridWidth - 1);

            Debug.Assert((gridX & 1) == 0);
            Debug.Assert((gridZ & 1) == 0);
            return (gridX, gridZ);
        }
    }

    private VcQuadTreeFile? Get(int x, int z)
    {
        var gridX = x >> CellWidthBits;
        var gridZ = z >> CellWidthBits;
        var topLevelCell = gridX + (gridZ * NumCells);
        var cell = _grid[topLevelCell];

        var localX = (x - (gridX << CellWidthBits)) >> (CellWidthBits - cell.Subdivisions);
        var localZ = (z - (gridZ << CellWidthBits)) >> (CellWidthBits - cell.Subdivisions);
        var nodeOffset = localX + (localZ * cell.NodeWidth);
        return _workspace[cell.WorkspaceIndex + nodeOffset];
    }

    private void Set(VcQuadTreeFile node, int minX, int minZ, int maxX, int maxZ)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minX);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minX, maxX);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(maxX, GridWidth);
        ArgumentOutOfRangeException.ThrowIfNegative(minZ);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minZ, maxZ);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(maxZ, GridWidth);

        if (node.Identifier is null)
        {
            var level = GridWidthBits - int.Log2((maxX - minX) + 1);
            node.Identifier = GetNameFromData(minX, minZ, level);
        }

        var gridX = minX >> CellWidthBits;
        var gridZ = minZ >> CellWidthBits;
        var gridMaxX = maxX >> CellWidthBits;
        var gridMaxZ = maxZ >> CellWidthBits;

        var topLevelXEnd = gridMaxX + (gridZ * NumCells) + 1;
        var minZ2 = gridZ << CellWidthBits;
        while (gridZ <= gridMaxZ)
        {
            if (gridX <= gridMaxX)
            {
                var topLevelCell = gridX + ~gridMaxX + topLevelXEnd;
                var minX2 = gridX << CellWidthBits;
                while (topLevelCell < topLevelXEnd)
                {
                    var cell = _grid[topLevelCell];
                    var localLevel = CellWidthBits - cell.Subdivisions;

                    var minX2Start = minX2 >> localLevel;
                    var localMinX = (minX >> localLevel) - minX2Start;
                    var localMaxX = (maxX >> localLevel) - minX2Start;
                    if (localMinX < 0)
                    {
                        localMinX = 0;
                    }

                    if (localMaxX >= cell.NodeWidth)
                    {
                        localMaxX = cell.NodeWidth - 1;
                    }
                    
                    var minZ2Start = minZ2 >> localLevel;
                    var localMinZ = (minZ >> localLevel) - minZ2Start;
                    var localMaxZ = (maxZ >> localLevel) - minZ2Start;
                    if (localMinZ < 0)
                    {
                        localMinZ = 0;
                    }

                    if (localMaxZ >= cell.NodeWidth)
                    {
                        localMaxZ = cell.NodeWidth - 1;
                    }

                    if (localMinZ <= localMaxZ)
                    {
                        var nodeOffsetMax = localMinX + (cell.NodeWidth * localMinZ);
                        while (localMinZ <= localMaxZ)
                        {
                            if (localMinX <= localMaxX)
                            {
                                var nodeOffset = nodeOffsetMax;
                                while (nodeOffset <= (localMaxX - localMinX) + nodeOffsetMax)
                                {
                                    _workspace[cell.WorkspaceIndex + nodeOffset] = node;
                                    ++nodeOffset;
                                }
                            }
                            ++localMinZ;
                            nodeOffsetMax += cell.NodeWidth;
                        }
                    }

                    ++topLevelCell;
                    minX2 += CellWidth;
                }
            }
            
            ++gridZ;
            minZ2 += CellWidth;
            topLevelXEnd += NumCells;
        }
    }

    private static int GetCellIndex(int x, int z)
    {
        return (x >> CellWidthBits) + ((z >> CellWidthBits) * NumCells);
    }

    public IEnumerable<TraversalData> TraverseGrid()
    {
        return TraverseGrid(0, 0, 0);
    }

    private IEnumerable<TraversalData> TraverseGrid(int x, int z, int level)
    {
        var isLeaf = IsLeaf(x, z, level);
        if (isLeaf)
        {
            var current = Get(x, z);
            if (current is null)
            {
                yield break;
            }

            var nodeWidth = GridWidth >> level;
            var localBoundsMin = (new Vector3(x, 0, z) * _scale) + BoundsMin;
            var localBoundsMax = (new Vector3(x + nodeWidth, 1, z + nodeWidth) * _scale) + BoundsMin;
            yield return new TraversalData(current, x, z, level)
            {
                BoundsMin = localBoundsMin,
                BoundsMax = localBoundsMax
            };

            yield break;
        }

        var nextLevel = level + 1;
        if (nextLevel > GridWidthBits)
        {
            yield break;
        }
        
        var width = GridWidth >> nextLevel;
        foreach (var traversalData in TraverseGrid(x, z, nextLevel))
        {
            yield return traversalData;
        }
        
        foreach (var traversalData in TraverseGrid(x, z + width, nextLevel))
        {
            yield return traversalData;
        }
        
        foreach (var traversalData in TraverseGrid(x + width, z, nextLevel))
        {
            yield return traversalData;
        }
        
        foreach (var traversalData in TraverseGrid(x + width, z + width, nextLevel))
        {
            yield return traversalData;
        }
    }

    private bool IsLeaf(int x, int z, int level)
    {
        var startCellIndex = GetCellIndex(x, z);
        var startCell = _grid[startCellIndex];

        if (startCell.Subdivisions != 0)
        {
            var subdivisions = level - GridSubdivisions;
            if (subdivisions <= 0)
            {
                return false;
            }
            
            if (startCell.Subdivisions == subdivisions)
            {
                return true;
            }

            var expectedWidth2 = GridWidth >> level;
            var travel = CellWidth >> startCell.Subdivisions;
            var startQt2 = Get(x, z);
            for (var zi = z; zi < z + expectedWidth2; zi += travel)
            {
                for (var xi = x; xi < x + expectedWidth2; xi += travel)
                {
                    var iqt = Get(xi, zi);
                    if (startQt2 == iqt)
                    {
                        continue;
                    }

                    return false;
                }
            }

            return true;
        }

        var expectedWidth = GridWidth >> level;
        var expectedCellWidth = (expectedWidth >> CellWidthBits);
        var cellIndex = startCellIndex;
        var startQt = Get(x, z);
        for (var iz = 0; iz < expectedCellWidth; ++iz)
        {
            for (var ix = 0; ix < expectedCellWidth; ++ix)
            {
                var iCell = _grid[cellIndex + ix];
                var iqt = _workspace[iCell.WorkspaceIndex];
                if (iCell.Subdivisions == 0 && startQt == iqt)
                {
                    continue;
                }

                return false;
            }
            
            cellIndex += NumCells;
        }

        return true;
    }

    private readonly record struct CellData(int WorkspaceIndex, int Subdivisions)
    {
        /// <summary>
        /// The number of nodes in one direction.
        /// </summary>
        public int NodeWidth { get; } = 1 << Subdivisions;
        public int NodeCount => 1 << (2 * Subdivisions);
    }

    public record TraversalData(VcQuadTreeFile QuadTree, int X, int Y, int Level)
    {
        public Vector3 BoundsMin { get; init; }

        public Vector3 BoundsMax { get; init; }
    };
}
