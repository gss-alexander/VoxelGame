using Client.Blocks;
using Client.Chunks.Structures;
using Silk.NET.Maths;

namespace Client.Chunks.Generation;

public class StructureGenerator
{
    private readonly BlockDatabase _blockDatabase;
    private readonly NoiseGenerator _noiseGenerator;
    private readonly int _seed;

    public StructureGenerator(BlockDatabase blockDatabase, NoiseGenerator noiseGenerator, int _seed)
    {
        _blockDatabase = blockDatabase;
        _noiseGenerator = noiseGenerator;
        this._seed = _seed;
    }

    /// <summary>
    /// Determines if a tree should be placed at this position
    /// </summary>
    public bool ShouldPlaceTree(Vector2D<int> horizontalWorldBlockPosition, BiomeData biomeData, out int treeHeight)
    {
        treeHeight = 0;

        if (biomeData.TreeDensity <= 0)
            return false;

        // Use position-based hash for deterministic randomness
        var random = new Random(HashPosition(horizontalWorldBlockPosition));

        // Check density
        if (random.NextDouble() > biomeData.TreeDensity)
            return false;

        // Use noise to create clumps of trees instead of uniform distribution
        float clumpNoise = _noiseGenerator.GetNoise(
            horizontalWorldBlockPosition.X * 0.05f,
            horizontalWorldBlockPosition.Y * 0.05f,
            seedOffset: 2000
        );

        // Only place trees in "high" areas of the clump noise
        if (clumpNoise < 0.2f)
            return false;

        // Determine tree height
        treeHeight = random.Next(biomeData.MinTreeHeight, biomeData.MaxTreeHeight + 1);

        return true;
    }

    /// <summary>
    /// Places a tree structure in the chunk
    /// </summary>
    public void PlaceTree(ChunkData chunkData, Vector2D<int> localPosition, int surfaceHeight, int treeHeight, BiomeType biome)
    {
        var logId = _blockDatabase.GetInternalId("log");
        var leavesId = _blockDatabase.GetInternalId("leaves");

        int x = localPosition.X;
        int z = localPosition.Y;

        // Ensure spacing from chunk borders
        if (x >= Chunk.Size - 2 || x <= 1 || z >= Chunk.Size - 2 || z <= 1)
            return;

        if (surfaceHeight + treeHeight >= Chunk.Height)
            return;

        // Different tree styles based on biome
        if (biome == BiomeType.Desert)
        {
            // Cactus-style (thin, no leaves at top)
            for (var y = surfaceHeight; y < surfaceHeight + treeHeight && y < Chunk.Height; y++)
            {
                chunkData.SetBlock(new Vector3D<int>(x, y, z), logId);
            }
        }
        else
        {
            // Place trunk
            for (var y = surfaceHeight; y < surfaceHeight + treeHeight && y < Chunk.Height; y++)
            {
                chunkData.SetBlock(new Vector3D<int>(x, y, z), logId);
            }

            // Place canopy with varied shapes based on tree height
            if (treeHeight >= 6)
            {
                // Large tree - pyramid style canopy
                PlaceLargeTreeCanopy(chunkData, x, z, surfaceHeight, treeHeight, leavesId);
            }
            else if (treeHeight >= 4)
            {
                // Medium tree - round canopy
                PlaceMediumTreeCanopy(chunkData, x, z, surfaceHeight, treeHeight, leavesId);
            }
            else
            {
                // Small tree - simple canopy
                PlaceSmallTreeCanopy(chunkData, x, z, surfaceHeight, treeHeight, leavesId);
            }
        }
    }

