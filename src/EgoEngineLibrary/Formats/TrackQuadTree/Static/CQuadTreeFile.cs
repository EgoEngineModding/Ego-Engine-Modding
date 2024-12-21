using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EgoEngineLibrary.Formats.TrackQuadTree.Static;

public class CQuadTreeFile : QuadTreeFile<CQuadTreeTypeInfo, CQuadTreeHeader, CQuadTreeNode>
{
    public CQuadTreeFile(byte[] bytes, CQuadTreeTypeInfo? typeInfo = null)
        : base(bytes, typeInfo ?? Identify(bytes))
    {
    }

    public static CQuadTreeTypeInfo Identify(byte[] bytes)
    {
        return CQuadTreeTypeInfo.Get(CQuadTreeType.Dirt);
    }

    public static void Validate(QuadTreeMeshData data, out CQuadTreeTypeInfo typeInfo)
    {
        if (data.TypeInfo is not CQuadTreeTypeInfo info)
        {
            throw new InvalidCastException("Data is not of CQuadTreeType.");
        }
        
        typeInfo = info;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(data.Vertices.Count, CQuadTreeTriangle.MaxVertices, nameof(data.Vertices));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(data.Materials.Count, typeInfo.MaxMaterials, nameof(data.Materials));
    }

    public static CQuadTreeFile Create(QuadTreeMeshData data)
    {
        Validate(data, out var typeInfo);

        data.Optimize();
        var quadTree = CQuadTree.Create(data);
        var nodes = quadTree.Traverse().ToArray();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(nodes.Length, CQuadTreeNode.MaxNodes, nameof(quadTree));

        return typeInfo.Type switch
        {
            CQuadTreeType.Dirt => Create(quadTree, nodes, typeInfo),
            _ => throw new NotSupportedException($"Type {typeInfo} is not supported.")
        };
    }

