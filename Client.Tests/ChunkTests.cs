using Client.Chunks;
using FluentAssertions;
using Silk.NET.Maths;

namespace Client.Tests;

public class ChunkTests
{
    [TestCase(0, 0, 0, 0, 0)]
    [TestCase(5, 8, 12, 0, 0)]
    [TestCase(11, 4, 22, 0, 1)] // chunk X should be 1 as chunk 0 contains block x positions 0 - 15
    [TestCase(-4, 2, 16, -1, 1)] // chunk Z should be 1 as chunk -1 contains block x positions -1 - -16
    public void BlockToChunkPosition_Returns_ExpectedChunkPositions(int bX, int bY, int bZ, int cX, int cY)
    {
        var blockPosition = new Vector3D<int>(bX, bY, bZ);
        var expectedChunkPosition = new Vector2D<int>(cX, cY);

        var chunkPosition = Chunk.BlockToChunkPosition(blockPosition);

        chunkPosition.Should().Be(expectedChunkPosition);
    }
}