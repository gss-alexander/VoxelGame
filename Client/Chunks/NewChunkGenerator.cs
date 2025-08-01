using Client.Blocks;
using Client.Chunks.Structures;
using Silk.NET.Maths;

namespace Client.Chunks;
public class NewChunkGenerator
{
    private readonly FastNoiseLite _noise;
    private readonly BlockDatabase _blockDatabase;

    public NewChunkGenerator(FastNoiseLite noise, BlockDatabase blockDatabase)
    {
        _noise = noise;
        _blockDatabase = blockDatabase;
    }

    public ChunkData Generate(Vector2D<int> chunkPosition, int seed)
    {
        var fillBlockId = _blockDatabase.GetInternalId("air");
        var chunkData = new ChunkData(chunkPosition, fillBlockId);
        
        GenerateTerrain(ref chunkData, seed);

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
                
                if (ShouldPlaceTree(worldPosition, seed))
                {
                    structures.Add(new WorldStructure(
                        StructureType.Tree,
                        localPosition,
                        false
                    ));
                }
            }
        }

        foreach (var structure in structures)
        {
            if (StructureFitsInChunk(structure, chunkPosition))
            {
                PlaceStructure(structure, ref chunkData, seed);
            }
        }
        
        CheckNeighbourStructures(chunkPosition, seed, ref chunkData);

        return chunkData;
    }

    private void CheckNeighbourStructures(Vector2D<int> chunkPosition, int seed, ref ChunkData chunkData)
    {
        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;

                var neighbourX = chunkPosition.X + dx;
                var neighbourZ = chunkPosition.Y + dz;

                var neighbourStructures = GetVirtualStructures(new Vector2D<int>(neighbourX, neighbourZ), seed);

                foreach (var structure in neighbourStructures)
                {
                    if (StructureExtendsIntoChunk(structure, chunkPosition))
                    {
                        PlaceStructurePart(structure, chunkPosition, ref chunkData);
                    }
                }
            }
        }
    }

    private void GenerateTerrain(ref ChunkData chunkData, int seed)
    {
        _noise.SetSeed(seed);
        
        var stoneId = _blockDatabase.GetInternalId("cobblestone");
        var dirtId = _blockDatabase.GetInternalId("dirt");
        var grassId = _blockDatabase.GetInternalId("grass");
        
        for (var x = 0; x < Chunk.Size; x++)
        {
            for (var z = 0; z < Chunk.Size; z++)
            {
                var worldX = chunkData.Position.X * Chunk.Size + x;
                var worldZ = chunkData.Position.Y * Chunk.Size + z;
                var height = GetNoiseHeight(new Vector2D<int>(worldX, worldZ), seed);
                
                for (var y = 0; y < Chunk.Height; y++)
                {
                    var pos = new Vector3D<int>(x, y, z);
                    if (y < height - 3)
                        chunkData.SetBlock(pos, stoneId);
                    else if (y < height - 1)
                        chunkData.SetBlock(pos, dirtId);
                    else if (y == height - 1)
                        chunkData.SetBlock(pos, grassId);
                }
            }
        }
    }

    private bool CanPlaceTree(Vector3D<int> worldBlockPosition, int seed)
    {
        // Check 3x3 area around tree base for suitable terrain
        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dz = -1; dz <= 1; dz++)
            {
                var checkPos = new Vector2D<int>(
                    worldBlockPosition.X + dx,
                    worldBlockPosition.Z + dz
                );
                
                var height = GetNoiseHeight(checkPos, seed);
                
                // Tree needs minimum height and relatively flat ground
                if (height < 64 || Math.Abs(height - worldBlockPosition.Y) > 2)
                    return false;
            }
        }
        return true;
    }

    private bool ShouldPlaceTree(Vector2D<int> horizontalWorldBlockPosition, int seed)
    {
        // Use position-based random with seed for deterministic placement
        var random = new System.Random(HashPosition(horizontalWorldBlockPosition, seed));
        
        // Only try to place trees on grass at surface level
        var surfaceHeight = GetNoiseHeight(horizontalWorldBlockPosition, seed);
        var treePosition = new Vector3D<int>(
            horizontalWorldBlockPosition.X, 
            surfaceHeight, 
            horizontalWorldBlockPosition.Y
        );
        
        // 5% chance of tree placement
        if (random.NextDouble() > 0.05) return false;
        
        return CanPlaceTree(treePosition, seed);
    }

    private int GetNoiseHeight(Vector2D<int> horizontalWorldBlockPosition, int seed)
    {
        _noise.SetSeed(seed);
        var noiseValue = _noise.GetNoise(horizontalWorldBlockPosition.X, horizontalWorldBlockPosition.Y);
        
        // Convert noise (-1 to 1) to height (60 to 120)
        return (int)(60 + (noiseValue + 1) * 30);
    }

    private bool StructureFitsInChunk(WorldStructure structure, Vector2D<int> chunkPosition)
    {
        if (structure.Type == StructureType.Tree)
        {
            // Tree is 3x3 base, 5 blocks tall - check if it extends beyond chunk boundaries
            var worldPos = new Vector2D<int>(
                chunkPosition.X * Chunk.Size + structure.HorizontalLocalPosition.X,
                chunkPosition.Y * Chunk.Size + structure.HorizontalLocalPosition.Y
            );
            
            // Check if tree extends beyond current chunk
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dz = -1; dz <= 1; dz++)
                {
                    var checkX = worldPos.X + dx;
                    var checkZ = worldPos.Y + dz;
                    
                    var checkChunkX = checkX / Chunk.Size;
                    var checkChunkZ = checkZ / Chunk.Size;
                    
                    if (checkChunkX != chunkPosition.X || checkChunkZ != chunkPosition.Y)
                        return false;
                }
            }
        }
        
        return true;
    }

    private WorldStructure[] GetVirtualStructures(Vector2D<int> chunkPosition, int seed)
    {
        var structures = new List<WorldStructure>();
        
        // Virtually generate structures for this chunk without creating actual chunk
        for (var x = 0; x < Chunk.Size; x++)
        {
            for (var z = 0; z < Chunk.Size; z++)
            {
                var worldPosition = new Vector2D<int>(
                    chunkPosition.X * Chunk.Size + x,
                    chunkPosition.Y * Chunk.Size + z
                );
                
                if (ShouldPlaceTree(worldPosition, seed))
                {
                    structures.Add(new WorldStructure(
                        StructureType.Tree,
                        new Vector2D<int>(x, z), // Local position in that chunk
                        false
                    ));
                }
            }
        }
        
        return structures.ToArray();
    }

    private bool StructureExtendsIntoChunk(WorldStructure structure, Vector2D<int> targetChunkPosition)
    {
        if (structure.Type == StructureType.Tree)
        {
            // Convert structure's local position to world position
            // Note: structure.LocalPosition is relative to its origin chunk
            var originChunkX = targetChunkPosition.X; // This needs to be calculated based on which neighbor we're checking
            var originChunkZ = targetChunkPosition.Y;
            
            // For proper implementation, you'd need to pass the origin chunk position
            // This is a simplified version - in practice you'd track which neighbor chunk this structure came from
            
            var worldStructurePos = new Vector2D<int>(
                originChunkX * Chunk.Size + structure.HorizontalLocalPosition.X,
                originChunkZ * Chunk.Size + structure.HorizontalLocalPosition.Y
            );
            
            // Check if any part of the 3x3 tree base falls into target chunk
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dz = -1; dz <= 1; dz++)
                {
                    var checkX = worldStructurePos.X + dx;
                    var checkZ = worldStructurePos.Y + dz;
                    
                    var checkChunkX = checkX / Chunk.Size;
                    var checkChunkZ = checkZ / Chunk.Size;
                    
                    if (checkChunkX == targetChunkPosition.X && checkChunkZ == targetChunkPosition.Y)
                        return true;
                }
            }
        }
        
        return false;
    }

    private void PlaceStructurePart(WorldStructure structure, Vector2D<int> chunkPosition, ref ChunkData chunkData)
    {
        if (structure.Type == StructureType.Tree)
        {
            var logId = _blockDatabase.GetInternalId("log");
            var leavesId = _blockDatabase.GetInternalId("leaves");
            
            // This is simplified - you'd need to calculate which part of the tree structure
            // falls into this specific chunk and place only those blocks
            
            // Get the world position of the tree base
            var treeWorldPos = new Vector2D<int>(
                chunkPosition.X * Chunk.Size + structure.HorizontalLocalPosition.X,
                chunkPosition.Y * Chunk.Size + structure.HorizontalLocalPosition.Y
            );
            
            var treeHeight = GetNoiseHeight(treeWorldPos, 0) + 5; // Tree height above ground
            
            // Place only the parts that fall into this chunk
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dz = -1; dz <= 1; dz++)
                {
                    var worldX = treeWorldPos.X + dx;
                    var worldZ = treeWorldPos.Y + dz;
                    
                    // Convert to local chunk coordinates
                    var localX = worldX - (chunkPosition.X * Chunk.Size);
                    var localZ = worldZ - (chunkPosition.Y * Chunk.Size);
                    
                    // Only place if within current chunk bounds
                    if (localX >= 0 && localX < Chunk.Size && localZ >= 0 && localZ < Chunk.Size)
                    {
                        // Place tree trunk and leaves
                        for (var y = treeHeight - 5; y < treeHeight; y++)
                        {
                            if (y >= 0 && y < Chunk.Height)
                            {
                                var pos = new Vector3D<int>(localX, y, localZ);
                                if (dx == 0 && dz == 0) // Center - trunk
                                    chunkData.SetBlock(pos, logId);
                                else if (y >= treeHeight - 2) // Top layers - leaves
                                    chunkData.SetBlock(pos, leavesId);
                            }
                        }
                    }
                }
            }
        }
    }

    private void PlaceStructure(WorldStructure structure, ref ChunkData chunkData, int seed)
    {
        if (structure.Type == StructureType.Tree)
        {
            var logId = _blockDatabase.GetInternalId("log");
            var leavesId = _blockDatabase.GetInternalId("leaves");
            
            var x = structure.HorizontalLocalPosition.X;
            var z = structure.HorizontalLocalPosition.Y;
            
            var worldPos = new Vector2D<int>(
                chunkData.Position.X * Chunk.Size + x,
                chunkData.Position.Y * Chunk.Size + z
            );
            
            var surfaceHeight = GetNoiseHeight(worldPos, seed);
            
            // Place trunk (5 blocks tall)
            for (var y = surfaceHeight; y < surfaceHeight + 5 && y < Chunk.Height; y++)
            {
                chunkData.SetBlock(new Vector3D<int>(x, y, z), logId);
            }
            
            // Place leaves in 3x3 pattern on top 2 layers
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dz = -1; dz <= 1; dz++)
                {
                    var leafX = x + dx;
                    var leafZ = z + dz;
                    
                    if (leafX >= 0 && leafX < Chunk.Size && leafZ >= 0 && leafZ < Chunk.Size)
                    {
                        for (var y = surfaceHeight + 3; y < surfaceHeight + 5 && y < Chunk.Height; y++)
                        {
                            chunkData.SetBlock(new Vector3D<int>(leafX, y, leafZ), leavesId);
                        }
                    }
                }
            }
        }
    }
    
    private int HashPosition(Vector2D<int> position, int seed)
    {
        // Simple hash function for deterministic random based on position and seed
        return (position.X * 73856093) ^ (position.Y * 19349663) ^ (seed * 83492791);
    }
}