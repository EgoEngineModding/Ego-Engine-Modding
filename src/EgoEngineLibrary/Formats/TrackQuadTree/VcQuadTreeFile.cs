using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public class VcQuadTreeFile
{
    private const float VertXScale = 1 << 24;
    private const float VertYScale = 1 << 16;
    private static readonly Vector3 ScaleFactor = new(1.0f / VertXScale, 1.0f / VertYScale, 1.0f / VertXScale);
    private static readonly Vector3 EncodeScaleFactor = new(VertXScale, VertYScale, VertXScale);
    private static readonly Vector3 Half = new(0.5f);

    private byte[] _bytes;
    private readonly Vector3 _vertexScale;
    private readonly Vector3 _vertexOffset;
    
    /// <summary>
    /// Identifier useful for debugging purposes.
    /// </summary>
    public string? Identifier { get; set; }

    public byte[] Bytes => _bytes;

    public VcQuadTreeTypeInfo TypeInfo { get; private set; }

    public int NumTriangles => -Header.NumTriangles;

    public int NumVertices => -Header.NumVertices;

    public int NumMaterials
    {
        get
        {
            return TypeInfo.NegativeMaterials ? -Header.NumMaterials : Header.NumMaterials;
        }
    }

    public unsafe int NumNodes
    {
        get
        {
            return Convert.ToInt32((Header.TrianglesOffset - Header.NodesOffset) / sizeof(VcQuadTreeNode));
        }
    }

    public Vector3 BoundsMin => Header.BoundMin;

    public Vector3 BoundsMax => Header.BoundMax;

    public Vector2 BoundsMinXz => new(Header.BoundMin.X, Header.BoundMin.Z);

    public Vector2 BoundsMaxXz => new(Header.BoundMax.X, Header.BoundMax.Z);

    private ref VcQuadTreeHeader Header
    {
        get
        {
            return ref Unsafe.As<byte, VcQuadTreeHeader>(ref _bytes[0]);
        }
    }

    public VcQuadTreeFile(byte[] bytes, VcQuadTreeTypeInfo typeInfo)
    {
        _bytes = bytes;
        TypeInfo = typeInfo;

        ref var header = ref Header;
        _vertexScale = (header.BoundMax - header.BoundMin) * ScaleFactor;
        _vertexOffset = header.BoundMin;
    }

    public static VcQuadTreeFile Create(VcQuadTree quadTree)
    {
        if (quadTree.Data.TypeInfo is not VcQuadTreeTypeInfo typeInfo)
        {
            throw new InvalidCastException("QuadTree is not a VcQuadTree");
        }

        const int maxNodes = short.MaxValue;
        const int maxVerts = ((1 << 10) - 1) + byte.MaxValue;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quadTree.Data.Vertices.Count, maxVerts, nameof(quadTree.Data.Vertices));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quadTree.Data.Materials.Count, typeInfo.MaxMaterials, nameof(quadTree.Data.Materials));

        // Encode node data
        var nodes = quadTree.Traverse().ToArray();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(nodes.Length, maxNodes, nameof(quadTree));

        return typeInfo.Type switch
        {
            VcQuadTreeType.RaceDriverGrid or VcQuadTreeType.Dirt3 =>
                Create<VcQuadTreeTriangle1>(quadTree, nodes, typeInfo),
            VcQuadTreeType.DirtShowdown => Create<VcQuadTreeTriangle2>(quadTree, nodes, typeInfo),
            _ => throw new NotSupportedException($"Type {typeInfo} is not supported.")
        };
    }

    private static unsafe VcQuadTreeFile Create<T>(VcQuadTree quadTree, VcQuadTree[] nodes, VcQuadTreeTypeInfo typeInfo)
        where T : unmanaged, IVcQuadTreeTriangle
    {
        // Encode node data
        var fileNodes = new VcQuadTreeNode[nodes.Length];
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
            int lastTriIndex = 0;
            foreach (var triIndex in node.Elements)
            {
                if (!pastFirst)
                {
                    lastTriIndex = triIndex;
                    nodeTriangleListData.Add((byte)((triIndex >> 8) & 0xFF));
                    nodeTriangleListData.Add((byte)(triIndex & 0xFF));
                    
                    pastFirst = true;
                    continue;
                }
                
                var triIndexOffset = triIndex - lastTriIndex;
                lastTriIndex = triIndex;
                while (triIndexOffset >= 254)
                {
                    triIndexOffset -= 254;
                    nodeTriangleListData.Add(254);
                }
                
                nodeTriangleListData.Add(Convert.ToByte(triIndexOffset));
            }
            
            nodeTriangleListData.Add(255);
        }

        // Update header
        var numMaterials = typeInfo.ForceMaxMaterials ? typeInfo.MaxMaterials : quadTree.Data.Materials.Count;
        var headerSize = sizeof(VcQuadTreeHeader) + numMaterials * 4;
        var vertsSize = quadTree.Data.Vertices.Count * sizeof(VcQuadTreeVertex);
        var nodesSize = nodes.Length * sizeof(VcQuadTreeNode);
        var trisSize = quadTree.Data.Triangles.Count * sizeof(T);
        var numBytes = headerSize + vertsSize + nodesSize + trisSize + nodeTriangleListData.Count;
        var bytes = new byte[numBytes];
        ref var header = ref Unsafe.As<byte, VcQuadTreeHeader>(ref bytes[0]);
        header.BoundMin = quadTree.BoundsMin;
        header.BoundMax = quadTree.BoundsMax;
        header.NumTriangles = -quadTree.Data.Triangles.Count;
        header.NumVertices = -quadTree.Data.Vertices.Count;
        header.NumMaterials = typeInfo.NegativeMaterials ? -numMaterials : numMaterials;
        header.VerticesOffset = Convert.ToUInt32(headerSize);
        header.NodesOffset = Convert.ToUInt32(headerSize + vertsSize);
        header.TrianglesOffset = Convert.ToUInt32(headerSize + vertsSize + nodesSize);
        header.TriangleReferencesOffset = Convert.ToUInt32(headerSize + vertsSize + nodesSize + trisSize);

        var qtc = new VcQuadTreeFile(bytes, typeInfo);

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
        var matToFill = materials.Length <= 0 ? defaultMat : materials[^1];
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

        var triangles = qtc.GetTriangles<T>(qtc.Header);
        for (var i = 0; i < triangles.Length; ++i)
        {
            var triangle = quadTree.Data.Triangles[i];
            triangles[i].MaterialIndex = triangle.MaterialIndex;
            triangles[i].Vertex0 = triangle.A;
            triangles[i].Vertex1 = triangle.B;
            triangles[i].Vertex2 = triangle.C;
            triangles[i].Sheet = quadTree.Data.SheetInfo[i];
        }

        CollectionsMarshal.AsSpan(nodeTriangleListData)
            .CopyTo(bytes.AsSpan(Convert.ToInt32(qtc.Header.TriangleReferencesOffset)));
        return qtc;
    }

    public QuadTreeDataTriangle[] GetTriangles()
    {
        return TypeInfo.Type switch
        {
            VcQuadTreeType.RaceDriverGrid or VcQuadTreeType.Dirt3 => GetTriangles<VcQuadTreeTriangle1>(),
            VcQuadTreeType.DirtShowdown => GetTriangles<VcQuadTreeTriangle2>(),
            _ => throw new NotSupportedException($"Type {TypeInfo} is not supported.")
        };
    }

    private QuadTreeDataTriangle[] GetTriangles<T>() where T : unmanaged, IVcQuadTreeTriangle
    {
        var header = Header;
        var tris = GetTriangles<T>(header);
        var vertices = GetVertices(header);
        var materials = new List<string>(NumMaterials);
        GetMaterials(materials);

        var triangles = new QuadTreeDataTriangle[NumTriangles];
        for (var i = 0; i < NumTriangles; ++i)
        {
            var triangle = tris[i];
            var material = TypeInfo.GetMaterial(materials[triangle.MaterialIndex], triangle.Sheet);
            var position0 = (vertices[triangle.Vertex0].Position * _vertexScale) + _vertexOffset;
            var position1 = (vertices[triangle.Vertex1].Position * _vertexScale) + _vertexOffset;
            var position2 = (vertices[triangle.Vertex2].Position * _vertexScale) + _vertexOffset;
            triangles[i] = new QuadTreeDataTriangle(position0, position1, position2, material);
        }

        return triangles;
    }

    public unsafe void GetMaterials(ICollection<string> materials)
    {
        Span<byte> materialBytes = _bytes.AsSpan(sizeof(VcQuadTreeHeader), NumMaterials * 4);
        while (materialBytes.Length > 0)
        {
            var material = string.Create(4, (nuint)(&materialBytes), static (material, ptr) =>
            {
                var state = *(ReadOnlySpan<byte>*)ptr;
                material[0] = (char)state[0];
                material[1] = (char)state[1];
                material[2] = (char)state[2];
                material[3] = (char)state[3];
            });

            materialBytes = materialBytes[4..];
            materials.Add(material);
        }
    }

    private unsafe void SetMaterials(ReadOnlySpan<int> materials)
    {
        if (NumMaterials == materials.Length)
        {
            materials.CopyTo(GetMaterials());
            return;
        }
        
        // Resize data
        var bytesDelta = (materials.Length - NumMaterials) * 4;
        var originalLength = _bytes.Length;
        var newLength = originalLength + bytesDelta;
        var headerLength = sizeof(VcQuadTreeHeader) + NumMaterials * 4;
        if (bytesDelta > 0)
        {
            Array.Resize(ref _bytes, newLength);
            Array.Copy(_bytes, headerLength, _bytes, headerLength + bytesDelta, originalLength - headerLength);
        }
        else
        {
            Array.Copy(_bytes, headerLength, _bytes, headerLength + bytesDelta, originalLength - headerLength);
            Array.Resize(ref _bytes, newLength);
        }
        
        // Copy data
        var sourceMaterialBytes = MemoryMarshal.Cast<int, byte>(materials);
        Span<byte> targetMaterialBytes = _bytes.AsSpan(sizeof(VcQuadTreeHeader), sourceMaterialBytes.Length);
        sourceMaterialBytes.CopyTo(targetMaterialBytes);
        
        // Update offsets
        ref var header = ref Header;
        header.NumMaterials = materials.Length;
        header.VerticesOffset = (uint)(header.VerticesOffset + bytesDelta);
        header.NodesOffset = (uint)(header.NodesOffset + bytesDelta);
        header.TrianglesOffset = (uint)(header.TrianglesOffset + bytesDelta);
        header.TriangleReferencesOffset = (uint)(header.TriangleReferencesOffset + bytesDelta);
    }

    public void ConvertType(VcQuadTreeType targetType)
    {
        if (targetType == TypeInfo.Type)
        {
            return;
        }

        if (TypeInfo.Type == VcQuadTreeType.DirtShowdown && targetType == VcQuadTreeType.Dirt3)
        {
            ConvertTriangle2To1(out var materialList);
            SetMaterials(materialList);
            TypeInfo = VcQuadTreeTypeInfo.Get(VcQuadTreeType.Dirt3);
        }
        else
        {
            throw new NotSupportedException($"Converting type {TypeInfo} to type {targetType} is not supported.");
        }
    }

    private unsafe void ConvertTriangle2To1(out Span<int> materialList)
    {
        // Target has 4 bits for material id.
        var numMaterials = 0;
        materialList = new int[16];
        
        var header = Header;
        var headerMaterials = GetMaterials();
        var triangleBytes = Bytes.AsSpan(Convert.ToInt32(header.TrianglesOffset),
            sizeof(VcQuadTreeTriangle2) * NumTriangles);
        var source = MemoryMarshal.Cast<byte, VcQuadTreeTriangle2>(triangleBytes);

        Span<byte> destination = new byte[sizeof(VcQuadTreeTriangle1) * NumTriangles];
        var target = MemoryMarshal.Cast<byte, VcQuadTreeTriangle1>(destination);
        for (var i = 0; i < source.Length; ++i)
        {
            ref var sourceTri = ref source[i];
            ref var targetTri = ref target[i];

            var sourceMaterialId = sourceTri.MaterialIndex;
            var sourceSheet = sourceTri.Sheet;
            var material = headerMaterials[sourceMaterialId];
            if ((sourceSheet & 0x01) != 0)
            {
                // Convert the * at the end
                material = (material & 0xFFFFFF) | 0x2A000000;
            }
            
            var targetMaterialId = GetMaterialIndex(materialList, material);
            if (targetMaterialId == -1)
            {
                targetMaterialId = numMaterials;
                materialList[numMaterials] = material;
                ++numMaterials;
            }

            targetTri.Sheet = sourceSheet >> 1;
            targetTri.MaterialIndex = targetMaterialId;
            targetTri.Vertex0 = sourceTri.Vertex0;
            targetTri.Vertex1 = sourceTri.Vertex1;
            targetTri.Vertex2 = sourceTri.Vertex2;
        }
        
        destination.CopyTo(triangleBytes);
        if (numMaterials <= 0)
        {
            return;
        }

        // Fill remaining list
        var lastMaterial = materialList[numMaterials - 1];
        while (numMaterials < 16)
        {
            materialList[numMaterials] = lastMaterial;
            ++numMaterials;
        }

        return;

        static int GetMaterialIndex(ReadOnlySpan<int> materials, int material)
        {
            return materials.IndexOf(material);
        }
    }

    public void DumpObj(TextWriter writer)
    {
        switch (TypeInfo.Type)
        {
            case VcQuadTreeType.RaceDriverGrid:
            case VcQuadTreeType.Dirt3:
                DumpObj<VcQuadTreeTriangle1>(writer);
                break;
            case VcQuadTreeType.DirtShowdown:
                DumpObj<VcQuadTreeTriangle2>(writer);
                break;
            default:
                throw new NotSupportedException($"Dumping type {TypeInfo} is not supported.");
        }
    }

    private void DumpObj<T>(TextWriter writer) where T : unmanaged, IVcQuadTreeTriangle
    {
        var header = Header;
        var vertices = GetVertices(header);
        var triangles = GetTriangles<T>(header);

        for (var i = 0; i < vertices.Length; ++i)
        {
            var position = (vertices[i].Position * _vertexScale) + _vertexOffset;
            writer.WriteLine(
                $"v {position.X.ToString("R", CultureInfo.InvariantCulture)} {position.Y.ToString("R", CultureInfo.InvariantCulture)} {position.Z.ToString("R", CultureInfo.InvariantCulture)}");
        }

        for (var i = 0; i < triangles.Length; ++i)
        {
            var tri = triangles[i];
            writer.WriteLine($"f {tri.Vertex0 + 1} {tri.Vertex1 + 1} {tri.Vertex2 + 1}");
        }
    }

    public int GetNodeChild(int nodeIndex, int childSelect, ref Vector2 xzMinBounds, ref Vector2 xzMaxBounds)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(childSelect, 0, nameof(childSelect));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(childSelect, 3, nameof(childSelect));

        var nodes = GetNodes(Header);
        var node = nodes[nodeIndex];
        if (node.IsLeaf)
        {
            return -1;
        }

        float width = (xzMaxBounds.X - xzMinBounds.X) * 0.5f;
        if ((childSelect & 0b01) != 0)
        {
            xzMinBounds.X += width;
        }
        else
        {
            xzMaxBounds.X -= width;
        }

        width = (xzMaxBounds.Y - xzMinBounds.Y) * 0.5f;
        if ((childSelect & 0b10) != 0)
        {
            xzMaxBounds.Y -= width;
        }
        else
        {
            xzMinBounds.Y += width;
        }
        
        var childIndex = node.ChildIndex + childSelect;
        return childIndex;
    }

    public int GetNodeTriangles(int nodeIndex, Span<int> indices)
    {
        var node = GetNodes(Header)[nodeIndex];
        if (!node.HasTriangles)
        {
            return 0;
        }

        var offset = Convert.ToInt32(Header.TriangleReferencesOffset + node.TriangleListOffset);
        var refsData = _bytes.AsSpan(offset);
        var currentIndex = (refsData[0] << 8) + refsData[1];
        if (indices.Length > 0)
        {
            indices[0] = currentIndex;
        }
        
        var count = 1;
        var countData = refsData[2..];
        while (true)
        {
            if (countData[0] == 0xFF)
            {
                break;
            }

            currentIndex += countData[0];
            if (countData[0] != 0xFE)
            {
                if (indices.Length > count)
                {
                    indices[count] = currentIndex;
                }
                ++count;
            }

            countData = countData[1..];
        }
        
        return count;
    }

    private unsafe Span<int> GetMaterials()
    {
        return MemoryMarshal.Cast<byte, int>(_bytes.AsSpan(sizeof(VcQuadTreeHeader), NumMaterials * 4));
    }

    private unsafe Span<T> GetTriangles<T>(VcQuadTreeHeader header) where T : unmanaged, IVcQuadTreeTriangle
    {
        return MemoryMarshal.Cast<byte, T>(_bytes.AsSpan(Convert.ToInt32(header.TrianglesOffset),
            sizeof(T) * NumTriangles));
    }

    private unsafe Span<VcQuadTreeVertex> GetVertices(VcQuadTreeHeader header)
    {
        return MemoryMarshal.Cast<byte, VcQuadTreeVertex>(_bytes.AsSpan(Convert.ToInt32(header.VerticesOffset),
            sizeof(VcQuadTreeVertex) * NumVertices));
    }

    private Span<VcQuadTreeNode> GetNodes(VcQuadTreeHeader header)
    {
        var nodesLength = Convert.ToInt32(header.TrianglesOffset - header.NodesOffset);
        return MemoryMarshal.Cast<byte, VcQuadTreeNode>(_bytes.AsSpan(Convert.ToInt32(header.NodesOffset),
            nodesLength));
    }

    private struct VcQuadTreeHeader
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

    private struct VcQuadTreeVertex
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
                ArgumentOutOfRangeException.ThrowIfNegative(value, "Packed position X cannot be negative");
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxX,
                    $"Packed position X cannot be greater than {MaxX}");
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
                ArgumentOutOfRangeException.ThrowIfNegative(value, "Packed position Y cannot be negative");
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxY,
                    $"Packed position Y cannot be greater than {MaxY}");
                YLow = (byte)value;
                YHigh = (byte)(value >> 8);
            }
        }

        private int Z
        {
            get => (ZHigh << 16) | (ZMid << 8) | ZLow;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value, "Packed position Z cannot be negative");
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxX,
                    $"Packed position Z cannot be greater than {MaxX}");
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

    private interface IVcQuadTreeTriangle
    {
        int Sheet { get; set; }
    
        int MaterialIndex { get; set; }
    
        int Vertex0 { get; set; }

        int Vertex1 { get; set; }

        int Vertex2 { get; set; }
    }

    private struct VcQuadTreeTriangle1 : IVcQuadTreeTriangle
    {
        private const int MaxVert0 = (1 << 10) - 1;
        private const int MaxVertOffset = byte.MaxValue;
        private const int MaxMaterialIndex = (1 << 4) - 1;

        // unsure if this means sheet
        byte Sheet2BitsVertex0Top6Bits { get; set; }
        byte Vertex0Bottom4BitsMaterialId4Bits { get; set; }
        byte Vertex1Offset { get; set; }
        byte Vertex2Offset { get; set; }

        public int Sheet
        {
            get => Sheet2BitsVertex0Top6Bits >> 6;
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 3, nameof(Sheet));
                Sheet2BitsVertex0Top6Bits = (byte)((Sheet2BitsVertex0Top6Bits & 0x3F) | ((value & 0x03) << 6));
            }
        }

        public int MaterialIndex
        {
            get => (Vertex0Bottom4BitsMaterialId4Bits & 0x0F);
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxMaterialIndex, nameof(MaterialIndex));
                Vertex0Bottom4BitsMaterialId4Bits = (byte)((Vertex0Bottom4BitsMaterialId4Bits & 0xF0) | (value & 0x0F));
            }
        }

        public int Vertex0
        {
            get => ((Sheet2BitsVertex0Top6Bits & 0x3F) << 4) | (Vertex0Bottom4BitsMaterialId4Bits >> 4);
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxVert0, nameof(Vertex0));
                Sheet2BitsVertex0Top6Bits = (byte)((Sheet2BitsVertex0Top6Bits & 0xC0) | ((value >>> 4) & 0x3F));
                Vertex0Bottom4BitsMaterialId4Bits =
                    (byte)((Vertex0Bottom4BitsMaterialId4Bits & 0x0F) | ((value & 0x0F) << 4));
            }
        }

        public int Vertex1
        {
            get => Vertex0 + Vertex1Offset;
            set
            {
                var offset = value - Vertex0;
                ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, MaxVertOffset, nameof(Vertex1));
                Vertex1Offset = (byte)offset;
            }
        }

        public int Vertex2
        {
            get => Vertex0 + Vertex2Offset;
            set
            {
                var offset = value - Vertex0;
                ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, MaxVertOffset, nameof(Vertex2));
                Vertex2Offset = (byte)offset;
            }
        }
    }

    private struct VcQuadTreeTriangle2 : IVcQuadTreeTriangle
    {
        private const int MaxVert0 = (1 << 10) - 1;
        private const int MaxVertOffset = byte.MaxValue;
        private const int MaxMaterialIndex = (1 << 4) - 1;

        byte Vertex0Top8Bits { get; set; }
        byte Vertex0Bottom2BitsSheet3BitsMaterialId3Bits { get; set; }
        byte Vertex1Offset { get; set; }
        byte Vertex2Offset { get; set; }

        public int Sheet
        {
            get => (Vertex0Bottom2BitsSheet3BitsMaterialId3Bits & 0x38) >> 3;
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 7, nameof(Sheet));
                Vertex0Bottom2BitsSheet3BitsMaterialId3Bits =
                    (byte)((Vertex0Bottom2BitsSheet3BitsMaterialId3Bits & 0xC7) | ((value & 0x07) << 3));
            }
        }

        public int MaterialIndex
        {
            get => Vertex0Bottom2BitsSheet3BitsMaterialId3Bits & 0x07;
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxMaterialIndex, nameof(MaterialIndex));
                Vertex0Bottom2BitsSheet3BitsMaterialId3Bits =
                    (byte)((Vertex0Bottom2BitsSheet3BitsMaterialId3Bits & 0xF8) | (value & 0x07));
            }
        }

        public int Vertex0
        {
            get => (Vertex0Top8Bits << 2) | (Vertex0Bottom2BitsSheet3BitsMaterialId3Bits >> 6);
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxVert0, nameof(Vertex0));
                Vertex0Top8Bits = (byte)(value >>> 2);
                Vertex0Bottom2BitsSheet3BitsMaterialId3Bits =
                    (byte)((Vertex0Bottom2BitsSheet3BitsMaterialId3Bits & 0x3F) | ((value & 0x03) << 6));
            }
        }

        public int Vertex1
        {
            get => Vertex0 + Vertex1Offset;
            set
            {
                var offset = value - Vertex0;
                ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, MaxVertOffset, nameof(Vertex1));
                Vertex1Offset = (byte)offset;
            }
        }

        public int Vertex2
        {
            get => Vertex0 + Vertex2Offset;
            set
            {
                var offset = value - Vertex0;
                ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, MaxVertOffset, nameof(Vertex2));
                Vertex2Offset = (byte)offset;
            }
        }
    }

    private struct VcQuadTreeNode
    {
        private const int MaxTriangleListOffset = 0x7FFE;
        private const int MaxChildIndex = 0x7FFF;
        private const int LeafBit = 0x8000;
        private const int LeafWithoutTrianglesId = 0xFFFF;
    
        byte Data0 { get; set; }
        byte Data1 { get; set; }

        private int ChildIndexOrTriangleListOffset
        {
            get => (Data0 << 8) | Data1;
            set
            {
                Data0 = (byte)((value >> 8) & 0xFF);
                Data1 = (byte)(value & 0xFF);
            }
        }

        public int TriangleListOffset
        {
            get => ChildIndexOrTriangleListOffset & (~LeafBit);
            set
            {
                if (value == -1)
                {
                    ChildIndexOrTriangleListOffset = LeafWithoutTrianglesId;
                    return;
                }
            
                ArgumentOutOfRangeException.ThrowIfNegative(value);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxTriangleListOffset, nameof(TriangleListOffset));
                ChildIndexOrTriangleListOffset = value | LeafBit;
            }
        }

        public int ChildIndex
        {
            get => ChildIndexOrTriangleListOffset;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxChildIndex, nameof(ChildIndex));

                ChildIndexOrTriangleListOffset = value;
            }
        }
    
        public bool IsLeaf => (ChildIndexOrTriangleListOffset & LeafBit) != 0;

        public bool HasTriangles => IsLeaf && ChildIndexOrTriangleListOffset != LeafWithoutTrianglesId;
    }
}
