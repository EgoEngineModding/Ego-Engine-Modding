namespace EgoEngineLibrary.Formats.TrackQuadTree;

public interface IQuadTreeTypeInfo
{
    bool NegativeTriangles { get; }
    
    bool NegativeVertices { get; }
    
    bool NegativeMaterials { get; }
    
    int MaxMaterials { get; }
    
    int GetSheetInfo(ref string material);

    string GetMaterial(string material, int sheetInfo);
    
    int GetTriangleIndexOffset(int minIndex, int index);
    
    bool ShouldSplit(QuadTreeMeshData data);
}
