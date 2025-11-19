using Silk.NET.Maths;

namespace Client.Chunks.Generation;

public class NoiseGenerator
{
    private readonly FastNoiseLite _noise;
    private readonly int _seed;

    public NoiseGenerator(FastNoiseLite noise, int seed)
    {
        _noise = noise;
        _seed = seed;
    }

    /// <summary>
    /// Generates multi-octave Perlin noise for more interesting terrain
    /// </summary>
    public float GetMultiOctaveNoise(float x, float y, int octaves = 4, float persistence = 0.5f, float lacunarity = 2.0f)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            _noise.SetSeed(_seed + i);
            total += _noise.GetNoise(x * frequency, y * frequency) * amplitude;

            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue;
    }

    /// <summary>
    /// Gets noise value for a specific purpose with optional offset
    /// </summary>
    public float GetNoise(float x, float y, int seedOffset = 0, float scale = 1.0f)
    {
        _noise.SetSeed(_seed + seedOffset);
        return _noise.GetNoise(x * scale, y * scale);
    }

    /// <summary>
    /// Gets ridged noise - useful for mountains and valleys
    /// </summary>
    public float GetRidgedNoise(float x, float y, int octaves = 3)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            _noise.SetSeed(_seed + i + 100);
            float noiseValue = Math.Abs(_noise.GetNoise(x * frequency, y * frequency));
            noiseValue = 1.0f - noiseValue; // Invert for ridges
            noiseValue = noiseValue * noiseValue; // Square for sharper ridges

            total += noiseValue * amplitude;

            maxValue += amplitude;
            amplitude *= 0.5f;
            frequency *= 2.0f;
        }

        return total / maxValue;
    }

    /// <summary>
    /// Gets cellular/Voronoi noise - useful for specific features
    /// </summary>
    public float GetCellularNoise(float x, float y)
    {
        var originalType = _noise.GetNoiseType();
        _noise.SetSeed(_seed + 200);
        _noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        float value = _noise.GetNoise(x, y);
        _noise.SetNoiseType(originalType);
        return value;
    }
}
