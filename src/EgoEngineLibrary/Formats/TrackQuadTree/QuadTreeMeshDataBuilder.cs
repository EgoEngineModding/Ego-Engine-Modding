using System;
using System.Collections.Generic;
using System.Numerics;

using EgoEngineLibrary.Collections;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public class QuadTreeMeshDataBuilder
{
    private readonly List<string> _materials;
    private readonly OrderedSet<Vector3> _vertices;
    private readonly OrderedSet<QuadTreeTriangle> _triangles;
    private readonly List<int> _sheetInfo;

    public IReadOnlyList<string> Materials => _materials;

    public IReadOnlyList<Vector3> Vertices => _vertices;

    public IReadOnlyList<QuadTreeTriangle> Triangles => _triangles;
    
    public IReadOnlyList<int> SheetInfo => _sheetInfo;

    public Vector3 BoundsMin { get; private set; } = new(float.MaxValue);

    public Vector3 BoundsMax { get; private set; } = new(float.MinValue);

    public IQuadTreeTypeInfo TypeInfo { get; }

    public QuadTreeMeshDataBuilder(IQuadTreeTypeInfo typeInfo)
    {
        TypeInfo = typeInfo;
        _materials = [];
        _vertices = new OrderedSet<Vector3>(VertexEqualityComparer.Instance);
        _triangles = [];
        _sheetInfo = [];
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
        if (_triangles.IndexOf(tri) != -1)
        {
            return;
        }

        _triangles.Add(tri);
        _sheetInfo.Add(sheetInfo);

        var bounds = data.GetBounds();
        BoundsMin = Vector3.Min(BoundsMin, bounds.BoundsMin);
        BoundsMax = Vector3.Max(BoundsMax, bounds.BoundsMax);
    }

    public void Reset()
    {
        BoundsMin = new Vector3(float.MaxValue);
        BoundsMax = new Vector3(float.MinValue);
        _materials.Clear();
        _vertices.Clear();
        _triangles.Clear();
        _sheetInfo.Clear();
    }

    public QuadTreeMeshData Build()
    {
        return new QuadTreeMeshData(this);
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

    private class VertexEqualityComparer : IEqualityComparer<Vector3>
    {
        private const int FractionalDigit = 4;
        private const float Tolerance = 0.0001f;
        public static readonly VertexEqualityComparer Instance = new();

        public bool Equals(Vector3 x, Vector3 y)
        {
            var diff = Vector3.Abs(x - y);
            return diff is { X: < Tolerance, Y: < Tolerance, Z: < Tolerance };
        }

        public int GetHashCode(Vector3 obj)
        {
            return HashCode.Combine(
                float.Round(obj.X, FractionalDigit, MidpointRounding.ToZero),
                float.Round(obj.Y, FractionalDigit, MidpointRounding.ToZero),
                float.Round(obj.Z, FractionalDigit, MidpointRounding.ToZero));
        }
    }
}
