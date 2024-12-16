﻿using System;
using System.Numerics;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public record struct QuadTreeTriangle(int A, int B, int C, int MaterialIndex)
{
    public int this[int index]
    {
        get
        {
            return index switch
            {
                0 => A,
                1 => B,
                2 => C,
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }
        set
        {
            switch (index)
            {
                case 0:
                    A = value;
                    break;
                case 1:
                    B = value;
                    break;
                case 2:
                    C = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }

    public void EnsureFirstIndexLowest()
    {
        while (A > B || A > C)
        {
            // 'A' index must always be less than others (shift to make C A B)
            (A, B) = (B, A);
            (A, C) = (C, A);
        }
    }
}

public readonly record struct QuadTreeDataTriangle(Vector3 Position0, Vector3 Position1, Vector3 Position2, string Material)
{
    public (Vector3 BoundsMin, Vector3 BoundsMax) GetBounds()
    {
        var boundsMin = Vector3.Min(Vector3.Min(Position0, Position1), Position2);
        var boundsMax = Vector3.Max(Vector3.Max(Position0, Position1), Position2);
        return (boundsMin, boundsMax);
    }
}
