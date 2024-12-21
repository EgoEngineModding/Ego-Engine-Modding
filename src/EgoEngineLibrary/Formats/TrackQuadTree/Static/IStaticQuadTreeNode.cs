namespace EgoEngineLibrary.Formats.TrackQuadTree.Static;

public interface IStaticQuadTreeNode
{
    int TriangleListOffset { get; }

    int ChildIndex { get; }
    
    bool IsLeaf { get; }

    bool HasTriangles { get; }
}
