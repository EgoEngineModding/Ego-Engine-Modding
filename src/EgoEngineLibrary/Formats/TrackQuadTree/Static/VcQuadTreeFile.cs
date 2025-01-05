using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using EgoEngineLibrary.Collections;

namespace EgoEngineLibrary.Formats.TrackQuadTree.Static;

public class VcQuadTreeFile : QuadTreeFile<VcQuadTreeTypeInfo, VcQuadTreeHeader, VcQuadTreeNode>
{
    public Vector2 BoundsMinXz => new(Header.BoundMin.X, Header.BoundMin.Z);

    public Vector2 BoundsMaxXz => new(Header.BoundMax.X, Header.BoundMax.Z);

    public VcQuadTreeFile(byte[] bytes, VcQuadTreeTypeInfo? typeInfo = null)
        : base(bytes, typeInfo ?? Identify(bytes))
    {
    }

    public static VcQuadTreeTypeInfo Identify(byte[] bytes)
    {
        var header = Unsafe.As<byte, VcQuadTreeHeader>(ref bytes[0]);
        if (header.NumMaterials < 0)
        {
            return VcQuadTreeTypeInfo.Get(VcQuadTreeType.RaceDriverGrid);
        }

        return header.NumMaterials == 16
            ? VcQuadTreeTypeInfo.Get(VcQuadTreeType.Dirt3)
            : VcQuadTreeTypeInfo.Get(VcQuadTreeType.DirtShowdown);
    }

    public static void Validate(QuadTreeMeshData data, out VcQuadTreeTypeInfo typeInfo)
    {
        if (data.TypeInfo is not VcQuadTreeTypeInfo info)
        {
            throw new InvalidCastException("Data is not of VcQuadTreeType.");
        }

        typeInfo = info;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(data.Vertices.Count, VcQuadTreeTriangle1.MaxVertices, nameof(data.Vertices));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(data.Materials.Count, typeInfo.MaxMaterials, nameof(data.Materials));
    }

    public static VcQuadTreeFile Create(QuadTreeMeshData data)
    {
        Validate(data, out var typeInfo);

        var quadTree = VcQuadTree.Create(data);
        var nodes = quadTree.Traverse().ToArray();
        Debug.Assert(nodes.Length <= VcQuadTreeNode.MaxNodes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(nodes.Length, VcQuadTreeNode.MaxNodes, nameof(quadTree));

        return typeInfo.Type switch
        {
            VcQuadTreeType.RaceDriverGrid or VcQuadTreeType.Dirt3 =>
                Create<VcQuadTreeTriangle1>(quadTree, nodes, typeInfo),
            VcQuadTreeType.DirtShowdown => Create<VcQuadTreeTriangle2>(quadTree, nodes, typeInfo),
            _ => throw new NotSupportedException($"Type {typeInfo} is not supported.")
        };
    }

    private unsafe static VcQuadTreeFile Create<T>(Vector3 boundsMin, Vector3 boundsMax, int numMaterials,
        int numVertices, int numTriangles, int numNodes, int nodeTriangleListSize, VcQuadTreeTypeInfo typeInfo)
        where T : unmanaged, IVcQuadTreeTriangle
    {
        numMaterials = typeInfo.ForceMaxMaterials ? typeInfo.MaxMaterials : numMaterials;
        var headerSize = sizeof(VcQuadTreeHeader) + numMaterials * 4;
        var verticesSize = numVertices * sizeof(VcQuadTreeVertex);
        var nodesSize = numNodes * sizeof(VcQuadTreeNode);
        var trisSize = numTriangles * sizeof(T);
        var numBytes = headerSize + verticesSize + nodesSize + trisSize + nodeTriangleListSize;

        var bytes = new byte[numBytes];
        ref var header = ref Unsafe.As<byte, VcQuadTreeHeader>(ref bytes[0]);
        header.BoundMin = boundsMin;
        header.BoundMax = boundsMax;
        header.NumTriangles = -numTriangles;
        header.NumVertices = -numVertices;
        header.NumMaterials = typeInfo.NegativeMaterials ? -numMaterials : numMaterials;
        header.VerticesOffset = Convert.ToUInt32(headerSize);
        header.NodesOffset = Convert.ToUInt32(headerSize + verticesSize);
        header.TrianglesOffset = Convert.ToUInt32(headerSize + verticesSize + nodesSize);
        header.TriangleReferencesOffset = Convert.ToUInt32(headerSize + verticesSize + nodesSize + trisSize);
        
        return new VcQuadTreeFile(bytes, typeInfo);
    }

    private static VcQuadTreeFile Create<T>(VcQuadTree quadTree, VcQuadTree[] nodes, VcQuadTreeTypeInfo typeInfo)
        where T : unmanaged, IVcQuadTreeTriangle
    {
        var triangleRemap = BuildTriangleRemap(quadTree);

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
            foreach (var triIndex in node.Elements.Select(x => triangleRemap[x]).Order())
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

        var qtc = Create<T>(quadTree.BoundsMin, quadTree.BoundsMax, quadTree.Data.Materials.Count,
            quadTree.Data.Vertices.Count, quadTree.Data.Triangles.Count, nodes.Length, nodeTriangleListData.Count,
            typeInfo);

        var materials = qtc.GetMaterials();
        for (var i = 0; i < quadTree.Data.Materials.Count; ++i)
        {
            var mat = quadTree.Data.Materials[i];
            materials[i] = Convert.ToByte(mat[0]) |
                           (Convert.ToByte(mat[1]) << 8) |
                           (Convert.ToByte(mat[2]) << 16) |
                           (Convert.ToByte(mat[3]) << 24);
        }

        var matToFill = quadTree.Data.Materials.Count <= 0 ? DefaultMaterial : materials[quadTree.Data.Materials.Count - 1];
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
            var oi = triangleRemap[i];
            var triangle = quadTree.Data.Triangles[i];
            triangles[oi].MaterialIndex = triangle.MaterialIndex;
            triangles[oi].Vertex0 = triangle.A;
            triangles[oi].Vertex1 = triangle.B;
            triangles[oi].Vertex2 = triangle.C;
            triangles[oi].Sheet = quadTree.Data.SheetInfo[i];
        }

        CollectionsMarshal.AsSpan(nodeTriangleListData).CopyTo(qtc.GetNodeTriangleList(qtc.Header));
        return qtc;
    }

