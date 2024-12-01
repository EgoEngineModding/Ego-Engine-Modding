using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using EgoEngineLibrary.Collections;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public class VcQuadTree : QuadTree<VcQuadTree, int>
{
    private readonly QuadTreeMeshData _data;
    private readonly SortedSet<int> _triangleIndices;
        
    public Vector3 BoundsMin { get; }

    public Vector3 BoundsMax { get; }

    public override IReadOnlyCollection<int> Elements => _triangleIndices;
    
    public QuadTreeMeshData Data => _data;

    public static VcQuadTree Create(Vector3 boundsMin, Vector3 boundsMax, QuadTreeMeshData data)
    {
        var qt = new VcQuadTree(boundsMin, boundsMax, data);
        for (var i = 0; i < qt._data.Triangles.Count; ++i)
        {
            qt.Add(i);
        }

        return qt;
    }

    private VcQuadTree(Vector3 boundsMin, Vector3 boundsMax, QuadTreeMeshData data) : base(
        new QuadTreeBounds(new Vector2(boundsMin.X, boundsMin.Z), new Vector2(boundsMax.X, boundsMax.Z)), 4)
    {
        _data = data;
        _triangleIndices = [];
        BoundsMin = boundsMin;
        BoundsMax = boundsMax;
    }

    protected override VcQuadTree CreateChild(QuadTreeBounds bounds)
    {
        return new VcQuadTree(
            new Vector3(bounds.Min.X, BoundsMin.Y, bounds.Min.Y),
            new Vector3(bounds.Max.X, BoundsMax.Y, bounds.Max.Y), _data) { Level = Level + 1 };
    }

    protected override bool AddElement(int data)
    {
        if (!SeparatingAxisTheorem.Intersect(Bounds, _data.GetTriangle(data)))
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

    protected override bool ShouldSplit()
    {
        return _triangleIndices.Count > 64;
    }

    public override IEnumerable<VcQuadTree> Traverse()
    {
        if (Level == 0)
        {
            yield return this;
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

    public void Optimize()
    {
        var triMap = _data.Remap(TraverseFromBottomLeft().SelectMany(x => x.Elements));
        var array = Traverse().ToArray();
        foreach (var node in Traverse())
        {
            if (node._triangleIndices.Count == 0)
            {
                continue;
            }
            
            var oldIndices = node._triangleIndices.ToArray();
            node._triangleIndices.Clear();
            foreach (var index in oldIndices)
            {
                node._triangleIndices.Add(triMap[index]);
            }
        }
        _data.PatchUp();
    }
}