    private void PlaceSmallTreeCanopy(ChunkData chunkData, int x, int z, int surfaceHeight, int treeHeight, int leavesId)
    {
        // Simple 3x3 canopy at the top
        int canopyStart = surfaceHeight + treeHeight - 1;
        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue; // Skip center (trunk)

                var leafX = x + dx;
                var leafZ = z + dz;

                if (leafX >= 0 && leafX < Chunk.Size && leafZ >= 0 && leafZ < Chunk.Size)
                {
                    for (int y = canopyStart; y <= canopyStart + 1 && y < Chunk.Height; y++)
                    {
                        chunkData.SetBlock(new Vector3D<int>(leafX, y, leafZ), leavesId);
                    }
                }
            }
        }

        // Top leaf block
        if (surfaceHeight + treeHeight < Chunk.Height)
        {
            chunkData.SetBlock(new Vector3D<int>(x, surfaceHeight + treeHeight, z), leavesId);
        }
    }

    private void PlaceMediumTreeCanopy(ChunkData chunkData, int x, int z, int surfaceHeight, int treeHeight, int leavesId)
    {
        // Round canopy with multiple layers
        int canopyStart = surfaceHeight + treeHeight - 2;

        // Bottom layer - 5x5 minus corners
        for (var dx = -2; dx <= 2; dx++)
        {
            for (var dz = -2; dz <= 2; dz++)
            {
                if (Math.Abs(dx) == 2 && Math.Abs(dz) == 2) continue; // Skip corners

                var leafX = x + dx;
                var leafZ = z + dz;

                if (leafX >= 0 && leafX < Chunk.Size && leafZ >= 0 && leafZ < Chunk.Size && canopyStart < Chunk.Height)
                {
                    chunkData.SetBlock(new Vector3D<int>(leafX, canopyStart, leafZ), leavesId);
                }
            }
        }

        // Middle layer - 3x3
        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dz = -1; dz <= 1; dz++)
            {
                var leafX = x + dx;
                var leafZ = z + dz;

                if (leafX >= 0 && leafX < Chunk.Size && leafZ >= 0 && leafZ < Chunk.Size && canopyStart + 1 < Chunk.Height)
                {
                    chunkData.SetBlock(new Vector3D<int>(leafX, canopyStart + 1, leafZ), leavesId);
                }
            }
        }

        // Top layer - single block
        if (surfaceHeight + treeHeight < Chunk.Height)
        {
            chunkData.SetBlock(new Vector3D<int>(x, surfaceHeight + treeHeight, z), leavesId);
        }
    }

    private void PlaceLargeTreeCanopy(ChunkData chunkData, int x, int z, int surfaceHeight, int treeHeight, int leavesId)
    {
        // Large pyramid-style canopy
        int canopyStart = surfaceHeight + treeHeight - 4;

        // Bottom layer - 5x5 full
        PlaceCanopyLayer(chunkData, x, z, canopyStart, 2, leavesId);

        // Second layer - 5x5 minus corners
        for (var dx = -2; dx <= 2; dx++)
        {
            for (var dz = -2; dz <= 2; dz++)
            {
                if (Math.Abs(dx) == 2 && Math.Abs(dz) == 2) continue;

                var leafX = x + dx;
                var leafZ = z + dz;

                if (leafX >= 0 && leafX < Chunk.Size && leafZ >= 0 && leafZ < Chunk.Size && canopyStart + 1 < Chunk.Height)
                {
                    chunkData.SetBlock(new Vector3D<int>(leafX, canopyStart + 1, leafZ), leavesId);
                }
            }
        }

        // Third layer - 3x3
        PlaceCanopyLayer(chunkData, x, z, canopyStart + 2, 1, leavesId);

        // Fourth layer - 3x3
        PlaceCanopyLayer(chunkData, x, z, canopyStart + 3, 1, leavesId);

        // Top - single block
        if (surfaceHeight + treeHeight < Chunk.Height)
        {
            chunkData.SetBlock(new Vector3D<int>(x, surfaceHeight + treeHeight, z), leavesId);
        }
    }

    private void PlaceCanopyLayer(ChunkData chunkData, int centerX, int centerZ, int y, int radius, int leavesId)
    {
        if (y >= Chunk.Height) return;

        for (var dx = -radius; dx <= radius; dx++)
        {
            for (var dz = -radius; dz <= radius; dz++)
            {
                var leafX = centerX + dx;
                var leafZ = centerZ + dz;

                if (leafX >= 0 && leafX < Chunk.Size && leafZ >= 0 && leafZ < Chunk.Size)
                {
                    chunkData.SetBlock(new Vector3D<int>(leafX, y, leafZ), leavesId);
                }
            }
        }
    }

    private int HashPosition(Vector2D<int> position)
    {
        return (position.X * 73856093) ^ (position.Y * 19349663) ^ (_seed * 83492791);
    }
}
