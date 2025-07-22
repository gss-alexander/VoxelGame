using System.Numerics;
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

    public void UpdateChunkVisibility(Vector3 playerWorldPosition, float renderDistance)
    {
        // Hide all chunks that are outside of the render distance of the player
        _chunksToHide.Clear();
        foreach (var chunk in _visibleChunks)
        {
            var chunkPositionInWorld = chunk.Position * Chunk.Size;

            playerWorldPosition.Y = 0f;
            var distanceFromPlayer = Vector3.Distance(playerWorldPosition,
                new Vector3(chunkPositionInWorld.X, 0f, chunkPositionInWorld.Y));

            if (distanceFromPlayer > renderDistance)
            {
                _chunksToHide.Add(chunk);
            }
        }
        foreach (var chunk in _chunksToHide)
        {
            _visibleChunks.Remove(chunk);
        }
        
        // Find chunks that are within the render distance of the player and is not currently visible
        var playerChunkX = (int)MathF.Floor(playerWorldPosition.X / Chunk.Size);
        var playerChunkZ = (int)MathF.Floor(playerWorldPosition.Z / Chunk.Size);
        var chunkRadius = (int)MathF.Ceiling(renderDistance / Chunk.Size);
        var chunksInRange = new List<Vector2D<int>>();
        for (var x = playerChunkX - chunkRadius; x <= playerChunkX + chunkRadius; x++)
        {
            for (var z = playerChunkZ - chunkRadius; z <= playerChunkZ + chunkRadius; z++)
            {
                var chunkWorldCenter = new Vector2(x * Chunk.Size + Chunk.Size / 2f, z * Chunk.Size + Chunk.Size / 2f);
                var playerPosition2D = new Vector2(playerWorldPosition.X, playerWorldPosition.Z);
                if (Vector2.Distance(chunkWorldCenter, playerPosition2D) <= renderDistance)
                {
                    chunksInRange.Add(new Vector2D<int>(x, z));
                }
            }
        }
        foreach (var chunkPosition in chunksInRange)
        {
            if (_visibleChunks.All(chunk => chunk.Position != chunkPosition))
            {
                var newChunk = CreateChunk(chunkPosition.X, chunkPosition.Y);
                _visibleChunks.Add(newChunk);
            }
        }
    }
    
    public void RenderChunks()
    {
        foreach (var chunk in _visibleChunks)
        {
            chunk.Render();
        }
    }

    private Chunk CreateChunk(int worldX, int worldY)
    {
        var chunk = new Chunk(worldX, worldY);
        chunk.Initialize(_gl);
        return chunk;
    }
}