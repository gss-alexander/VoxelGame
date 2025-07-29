using Client.Blocks;
using Client.Chunks.Generation;
using Silk.NET.Maths;

namespace Client.Chunks;

// Responsible for generating chunk data with terrain generation
public class ChunkGenerator
{
    private readonly FastNoiseLite _noise;
    private readonly BlockDatabase _blockDatabase;

    public ChunkGenerator(FastNoiseLite noise, BlockDatabase blockDatabase)
    {
        _noise = noise;
        _blockDatabase = blockDatabase;
    }

    public ChunkData GenerateFlatWorld(Vector2D<int> chunkPosition)
    {
        var chunkData = new ChunkData(chunkPosition, _blockDatabase.GetInternalId("air"));
        for (var x = 0; x < Chunk.Size; x++)
        {
            for (var z = 0; z < Chunk.Size; z++)
            {
                chunkData.SetBlock(new Vector3D<int>(x, 0, z), _blockDatabase.GetInternalId("grass"));
            }
        }

        return chunkData;
    }

    public ChunkData Generate(Vector2D<int> chunkPosition)
    {
        var chunkData = new ChunkData(chunkPosition, _blockDatabase.GetInternalId("air"));

        var heightMap = GenerateHeightMap(chunkPosition);
        
        BuildBaseTerrain(ref chunkData, heightMap);

        return chunkData;
    }

    private void BuildBaseTerrain(ref ChunkData chunkData, Heightmap heightmap)
    {
        for (var x = 0; x < Chunk.Size; x++)
        {
            for (var z = 0; z < Chunk.Size; z++)
            {
                var height = heightmap.Get(x, z);
                
                // set the bottom blocks to be cobblestone
                for (var y = 0; y < height - 4; y++)
                {
                    chunkData.SetBlock(new Vector3D<int>(x, y, z), _blockDatabase.GetInternalId("cobblestone"));
                }
                
                // set the next 4 blocks to be dirt
                for (var y = height - 4; y < height; y++)
                {
                    chunkData.SetBlock(new Vector3D<int>(x, y, z), _blockDatabase.GetInternalId("dirt"));
                }

                chunkData.SetBlock(new Vector3D<int>(x, height, z), _blockDatabase.GetInternalId("grass"));
            }
        }
    }

    private Heightmap GenerateHeightMap(Vector2D<int> chunkPosition)
    {
        const int seaLevel = 64; // Minimum height
        const int maxMountainHeight = 150;
        
        var map = new Heightmap(Chunk.Size, Chunk.Size);
        for (var x = 0; x < Chunk.Size; x++)
        {
            for (var z = 0; z < Chunk.Size; z++)
            {
                var worldX = x + (chunkPosition.X * Chunk.Size);
                var worldZ = z + (chunkPosition.Y * Chunk.Size);

                var heightNoise = GenerateHeightNoise(worldX, worldZ);
                var scaledNoise = ApplyHeightCurve(heightNoise);
                var height = (int)Math.Clamp(seaLevel + (scaledNoise * maxMountainHeight), 1, Chunk.Height - 1);
                
                map.Set(x, z, height);
            }
        }

        return map;
    }

    private float GenerateHeightNoise(int worldX, int worldZ)
    {
        var noiseValue = 0f;
        var amplitude = 1f;
        var frequency = 0.008f; // Base frequency for large landforms
        var maxValue = 0f;

        // Generate 6 octaves for detailed terrain
        for (int octave = 0; octave < 6; octave++)
        {
            _noise.SetFrequency(frequency);
            noiseValue += _noise.GetNoise(worldX, worldZ) * amplitude;
            maxValue += amplitude;

            amplitude *= 0.5f; // Each octave contributes half as much
            frequency *= 2.1f; // Slightly irregular frequency multiplication for more organic feel
        }

        // Normalize to [-1, 1] range
        return noiseValue / maxValue;
    }

    private float ApplyHeightCurve(float noiseValue)
    {
        // Clamp noise to prevent extreme values
        noiseValue = Math.Clamp(noiseValue, -1f, 1f);
    
        if (noiseValue >= 0)
        {
            // Positive values: exponential curve for dramatic mountains
            // Using power of 1.8 creates good balance between flat areas and mountains
            return MathF.Pow(noiseValue, 1.5f);
        }
        else
        {
            // Negative values: gentler curve for valleys and low areas
            return -MathF.Pow(-noiseValue, 1.2f) * 0.3f; // Scale down valleys
        }
    }
}