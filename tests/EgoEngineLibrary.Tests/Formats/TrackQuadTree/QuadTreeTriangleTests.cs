using EgoEngineLibrary.Formats.TrackQuadTree;

namespace EgoEngineLibrary.Tests.Formats.TrackQuadTree;

public class QuadTreeTriangleTests
{

    [Theory]
    [InlineData(0, 1, 2, 0, 1, 2)]
    [InlineData(0, 2, 1, 0, 2, 1)]
    [InlineData(1, 0, 2, 0, 2, 1)]
    [InlineData(1, 2, 0, 0, 1, 2)]
    [InlineData(2, 0, 1, 0, 1, 2)]
    [InlineData(2, 1, 0, 0, 2, 1)]
    public void PatchUp_Test(int a, int b, int c, int a2, int b2, int c2)
    {
        // Arrange
        var t = new QuadTreeTriangle(a, b, c, 5);

        // Act
        t.EnsureFirstIndexLowest();

        // Assert
        Assert.Equal(a2, t.A);
        Assert.Equal(b2, t.B);
        Assert.Equal(c2, t.C);
        Assert.Equal(5, t.MaterialIndex);
    }
}
