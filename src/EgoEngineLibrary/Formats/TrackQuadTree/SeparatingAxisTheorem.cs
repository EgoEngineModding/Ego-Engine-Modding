using System;
using System.Numerics;

using EgoEngineLibrary.Collections;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public static class SeparatingAxisTheorem
{
    public static bool Intersect(QuadTreeBounds rectangle, QuadTreeTriangleData triangle)
    {
        // Normals aren't normalized since it's not necessary for merely getting a bool value
        var rectVertices = new[]
        {
            rectangle.Min,
            new Vector2(rectangle.Max.X, rectangle.Min.Y),
            rectangle.Max,
            new Vector2(rectangle.Min.X, rectangle.Max.Y),
        };
        var edgeNormals = new[]
        {
            new Vector2(0, rectangle.Max.X - rectangle.Min.X),
            new Vector2(rectangle.Max.Y - rectangle.Min.Y, 0),
        };
        // Swap order to account for winding due to use of XZ plane
        var triVertices = new[]
        {
            new Vector2(triangle.Position0.X, triangle.Position0.Z),
            new Vector2(triangle.Position2.X, triangle.Position2.Z),
            new Vector2(triangle.Position1.X, triangle.Position1.Z),
        };

        var result = Intersect(rectVertices, triVertices, edgeNormals);
        if (result is false)
        {
            return false;
        }

        var edge1 = triVertices[1] - triVertices[0];
        var edge2 = triVertices[2] - triVertices[1];
        var edge3 = triVertices[0] - triVertices[2];
        edgeNormals =
        [
            new Vector2(-edge1.Y, edge1.X),
            new Vector2(-edge2.Y, edge2.X),
            new Vector2(-edge3.Y, edge3.X)
        ];
        return Intersect(rectVertices, triVertices, edgeNormals);
    }
    
    public static bool Intersect(QuadTreeBounds rectangle, (Vector3 Min, Vector3 Max) bounds)
    {
        // Normals aren't normalized since it's not necessary for merely getting a bool value
        var rectVertices = new[]
        {
            rectangle.Min,
            new Vector2(rectangle.Max.X, rectangle.Min.Y),
            rectangle.Max,
            new Vector2(rectangle.Min.X, rectangle.Max.Y),
        };
        var edgeNormals = new[]
        {
            new Vector2(0, rectangle.Max.X - rectangle.Min.X),
            new Vector2(rectangle.Max.Y - rectangle.Min.Y, 0),
        };
        // Swap order to account for winding due to use of XZ plane
        var rectVertices2 = new[]
        {
            new Vector2(bounds.Min.X, bounds.Min.Z),
            new Vector2(bounds.Max.X, bounds.Min.Z),
            new Vector2(bounds.Max.X, bounds.Max.Z),
            new Vector2(bounds.Min.X, bounds.Max.Z),
        };

        var result = Intersect(rectVertices, rectVertices2, edgeNormals);
        if (result is false)
        {
            return false;
        }

        edgeNormals =
        [
            new Vector2(0, bounds.Max.X - bounds.Min.X),
            new Vector2(bounds.Max.Z - bounds.Min.Z, 0)
        ];
        return Intersect(rectVertices, rectVertices2, edgeNormals);
    }

    private static bool Intersect(
        ReadOnlySpan<Vector2> obj1Vertices,
        ReadOnlySpan<Vector2> obj2Vertices,
        ReadOnlySpan<Vector2> normals)
    {
        for (var i = 0; i < normals.Length; ++i)
        {
            var normal = normals[i];
            var p1 = GetProjectionMinMax(obj1Vertices, normal);
            var p2 = GetProjectionMinMax(obj2Vertices, normal);
            var distance = p1.X < p2.X ? p2.X - p1.Y : p1.X - p2.Y;
            if (distance > 0)
            {
                return false;
            }
        }

        return true;
    }

    private static Vector2 GetProjectionMinMax(ReadOnlySpan<Vector2> vertices, Vector2 normal)
    {
        float min = Vector2.Dot(normal, vertices[0]);
        var max = min;
        for (var i = 1; i < vertices.Length; ++i)
        {
            var vertex = vertices[i];
            var dot = Vector2.Dot(vertex, normal);
            if (dot < min) min = dot;
            if (dot > max) max = dot;
        }

        return new Vector2(min, max);
    }
}