    public override QuadTreeDataTriangle[] GetTriangles()
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

    public VcQuadTreeFile ConvertType(VcQuadTreeTypeInfo targetTypeInfo)
    {
        if (targetTypeInfo.Type == TypeInfo.Type)
        {
            return new VcQuadTreeFile(_bytes.ToArray(), TypeInfo);
        }

        return (TypeInfo.Type, targetTypeInfo.Type) switch
        {
            (VcQuadTreeType.RaceDriverGrid, VcQuadTreeType.Dirt3)
                or (VcQuadTreeType.Dirt3, VcQuadTreeType.RaceDriverGrid) =>
                ConvertType<VcQuadTreeTriangle1, VcQuadTreeTriangle1>(targetTypeInfo),
            (VcQuadTreeType.RaceDriverGrid, VcQuadTreeType.DirtShowdown)
                or (VcQuadTreeType.Dirt3, VcQuadTreeType.DirtShowdown) =>
                ConvertType<VcQuadTreeTriangle1, VcQuadTreeTriangle2>(targetTypeInfo),
            (VcQuadTreeType.DirtShowdown, VcQuadTreeType.Dirt3)
                or (VcQuadTreeType.DirtShowdown, VcQuadTreeType.RaceDriverGrid) =>
                ConvertType<VcQuadTreeTriangle2, VcQuadTreeTriangle1>(targetTypeInfo),
            _ => throw new NotSupportedException(
                $"Converting type {TypeInfo} to type {targetTypeInfo} is not supported.")
        };
    }

