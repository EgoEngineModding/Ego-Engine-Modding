using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

using Microsoft.Toolkit.HighPerformance.Buffers;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public class QuadTreeMeshData
{
    private readonly string[] _materials;
    private readonly Vector3[] _vertices;
    private readonly QuadTreeTriangle[] _triangles;
    private readonly int[] _sheetInfo;

    public IReadOnlyList<string> Materials => _materials;

    public IReadOnlyList<Vector3> Vertices => _vertices;

    public IReadOnlyList<QuadTreeTriangle> Triangles => _triangles;

    public IReadOnlyList<QuadTreeDataTriangle> DataTriangles { get; }

    public IReadOnlyList<int> SheetInfo => _sheetInfo;

    public Vector3 BoundsMin { get; }

    public Vector3 BoundsMax { get; }

    public IQuadTreeTypeInfo TypeInfo { get; }

    public QuadTreeMeshData(QuadTreeMeshDataBuilder data)
    {
        TypeInfo = data.TypeInfo;
        _materials = data.Materials.ToArray();
        _vertices = data.Vertices.ToArray();
        _triangles = data.Triangles.ToArray();
        _sheetInfo = data.SheetInfo.ToArray();
        BoundsMin = data.BoundsMin;
        BoundsMax = data.BoundsMax;
        DataTriangles = new DataTriangleList(this);
    }

    public bool Optimize()
    {
        Reorder();
        return PatchUp();
    }

    public void Reorder()
    {
        if (_vertices.Length < 3)
        {
            return;
        }

        // Attempt to reduce PatchUp iterations
        var smallestVertexIndex = 0;
        var length = float.MaxValue;
        for (var i = 0; i < _vertices.Length; ++i)
        {
            var vLength = _vertices[i].LengthSquared();
            if (vLength < length)
            {
                smallestVertexIndex = i;
                length = vLength;
            }
        }

        MeshOpt.OptimizeVertexCacheFifo(_triangles, _triangles, _vertices.Length, _vertices.Length, smallestVertexIndex);
        MeshOpt.OptimizeVertexFetch(_vertices, _triangles, _vertices);
    }

    public bool PatchUp()
    {
        // Brute-force algorithm to make sure triangle indices are within range of min index
        var maxIterations = _triangles.Length * 3;
        if (_triangles.Length == 0)
        {
            return true;
        }
        
        var iterations = 0;
        for (var ti = 0; ti < _triangles.Length; ++ti)
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
            Debug.Assert(insertIndex < index);
            MoveVertex(insertIndex, index);

            var updateCount = index - insertIndex + 1;
            using var indexMapBuffer = SpanOwner<int>.Allocate(updateCount);
            var indexMap = indexMapBuffer.Span;
            indexMap[index - insertIndex] = insertIndex;
            for (var i = insertIndex; i < index; ++i)
            {
                indexMap[i - insertIndex] = i + 1;
            }

            // Adjust all triangles
            for (var i = 0; i < _triangles.Length; ++i)
            {
                ref var tri = ref _triangles[i];

                if (tri.A >= insertIndex && tri.A <= index)
                {
                    tri.A = indexMap[tri.A - insertIndex];
                }

                if (tri.B >= insertIndex && tri.B <= index)
                {
                    tri.B = indexMap[tri.B - insertIndex];
                }

                if (tri.C >= insertIndex && tri.C <= index)
                {
                    tri.C = indexMap[tri.C - insertIndex];
                }
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
        for (var i = 0; i < _triangles.Length; ++i)
        {
            ref var tri = ref _triangles[i];
            tri.EnsureFirstIndexLowest();
        }

        Debug.WriteLine(iterations);
        return true;

        void MoveVertex(int insertIndex, int index)
        {
            var pos = _vertices[index];
            Array.Copy(_vertices, insertIndex, _vertices, insertIndex + 1, index - insertIndex);
            _vertices[insertIndex] = pos;
        }
    }

    private class DataTriangleList(QuadTreeMeshData data) : IReadOnlyList<QuadTreeDataTriangle>
    {
        public int Count => data._triangles.Length;

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
