using System.Collections.Generic;
using System.Numerics;

using EgoEngineLibrary.Collections;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

internal static class CellQuadTree
{
    public static readonly Vector3 Padding = new(0, 0.1f, 0);
}

public abstract class CellQuadTree<T> : QuadTree<T, int> where T : CellQuadTree<T>
{
    protected readonly QuadTreeMeshData _data;
    protected readonly SortedSet<int> _triangleIndices;
        
    public Vector3 BoundsMin { get; }

    public Vector3 BoundsMax { get; }

    public override IReadOnlyCollection<int> Elements => _triangleIndices;

    public QuadTreeMeshData Data => _data;

    protected CellQuadTree(Vector3 boundsMin, Vector3 boundsMax, QuadTreeMeshData data, int maxDepth) : base(
        new QuadTreeBounds(new Vector2(boundsMin.X, boundsMin.Z), new Vector2(boundsMax.X, boundsMax.Z)), maxDepth)
    {
        _data = data;
        _triangleIndices = [];
        BoundsMin = boundsMin;
        BoundsMax = boundsMax;
    }

    protected override bool AddElement(int data)
    {
        if (!SeparatingAxisTheorem.Intersect(Bounds, _data.DataTriangles[data]))
        {
            return false;
        }
        
        _triangleIndices.Add(data);
        return true;
    }

    protected override void ClearElements()
    {
        _triangleIndices.Clear();
    }

    public override IEnumerable<T> Traverse()
    {
        if (Level == 0)
        {
            yield return (T)this;
        }

        if (IsLeaf)
        {
            yield break;
        }

        yield return Children[0];
        yield return Children[1];
        yield return Children[2];
        yield return Children[3];

        foreach (var child in Children[0].Traverse())
        {
            yield return child;
        }

        foreach (var child in Children[1].Traverse())
        {
            yield return child;
        }

        foreach (var child in Children[2].Traverse())
        {
            yield return child;
        }

        foreach (var child in Children[3].Traverse())
        {
            yield return child;
        }
    }
}
