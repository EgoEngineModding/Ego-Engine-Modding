using System.Numerics;

namespace EgoEngineLibrary.Formats.TrackQuadTree.Static;

public struct VcQuadTreeHeader : IStaticQuadTreeHeader
{
    public Vector3 BoundMin { get; set; }

    public Vector3 BoundMax { get; set; }

    public int NumTriangles { get; set; }

    public int NumVertices { get; set; }

    public int NumMaterials { get; set; }

    public uint VerticesOffset { get; set; }

    public uint NodesOffset { get; set; }

    public uint TrianglesOffset { get; set; }

    public uint TriangleReferencesOffset { get; set; }
}
