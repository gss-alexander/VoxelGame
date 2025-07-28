﻿using System.Numerics;
using Client.Blocks;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client.Chunks;

public class Chunk
{
    public Vector2 ChunkWorldCenter => new Vector2(Position.X, Position.Y) * Size;
    public Vector2D<int> Position => _data.Position;
    
    public const int Size = 16;
    public const int Height = 256;

    private readonly ChunkData _data;
    private readonly BlockDatabase _blockDatabase;
    private readonly ChunkRenderer _chunkRenderer;

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

    public Chunk(GL gl, ChunkData data, BlockTextures blockTextures, BlockDatabase blockDatabase)
    {
        _data = data;
        _blockDatabase = blockDatabase;
        _chunkRenderer = new ChunkRenderer(gl, blockTextures, blockDatabase);
        _chunkRenderer.RegenerateMeshes(_data);
    }

    public void RenderOpaque()
    {
        _chunkRenderer.RenderOpaque();
    }

    public void RenderTransparent()
    {
        _chunkRenderer.RenderTransparent();
    }

    public bool HasTransparentBlocks()
    {
        return _chunkRenderer.HasTransparentBlocks;
    }

    public void GenerateFlatWorld()
    {
        const int height = 6;
        for (var x = 0; x < Size; x++)
        {
            for (var z = 0; z < Size; z++)
            {
                for (var y = 0; y < height - 2; y++)
                {
                    SetBlock(x, y, z, _blockDatabase.GetInternalId("cobblestone"), false);
                }
                
                SetBlock(x, height - 2, z, _blockDatabase.GetInternalId("dirt"), false);
                SetBlock(x, height - 1, z, _blockDatabase.GetInternalId("grass"), false);
            }
        }
    }

    public void SetBlock(int x, int y, int z, int block, bool regenerateMesh = true)
    {
        _data.SetBlock(new Vector3D<int>(x, y, z), block);
        if (regenerateMesh)
        {
            _chunkRenderer.RegenerateMeshes(_data);
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