using System.Collections.Concurrent;
using System.Numerics;
using Client.Blocks;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client.Chunks;

public class ChunkSystem
{
    public int VisibleChunkCount => _visibleChunks.Count;
    
    private readonly GL _gl;
    private readonly BlockTextures _blockTextures;
    private readonly BlockDatabase _blockDatabase;

    private readonly Dictionary<Vector2D<int>, Chunk> _visibleChunks = new();
    
    // Will be updated by chunk visibility checks
    private readonly List<Vector2D<int>> _chunksToHide = new();

    private FastNoiseLite _noise;
    private readonly FinalNewChunkGenerator _chunkGenerator;

    private readonly Dictionary<Vector2D<int>, List<ValueTuple<Vector3D<int>, int>>> _modifiedBlocks = new();
    private readonly ObjectPool<ChunkRenderer> _chunkRendererPool;

    private readonly ConcurrentQueue<Vector2D<int>> _chunkGenerationQueue = new();
    private readonly ConcurrentQueue<Chunk> _readyChunksAwaitingRenderingQueue = new();
    private readonly ConcurrentDictionary<Vector2D<int>, Tuple<Chunk, ChunkMeshBuilder.ChunkMeshGenerationResult>> _readyChunksAwaitingRendering = new();
    private readonly HashSet<Vector2D<int>> _chunksBeingGenerated = new();

    private CancellationTokenSource _cancellationTokenSource;
    
    public ChunkSystem(GL gl, BlockTextures blockTextures, BlockDatabase blockDatabase)
    {
        _gl = gl;
        _blockTextures = blockTextures;
        _blockDatabase = blockDatabase;
        _noise = new FastNoiseLite(DateTime.Now.Millisecond);
        _noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        _chunkRendererPool =
            new ObjectPool<ChunkRenderer>(() => new ChunkRenderer(_gl, _blockTextures, _blockDatabase), _ => {});
        _chunkRendererPool.Prewarm(128);
        _chunkGenerator = new FinalNewChunkGenerator(_noise, _blockDatabase, 12345);
    }

    public void StartChunkGenerationThread()
    {
        Console.WriteLine($"[Chunk System]: Starting chunk generation thread");
        _cancellationTokenSource = new CancellationTokenSource();
        Task.Run(() =>
        {
            ChunkGenerationLoop(_cancellationTokenSource.Token);
        });
    }

    public void StopChunkGenerationThread()
    {
        _cancellationTokenSource.Cancel();
        Console.WriteLine($"[Chunk System]: Stopped chunk generation thread");
    }

