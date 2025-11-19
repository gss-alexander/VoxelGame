namespace Client.Chunks.Generation;

public class BiomeData
{
    public BiomeType Type { get; set; }
    public int BaseHeight { get; set; }
    public float HeightVariation { get; set; }
    public float TreeDensity { get; set; }
    public int MinTreeHeight { get; set; }
    public int MaxTreeHeight { get; set; }
    public string SurfaceBlock { get; set; } = "grass";
    public string SubSurfaceBlock { get; set; } = "dirt";
    public string DeepBlock { get; set; } = "stone";
    public int SurfaceDepth { get; set; } = 1;
    public int SubSurfaceDepth { get; set; } = 3;

    public static BiomeData GetBiomeData(BiomeType type)
    {
        return type switch
        {
            BiomeType.Plains => new BiomeData
            {
                Type = BiomeType.Plains,
                BaseHeight = 68,
                HeightVariation = 8f,
                TreeDensity = 0.002f, // Sparse trees
                MinTreeHeight = 4,
                MaxTreeHeight = 6,
                SurfaceBlock = "grass",
                SubSurfaceBlock = "dirt",
                DeepBlock = "stone",
                SurfaceDepth = 1,
                SubSurfaceDepth = 3
            },
            BiomeType.Forest => new BiomeData
            {
                Type = BiomeType.Forest,
                BaseHeight = 70,
                HeightVariation = 12f,
                TreeDensity = 0.015f, // Dense trees
                MinTreeHeight = 5,
                MaxTreeHeight = 9,
                SurfaceBlock = "grass",
                SubSurfaceBlock = "dirt",
                DeepBlock = "stone",
                SurfaceDepth = 1,
                SubSurfaceDepth = 4
            },
            BiomeType.Mountains => new BiomeData
            {
                Type = BiomeType.Mountains,
                BaseHeight = 90,
                HeightVariation = 45f,
                TreeDensity = 0.001f, // Very sparse trees
                MinTreeHeight = 4,
                MaxTreeHeight = 6,
                SurfaceBlock = "stone",
                SubSurfaceBlock = "stone",
                DeepBlock = "stone",
                SurfaceDepth = 0,
                SubSurfaceDepth = 0
            },
            BiomeType.Desert => new BiomeData
            {
                Type = BiomeType.Desert,
                BaseHeight = 66,
                HeightVariation = 6f,
                TreeDensity = 0.0005f, // Very rare cacti
                MinTreeHeight = 2,
                MaxTreeHeight = 4,
                SurfaceBlock = "dirt", // Will use sand if available
                SubSurfaceBlock = "dirt",
                DeepBlock = "stone",
                SurfaceDepth = 4,
                SubSurfaceDepth = 2
            },
            BiomeType.Hills => new BiomeData
            {
                Type = BiomeType.Hills,
                BaseHeight = 75,
                HeightVariation = 20f,
                TreeDensity = 0.005f, // Moderate trees
                MinTreeHeight = 4,
                MaxTreeHeight = 7,
                SurfaceBlock = "grass",
                SubSurfaceBlock = "dirt",
                DeepBlock = "stone",
                SurfaceDepth = 1,
                SubSurfaceDepth = 3
            },
            BiomeType.River => new BiomeData
            {
                Type = BiomeType.River,
                BaseHeight = 60,
                HeightVariation = 2f,
                TreeDensity = 0.0f, // No trees in water
                MinTreeHeight = 0,
                MaxTreeHeight = 0,
                SurfaceBlock = "dirt",
                SubSurfaceBlock = "dirt",
                DeepBlock = "stone",
                SurfaceDepth = 2,
                SubSurfaceDepth = 3
            },
            BiomeType.Beach => new BiomeData
            {
                Type = BiomeType.Beach,
                BaseHeight = 64,
                HeightVariation = 3f,
                TreeDensity = 0.0f, // No trees on beach
                MinTreeHeight = 0,
                MaxTreeHeight = 0,
                SurfaceBlock = "dirt", // Will use sand if available
                SubSurfaceBlock = "dirt",
                DeepBlock = "stone",
                SurfaceDepth = 3,
                SubSurfaceDepth = 2
            },
            _ => GetBiomeData(BiomeType.Plains)
        };
    }
}
