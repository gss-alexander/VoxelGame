using System.Numerics;
using Silk.NET.Maths;

namespace Client;

public static class Block
{
    public static Vector3D<int> WorldToBlockPosition(Vector3 worldPosition)
    {
        return new Vector3D<int>(
            (int)MathF.Floor(worldPosition.X + 0.5f),
            (int)MathF.Floor(worldPosition.Y + 0.5f),
            (int)MathF.Floor(worldPosition.Z + 0.5f)
        );
    }
}