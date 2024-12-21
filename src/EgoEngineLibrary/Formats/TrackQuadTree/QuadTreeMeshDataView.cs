using System.Collections.Generic;

using EgoEngineLibrary.Collections;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public class QuadTreeMeshDataView
{
    private readonly QuadTreeMeshData _data;
    private readonly OrderedSet<int> _triangleIndices;
    private readonly OrderedSet<int> _vertexIndices;
    private readonly OrderedSet<int> _materialIndices;
    
    public IReadOnlyCollection<int> TriangleIndices => _triangleIndices;

    public int NumVertices => _vertexIndices.Count;

    public int NumMaterials => _materialIndices.Count;

    public QuadTreeMeshDataView(QuadTreeMeshData data)
    {
        _data = data;
        _triangleIndices = [];
        _vertexIndices = [];
        _materialIndices = [];
    }

    public void Add(int data)
    {
        if (!_triangleIndices.Add(data))
        {
            return;
        }

        var tri = _data.Triangles[data];
        _vertexIndices.Add(tri.A);
        _vertexIndices.Add(tri.B);
        _vertexIndices.Add(tri.C);
            
        _materialIndices.Add(tri.MaterialIndex);
    }

    public void Clear()
    {
        _triangleIndices.Clear();
        _vertexIndices.Clear();
        _materialIndices.Clear();
    }

    public QuadTreeMeshData ToData()
    {
        var data = new QuadTreeMeshDataBuilder(_data.TypeInfo);
        foreach (var index in _triangleIndices)
        {
           data.Add(_data.DataTriangles[index]);
        }

        return data.Build();
    }
}
