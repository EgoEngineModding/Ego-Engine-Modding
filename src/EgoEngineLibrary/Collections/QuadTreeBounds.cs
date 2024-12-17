using System.Numerics;

namespace EgoEngineLibrary.Collections;

public readonly record struct QuadTreeBounds(Vector2 Min, Vector2 Max)
{
    public Vector2 Size => Max - Min;
    
    public Vector2 HalfSize => Size * 0.5f;
}
