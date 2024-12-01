using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public class QuadTreeMeshData
{
    private readonly List<string> _materials;
    private readonly List<Vector3> _vertices;
    private readonly List<QuadTreeTriangle> _triangles;

    public IReadOnlyList<string> Materials => _materials;
    public IReadOnlyList<Vector3> Vertices => _vertices;
    public IReadOnlyList<QuadTreeTriangle> Triangles => _triangles;

    public Vector3 BoundsMin { get; private set; } = new(float.MaxValue);

    public Vector3 BoundsMax { get; private set; } = new(float.MinValue);

    public IQuadTreeTypeInfo TypeInfo { get; }

    public QuadTreeMeshData(IQuadTreeTypeInfo typeInfo)
    {
        TypeInfo = typeInfo;
        _materials = [];
        _vertices = [];
        _triangles = [];
    }

    public bool IsValid()
    {
        var tri = _triangles[^1];
        return TypeInfo.ValidateTriangle(tri);
    }

    public void Add(QuadTreeTriangleData data)
    {
        var matIndex = _materials.IndexOf(data.Material);
        if (matIndex == -1)
        {
            matIndex = _materials.Count;
            _materials.Add(data.Material);
        }

        var a = GetVertexIndex(data.Position0);
        var b = GetVertexIndex(data.Position1);
        var c = GetVertexIndex(data.Position2);
        var tri = new QuadTreeTriangle(a, b, c, matIndex);
        if (_triangles.IndexOf(tri) == -1)
        {
            _triangles.Add(tri);
        }

        var bounds = data.GetBounds();
        BoundsMin = Vector3.Min(BoundsMin, bounds.BoundsMin);
        BoundsMax = Vector3.Max(BoundsMax, bounds.BoundsMax);
    }

    public void ClearElements()
    {
        BoundsMin = new Vector3(float.MaxValue);
        BoundsMax = new Vector3(float.MinValue);
        _materials.Clear();
        _vertices.Clear();
        _triangles.Clear();
    }

    public QuadTreeTriangleData[] GetTriangles()
    {
        var tris = new QuadTreeTriangleData[_triangles.Count];
        for (int i = 0; i < _triangles.Count; ++i)
        {
            tris[i] = GetTriangle(i);
        }

        return tris;
    }

    public QuadTreeTriangleData GetTriangle(int index)
    {
        var tri = _triangles[index];
        var p0 = _vertices[tri.A];
        var p1 = _vertices[tri.B];
        var p2 = _vertices[tri.C];
        var matIndex = _materials[tri.MaterialIndex];
        return new QuadTreeTriangleData(p0, p1, p2, matIndex);
    }

    private int GetVertexIndex(Vector3 position)
    {
        var index = _vertices.IndexOf(position);
        if (index != -1)
        {
            return index;
        }

        index = _vertices.Count;
        _vertices.Add(position);
        return index;
    }

    public Dictionary<int, int> Remap(IEnumerable<int> triangleIndices)
    {
        // Rebuild vertices and triangles in node triangle reference order
        var oldVertices = _vertices.ToArray();
        _vertices.Clear();
        var oldTriangles = _triangles.ToArray();
        _triangles.Clear();
    
        var triMap = new Dictionary<int, int>(oldTriangles.Length);
        foreach (var index in triangleIndices)
        {
            if (triMap.ContainsKey(index))
            {
                continue;
            }
    
            var tri = oldTriangles[index];
            tri.A = GetVertexIndex(oldVertices[tri.A]);
            tri.B = GetVertexIndex(oldVertices[tri.B]);
            tri.C = GetVertexIndex(oldVertices[tri.C]);
    
            triMap.Add(index, _triangles.Count);
            _triangles.Add(tri);
        }
    
        if (oldTriangles.Length != _triangles.Count)
        {
            // This is unexpected unless there's a bug
            throw new InvalidDataException("Not all vertices were referenced by the given triangles.");
        }
        
        if (oldVertices.Length != _vertices.Count)
        {
            // This is unexpected unless there's a bug
            throw new InvalidDataException("Not all vertices were referenced by the given triangles.");
        }
    
        return triMap;
    }

    public void PatchUp()
    {
        var maxIterations = (int)(_triangles.Count * 20.0f);
        if (maxIterations == 0)
        {
            return;
        }
        
        var iterations = 0;
        var triSpan = CollectionsMarshal.AsSpan(_triangles);
        for (var ti = 0; ti < _triangles.Count; ++ti)
        {
            var finalTri = _triangles[ti];
            var minIndex = Math.Min(Math.Min(finalTri.A, finalTri.B), finalTri.C);
            int index = finalTri.A;
            int offset = TypeInfo.GetTriangleIndexOffset(minIndex, index);
            if (offset == 0)
            {
                index = finalTri.B;
                offset = TypeInfo.GetTriangleIndexOffset(minIndex, index);
                if (offset == 0)
                {
                    index = finalTri.C;
                    offset = TypeInfo.GetTriangleIndexOffset(minIndex, index);
                }
            }

            if (offset == 0)
            {
                continue;
            }

            var insertIndex = minIndex + offset;
            var indexMap = new Dictionary<int, int>();
            MoveVertex(ref insertIndex, ref index, indexMap);
            for (var i = insertIndex; i < index; ++i)
            {
                indexMap[i] = i + 1;
            }

            // Adjust all triangles
            for (var i = 0; i < triSpan.Length; ++i)
            {
                ref var tri = ref triSpan[i];
                tri.A = indexMap.GetValueOrDefault(tri.A, tri.A);
                tri.B = indexMap.GetValueOrDefault(tri.B, tri.B);
                tri.C = indexMap.GetValueOrDefault(tri.C, tri.C);
            }
            
            // Start from beginning in case fixing one broke another
            ti = -1;
            ++iterations;
            if (iterations >= maxIterations)
            {
                break;
            }
        }

        // Ensure A is the lowest index
        for (var i = 0; i < triSpan.Length; ++i)
        {
            ref var tri = ref triSpan[i];
            tri.EnsureFirstIndexLowest();
        }

        return;

        void MoveVertex(ref int insertIndex, ref int index, Dictionary<int, int> indexMap)
        {
            var pos = _vertices[index];
            indexMap[index] = insertIndex;
            if (insertIndex > index)
            {
                _vertices.Insert(insertIndex, pos);
                _vertices.RemoveAt(index);
                (insertIndex, index) = (index, insertIndex);
            }
            else
            {
                _vertices.RemoveAt(index);
                _vertices.Insert(insertIndex, pos);
            }
        }
    }
}
