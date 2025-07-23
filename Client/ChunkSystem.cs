﻿using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client;

public class ChunkSystem
{
    public int VisibleChunkCount => _visibleChunks.Count;
    
    private readonly GL _gl;
    
    private readonly List<Chunk> _visibleChunks = new();
    
    // Will be updated by chunk visibility checks
    private readonly List<Chunk> _chunksToHide = new();
    
    public ChunkSystem(GL gl)
    {
        _gl = gl;
    }

    public bool IsBlockSolid(Vector3D<int> blockPosition)
    {
        var chunkPosition = Chunk.BlockToChunkPosition(blockPosition);
        var chunk = _visibleChunks.FirstOrDefault(c => c.Position == chunkPosition);
        if (chunk == null)
        {
            return false;
        }

        var localPosition = BlockToLocalPosition(blockPosition);
        return chunk.IsBlockSolid(localPosition.X, localPosition.Y, localPosition.Z);
    }

    
    public Vector3D<int> BlockToLocalPosition(Vector3D<int> blockPosition)
    {
        int localX = ((blockPosition.X % 16) + 16) % 16;
        int localY = ((blockPosition.Y % 16) + 16) % 16;
        int localZ = ((blockPosition.Z % 16) + 16) % 16;
    
        return new Vector3D<int>(localX, localY, localZ);
    } 

    public void DestroyBlock(Vector3D<int> blockPosition)
    {
        SetBlock(blockPosition, BlockType.Air);
    }

    public void PlaceBlock(Vector3D<int> blockPosition, BlockType block)
    {
        SetBlock(blockPosition, block);
    }

    private void SetBlock(Vector3D<int> blockPosition, BlockType blockType)
    {
        var chunkPosition = Chunk.BlockToChunkPosition(blockPosition);
        var chunk = _visibleChunks.FirstOrDefault(c => c.Position == chunkPosition);
        if (chunk == null)
        {
            return;
        }

        var localPosition = BlockToLocalPosition(blockPosition);
        chunk.SetBlock(localPosition.X, localPosition.Y, localPosition.Z, blockType);
    }

    public void UpdateChunkVisibility(Vector3 playerWorldPosition, int renderDistance)
    {
        var playerChunkPosition = Chunk.WorldToChunkPosition(playerWorldPosition);
        
        // Hide all chunks that are outside of the render distance of the player
        _chunksToHide.Clear();
        foreach (var chunk in _visibleChunks)
        {
            var distance = CalculateChunkPositionDistance(playerChunkPosition, chunk.Position);
            if (CalculateChunkPositionDistance(playerChunkPosition, chunk.Position) > renderDistance)
            {
                Console.WriteLine(distance);
                _chunksToHide.Add(chunk);
            }
        }
        foreach (var chunk in _chunksToHide)
        {
            Console.WriteLine($"Removing chunk outside of render distance ({chunk.Position.X},{chunk.Position.Y})");
            _visibleChunks.Remove(chunk);
        }
        
        // Find chunks that are within the render distance of the player and is not currently visible
        var chunksInRange = new List<Vector2D<int>>();
        for (var x = -renderDistance; x <= renderDistance; x++)
        {
            for (var y = -renderDistance; y <= renderDistance; y++)
            {
                var playerX = x + playerChunkPosition.X;
                var playerY = y + playerChunkPosition.Y;
                if (_visibleChunks.All(chunk => chunk.Position.X != playerX || chunk.Position.Y != playerY))
                {
                    chunksInRange.Add(new Vector2D<int>(playerX, playerY));
                }
            }
        }
        foreach (var chunkPosition in chunksInRange)
        {
            Console.WriteLine($"Creating new chunk at ({chunkPosition.X},{chunkPosition.Y})");
            var newChunk = CreateChunk(chunkPosition.X, chunkPosition.Y);
            _visibleChunks.Add(newChunk);
        }
    }
    
    public void RenderChunks()
    {
        foreach (var chunk in _visibleChunks)
        {
            chunk.Render();
        }
    }

    private static int CalculateChunkPositionDistance(Vector2D<int> a, Vector2D<int> b)
    {
        // Chebyshev distance calculation
        return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    }

    private Chunk CreateChunk(int worldX, int worldY)
    {
        var chunk = new Chunk(worldX, worldY);
        chunk.Initialize(_gl);
        return chunk;
    }
}