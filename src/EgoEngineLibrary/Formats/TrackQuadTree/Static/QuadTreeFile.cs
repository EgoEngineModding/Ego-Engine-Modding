using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EgoEngineLibrary.Formats.TrackQuadTree.Static;

public abstract class QuadTreeFile
{
    private const float VertXScale = 1 << 24;
    private const float VertYScale = 1 << 16;
    protected static readonly Vector3 ScaleFactor = new(1.0f / VertXScale, 1.0f / VertYScale, 1.0f / VertXScale);
    protected static readonly Vector3 EncodeScaleFactor = new(VertXScale, VertYScale, VertXScale);
    protected static readonly Vector3 Half = new(0.5f);

    /// <summary>
    /// Identifier useful for debugging purposes.
    /// </summary>
    public string? Identifier { get; set; }

    public abstract QuadTreeDataTriangle[] GetTriangles();

    public abstract int GetNodeTriangles(int nodeIndex, Span<int> indices);

    protected static int[] BuildTriangleRemap<T>(T[] nodes, int triangleCount) where T : CellQuadTree<T>
    {
        var currentTriangle = 0;
        int[] remap = new int[triangleCount];
        Array.Fill(remap, -1);
        foreach (var node in nodes)
        {
            foreach (var triIndex in node.Elements)
            {
                ref int remapIndex = ref remap[triIndex];
                if (remapIndex != -1)
                {
                    continue;
                }

                remapIndex = currentTriangle++;
            }
        }

        Debug.Assert(currentTriangle == triangleCount);
        Debug.Assert(Array.IndexOf(remap, -1) == -1);
        return remap;
    }
}

public abstract class QuadTreeFile<TTypeInfo, THeader, TNode> : QuadTreeFile
    where TTypeInfo : IQuadTreeTypeInfo
    where THeader : unmanaged, IStaticQuadTreeHeader
    where TNode : unmanaged, IStaticQuadTreeNode
{
    protected byte[] _bytes;
    protected readonly Vector3 _vertexScale;
    protected readonly Vector3 _vertexOffset;

    public byte[] Bytes => _bytes;

    public TTypeInfo TypeInfo { get; protected set; }

    protected ref THeader Header
    {
        get
        {
            return ref Unsafe.As<byte, THeader>(ref _bytes[0]);
        }
    }

    public int NumTriangles => TypeInfo.NegativeTriangles ? -Header.NumTriangles : Header.NumTriangles;

    public int NumVertices => TypeInfo.NegativeVertices ? -Header.NumVertices : Header.NumVertices;

    public int NumMaterials => TypeInfo.NegativeMaterials ? -Header.NumMaterials : Header.NumMaterials;

    public unsafe int NumNodes
    {
        get
        {
            return Convert.ToInt32((Header.TrianglesOffset - Header.NodesOffset) / sizeof(TNode));
        }
    }

    public Vector3 BoundsMin => Header.BoundMin;

    public Vector3 BoundsMax => Header.BoundMax;

    protected QuadTreeFile(byte[] bytes, TTypeInfo typeInfo)
    {
        _bytes = bytes;
        TypeInfo = typeInfo;

        ref var header = ref Header;
        _vertexScale = (header.BoundMax - header.BoundMin) * ScaleFactor;
        _vertexOffset = header.BoundMin;
    }

    public unsafe void GetMaterials(ICollection<string> materials)
    {
        var mats = GetMaterials();
        while (mats.Length > 0)
        {
            var material = string.Create(4, mats[0], static (material, mat) =>
            {
                Span<byte> state = stackalloc byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(state, mat);
                material[0] = (char)state[0];
                material[1] = (char)state[1];
                material[2] = (char)state[2];
                material[3] = (char)state[3];
            });

            mats = mats[1..];
            materials.Add(material);
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

    protected QuadTreeTriangle[] GetNodeTriangles<T>(int nodeIndex) where T : unmanaged, IQuadTreeTriangle
    {
        var count = GetNodeTriangles(nodeIndex, []);
        if (count <= 0)
        {
            return [];
        }
        
        var indices = new int[count];
        GetNodeTriangles(nodeIndex, indices);

        var tris = GetTriangles<T>(Header);
        var triangles = new QuadTreeTriangle[count];
        for (var i = 0; i < count; ++i)
        {
            Debug.Assert(indices[i] < tris.Length);
            var triangle = tris[indices[i]];
            var material = triangle.MaterialIndex;
            var position0 = triangle.Vertex0;
            var position1 = triangle.Vertex1;
            var position2 = triangle.Vertex2;
            triangles[i] = new QuadTreeTriangle(position0, position1, position2, material);
        }
        
        return triangles;
    }

    public int GetDepth()
    {
        var maxLevel = 0;
        var nodes = GetNodes(Header);
        var levelData = new Stack<(int Level, int ChildrenRemaining)>();
        levelData.Push((0, 1));
        for (var i = 1; i < nodes.Length; ++i)
        {
            (int Level, int ChildrenRemaining) data;
            if (((i - 1) & 0b11) == 0)
            {
                while (levelData.TryPop(out data))
                {
                    if (data.ChildrenRemaining == 0)
                    {
                        continue;
                    }
                    
                    data.ChildrenRemaining -= 1;
                    if (data.ChildrenRemaining > 0)
                    {
                        levelData.Push(data);
                    }

                    break;
                }

                data = (data.Level + 1, 0);
                maxLevel = Math.Max(maxLevel, data.Level);
            }
            else
            {
                data = levelData.Pop();
            }
            
            var node = nodes[i];
            if (!node.IsLeaf)
            {
                data.ChildrenRemaining += 1;
            }

            levelData.Push(data);
        }

        return maxLevel;
    }

    protected unsafe Span<int> GetMaterials()
    {
        var materials = MemoryMarshal.Cast<byte, int>(_bytes.AsSpan(sizeof(THeader), NumMaterials * 4));
        if (!BitConverter.IsLittleEndian)
        {
            BinaryPrimitives.ReverseEndianness(materials, materials);
        }

        return materials;
    }

    protected Span<TNode> GetNodes(THeader header)
    {
        var nodesLength = Convert.ToInt32(header.TrianglesOffset - header.NodesOffset);
        return MemoryMarshal.Cast<byte, TNode>(_bytes.AsSpan(Convert.ToInt32(header.NodesOffset),
            nodesLength));
    }

    protected unsafe Span<T> GetTriangles<T>(THeader header) where T : unmanaged, IQuadTreeTriangle
    {
        return MemoryMarshal.Cast<byte, T>(_bytes.AsSpan(Convert.ToInt32(header.TrianglesOffset),
            sizeof(T) * NumTriangles));
    }

    protected interface IQuadTreeTriangle
    {
        int MaterialIndex { get; }
    
        int Vertex0 { get; }

        int Vertex1 { get; }

        int Vertex2 { get; }
    }
}
