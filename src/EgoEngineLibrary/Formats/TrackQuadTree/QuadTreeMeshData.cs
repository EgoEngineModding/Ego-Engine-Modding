using System;
using System.Collections;
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
    private readonly List<int> _sheetInfo;

    public IReadOnlyList<string> Materials => _materials;

    public IReadOnlyList<Vector3> Vertices => _vertices;

    public IReadOnlyList<QuadTreeTriangle> Triangles => _triangles;

    public IReadOnlyList<QuadTreeDataTriangle> DataTriangles { get; }
    
    public IReadOnlyList<int> SheetInfo => _sheetInfo;

    public Vector3 BoundsMin { get; private set; } = new(float.MaxValue);

    public Vector3 BoundsMax { get; private set; } = new(float.MinValue);

    public IQuadTreeTypeInfo TypeInfo { get; }

    public QuadTreeMeshData(IQuadTreeTypeInfo typeInfo)
    {
        TypeInfo = typeInfo;
        _materials = [];
        _vertices = [];
        _triangles = [];
        _sheetInfo = [];
        DataTriangles = new DataTriangleList(this);
    }

    public bool ShouldSplit()
    {
        return TypeInfo.ShouldSplit(this);
    }

    public void Add(in QuadTreeDataTriangle data)
    {
        var mat = data.Material;
        var sheetInfo = TypeInfo.GetSheetInfo(ref mat);
        
        var matIndex = _materials.IndexOf(mat);
        if (matIndex == -1)
        {
            matIndex = _materials.Count;
            _materials.Add(mat);
        }

        var a = GetVertexIndex(data.Position0);
        var b = GetVertexIndex(data.Position1);
        var c = GetVertexIndex(data.Position2);
        var tri = new QuadTreeTriangle(a, b, c, matIndex);
        tri.EnsureFirstIndexLowest();
        if (_triangles.IndexOf(tri) == -1)
        {
            _triangles.Add(tri);
            _sheetInfo.Add(sheetInfo);
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
        _sheetInfo.Clear();
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

    public bool Optimize()
    {
        Reorder();
        return PatchUp();
    }

    public void Reorder()
    {
        if (_vertices.Count < 3)
        {
            return;
        }
        
        MeshOpt.OptimizeVertexCacheFifo(_triangles, _triangles, _vertices.Count, _vertices.Count);
        MeshOpt.OptimizeVertexFetch(_vertices, _triangles, _vertices);
    }

    public bool PatchUp()
    {
        // Brute-force algorithm to make sure triangle indices are within range of min index
        var maxIterations = _triangles.Count * 3;
        if (_triangles.Count == 0)
        {
            return true;
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
                // Not using the bool currently, but if necessary in future may need to Split QT on false
                throw new InvalidDataException("Failed to adjust triangle indices to be in range of each other.");
                return false;
            }
        }

        // Ensure A is the lowest index
        for (var i = 0; i < triSpan.Length; ++i)
        {
            ref var tri = ref triSpan[i];
            tri.EnsureFirstIndexLowest();
        }

        return true;

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
    
    private class DataTriangleList(QuadTreeMeshData data) : IReadOnlyList<QuadTreeDataTriangle>
    {
        public int Count => data._triangles.Count;

        public QuadTreeDataTriangle this[int index] => GetTriangle(index);

        public IEnumerator<QuadTreeDataTriangle> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
            {
                yield return GetTriangle(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private QuadTreeDataTriangle GetTriangle(int index)
        {
            var tri = data._triangles[index];
            var p0 = data._vertices[tri.A];
            var p1 = data._vertices[tri.B];
            var p2 = data._vertices[tri.C];
            var matIndex = data._materials[tri.MaterialIndex];
            return new QuadTreeDataTriangle(p0, p1, p2, matIndex);
        }
    }
}
