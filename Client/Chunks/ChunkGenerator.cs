using Client.Blocks;
using Client.Chunks.Structures;
using Silk.NET.Maths;

namespace Client.Chunks;

public class ChunkGenerator
{
    private const int SeaLevel = 60;
    
    private readonly FastNoiseLite _noise;
    private readonly BlockDatabase _blockDatabase;
    private readonly int _seed;

    public ChunkGenerator(FastNoiseLite noise, BlockDatabase blockDatabase, int seed)
    {
        _noise = noise;
        _blockDatabase = blockDatabase;
        _seed = seed;
    }

    public ChunkData Generate(Vector2D<int> chunkPosition)
    {
        var fillBlockId = _blockDatabase.GetInternalId("air");
        var chunkData = new ChunkData(chunkPosition, fillBlockId);
        
        GenerateTerrain(chunkData);

        var potentialStructures = GetPotentialWorldStructures(chunkPosition);
        foreach (var structure in potentialStructures)
        {
            PlaceStructure(chunkData, structure);
        }

        return chunkData;
    }

    public bool IsVirtualBlockSolid(Vector3D<int> blockWorldPosition)
    {
        var height = GetHeightAtPosition(new Vector2D<int>(blockWorldPosition.X, blockWorldPosition.Z));

        if (blockWorldPosition.Y < height - 3)
            return true;
        else if (blockWorldPosition.Y < height - 1)
            return true;
        else if (blockWorldPosition.Y == height - 1)
            return true;

        return false;
    }

    public int GetVirtualBlock(Vector3D<int> blockWorldPosition)
    {
        var height = GetHeightAtPosition(new Vector2D<int>(blockWorldPosition.X, blockWorldPosition.Z));

        if (blockWorldPosition.Y < height - 3)
            return _blockDatabase.GetInternalId("cobblestone");
        else if (blockWorldPosition.Y < height - 1)
            return _blockDatabase.GetInternalId("dirt");
        else if (blockWorldPosition.Y == height - 1)
            return _blockDatabase.GetInternalId("grass");

        return _blockDatabase.GetInternalId("air");
    }

    private void GenerateTerrain(ChunkData chunkData)
    {
        _noise.SetSeed(_seed);
        
        var stoneId = _blockDatabase.GetInternalId("cobblestone");
        var dirtId = _blockDatabase.GetInternalId("dirt");
        var grassId = _blockDatabase.GetInternalId("grass");
        var bedrockId = _blockDatabase.GetInternalId("bedrock");
        
        for (var x = 0; x < Chunk.Size; x++)
        {
            for (var z = 0; z < Chunk.Size; z++)
            {
                var worldX = chunkData.Position.X * Chunk.Size + x;
                var worldZ = chunkData.Position.Y * Chunk.Size + z;
                var height = GetHeightAtPosition(new Vector2D<int>(worldX, worldZ));
                
                for (var y = 0; y < Chunk.Height; y++)
                {
                    var pos = new Vector3D<int>(x, y, z);
                    if (y == 0)
                        chunkData.SetBlock(pos, bedrockId);
                    else if (y < height - 3)
                        chunkData.SetBlock(pos, stoneId);
                    else if (y < height - 1)
                        chunkData.SetBlock(pos, dirtId);
                    else if (y == height - 1)
                        chunkData.SetBlock(pos, grassId);
                }
            }
        }
    }

    private void PlaceStructure(ChunkData chunkData, WorldStructure structure)
    {
        var logId = _blockDatabase.GetInternalId("log");
        var leavesId = _blockDatabase.GetInternalId("leaves");
        
        if (structure.Type == StructureType.Tree)
        {
            var x = structure.HorizontalLocalPosition.X;
            var z = structure.HorizontalLocalPosition.Y;
            
            // ensure that it has some spacing from borders of chunk
            if (x >= Chunk.Size - 1 || x == 0 || z >= Chunk.Size - 1 || z == 0) return;
            
            var worldX = chunkData.Position.X * Chunk.Size + x;
            var worldZ = chunkData.Position.Y * Chunk.Size + z;
            
            var surfaceHeight = GetHeightAtPosition(new Vector2D<int>(worldX, worldZ));
            
            for (var y = surfaceHeight; y < surfaceHeight + 5 && y < Chunk.Height; y++)
            {
                chunkData.SetBlock(new Vector3D<int>(x, y, z), logId);
            }
            
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dz = -1; dz <= 1; dz++)
                {
                    var leafX = x + dx;
                    var leafZ = z + dz;
                    for (var y = surfaceHeight + 3; y < surfaceHeight + 5 && y < Chunk.Height; y++)
                    {
                        chunkData.SetBlock(new Vector3D<int>(leafX, y, leafZ), leavesId);
                    }
                }
            }
        }
    }

    private List<WorldStructure> GetPotentialWorldStructures(Vector2D<int> chunkPosition)
    {
        var structures = new List<WorldStructure>();
        for (var x = 0; x < Chunk.Size; x++)
        {
            for (var z = 0; z < Chunk.Size; z++)
            {
                var localPosition = new Vector2D<int>(x, z);
                var worldPosition = new Vector2D<int>(
                    chunkPosition.X * Chunk.Size + x,
                    chunkPosition.Y * Chunk.Size + z
                );
                
                if (ShouldPlaceTree(worldPosition))
                {
                    structures.Add(new WorldStructure(
                        StructureType.Tree,
                        localPosition,
                        false
                    ));
                }
            }
        }

        return structures;
    }
    
    private bool ShouldPlaceTree(Vector2D<int> horizontalWorldBlockPosition)
    {
        _noise.SetSeed(_seed);
        
        var random = new System.Random(HashPosition(horizontalWorldBlockPosition));
        
        return !(random.NextDouble() > 0.01);
    }
    
    private int GetHeightAtPosition(Vector2D<int> horizontalWorldBlockPosition)
    {
        _noise.SetSeed(_seed);
        var noiseValue = _noise.GetNoise(horizontalWorldBlockPosition.X, horizontalWorldBlockPosition.Y);
        
        // Convert noise (-1 to 1) to height (60 to 120)
        return (int)(SeaLevel + (noiseValue + 1) * 30);
    }
    
    private int HashPosition(Vector2D<int> position)
    {
        // Simple hash function for deterministic random based on position and seed
        return (position.X * 73856093) ^ (position.Y * 19349663) ^ (_seed * 83492791);
    }
}