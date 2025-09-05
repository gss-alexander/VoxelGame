using System.Numerics;
using Client.Blocks;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client.Chunks;

public class Chunk
{
    public ChunkData Data => _data;
    public ChunkRenderer? Renderer { get; private set; }

    public Vector2 ChunkWorldCenter => new Vector2(Position.X, Position.Y) * Size;
    public Vector2D<int> Position => _data.Position;
    
    public const int Size = 16;
    public const int Height = 256;

    private readonly ChunkData _data;
    private readonly BlockTextures _blockTextures;
    private readonly BlockDatabase _blockDatabase;

    public static Vector2D<int> WorldToChunkPosition(Vector3 worldPosition)
    {
        var x = (int)MathF.Floor(worldPosition.X / Size);
        var y = (int)MathF.Floor(worldPosition.Z / Size);
        return new Vector2D<int>(x, y);
    }

    public static Vector2D<int> BlockToChunkPosition(Vector3D<int> blockPosition)
    {
        return new Vector2D<int>(
            blockPosition.X >= 0 ? blockPosition.X / Size : (blockPosition.X + 1) / Size - 1,
            blockPosition.Z >= 0 ? blockPosition.Z / Size : (blockPosition.Z + 1) / Size - 1
        );
    }

    public Chunk(ChunkData data, BlockTextures blockTextures, BlockDatabase blockDatabase)
    {
        _data = data;
        _blockTextures = blockTextures;
        _blockDatabase = blockDatabase;
    }

    public void SetRenderer(ChunkRenderer renderer, Mesh opaqueMesh, Mesh transparentMesh)
    {
        Renderer = renderer;
        Renderer.SetMeshes(opaqueMesh, transparentMesh);
    }

    public void RenderOpaque()
    {
        Renderer.RenderOpaque();
    }

    public void RenderTransparent()
    {
        Renderer.RenderTransparent();
    }

    public bool HasTransparentBlocks()
    {
        return Renderer.HasTransparentBlocks;
    }

    public void SetBlock(int x, int y, int z, int block, bool regenerateMesh = true)
    {
        _data.SetBlock(new Vector3D<int>(x, y, z), block);
        if (regenerateMesh)
        {
            var newMeshes = ChunkMeshBuilder.Create(_data, _blockDatabase, _blockTextures);
            Renderer.SetMeshes(newMeshes.Opaque, newMeshes.Transparent);
        }
    }

    public int GetBlock(int x, int y, int z)
    {
        return _data.GetBlock(new Vector3D<int>(x, y, z));
    }

    public bool IsBlockSolid(int x, int y, int z)
    {
        if (x >= Size || x < 0 || y >= Height || y < 0 || z >= Size || z < 0)
        {
            return false;
        }
        
        var block = _blockDatabase.GetById(GetBlock(x, y, z));
        return block.IsSolid;
    }
}