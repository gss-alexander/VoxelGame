using System.Numerics;
using Silk.NET.Maths;

namespace Client.Blocks;

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

    public static Vector3 GetCenterPosition(Vector3D<int> blockPosition)
    {
        return new Vector3(blockPosition.X, blockPosition.Y, blockPosition.Z);
    }

    public static Vector3D<int> GetFaceNeighbour(Vector3D<int> originBlockPosition,
        BlockGeometry.FaceDirection faceDirection)
    {
        var offset = faceDirection switch
        {
            BlockGeometry.FaceDirection.Top => new Vector3D<int>(0, 1, 0),
            BlockGeometry.FaceDirection.Bottom => new Vector3D<int>(0, -1, 0),
            BlockGeometry.FaceDirection.Right => new Vector3D<int>(1, 0, 0),
            BlockGeometry.FaceDirection.Left => new Vector3D<int>(-1, 0, 0),
            BlockGeometry.FaceDirection.Front => new Vector3D<int>(0, 0, 1),
            BlockGeometry.FaceDirection.Back => new Vector3D<int>(0, 0, -1),
            _ => throw new NotImplementedException()
        };

        return originBlockPosition + offset;
    }
}