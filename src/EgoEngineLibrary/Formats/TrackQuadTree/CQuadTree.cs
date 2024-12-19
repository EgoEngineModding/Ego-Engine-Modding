using System.IO;
using System.Numerics;

using EgoEngineLibrary.Collections;
using EgoEngineLibrary.Formats.TrackQuadTree.Static;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public class CQuadTree : CellQuadTree<CQuadTree>
{
    public static CQuadTree Create(Vector3 boundsMin, Vector3 boundsMax, QuadTreeMeshData data)
    {
        if (data.Materials.Count > data.TypeInfo.MaxMaterials)
        {
            throw new InvalidDataException(
                $"{nameof(CQuadTree)} cannot have more than {data.TypeInfo.MaxMaterials} materials.");
        }
        
        data.Optimize();
        var qt = new CQuadTree(boundsMin, boundsMax, data);
        for (var i = 0; i < qt._data.Triangles.Count; ++i)
        {
            qt.Add(i);
        }

        return qt;
    }

    private CQuadTree(Vector3 boundsMin, Vector3 boundsMax, QuadTreeMeshData data)
        : base(boundsMin, boundsMax, data, 12)
    {
    }

    protected override CQuadTree CreateChild(QuadTreeBounds bounds)
    {
        return new CQuadTree(
            new Vector3(bounds.Min.X, BoundsMin.Y, bounds.Min.Y),
            new Vector3(bounds.Max.X, BoundsMax.Y, bounds.Max.Y), Data) { Level = Level + 1 };
    }

    protected override bool ShouldSplit()
    {
        return _triangleIndices.Count > 16;
    }

    public CQuadTreeFile CreateFile()
    {
        return CQuadTreeFile.Create(this);
    }
}
