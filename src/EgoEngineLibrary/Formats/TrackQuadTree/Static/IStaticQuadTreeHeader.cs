using System.Numerics;

namespace EgoEngineLibrary.Formats.TrackQuadTree.Static;

public interface IStaticQuadTreeHeader
{
    Vector3 BoundMin { get; }

    Vector3 BoundMax { get; }

    int NumTriangles { get; }

    int NumVertices { get; }

    int NumMaterials { get; }

    uint VerticesOffset { get; }

    uint NodesOffset { get; }

    uint TrianglesOffset { get; }

    uint TriangleReferencesOffset { get; }
}
