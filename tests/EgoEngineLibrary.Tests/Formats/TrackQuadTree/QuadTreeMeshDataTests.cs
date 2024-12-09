using System.Numerics;

using EgoEngineLibrary.Formats.TrackQuadTree;

namespace EgoEngineLibrary.Tests.Formats.TrackQuadTree;

public class QuadTreeMeshDataTests
{
    [Theory]
    [InlineData(2, 1, 3, new[] { 0, 1, 2, 3, 4, 5, 6 })]
    [InlineData(0, 3, 6, new[] { 0, 1, 2, 6, 3, 4, 5 })]
    [InlineData(0, 6, 3, new[] { 0, 1, 2, 6, 3, 4, 5 })]
    [InlineData(3, 0, 6, new[] { 0, 1, 2, 6, 3, 4, 5 })]
    [InlineData(3, 6, 0, new[] { 0, 1, 2, 6, 3, 4, 5 })]
    [InlineData(6, 0, 3, new[] { 0, 1, 2, 6, 3, 4, 5 })]
    [InlineData(6, 3, 0, new[] { 0, 1, 2, 6, 3, 4, 5 })]
    [InlineData(0, 5, 6, new[] { 0, 1, 5, 6, 2, 3, 4 })]
    [InlineData(0, 6, 5, new[] { 0, 1, 2, 5, 6, 3, 4 })]
    [InlineData(5, 0, 6, new[] { 0, 1, 5, 6, 2, 3, 4 })]
    [InlineData(5, 6, 0, new[] { 0, 1, 5, 6, 2, 3, 4 })]
    [InlineData(6, 0, 5, new[] { 0, 1, 2, 5, 6, 3, 4 })]
    [InlineData(6, 5, 0, new[] { 0, 1, 2, 5, 6, 3, 4 })]
    public void PatchUp_Test(int a, int b, int c, int[] expectedVertices)
    {
        // Arrange
        var typeInfo = new TypeInfo();
        var data = new QuadTreeMeshData(typeInfo);

        var numVertices = Math.Max(Math.Max(a, b), c) - 1;
        for (var i = 0; i < numVertices; ++i)
        {
            data.Add(new QuadTreeDataTriangle(new Vector3(i), new Vector3(i + 1), new Vector3(i + 2), string.Empty));
        }

        numVertices += 2;
        data.Add(new QuadTreeDataTriangle(data.Vertices[a], data.Vertices[b], data.Vertices[c], string.Empty));

        // Act
        data.PatchUp();

        // Assert
        Assert.Equal(numVertices, data.Vertices.Count);
        Assert.Equal(numVertices - 1, data.Triangles.Count);
        for (var i = 0; i < numVertices; ++i)
        {
            Assert.Equal(expectedVertices[i], data.Vertices[i].X);
            Assert.Equal(expectedVertices[i], data.Vertices[i].Y);
            Assert.Equal(expectedVertices[i], data.Vertices[i].Z);
        }

        for (var i = 0; i < data.Triangles.Count; ++i)
        {
            var tri = data.Triangles[i];
            Assert.True(tri.A != tri.B && tri.A != tri.C && tri.B != tri.C);
            Assert.True(tri.A < tri.B);
            Assert.True(tri.A < tri.C);
        }
    }

    private class TypeInfo : IQuadTreeTypeInfo
    {
        public int GetTriangleIndexOffset(int minIndex, int index)
        {
            Assert.True(minIndex <= index);
            var offset = index - minIndex;
            return offset switch
            {
                > 4 => offset / 2,
                _ => 0
            };
        }

        public bool ShouldSplit(QuadTreeMeshData data)
        {
            return true;
        }
    }
}
