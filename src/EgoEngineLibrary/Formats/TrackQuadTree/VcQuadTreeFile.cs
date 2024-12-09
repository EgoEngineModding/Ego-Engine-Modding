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

    public int NumTriangles => -(GetHeader()).NumTriangles;

    public int NumVertices => -(GetHeader()).NumVertices;

    public int NumMaterials
    {
        get
        {
            ref var header = ref GetHeader();
            return TypeInfo.NegativeMaterials ? -header.NumMaterials : header.NumMaterials;
        }
    }
    
    public unsafe int NumNodes
    {
        get
        {
            ref var header = ref GetHeader();
            return Convert.ToInt32((header.TrianglesOffset - header.NodesOffset) / sizeof(VcQuadTreeNode));
        }
    }

    public Vector2 BoundsMinXz
    {
        get
        {
            ref var header = ref GetHeader();
            return new Vector2(header.BoundMin.X, header.BoundMin.Z);
        }
    }

    public Vector2 BoundsMaxXz
    {
        get
        {
            ref var header = ref GetHeader();
            return new Vector2(header.BoundMax.X, header.BoundMax.Z);
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

    public static unsafe VcQuadTreeFile Create(VcQuadTree quadTree)
    {
        // TODO: validate materials
        const int MaxNodes = short.MaxValue;
        const int MaxVerts = ushort.MaxValue + byte.MaxValue;
        //const int MaxTris = ushort.MaxValue;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quadTree.Data.Vertices.Count, MaxVerts,
            $"The number of vertices cannot be greater than {MaxVerts}.");
        // ArgumentOutOfRangeException.ThrowIfGreaterThan(quadTree.Data.Triangles.Count, ushort.MaxValue,
        //     $"The number of triangles cannot be greater than {MaxTris}.");
        
        // Encode node data
        var nodes = quadTree.Traverse().ToArray();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(nodes.Length, MaxNodes,
            $"The number of nodes cannot be greater than {MaxNodes}.");
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

        var headerSize = sizeof(VcQuadTreeHeader) + quadTree.Data.Materials.Count * 4;
        var vertsSize = quadTree.Data.Vertices.Count * sizeof(VcQuadTreeVertex);
        var nodesSize = nodes.Length * sizeof(VcQuadTreeNode);
        var trisSize = quadTree.Data.Triangles.Count * sizeof(VcQuadTreeTriangle1);
        var numBytes = headerSize + vertsSize + nodesSize + trisSize + nodeTriangleListData.Count;
        var bytes = new byte[numBytes];
        ref var header = ref Unsafe.As<byte, VcQuadTreeHeader>(ref bytes[0]);
        header.BoundMin = quadTree.BoundsMin;
        header.BoundMax = quadTree.BoundsMax;
        header.NumTriangles = -quadTree.Data.Triangles.Count;
        header.NumVertices = -quadTree.Data.Vertices.Count;
        header.NumMaterials = VcQuadTreeTypeInfo.Get(VcQuadTreeType.RaceDriverGrid).NegativeMaterials
            ? -quadTree.Data.Materials.Count
            : quadTree.Data.Materials.Count;
        header.VerticesOffset = Convert.ToUInt32(headerSize);
        header.NodesOffset = Convert.ToUInt32(headerSize + vertsSize);
        header.TrianglesOffset = Convert.ToUInt32(headerSize + vertsSize + nodesSize);
        header.TriangleReferencesOffset = Convert.ToUInt32(headerSize + vertsSize + nodesSize + trisSize);
        
        // Only support one type for now
        var qtc = new VcQuadTreeFile(bytes, VcQuadTreeTypeInfo.Get(VcQuadTreeType.RaceDriverGrid));

        var materials = qtc.GetMaterials();
        for (var i = 0; i < materials.Length; ++i)
        {
            var mat = quadTree.Data.Materials[i];
            materials[i] = Convert.ToByte(mat[0]) |
                           (Convert.ToByte(mat[1]) << 8) |
                           (Convert.ToByte(mat[2]) << 16) |
                           (Convert.ToByte(mat[3]) << 24);
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

        var triangles = qtc.GetTriangles<VcQuadTreeTriangle1>(qtc.Header);
        for (var i = 0; i < triangles.Length; ++i)
        {
            var triangle = quadTree.Data.Triangles[i];
            triangles[i].MaterialIndex = triangle.MaterialIndex;
            triangles[i].Vertex0 = triangle.A;
            triangles[i].Vertex1 = triangle.B;
            triangles[i].Vertex2 = triangle.C;
        }

        CollectionsMarshal.AsSpan(nodeTriangleListData)
            .CopyTo(bytes.AsSpan(Convert.ToInt32(qtc.Header.TriangleReferencesOffset)));
        return qtc;
    }

    public ref VcQuadTreeHeader GetHeader()
    {
        return ref Unsafe.As<byte, VcQuadTreeHeader>(ref _bytes[0]);
    }

    public ref VcQuadTreeHeader Header
    {
        get
        {
            return ref Unsafe.As<byte, VcQuadTreeHeader>(ref _bytes[0]);
        }
    }

    public QuadTreeDataTriangle[] GetTriangles()
    {
        var header = GetHeader();
        var vertices = GetVertices(header);
        var materials = new List<string>(NumTriangles);
        GetMaterials(materials);

        var triangles = new QuadTreeDataTriangle[NumTriangles];
        Span<int> indices = stackalloc int[3];
        for (var i = 0; i < NumTriangles; ++i)
        {
            var triangle = GetTriangle(header, i);
            var material = materials[triangle.MaterialIndex];
            triangle.GetIndices(indices);
            var position0 = (vertices[indices[0]].Position * _vertexScale) + _vertexOffset;
            var position1 = (vertices[indices[1]].Position * _vertexScale) + _vertexOffset;
            var position2 = (vertices[indices[2]].Position * _vertexScale) + _vertexOffset;
            triangles[i] = new QuadTreeDataTriangle(position0, position1, position2, material);
        }

        return triangles;
    }
    
    public Vector3 GetRawVertex(int index)
    {
        return GetVertices(Header)[index].Position;
    }
    
    public IVcQuadTreeTriangle GetTriangle(int index)
    {
        return GetTriangle(Header, index);
    }
    
    private IVcQuadTreeTriangle GetTriangle(VcQuadTreeHeader header, int index)
    {
        return TypeInfo.Type switch
        {
            VcQuadTreeType.RaceDriverGrid or VcQuadTreeType.Dirt3 => GetTriangles<VcQuadTreeTriangle1>(header)[index],
            VcQuadTreeType.GridAutosport => GetTriangles<VcQuadTreeTriangle2>(header)[index],
            _ => throw new NotImplementedException($"Type {TypeInfo} is not implemented.")
        };
    }

    public unsafe void GetMaterials(HashSet<string> materials)
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

    public unsafe void GetMaterials(IList<string> materials)
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

    public unsafe Span<int> GetMaterials()
    {
        return MemoryMarshal.Cast<byte, int>(_bytes.AsSpan(sizeof(VcQuadTreeHeader), NumMaterials * 4));
    }

    public unsafe void SetMaterials(ReadOnlySpan<int> materials)
    {
        // TODO: Validate num materials for type
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
        ref var header = ref GetHeader();
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

        if (TypeInfo.Type == VcQuadTreeType.GridAutosport && targetType == VcQuadTreeType.Dirt3)
        {
            ConvertTriangle2To1(out var materialList);
            SetMaterials(materialList);
            TypeInfo = VcQuadTreeTypeInfo.Get(VcQuadTreeType.Dirt3);
        }
        else
        {
            throw new NotImplementedException($"Converting type {TypeInfo} to type {targetType} is not implemented.");
        }
    }

    private unsafe void ConvertTriangle2To1(out Span<int> materialList)
    {
        // Target has 4 bits for material id.
        var numMaterials = 0;
        materialList = new int[16];
        
        var header = GetHeader();
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

            var sourceMaterialId = sourceTri.GetMaterialId();
            var sourceSheet = sourceTri.GetSheet();
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

            var sourceVert0 = sourceTri.GetVertex0Index();
            targetTri.Vertex0 = sourceVert0;
            targetTri.SetSheet((byte)(sourceSheet >> 1));
            targetTri.MaterialIndex = targetMaterialId;
            targetTri.Vertex1 = sourceVert0 + sourceTri.Vertex1Offset;
            targetTri.Vertex2 = sourceVert0 + sourceTri.Vertex2Offset;
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
    }

    private static int GetMaterialIndex(ReadOnlySpan<int> materials, int material)
    {
        return materials.IndexOf(material);
    }

    public void DumpObj(TextWriter writer)
    {
        switch (TypeInfo.Type)
        {
            case VcQuadTreeType.RaceDriverGrid:
            case VcQuadTreeType.Dirt3:
                DumpRdgObj(writer);
                break;
            case VcQuadTreeType.GridAutosport:
                DumpGridAutosportObj(writer);
                break;
            default:
                throw new NotImplementedException($"Dumping type {TypeInfo} is not implemented.");
        }
    }

    private void DumpGridAutosportObj(TextWriter writer)
    {
        var header = GetHeader();
        var vertexScale = (header.BoundMax - header.BoundMin) *
                          new Vector3(0.000000059604645f, 0.000015258789f, 0.000000059604645f);
        var vertexOffset = header.BoundMin;
        
        var vertices = GetVertices(header);
        var triangles = GetTriangles<VcQuadTreeTriangle2>(header);

        for (var i = 0; i < vertices.Length; ++i)
        {
            var position = (vertices[i].Position * vertexScale) + vertexOffset;
            writer.WriteLine(
                $"v {position.X.ToString("R", CultureInfo.InvariantCulture)} {position.Y.ToString("R", CultureInfo.InvariantCulture)} {position.Z.ToString("R", CultureInfo.InvariantCulture)}");
        }

        Span<int> indices = stackalloc int[3];
        for (var i = 0; i < triangles.Length; ++i)
        {
            triangles[i].GetIndices(indices);
            writer.WriteLine($"f {indices[0] + 1} {indices[1] + 1} {indices[2] + 1}");
        }
    }
    private void DumpRdgObj(TextWriter writer)
    {
        var header = GetHeader();
        var vertexScale = (header.BoundMax - header.BoundMin) *
                          new Vector3(0.000000059604645f, 0.000015258789f, 0.000000059604645f);
        var vertexOffset = header.BoundMin;
        
        var vertices = GetVertices(header);
        var triangles = GetTriangles<VcQuadTreeTriangle1>(header);

        for (var i = 0; i < vertices.Length; ++i)
        {
            var position = (vertices[i].Position * vertexScale) + vertexOffset;
            writer.WriteLine(
                $"v {position.X.ToString("R", CultureInfo.InvariantCulture)} {position.Y.ToString("R", CultureInfo.InvariantCulture)} {position.Z.ToString("R", CultureInfo.InvariantCulture)}");
        }

        Span<int> indices = stackalloc int[3];
        for (var i = 0; i < triangles.Length; ++i)
        {
            triangles[i].GetIndices(indices);
            writer.WriteLine($"f {indices[0] + 1} {indices[1] + 1} {indices[2] + 1}");
        }
    }

    public int GetNodeChild(int nodeIndex, int childSelect, ref Vector2 xzMinBounds, ref Vector2 xzMaxBounds)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(childSelect, 0, nameof(childSelect));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(childSelect, 3, nameof(childSelect));

        var nodes = GetNodes(GetHeader());
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

    private unsafe Span<T> GetTriangles<T>(VcQuadTreeHeader header) where T : unmanaged
    {
        return MemoryMarshal.Cast<byte, T>(_bytes.AsSpan(Convert.ToInt32(header.TrianglesOffset),
            sizeof(T) * NumTriangles));
    }

    private unsafe Span<VcQuadTreeVertex> GetVertices(VcQuadTreeHeader header)
    {
        return MemoryMarshal.Cast<byte, VcQuadTreeVertex>(_bytes.AsSpan(Convert.ToInt32(header.VerticesOffset),
            sizeof(VcQuadTreeVertex) * NumVertices));
    }

    private unsafe Span<VcQuadTreeNode> GetNodes(VcQuadTreeHeader header)
    {
        var nodesLength = Convert.ToInt32(header.TrianglesOffset - header.NodesOffset);
        return MemoryMarshal.Cast<byte, VcQuadTreeNode>(_bytes.AsSpan(Convert.ToInt32(header.NodesOffset),
            nodesLength));
    }
    
    public struct VcQuadTreeHeader
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

    public struct VcQuadTreeVertex
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

    private struct VcQuadTreeTriangle1 : IVcQuadTreeTriangle
    {
        private const int MaxVert0 = (1 << 10) - 1;
        private const int MaxVertOffset = byte.MaxValue;

        // unsure if this means sheet
        byte Sheet2BitsVertex0Top6Bits { get; set; }
        byte Vertex0Bottom4BitsMaterialId4Bits { get; set; }
        byte Vertex1Offset { get; set; }
        byte Vertex2Offset { get; set; }

        public int MaterialIndex
        {
            get => (Vertex0Bottom4BitsMaterialId4Bits & 0x0F);
            set
            {
                Vertex0Bottom4BitsMaterialId4Bits = (byte)((Vertex0Bottom4BitsMaterialId4Bits & 0xF0) | (value & 0x0F));
            }
        }

        public int Vertex0
        {
            get => ((Sheet2BitsVertex0Top6Bits & 0x3F) << 4) | (Vertex0Bottom4BitsMaterialId4Bits >> 4);
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxVert0,
                    $"Triangle vertex 0 cannot be greater than {MaxVert0}.");
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
                ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, MaxVertOffset,
                    $"Triangle vertex 1 offset cannot be greater than {MaxVertOffset}.");
                Vertex1Offset = (byte)offset;
            }
        }

        public int Vertex2
        {
            get => Vertex0 + Vertex2Offset;
            set
            {
                var offset = value - Vertex0;
                ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, MaxVertOffset,
                    $"Triangle vertex 2 offset cannot be greater than {MaxVertOffset}.");
                Vertex2Offset = (byte)offset;
            }
        }

        public void GetIndices(Span<int> indices)
        {
            var index0 = ((Sheet2BitsVertex0Top6Bits & 0x3F) << 4) | (Vertex0Bottom4BitsMaterialId4Bits >> 4);
            indices[0] = index0;
            indices[1] = index0 + Vertex1Offset;
            indices[2] = index0 + Vertex2Offset;
        }

        public void SetSheet(byte index)
        {
            Sheet2BitsVertex0Top6Bits = (byte)((Sheet2BitsVertex0Top6Bits & 0x3F) | ((index & 0x03) << 6));
        }
    }
}

