using System.Numerics;
using Silk.NET.Maths;

namespace Client;

public static class Vector2DExtensions
{
    public static Vector2 AsFloatVector(this Vector2D<int> vec)
    {
        return new Vector2(vec.X, vec.Y);
    }
}