    private static unsafe CQuadTreeFile Create(CQuadTree quadTree, CQuadTree[] nodes, CQuadTreeTypeInfo typeInfo)
    {
        var triangleRemap = BuildTriangleRemap(quadTree);
        
        // Encode node data
        var fileNodes = new CQuadTreeNode[nodes.Length];
        var nodeTriangleListData = new List<byte>();
        var nodeParents = new Stack<Queue<int>>();
        nodeParents.Push(new Queue<int>(4));
        for (var i = 0; i < nodes.Length; ++i)
        {
            Queue<int> levelQueue;
            if (((i - 1) & 0b11) == 0)
            {
                // Update parent index
                while (nodeParents.TryPeek(out var parentQueue))
                {
                    if (parentQueue.Count == 0)
                    {
                        nodeParents.Pop();
                        continue;
                    }
                    
                    var parentIndex = parentQueue.Dequeue();
                    ref var parentFileNode = ref fileNodes[parentIndex];
                    parentFileNode.ChildIndex = i;

                    if (parentQueue.Count == 0)
                    {
                        nodeParents.Pop();
                    }

                    break;
                }
                
                // Create a queue for this level
                levelQueue = new Queue<int>();
                nodeParents.Push(levelQueue);
            }
            else
            {
                // Get current level queue
                levelQueue = nodeParents.Peek();
            }
            
            var node = nodes[i];
            ref var fileNode = ref fileNodes[i];
            if (!node.IsLeaf)
            {
                levelQueue.Enqueue(i);
                continue;
            }

            if (node.Elements.Count == 0)
            {
                fileNode.TriangleListOffset = -1;
                continue;
            }
                
            fileNode.TriangleListOffset = nodeTriangleListData.Count;
            var pastFirst = false;
            int firstTriIndex = 0;
            foreach (var triIndex in node.Elements.Select(x => triangleRemap[x]).Order())
            {
                if (!pastFirst)
                {
                    firstTriIndex = triIndex;
                    nodeTriangleListData.Add((byte)((triIndex >> 16) & 0xFF));
                    nodeTriangleListData.Add((byte)((triIndex >> 8) & 0xFF));
                    nodeTriangleListData.Add((byte)(triIndex & 0xFF));
                    Debug.Assert(triIndex == ((nodeTriangleListData[^3] << 16) +
                                              (nodeTriangleListData[^2] << 8) +
                                              nodeTriangleListData[^1]));
                    
                    pastFirst = true;
                    continue;
                }
                
                var triIndexOffset = Convert.ToUInt16(triIndex - firstTriIndex);
                nodeTriangleListData.Add((byte)(triIndexOffset >> 8));
                nodeTriangleListData.Add((byte)(triIndexOffset));
            }

            if (node.Elements.Count == 1)
            {
                nodeTriangleListData[^3] |= 0x80;
            }
            else
            {
                nodeTriangleListData[^2] |= 0x80;
            }
        }

        // Update header
        var numMaterials = quadTree.Data.Materials.Count;
        var headerSize = sizeof(CQuadTreeHeader) + numMaterials * 4;
        var numVertices = quadTree.Data.Vertices.Count;
        var vertsSize = numVertices * sizeof(CQuadTreeVertex);
        var nodesSize = nodes.Length * sizeof(CQuadTreeNode);
        var numTriangles = quadTree.Data.Triangles.Count;
        var trisSize = numTriangles * sizeof(CQuadTreeTriangle);
        var numBytes = headerSize + vertsSize + nodesSize + trisSize + nodeTriangleListData.Count;
        var bytes = new byte[numBytes];
        ref var header = ref Unsafe.As<byte, CQuadTreeHeader>(ref bytes[0]);
        header.BoundMin = quadTree.BoundsMin;
        header.BoundMax = quadTree.BoundsMax;
        header.NumTriangles = typeInfo.NegativeTriangles ? -numTriangles : numTriangles;
        header.NumVertices = typeInfo.NegativeVertices ? -numVertices : numVertices;
        header.NumMaterials = typeInfo.NegativeMaterials ? -numMaterials : numMaterials;
        header.VerticesOffset = Convert.ToUInt32(headerSize);
        header.NodesOffset = Convert.ToUInt32(headerSize + vertsSize);
        header.TrianglesOffset = Convert.ToUInt32(headerSize + vertsSize + nodesSize);
        header.TriangleReferencesOffset = Convert.ToUInt32(headerSize + vertsSize + nodesSize + trisSize);

        var qtc = new CQuadTreeFile(bytes, typeInfo);

        var materials = qtc.GetMaterials();
        for (var i = 0; i < quadTree.Data.Materials.Count; ++i)
        {
            var mat = quadTree.Data.Materials[i];
            materials[i] = Convert.ToByte(mat[0]) |
                           (Convert.ToByte(mat[1]) << 8) |
                           (Convert.ToByte(mat[2]) << 16) |
                           (Convert.ToByte(mat[3]) << 24);
        }

        const int defaultMat = 0x41464544; // DEFA
        var matToFill = materials.Length <= 0 ? defaultMat : materials[quadTree.Data.Materials.Count - 1];
        for (var i = quadTree.Data.Materials.Count; i < materials.Length; ++i)
        {
            materials[i] = matToFill;
        }

        var boundsSize = quadTree.BoundsMax - quadTree.BoundsMin;
        var scale = EncodeScaleFactor / boundsSize;
        var vertices = qtc.GetVertices(qtc.Header);
        for (var i = 0; i < vertices.Length; ++i)
        {
            vertices[i].Position = (quadTree.Data.Vertices[i] - qtc._vertexOffset) * scale + Half;
        }

        var qtcNodes = qtc.GetNodes(qtc.Header);
        fileNodes.CopyTo(qtcNodes);

        var triangles = qtc.GetTriangles(qtc.Header);
        for (var i = 0; i < triangles.Length; ++i)
        {
            var oi = triangleRemap[i];
            var triangle = quadTree.Data.Triangles[i];
            triangles[oi].MaterialIndex = triangle.MaterialIndex;
            triangles[oi].Vertex0 = triangle.A;
            triangles[oi].Vertex1 = triangle.B;
            triangles[oi].Vertex2 = triangle.C;

            Debug.Assert(triangles[oi].Vertex0 == triangle.A);
            Debug.Assert(triangles[oi].Vertex1 == triangle.B);
            Debug.Assert(triangles[oi].Vertex2 == triangle.C);
        }

        CollectionsMarshal.AsSpan(nodeTriangleListData)
            .CopyTo(bytes.AsSpan(Convert.ToInt32(qtc.Header.TriangleReferencesOffset)));
        return qtc;
    }

    public override QuadTreeDataTriangle[] GetTriangles()
    {
        var header = Header;
        var tris = GetTriangles(header);
        var vertices = GetVertices(header);
        var materials = new List<string>(NumMaterials);
        GetMaterials(materials);

        var triangles = new QuadTreeDataTriangle[NumTriangles];
        for (var i = 0; i < NumTriangles; ++i)
        {
            var triangle = tris[i];
            var material = TypeInfo.GetMaterial(materials[triangle.MaterialIndex], 0);
            var position0 = (vertices[triangle.Vertex0].Position * _vertexScale) + _vertexOffset;
            var position1 = (vertices[triangle.Vertex1].Position * _vertexScale) + _vertexOffset;
            var position2 = (vertices[triangle.Vertex2].Position * _vertexScale) + _vertexOffset;
            triangles[i] = new QuadTreeDataTriangle(position0, position1, position2, material);
        }

        return triangles;
    }

    public QuadTreeTriangle[] GetNodeTriangles(int nodeIndex)
    {
        return GetNodeTriangles<CQuadTreeTriangle>(nodeIndex);
    }