public interface IVcQuadTreeTriangle
{
    int MaterialIndex { get; }
    
    int Vertex0 { get; }

    int Vertex1 { get; }

    int Vertex2 { get; }
    
    void GetIndices(Span<int> indices);
}

public struct VcQuadTreeTriangle2 : IVcQuadTreeTriangle
{
    public byte Vertex0Top8Bits { get; set; }
    public byte Vertex0Bottom2BitsSheet3BitsMaterialId3Bits { get; set; }
    public byte Vertex1Offset { get; set; }
    public byte Vertex2Offset { get; set; }

    public int MaterialIndex
    {
        get => Vertex0Bottom2BitsSheet3BitsMaterialId3Bits & 0x07;
    }

    public int Vertex0 => throw new NotImplementedException();
    public int Vertex1 => throw new NotImplementedException();
    public int Vertex2 => throw new NotImplementedException();

    public void GetIndices(Span<int> indices)
    {
        var index0 = (Vertex0Bottom2BitsSheet3BitsMaterialId3Bits >> 6) | (Vertex0Top8Bits << 2);
        indices[0] = index0;
        indices[1] = index0 + Vertex1Offset;
        indices[2] = index0 + Vertex2Offset;
    }

    public short GetVertex0Index()
    {
        return (short)((Vertex0Top8Bits << 2) | (Vertex0Bottom2BitsSheet3BitsMaterialId3Bits >> 6));
    }

    public byte GetSheet()
    {
        return (byte)((Vertex0Bottom2BitsSheet3BitsMaterialId3Bits & 0x38) >> 3);
    }

    public short GetMaterialId()
    {
        return (short)(Vertex0Bottom2BitsSheet3BitsMaterialId3Bits & 0x07);
    }
}

public struct VcQuadTreeNode
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
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxTriangleListOffset,
                $"Node triangle list offset cannot be greater than {MaxTriangleListOffset}.");
            ChildIndexOrTriangleListOffset = value | LeafBit;
        }
    }

    public int ChildIndex
    {
        get => ChildIndexOrTriangleListOffset;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxChildIndex,
                $"Node child index cannot be greater than {MaxChildIndex}.");

            ChildIndexOrTriangleListOffset = value;
        }
    }
    
    public bool IsLeaf => (ChildIndexOrTriangleListOffset & LeafBit) != 0;

    public bool HasTriangles => IsLeaf && ChildIndexOrTriangleListOffset != LeafWithoutTrianglesId;
}
