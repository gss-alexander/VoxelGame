using System.Diagnostics;
using Client.Blocks;
using Client.Chunks.Generation;
using Client.Chunks.Structures;
using Client.Diagnostics;
using Silk.NET.Maths;

namespace Client.Chunks;

public class ChunkGenerator
{
    private const int SeaLevel = 63;

    private readonly FastNoiseLite _noise;
    private readonly BlockDatabase _blockDatabase;
    private readonly int _seed;

    private readonly NoiseGenerator _noiseGenerator;
    private readonly BiomeGenerator _biomeGenerator;
    private readonly StructureGenerator _structureGenerator;

    private readonly Stopwatch _generationStopwatch = new();

    public ChunkGenerator(FastNoiseLite noise, BlockDatabase blockDatabase, int seed)
    {
        _noise = noise;
        _blockDatabase = blockDatabase;
        _seed = seed;

        // Initialize new generation systems
        _noiseGenerator = new NoiseGenerator(_noise, _seed);
        _biomeGenerator = new BiomeGenerator(_noiseGenerator);
        _structureGenerator = new StructureGenerator(_blockDatabase, _noiseGenerator, _seed);
    }

    public ChunkData Generate(Vector2D<int> chunkPosition)
    {
        _generationStopwatch.Restart();
        var fillBlockId = _blockDatabase.GetInternalId("air");
        var chunkData = new ChunkData(chunkPosition, fillBlockId);
        
        GenerateTerrain(chunkData);

        var potentialStructures = GetPotentialWorldStructures(chunkPosition);
        foreach (var structure in potentialStructures)
        {
            PlaceStructure(chunkData, structure);
        }

        _generationStopwatch.Stop();
        ChunkGenerationTimeTracking.TerrainGenerationTime.AddTime((float)_generationStopwatch.Elapsed.TotalSeconds);
        return chunkData;
    }

    public bool IsVirtualBlockSolid(Vector3D<int> blockWorldPosition)
    {
        var horizontalPos = new Vector2D<int>(blockWorldPosition.X, blockWorldPosition.Z);
        var biome = _biomeGenerator.GetBiomeAt(horizontalPos);
        var height = _biomeGenerator.GetHeightAt(horizontalPos, biome);

        return blockWorldPosition.Y <= height;
    }

    public int GetVirtualBlock(Vector3D<int> blockWorldPosition)
    {
        var horizontalPos = new Vector2D<int>(blockWorldPosition.X, blockWorldPosition.Z);
        var biome = _biomeGenerator.GetBiomeAt(horizontalPos);
        var biomeData = BiomeData.GetBiomeData(biome);
        var height = _biomeGenerator.GetHeightAt(horizontalPos, biome);

        return GetBlockAtPosition(blockWorldPosition, height, biomeData);
    }

    private void GenerateTerrain(ChunkData chunkData)
    {
        var bedrockId = _blockDatabase.GetInternalId("bedrock");

        for (var x = 0; x < Chunk.Size; x++)
        {
            for (var z = 0; z < Chunk.Size; z++)
            {
                var worldX = chunkData.Position.X * Chunk.Size + x;
                var worldZ = chunkData.Position.Y * Chunk.Size + z;
                var horizontalPos = new Vector2D<int>(worldX, worldZ);

                // Determine biome and height for this column
                var biome = _biomeGenerator.GetBiomeAt(horizontalPos);
                var biomeData = BiomeData.GetBiomeData(biome);
                var height = _biomeGenerator.GetHeightAt(horizontalPos, biome);

                for (var y = 0; y < Chunk.Height; y++)
                {
                    var pos = new Vector3D<int>(x, y, z);
                    var worldPos = new Vector3D<int>(worldX, y, worldZ);

                    if (y == 0)
                    {
                        chunkData.SetBlock(pos, bedrockId);
                    }
                    else
                    {
                        var blockId = GetBlockAtPosition(worldPos, height, biomeData);
                        chunkData.SetBlock(pos, blockId);
                    }
                }
            }
        }
    }

    private int GetBlockAtPosition(Vector3D<int> blockWorldPosition, int surfaceHeight, BiomeData biomeData)
    {
        int y = blockWorldPosition.Y;

        // Air above surface
        if (y > surfaceHeight)
        {
            // Water for areas below sea level
            if (y <= SeaLevel && surfaceHeight < SeaLevel)
            {
                // Could add water block here if available
                return _blockDatabase.GetInternalId("air");
            }
            return _blockDatabase.GetInternalId("air");
        }

        // Surface layer
        if (y == surfaceHeight)
        {
            return _blockDatabase.GetInternalId(biomeData.SurfaceBlock);
        }

        // Sub-surface layers
        if (y >= surfaceHeight - biomeData.SubSurfaceDepth)
        {
            return _blockDatabase.GetInternalId(biomeData.SubSurfaceBlock);
        }

        // Deep stone
        return _blockDatabase.GetInternalId(biomeData.DeepBlock);
    }

    private void PlaceStructure(ChunkData chunkData, WorldStructure structure)
    {
        if (structure.Type == StructureType.Tree)
        {
            var x = structure.HorizontalLocalPosition.X;
            var z = structure.HorizontalLocalPosition.Y;

            var worldX = chunkData.Position.X * Chunk.Size + x;
            var worldZ = chunkData.Position.Y * Chunk.Size + z;
            var horizontalPos = new Vector2D<int>(worldX, worldZ);

            var biome = _biomeGenerator.GetBiomeAt(horizontalPos);
            var surfaceHeight = _biomeGenerator.GetHeightAt(horizontalPos, biome);

            // Get tree height from structure data (stored when created)
            int treeHeight = structure.Metadata;

            _structureGenerator.PlaceTree(chunkData, structure.HorizontalLocalPosition, surfaceHeight, treeHeight, biome);
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

                // Get biome for this position
                var biome = _biomeGenerator.GetBiomeAt(worldPosition);
                var biomeData = BiomeData.GetBiomeData(biome);
                var surfaceHeight = _biomeGenerator.GetHeightAt(worldPosition, biome);

                // Only place trees on solid ground above sea level
                if (surfaceHeight >= SeaLevel - 1)
                {
                    if (_structureGenerator.ShouldPlaceTree(worldPosition, biomeData, out int treeHeight))
                    {
                        structures.Add(new WorldStructure(
                            StructureType.Tree,
                            localPosition,
                            false,
                            treeHeight // Store tree height in metadata
                        ));
                    }
                }
            }
        }

        return structures;
    }
    
}