    public override int GetNodeTriangles(int nodeIndex, Span<int> indices)
    {
        var node = GetNodes(Header)[nodeIndex];
        if (!node.HasTriangles)
        {
            return 0;
        }

        var offset = Convert.ToInt32(Header.TriangleReferencesOffset + node.TriangleListOffset);
        var refsData = _bytes.AsSpan(offset);
        var firstIndex = (refsData[0] << 16) + (refsData[1] << 8) + refsData[2];
        
        var stop = false;
        if (firstIndex > 0x7FFFFF)
        {
            firstIndex &= 0x7FFFFF;
            stop = true;
        }

        if (indices.Length > 0)
        {
            indices[0] = firstIndex;
        }

        var count = 1;
        var countData = refsData[3..];
        while (!stop)
        {
            var currentIndex = (countData[0] << 8) + countData[1];
            if (currentIndex > 0x7FFF)
            {
                currentIndex &= 0x7FFF;
                stop = true;
            }
            else
            {
                countData = countData[2..];
            }

            if (indices.Length > count)
            {
                indices[count] = firstIndex + currentIndex;
            }

            ++count;
        }

        return count;
    }

    private unsafe Span<CQuadTreeTriangle> GetTriangles(CQuadTreeHeader header)
    {
        return MemoryMarshal.Cast<byte, CQuadTreeTriangle>(_bytes.AsSpan(Convert.ToInt32(header.TrianglesOffset),
            sizeof(CQuadTreeTriangle) * NumTriangles));
    }

    private unsafe Span<CQuadTreeVertex> GetVertices(CQuadTreeHeader header)
    {
        return MemoryMarshal.Cast<byte, CQuadTreeVertex>(_bytes.AsSpan(Convert.ToInt32(header.VerticesOffset),
            sizeof(CQuadTreeVertex) * NumVertices));
    }

    private struct CQuadTreeVertex
    {
        private const int MaxX = (1 << 24) - 1;
        private const int MaxY = ushort.MaxValue;

        byte XHigh { get; set; }
        byte XMid { get; set; }
        byte XLow { get; set; }
        byte YHigh { get; set; }
        byte YLow { get; set; }
        byte ZHigh { get; set; }
        byte ZMid { get; set; }
        byte ZLow { get; set; }

        private int X
        {
            get => (XHigh << 16) | (XMid << 8) | XLow;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(X));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxX, nameof(X));
                XLow = (byte)value;
                XMid = (byte)(value >> 8);
                XHigh = (byte)(value >> 16);
            }
        }

        private int Y
        {
            get => (YHigh << 8) | YLow;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(Y));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxY, nameof(Y));
                YLow = (byte)value;
                YHigh = (byte)(value >> 8);
            }
        }

        private int Z
        {
            get => (ZHigh << 16) | (ZMid << 8) | ZLow;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(Z));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxX, nameof(Z));
                ZLow = (byte)value;
                ZMid = (byte)(value >> 8);
                ZHigh = (byte)(value >> 16);
            }
        }

        public Vector3 Position
        {
            get => new(X, Y, Z);
            set
            {
                X = Math.Clamp((int)value.X, 0, MaxX);
                Y = Math.Clamp((int)value.Y, 0, MaxY);
                Z = Math.Clamp((int)value.Z, 0, MaxX);
            }
        }
    }
    
    private struct CQuadTreeTriangle : IQuadTreeTriangle
    {
        private const int MaxVert0 = (1 << 24) - 1;
        private const int MaxVertOffset = (1 << 12) - 1;
        private const int MaxMaterialIndex = byte.MaxValue;
        public const int MaxVertices = MaxVert0 + MaxVertOffset + 1;

        private byte _vertex0High;
        private byte _vertex0Mid;
        private byte _vertex0Low;
        private byte _highNibblesOfBothOffsets;
        private byte _vertex1Low;
        private byte _vertex2Low;
        private byte _material;

        public int MaterialIndex
        {
            get => _material;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(MaterialIndex));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxMaterialIndex, nameof(MaterialIndex));
                _material = (byte)value;
            }
        }

        public int Vertex0
        {
            get => _vertex0Low + (_vertex0Mid << 8) + (_vertex0High << 16);
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(Vertex0));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxVert0, nameof(Vertex0));
                _vertex0High = (byte)(value >> 16);
                _vertex0Mid = (byte)(value >> 8);
                _vertex0Low = (byte)value;
            }
        }

        public int Vertex1
        {
            get => Vertex0 + _vertex1Low + ((_highNibblesOfBothOffsets & 0xF0) << 4);
            set
            {
                var offset = value - Vertex0;
                ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(Vertex1));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, MaxVertOffset, nameof(Vertex1));
                _highNibblesOfBothOffsets = (byte)((_highNibblesOfBothOffsets & 0x0F) | ((offset >> 4) & 0xF0));
                _vertex1Low = (byte)offset;
            }
        }

        public int Vertex2
        {
            get => Vertex0 + _vertex2Low + ((_highNibblesOfBothOffsets & 0x0F) << 8);
            set
            {
                var offset = value - Vertex0;
                ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(Vertex2));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, MaxVertOffset, nameof(Vertex2));
                _highNibblesOfBothOffsets = (byte)((_highNibblesOfBothOffsets & 0xF0) | ((offset >> 8) & 0x0F));
                _vertex2Low = (byte)offset;
            }
        }
    }
}
