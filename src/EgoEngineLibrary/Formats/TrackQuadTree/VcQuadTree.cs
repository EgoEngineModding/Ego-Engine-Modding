using System.Numerics;

using EgoEngineLibrary.Collections;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public class VcQuadTree : CellQuadTree<VcQuadTree>
{
    public static VcQuadTree Create(QuadTreeMeshData data)
    {
        var boundsMin = data.BoundsMin - CellQuadTree.Padding;
        var boundsMax = data.BoundsMax + CellQuadTree.Padding;
        var qt = new VcQuadTree(boundsMin, boundsMax, data);
        for (var i = 0; i < qt._data.Triangles.Count; ++i)
        {
            qt.Add(i);
        }

        return qt;
    }

    private VcQuadTree(Vector3 boundsMin, Vector3 boundsMax, QuadTreeMeshData data)
        : base(boundsMin, boundsMax, data, 16)
    {
    }

    protected override VcQuadTree CreateChild(QuadTreeBounds bounds)
    {
        return new VcQuadTree(
            new Vector3(bounds.Min.X, BoundsMin.Y, bounds.Min.Y),
            new Vector3(bounds.Max.X, BoundsMax.Y, bounds.Max.Y), Data) { Level = Level + 1 };
    }

    protected override bool ShouldSplit()
    {
        return _triangleIndices.Count > 64;
    }
}
