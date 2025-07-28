using Client.Blocks;
using Silk.NET.Maths;

namespace Client.Chunks;

// Stores all data for a chunk
public class ChunkData
{
    public Vector2D<int> Position { get; }
    private readonly int[] _blocks;

    public ChunkData(Vector2D<int> position, int fillBlockId)
    {
        Position = position;
        _blocks = new int[Chunk.Size * Chunk.Height * Chunk.Size];
        for (var x = 0; x < Chunk.Size; x++)
        {
            for (var y = 0; y < Chunk.Height; y++)
            {
                for (var z = 0; z < Chunk.Size; z++)
                {
                    SetBlock(new Vector3D<int>(x, y, z), fillBlockId);
                }
            }
        }
    }

    public void SetBlock(Vector3D<int> position, int blockType)
    {
        var localPosition = PositionToBlockIndex(position.X, position.Y, position.Z);
        _blocks[localPosition] = blockType;
    }

    public int GetBlock(Vector3D<int> position)
    {
        var localPosition = PositionToBlockIndex(position.X, position.Y, position.Z);
        return _blocks[localPosition];
    }
    
    private static int PositionToBlockIndex(int x, int y, int z)
    {
        return (x + (y * Chunk.Size)) + (z * Chunk.Size * Chunk.Height);
    }
}