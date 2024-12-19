using System.Collections.Generic;
using System.Numerics;

using EgoEngineLibrary.Collections;
using EgoEngineLibrary.Formats.TrackQuadTree.Static;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public class TrackGroundQuadTree : QuadTree<TrackGroundQuadTree, QuadTreeDataTriangle>
{
    private static readonly Vector3 Padding = new(0.1f);
    private readonly QuadTreeMeshData _data;

    public Vector3 BoundsMin { get; }

    public Vector3 BoundsMax { get; }

    public override IReadOnlyCollection<QuadTreeDataTriangle> Elements => _data.DataTriangles;

    public static TrackGroundQuadTree Create(Vector3 boundsMin, Vector3 boundsMax, VcQuadTreeTypeInfo typeInfo)
    {
        boundsMin -= Padding;
        boundsMax += Padding;
        var qtBounds = GetQuadTreeBounds(boundsMin, boundsMax);
        var qt = new TrackGroundQuadTree(boundsMin, boundsMax, qtBounds, new QuadTreeMeshData(typeInfo));
        return qt;
    }

    private TrackGroundQuadTree(Vector3 boundsMin, Vector3 boundsMax, QuadTreeBounds bounds, QuadTreeMeshData data)
        : base(bounds, 16)
    {
        _data = data;
        BoundsMin = boundsMin;
        BoundsMax = boundsMax;
    }

    private static QuadTreeBounds GetQuadTreeBounds(Vector3 boundsMin, Vector3 boundsMax)
    {
        return new QuadTreeBounds(new Vector2(boundsMin.X, boundsMin.Z), new Vector2(boundsMax.X, boundsMax.Z));
    }

    protected override TrackGroundQuadTree CreateChild(QuadTreeBounds bounds)
    {
        return new TrackGroundQuadTree(
            new Vector3(bounds.Min.X, BoundsMin.Y, bounds.Min.Y),
            new Vector3(bounds.Max.X, BoundsMax.Y, bounds.Max.Y),
            bounds, new QuadTreeMeshData(_data.TypeInfo)) { Level = Level + 1 };
    }

    protected override bool AddElement(QuadTreeDataTriangle data)
    {
        if (!SeparatingAxisTheorem.Intersect(Bounds, data))
        {
            return false;
        }

        _data.Add(data);
        return true;
    }

    protected override void ClearElements()
    {
        _data.ClearElements();
    }

    protected override bool ShouldSplit()
    {
        return _data.ShouldSplit();
    }

    public override IEnumerable<TrackGroundQuadTree> Traverse()
    {
        if (IsLeaf)
        {
            yield return this;
            yield break;
        }

        // Traverse BL, TL, BR, TR, depth first
        foreach (var child in Children[2].Traverse())
        {
            yield return child;
        }

        foreach (var child in Children[0].Traverse())
        {
            yield return child;
        }

        foreach (var child in Children[3].Traverse())
        {
            yield return child;
        }

        foreach (var child in Children[1].Traverse())
        {
            yield return child;
        }
    }

    public VcQuadTreeFile BuildVcQuadTree()
    {
        _data.Optimize();
        
        var quadTree = VcQuadTree.Create(
            new Vector3(_data.BoundsMin.X, BoundsMin.Y, _data.BoundsMin.Z),
            new Vector3(_data.BoundsMax.X, BoundsMax.Y, _data.BoundsMax.Z), _data);
        return VcQuadTreeFile.Create(quadTree);
    }
}
