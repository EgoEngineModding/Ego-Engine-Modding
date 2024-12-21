using System.Collections.Generic;
using System.Numerics;

using EgoEngineLibrary.Collections;
using EgoEngineLibrary.Formats.TrackQuadTree.Static;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public class TrackGroundQuadTree : QuadTree<TrackGroundQuadTree, int>
{
    private static readonly Vector3 Padding = new(0.1f);
    private readonly QuadTreeMeshData _data;
    private readonly QuadTreeMeshDataView _dataView;

    public Vector3 BoundsMin { get; }

    public Vector3 BoundsMax { get; }

    public override IReadOnlyCollection<int> Elements => _dataView.TriangleIndices;
    
    public static TrackGroundQuadTree Create(QuadTreeMeshData data)
    {
        var boundsMin = data.BoundsMin - Padding;
        var boundsMax = data.BoundsMax + Padding;
        var qtBounds = GetQuadTreeBounds(boundsMin, boundsMax);
        var qt = new TrackGroundQuadTree(boundsMin, boundsMax, qtBounds, data);
        for (var i = 0; i < qt._data.Triangles.Count; ++i)
        {
            qt.Add(i);
        }

        return qt;
    }

    private TrackGroundQuadTree(Vector3 boundsMin, Vector3 boundsMax, QuadTreeBounds bounds, QuadTreeMeshData data)
        : base(bounds, 16)
    {
        _data = data;
        _dataView = new QuadTreeMeshDataView(data);
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
            bounds, _data) { Level = Level + 1 };
    }

    protected override bool AddElement(int data)
    {
        if (!SeparatingAxisTheorem.Intersect(Bounds, _data.DataTriangles[data]))
        {
            return false;
        }

        _dataView.Add(data);
        return true;
    }

    protected override void ClearElements()
    {
        _dataView.Clear();
    }

    protected override bool ShouldSplit()
    {
        return _data.TypeInfo.ShouldSplit(_dataView);
    }

    public override IEnumerable<TrackGroundQuadTree> Traverse()
    {
        return TraverseFromBottomLeft();
    }

    public VcQuadTreeFile BuildVcQuadTree()
    {
        var data = _dataView.ToData();
        data.Optimize();
        return VcQuadTreeFile.Create(data);
    }
}
