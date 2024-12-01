using System.Collections.Generic;
using System.Numerics;

using EgoEngineLibrary.Collections;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public class TrackGroundQuadTree : QuadTree<TrackGroundQuadTree, QuadTreeTriangleData>
{
    public static readonly Vector3 Padding = new(0.1f);
    private readonly QuadTreeMeshData _data;

    public Vector3 BoundsMin { get; }

    public Vector3 BoundsMax { get; }

    public override IReadOnlyCollection<QuadTreeTriangleData> Elements => _data.GetTriangles();

    public TrackGroundQuadTree(Vector3 boundsMin, Vector3 boundsMax, IQuadTreeTypeInfo typeInfo)
        : this(boundsMin, boundsMax, new QuadTreeMeshData(typeInfo))
    {
    }

    private TrackGroundQuadTree(Vector3 boundsMin, Vector3 boundsMax, QuadTreeMeshData data)
        : base(new QuadTreeBounds(new Vector2(boundsMin.X, boundsMin.Z), new Vector2(boundsMax.X, boundsMax.Z)),
            TrackGround.CellWidthBits)
    {
        _data = data;
        BoundsMin = boundsMin;
        BoundsMax = boundsMax;
    }

    protected override TrackGroundQuadTree CreateChild(QuadTreeBounds bounds)
    {
        return new TrackGroundQuadTree(
            new Vector3(bounds.Min.X, BoundsMin.Y, bounds.Min.Y),
            new Vector3(bounds.Max.X, BoundsMax.Y, bounds.Max.Y), _data.TypeInfo) { Level = Level + 1 };
    }

    protected override bool AddElement(QuadTreeTriangleData data)
    {
        if (!Fits(data))
        {
            return false;
        }

        _data.Add(data);
        return true;
        bool Fits(QuadTreeTriangleData tri)
        {
            return SeparatingAxisTheorem.Intersect(Bounds, tri);
        }
    }

    protected override void ClearElements()
    {
        _data.ClearElements();
    }

    protected override bool ShouldSplit()
    {
        return !_data.IsValid();
    }

    public VcQuadTreeFile BuildVcQuadTree()
    {
        var quadTree = VcQuadTree.Create(_data.BoundsMin - Padding, _data.BoundsMax + Padding, _data);
        quadTree.Optimize();
        return VcQuadTreeFile.Create(quadTree);
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
}
