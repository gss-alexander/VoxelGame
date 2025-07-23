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

    public static Vector3D<int> GetFaceNeighbour(Vector3D<int> originBlockPosition,
        BlockData.FaceDirection faceDirection)
    {
        var offset = faceDirection switch
        {
            BlockData.FaceDirection.Top => new Vector3D<int>(0, 1, 0),
            BlockData.FaceDirection.Bottom => new Vector3D<int>(0, -1, 0),
            BlockData.FaceDirection.Right => new Vector3D<int>(1, 0, 0),
            BlockData.FaceDirection.Left => new Vector3D<int>(-1, 0, 0),
            BlockData.FaceDirection.Front => new Vector3D<int>(0, 0, 1),
            BlockData.FaceDirection.Back => new Vector3D<int>(0, 0, -1),
            _ => throw new NotImplementedException()
        };

        return originBlockPosition + offset;
    }
}