    private VcQuadTreeFile ConvertType<TSource, TTarget>(VcQuadTreeTypeInfo targetTypeInfo)
        where TSource : unmanaged, IVcQuadTreeTriangle
        where TTarget : unmanaged, IVcQuadTreeTriangle
    {
        // Target has 4 bits for material id.
        var sourceMaterials = GetMaterials();
        var targetMaterials = new OrderedSet<int>();

        var source = GetTriangles<TSource>(Header);
        var targetBuffer = new byte[Unsafe.SizeOf<TTarget>() * source.Length];
        var target = MemoryMarshal.Cast<byte, TTarget>(targetBuffer);
        for (var i = 0; i < source.Length; ++i)
        {
            var sourceTri = source[i];
            ref var targetTri = ref target[i];

            var sourceMaterialId = sourceTri.MaterialIndex;
            var sourceSheet = sourceTri.Sheet;
            var material = sourceMaterials[sourceMaterialId];
            if (TypeInfo.SupportsSheetMaterials != targetTypeInfo.SupportsSheetMaterials)
            {
                const int starValue = 0x2A000000;
                const int plusValue = 0x2B000000;
                if (TypeInfo.SupportsSheetMaterials)
                {
                    sourceSheet <<= 1;
                    // Add + instead of * to material
                    if ((material & 0xFF000000) == starValue)
                    {
                        material = (material & 0xFFFFFF) | plusValue;
                        sourceSheet |= 0b1;
                    }
                }
                else
                {
                    // Add * instead of + to material
                    if ((sourceSheet & 0x01) != 0)
                    {
                        material = (material & 0xFFFFFF) | starValue;
                    }

                    sourceSheet >>= 1;
                }
            }

            var targetMaterialId = targetMaterials.IndexOf(material);
            if (targetMaterialId == -1)
            {
                targetMaterialId = targetMaterials.Count;
                targetMaterials.Add(material);
            }

            if (targetMaterials.Count > targetTypeInfo.MaxMaterials)
            {
                throw new InvalidOperationException("Direct conversion not possible. Convert through glTF instead.");
            }

            targetTri.Sheet = sourceSheet;
            targetTri.MaterialIndex = targetMaterialId;
            targetTri.Vertex0 = sourceTri.Vertex0;
            targetTri.Vertex1 = sourceTri.Vertex1;
            targetTri.Vertex2 = sourceTri.Vertex2;
        }

        var numMaterials = targetTypeInfo.ForceMaxMaterials ? targetTypeInfo.MaxMaterials : targetMaterials.Count;
        var nodeTriangleListSize = _bytes.Length - Convert.ToInt32(Header.TriangleReferencesOffset);
        var targetFile = Create<TTarget>(BoundsMin, BoundsMax, numMaterials, NumVertices, NumTriangles,
            NumNodes, nodeTriangleListSize, targetTypeInfo);
        
        // Copy data
        GetVertices(Header).CopyTo(targetFile.GetVertices(targetFile.Header));
        target.CopyTo(targetFile.GetTriangles<TTarget>(targetFile.Header));
        GetNodes(Header).CopyTo(targetFile.GetNodes(targetFile.Header));
        GetNodeTriangleList(Header).CopyTo(targetFile.GetNodeTriangleList(targetFile.Header));

        // Copy materials
        var materials = targetFile.GetMaterials();
        for (var i = 0; i < targetMaterials.Count; ++i)
        {
            materials[i] = targetMaterials[i];
        }

        // Fill remaining
        var matToFill = targetMaterials.Count <= 0 ? DefaultMaterial : targetMaterials[^1];
        for (var i = targetMaterials.Count; i < materials.Length; ++i)
        {
            materials[i] = matToFill;
        }

        return targetFile;
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

    public QuadTreeTriangle[] GetNodeTriangles(int nodeIndex)
    {
        return TypeInfo.Type switch
        {
            VcQuadTreeType.RaceDriverGrid or VcQuadTreeType.Dirt3 => GetNodeTriangles<VcQuadTreeTriangle1>(nodeIndex),
            VcQuadTreeType.DirtShowdown => GetNodeTriangles<VcQuadTreeTriangle2>(nodeIndex),
            _ => throw new NotSupportedException($"Type {TypeInfo} is not supported.")
        };
    }

    public override int GetNodeTriangles(int nodeIndex, Span<int> indices)
    {
        var node = GetNodes(Header)[nodeIndex];
        if (!node.HasTriangles)
        {
            return 0;
        }

        var refsData = GetNodeTriangleList(Header)[node.TriangleListOffset..];
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

    private unsafe Span<VcQuadTreeVertex> GetVertices(VcQuadTreeHeader header)
    {
        return MemoryMarshal.Cast<byte, VcQuadTreeVertex>(_bytes.AsSpan(Convert.ToInt32(header.VerticesOffset),
            sizeof(VcQuadTreeVertex) * NumVertices));
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

    private interface IVcQuadTreeTriangle : IQuadTreeTriangle
    {
        int Sheet { get; set; }

        new int MaterialIndex { get; set; }

        new int Vertex0 { get; set; }

        new int Vertex1 { get; set; }

        new int Vertex2 { get; set; }
    }

    private struct VcQuadTreeTriangle1 : IVcQuadTreeTriangle
    {
        private const int MaxVert0 = (1 << 10) - 1;
        private const int MaxVertOffset = byte.MaxValue;
        private const int MaxMaterialIndex = (1 << 4) - 1;
        public const int MaxVertices = MaxVert0 + MaxVertOffset + 1;

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
                ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(Vertex1));
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
                ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(Vertex2));
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
                ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(Vertex1));
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
                ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(Vertex2));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, MaxVertOffset, nameof(Vertex2));
                Vertex2Offset = (byte)offset;
            }
        }
    }
}
