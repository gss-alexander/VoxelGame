using System.Numerics;
using Client.Blocks;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client.Chunks;

public class ChunkSystem
{
    public int VisibleChunkCount => _visibleChunks.Count;
    
    private readonly GL _gl;
    
    private readonly List<Chunk> _visibleChunks = new();
    
    // Will be updated by chunk visibility checks
    private readonly List<Chunk> _chunksToHide = new();

    private FastNoiseLite _noise;
    private readonly ChunkGenerator _chunkGenerator;

    private readonly Dictionary<Vector2D<int>, List<ValueTuple<Vector3D<int>, BlockType>>> _modifiedBlocks = new();
    
    public ChunkSystem(GL gl)
    {
        _gl = gl;
        _noise = new FastNoiseLite(DateTime.Now.Millisecond);
        _noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        _chunkGenerator = new ChunkGenerator(_noise);
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
        int localX = ((blockPosition.X % Chunk.Size) + Chunk.Size) % Chunk.Size;
        int localY = ((blockPosition.Y % Chunk.Height) + Chunk.Height) % Chunk.Height;
        int localZ = ((blockPosition.Z % Chunk.Size) + Chunk.Size) % Chunk.Size;
    
        return new Vector3D<int>(localX, localY, localZ);
    }

    public BlockType GetBlock(Vector3D<int> blockPosition)
    {
        var chunkPosition = Chunk.BlockToChunkPosition(blockPosition);
        var chunk = _visibleChunks.FirstOrDefault(c => c.Position == chunkPosition);
        if (chunk == null)
        {
            return BlockType.Air;
        }
        
        var localPosition = BlockToLocalPosition(blockPosition);
        return chunk.GetBlock(localPosition.X, localPosition.Y, localPosition.Z);
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

        if (!_modifiedBlocks.ContainsKey(chunkPosition))
        {
            _modifiedBlocks.Add(chunkPosition, new List<(Vector3D<int>, BlockType)>());
        }

        var modifiedBlocksList = _modifiedBlocks[chunkPosition];
        var existing = false;
        for (var i = 0; i < modifiedBlocksList.Count; i++)
        {
            var modifiedBlock = modifiedBlocksList[i];
            if (modifiedBlock.Item1 == blockPosition)
            {
                modifiedBlock.Item2 = blockType;
                modifiedBlocksList[i] = modifiedBlock;
                existing = true;
            }
        }

        if (!existing)
        {
            modifiedBlocksList.Add(new ValueTuple<Vector3D<int>, BlockType>(blockPosition, blockType));
        }
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
            chunk.RenderOpaque();
        }
    }

    public void RenderTransparency(Vector3 playerPosition)
    {
        var chunksWithTransparency = new List<Chunk>();
        foreach (var chunk in _visibleChunks)
        {
            if (chunk.HasTransparentBlocks())
            {
                chunksWithTransparency.Add(chunk);
            }
        }

        chunksWithTransparency = chunksWithTransparency.OrderBy(chunk =>
            Vector2.Distance(chunk.ChunkWorldCenter, new Vector2(playerPosition.X, playerPosition.Z))).ToList();
        foreach (var chunk in chunksWithTransparency)
        {
            chunk.RenderTransparent();
        }
    }

    private static int CalculateChunkPositionDistance(Vector2D<int> a, Vector2D<int> b)
    {
        // Chebyshev distance calculation
        return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    }

    private Chunk CreateChunk(int worldX, int worldY)
    {
        var chunkData = _chunkGenerator.Generate(new Vector2D<int>(worldX, worldY));
        var chunkPosition = new Vector2D<int>(worldX, worldY);
        if (_modifiedBlocks.TryGetValue(chunkPosition, out var modifiedBlockList))
        {
            foreach (var modifiedBlock in modifiedBlockList)
            {
                var pos = BlockToLocalPosition(modifiedBlock.Item1);
                chunkData.SetBlock(pos, modifiedBlock.Item2);
            }
        }
        var chunk = new Chunk(_gl, chunkData);
        return chunk;
    }
}