    private void ChunkGenerationLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_chunkGenerationQueue.TryDequeue(out var chunkPosition))
            {
                Console.WriteLine($"[Chunk System - Chunk generation thread]: Starting generation of chunk {chunkPosition}");
                var newChunk = CreateChunk(chunkPosition.X, chunkPosition.Y);
                var mesh = ChunkMeshBuilder.Create(newChunk.Data, _blockDatabase, _blockTextures);
                _readyChunksAwaitingRendering.TryAdd(chunkPosition, new Tuple<Chunk, ChunkMeshBuilder.ChunkMeshGenerationResult>(newChunk, mesh));
            }
        }
    }

    public bool IsBlockSolid(Vector3D<int> blockPosition)
    {
        var chunkPosition = Chunk.BlockToChunkPosition(blockPosition);
        if (!_visibleChunks.TryGetValue(chunkPosition, out var chunk))
        {
            return false;
        }

        var localPosition = BlockToLocalPosition(blockPosition);
        return chunk.IsBlockSolid(localPosition.X, localPosition.Y, localPosition.Z);
    }

    public bool IsVirtualBlockSolid(Vector3D<int> blockPosition)
    {
        var isVirtuallySolid = _chunkGenerator.IsVirtualBlockSolid(blockPosition);

        var isModifiedAndNotSolid = false;
        var chunkPosition = Chunk.BlockToChunkPosition(blockPosition);
        if (_modifiedBlocks.TryGetValue(chunkPosition, out var modifiedBlocksInChunk))
        {
            foreach (var (pos, id) in modifiedBlocksInChunk)
            {
                if (blockPosition == pos)
                {
                    isModifiedAndNotSolid = !_blockDatabase.GetById(id).IsSolid;
                    break;
                }
            }
        }

        return isVirtuallySolid && !isModifiedAndNotSolid;
    }
    
    public Vector3D<int> BlockToLocalPosition(Vector3D<int> blockPosition)
    {
        int localX = ((blockPosition.X % Chunk.Size) + Chunk.Size) % Chunk.Size;
        int localY = ((blockPosition.Y % Chunk.Height) + Chunk.Height) % Chunk.Height;
        int localZ = ((blockPosition.Z % Chunk.Size) + Chunk.Size) % Chunk.Size;
    
        return new Vector3D<int>(localX, localY, localZ);
    }

    public int GetBlock(Vector3D<int> blockPosition)
    {
        var chunkPosition = Chunk.BlockToChunkPosition(blockPosition);
        if (!_visibleChunks.TryGetValue(chunkPosition, out var chunk))
        {
            return _blockDatabase.GetInternalId("air");
        }
        
        var localPosition = BlockToLocalPosition(blockPosition);
        return chunk.GetBlock(localPosition.X, localPosition.Y, localPosition.Z);
    }

    public void DestroyBlock(Vector3D<int> blockPosition)
    {
        SetBlock(blockPosition, _blockDatabase.GetInternalId("air"));
    }

    public void PlaceBlock(Vector3D<int> blockPosition, int blockId)
    {
        SetBlock(blockPosition, blockId);
    }

    public void ApplyPersistedBlockChanges(Dictionary<Vector3D<int>, string> changes)
    {
        foreach (var (worldPosition, externalBlockId) in changes)
        {
            var chunkPosition = Chunk.BlockToChunkPosition(worldPosition);
            var blockId = _blockDatabase.GetInternalId(externalBlockId);
            RegisterModifiedBlock(chunkPosition, worldPosition, blockId);
        }
    }

    public Dictionary<Vector3D<int>, string> GetModifiedBlocks()
    {
        // Convert the internal modified block collection to one that is external friendly. Calculates the world position
        // as they are stored by their local chunk coordinates, and they use internal int IDs.
        
        var result = new Dictionary<Vector3D<int>, string>();
        foreach (var (_, modifiedBlocks) in _modifiedBlocks)
        {
            foreach (var (blockPosition, blockId) in modifiedBlocks)
            {
                var externalId = _blockDatabase.GetExternalId(blockId);
                result.Add(blockPosition, externalId);
            }
        }

        return result;
    }

    private void SetBlock(Vector3D<int> blockPosition, int blockId)
    {
        var chunkPosition = Chunk.BlockToChunkPosition(blockPosition);
        if (!_visibleChunks.TryGetValue(chunkPosition, out var chunk))
        {
            return;
        }

        var localPosition = BlockToLocalPosition(blockPosition);
        chunk.SetBlock(localPosition.X, localPosition.Y, localPosition.Z, blockId);
        RegisterModifiedBlock(chunkPosition, blockPosition, blockId);
    }

    private void RegisterModifiedBlock(Vector2D<int> chunkPosition, Vector3D<int> blockPosition, int blockId)
    {
        if (!_modifiedBlocks.ContainsKey(chunkPosition))
        {
            _modifiedBlocks.Add(chunkPosition, new List<(Vector3D<int>, int)>());
        }

        var modifiedBlocksList = _modifiedBlocks[chunkPosition];
        var existing = false;
        for (var i = 0; i < modifiedBlocksList.Count; i++)
        {
            var modifiedBlock = modifiedBlocksList[i];
            if (modifiedBlock.Item1 == blockPosition)
            {
                modifiedBlock.Item2 = blockId;
                modifiedBlocksList[i] = modifiedBlock;
                existing = true;
            }
        }

        if (!existing)
        {
            modifiedBlocksList.Add(new ValueTuple<Vector3D<int>, int>(blockPosition, blockId));
        }
    }

    public void ForceLoad(Vector3 playerWorldPosition, int renderDistance)
    {
        var playerChunkPosition = Chunk.WorldToChunkPosition(playerWorldPosition);
        
        // Hide all chunks that are outside of the render distance of the player
        _chunksToHide.Clear();
        foreach (var (chunkPos, chunk) in _visibleChunks)
        {
            var distance = CalculateChunkPositionDistance(playerChunkPosition, chunk.Position);
            if (CalculateChunkPositionDistance(playerChunkPosition, chunk.Position) > renderDistance)
            {
                _chunksToHide.Add(chunkPos);
            }
        }
        foreach (var chunkPos in _chunksToHide)
        {
            _chunkRendererPool.Release(_visibleChunks[chunkPos].Renderer);
            _visibleChunks.Remove(chunkPos);
        }
        
        // Find chunks that are within the render distance of the player and is not currently visible
        var chunksToGenerate = new List<Vector2D<int>>();
        for (var x = -renderDistance; x <= renderDistance; x++)
        {
            for (var y = -renderDistance; y <= renderDistance; y++)
            {
                var playerX = x + playerChunkPosition.X;
                var playerY = y + playerChunkPosition.Y;
                var currentChunkPos = new Vector2D<int>(playerX, playerY);
                if (!_visibleChunks.ContainsKey(currentChunkPos) && 
                    !_readyChunksAwaitingRendering.ContainsKey(currentChunkPos) &&
                    _chunksBeingGenerated.Add(currentChunkPos))
                {
                    chunksToGenerate.Add(currentChunkPos);
                }
            }
        }

        foreach (var chunk in chunksToGenerate.OrderBy(c => CalculateChunkPositionDistance(playerChunkPosition, c)))
        {
            _chunkGenerationQueue.Enqueue(chunk);
        }

        // Can I really call myself a programmer after this?
        while (_readyChunksAwaitingRendering.Count < renderDistance * 6)
        {
            continue;
        }

        var chunksToProcess = _readyChunksAwaitingRendering.Keys.ToList();
        foreach (var chunkPos in chunksToProcess)
        {
            if (_readyChunksAwaitingRendering.TryRemove(chunkPos, out var chunkToFinish))
            {
                _chunksBeingGenerated.Remove(chunkPos); // Remove from tracking
                if (!_visibleChunks.ContainsKey(chunkPos))
                {
                    var renderer = _chunkRendererPool.Get();
                    var (chunk, meshResult) = chunkToFinish;
                    chunk.SetRenderer(renderer, meshResult.Opaque, meshResult.Transparent);
                    _visibleChunks.Add(chunkPos, chunk);
                }
            }
        }
    }
    
    public void UpdateChunkVisibility(Vector3 playerWorldPosition, int renderDistance)
    {
        var playerChunkPosition = Chunk.WorldToChunkPosition(playerWorldPosition);
        
        // Hide all chunks that are outside of the render distance of the player
        _chunksToHide.Clear();
        foreach (var (chunkPos, chunk) in _visibleChunks)
        {
            var distance = CalculateChunkPositionDistance(playerChunkPosition, chunk.Position);
            if (CalculateChunkPositionDistance(playerChunkPosition, chunk.Position) > renderDistance)
            {
                _chunksToHide.Add(chunkPos);
            }
        }
        foreach (var chunkPos in _chunksToHide)
        {
            _chunkRendererPool.Release(_visibleChunks[chunkPos].Renderer);
            _visibleChunks.Remove(chunkPos);
        }
        
        // Find chunks that are within the render distance of the player and is not currently visible
        var chunksToGenerate = new List<Vector2D<int>>();
        for (var x = -renderDistance; x <= renderDistance; x++)
        {
            for (var y = -renderDistance; y <= renderDistance; y++)
            {
                var playerX = x + playerChunkPosition.X;
                var playerY = y + playerChunkPosition.Y;
                var currentChunkPos = new Vector2D<int>(playerX, playerY);
                if (!_visibleChunks.ContainsKey(currentChunkPos) && 
                    !_readyChunksAwaitingRendering.ContainsKey(currentChunkPos) &&
                    _chunksBeingGenerated.Add(currentChunkPos))
                {
                    chunksToGenerate.Add(currentChunkPos);
                }
            }
        }

        foreach (var chunk in chunksToGenerate.OrderBy(c => CalculateChunkPositionDistance(playerChunkPosition, c)))
        {
            _chunkGenerationQueue.Enqueue(chunk);
        }

        var chunksToProcess = _readyChunksAwaitingRendering.Keys.ToList();
        foreach (var chunkPos in chunksToProcess)
        {
            if (_readyChunksAwaitingRendering.TryRemove(chunkPos, out var chunkToFinish))
            {
                _chunksBeingGenerated.Remove(chunkPos); // Remove from tracking
                if (!_visibleChunks.ContainsKey(chunkPos))
                {
                    var renderer = _chunkRendererPool.Get();
                    var (chunk, meshResult) = chunkToFinish;
                    chunk.SetRenderer(renderer, meshResult.Opaque, meshResult.Transparent);
                    _visibleChunks.Add(chunkPos, chunk);
                }
            } 
        }
    }
    
    public void RenderChunks()
    {
        foreach (var (_, chunk) in _visibleChunks)
        {
            chunk.RenderOpaque();
        }
    }

    public void RenderTransparency(Vector3 playerPosition)
    {
        var chunksWithTransparency = new List<Chunk>();
        foreach (var (_, chunk) in _visibleChunks)
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
        var chunk = new Chunk(_gl, chunkData, _blockTextures, _blockDatabase);
        return chunk;
    }
}