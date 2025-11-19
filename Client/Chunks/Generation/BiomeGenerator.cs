using Silk.NET.Maths;

namespace Client.Chunks.Generation;

public class BiomeGenerator
{
    private readonly NoiseGenerator _noiseGenerator;
    private const int SeaLevel = 63;

    public BiomeGenerator(NoiseGenerator noiseGenerator)
    {
        _noiseGenerator = noiseGenerator;
    }

    /// <summary>
    /// Determines the biome at a given horizontal position
    /// </summary>
    public BiomeType GetBiomeAt(Vector2D<int> horizontalWorldBlockPosition)
    {
        // Use separate noise maps for temperature and moisture
        float temperature = _noiseGenerator.GetNoise(
            horizontalWorldBlockPosition.X * 0.0008f,
            horizontalWorldBlockPosition.Y * 0.0008f,
            seedOffset: 500
        );

        float moisture = _noiseGenerator.GetNoise(
            horizontalWorldBlockPosition.X * 0.0007f,
            horizontalWorldBlockPosition.Y * 0.0007f,
            seedOffset: 1000
        );

        // Use another noise layer to determine rivers
        float riverNoise = _noiseGenerator.GetNoise(
            horizontalWorldBlockPosition.X * 0.004f,
            horizontalWorldBlockPosition.Y * 0.004f,
            seedOffset: 1500
        );

        // Rivers form in narrow bands
        if (Math.Abs(riverNoise) < 0.05f)
        {
            return BiomeType.River;
        }

        // Determine biome based on temperature and moisture
        // Temperature: -1 (cold) to 1 (hot)
        // Moisture: -1 (dry) to 1 (wet)

        if (temperature > 0.4f && moisture < -0.3f)
        {
            return BiomeType.Desert;
        }
        else if (temperature < -0.2f || temperature > 0.6f)
        {
            return BiomeType.Mountains;
        }
        else if (moisture > 0.3f)
        {
            return BiomeType.Forest;
        }
        else if (temperature > 0.1f && temperature < 0.4f && moisture > -0.1f && moisture < 0.3f)
        {
            return BiomeType.Hills;
        }
        else
        {
            return BiomeType.Plains;
        }
    }

    /// <summary>
    /// Gets the terrain height at a position considering biome
    /// </summary>
    public int GetHeightAt(Vector2D<int> horizontalWorldBlockPosition, BiomeType biome)
    {
        var biomeData = BiomeData.GetBiomeData(biome);

        // Use different noise patterns based on biome
        float heightNoise;

        if (biome == BiomeType.Mountains)
        {
            // Mountains use ridged noise for dramatic peaks
            heightNoise = _noiseGenerator.GetRidgedNoise(
                horizontalWorldBlockPosition.X * 0.003f,
                horizontalWorldBlockPosition.Y * 0.003f,
                octaves: 4
            );
        }
        else if (biome == BiomeType.Hills)
        {
            // Hills use multi-octave for rolling terrain
            heightNoise = _noiseGenerator.GetMultiOctaveNoise(
                horizontalWorldBlockPosition.X * 0.006f,
                horizontalWorldBlockPosition.Y * 0.006f,
                octaves: 5,
                persistence: 0.55f
            );
        }
        else if (biome == BiomeType.River || biome == BiomeType.Beach)
        {
            // Water biomes should be flat
            heightNoise = _noiseGenerator.GetNoise(
                horizontalWorldBlockPosition.X * 0.01f,
                horizontalWorldBlockPosition.Y * 0.01f
            );
        }
        else
        {
            // Plains, Forest, Desert use standard multi-octave
            heightNoise = _noiseGenerator.GetMultiOctaveNoise(
                horizontalWorldBlockPosition.X * 0.008f,
                horizontalWorldBlockPosition.Y * 0.008f,
                octaves: 4,
                persistence: 0.5f
            );
        }

        // Convert noise (-1 to 1) to height based on biome
        int height = (int)(biomeData.BaseHeight + heightNoise * biomeData.HeightVariation);

        // Ensure minimum height
        return Math.Max(height, 5);
    }

    /// <summary>
    /// Blends biomes at borders for smoother transitions
    /// </summary>
    public (BiomeType primary, float blendFactor) GetBlendedBiomeAt(Vector2D<int> horizontalWorldBlockPosition)
    {
        BiomeType primaryBiome = GetBiomeAt(horizontalWorldBlockPosition);

        // Sample nearby positions to detect biome boundaries
        var nearby = new[]
        {
            GetBiomeAt(new Vector2D<int>(horizontalWorldBlockPosition.X + 8, horizontalWorldBlockPosition.Y)),
            GetBiomeAt(new Vector2D<int>(horizontalWorldBlockPosition.X - 8, horizontalWorldBlockPosition.Y)),
            GetBiomeAt(new Vector2D<int>(horizontalWorldBlockPosition.X, horizontalWorldBlockPosition.Y + 8)),
            GetBiomeAt(new Vector2D<int>(horizontalWorldBlockPosition.X, horizontalWorldBlockPosition.Y - 8))
        };

        int matchCount = nearby.Count(b => b == primaryBiome);
        float blendFactor = matchCount / 4.0f; // 1.0 = pure biome, <1.0 = transition zone

        return (primaryBiome, blendFactor);
    }
}
