using Silk.NET.Maths;

namespace Client.Chunks;

// Stores all data for a chunk
public struct ChunkData
{
    private readonly BlockType[] _blocks;

    public ChunkData()
    {
        _blocks = new BlockType[Chunk.Size * Chunk.Height * Chunk.Size];
    }

    public void SetBlock(Vector3D<int> position, BlockType blockType)
    {
        var localPosition = PositionToBlockIndex(position.X, position.Y, position.Z);
        _blocks[localPosition] = blockType;
    }

    public BlockType GetBlock(Vector3D<int> position)
    {
        var localPosition = PositionToBlockIndex(position.X, position.Y, position.Z);
        return _blocks[localPosition];
    }
    
    private static int PositionToBlockIndex(int x, int y, int z)
    {
        return (x + (y * Chunk.Size)) + (z * Chunk.Size * Chunk.Height);
    }
}