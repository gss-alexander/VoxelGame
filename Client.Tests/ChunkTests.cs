using System.Numerics;
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

    [TestCase(0f, 0f, 0f, 0, 0)]
    [TestCase(8f, 0f, 8f, 0, 0)]
    [TestCase(15.9f, 0f, 15.9f, 0, 0)]
    [TestCase(16f, 0f, 0f, 1, 0)]
    [TestCase(0f, 0f, 16f, 0, 1)]
    [TestCase(-0.1f, 0f, 0f, -1, 0)]
    [TestCase(0f, 0f, -0.1f, 0, -1)]
    public void WorldToChunkPosition_VariousPositions_CalculatesCorrectly(float wX, float wY, float wZ, int cX, int cY)
    {
        var worldPosition = new Vector3(wX, wY, wZ);
        var expectedChunkPosition = new Vector2D<int>(cX, cY);

        var chunkPosition = Chunk.WorldToChunkPosition(worldPosition);

        chunkPosition.Should().Be(expectedChunkPosition);
    }

    [TestCase(0, 0, 0, 0, 0, 0, 0)]
    [TestCase(1, 2, 5, 7, 10, 21, 87)]
    [TestCase(0, 0, 15, 0, 255, 15)]
    public void LocalChunkToWorldPosition_VariousPositions_CalculatesCorrectly(
        int cX, int cY, int lX, int lY, int lZ, int expectedX, int expectedZ)
    {
        var chunkPosition = new Vector2D<int>(cX, cY);
        var localPosition = new Vector3D<int>(lX, lY, lZ);

        var worldPosition = Chunk.LocalChunkToWorldPosition(chunkPosition, localPosition);

        worldPosition.X.Should().Be(expectedX);
        worldPosition.Y.Should().Be(lY);
        worldPosition.Z.Should().Be(expectedZ);
    }

    [Test]
    public void GetNeighbouringChunksForBlock_CenterOfChunk_ReturnsNoNeighbours()
    {
        var blockPosition = new Vector3D<int>(8, 64, 8);
        var resultList = new List<Vector2D<int>>();

        Chunk.GetNeighbouringChunksForBlock(blockPosition, resultList);

        resultList.Should().BeEmpty();
    }

    [Test]
    public void GetNeighbouringChunksForBlock_LeftEdge_ReturnsLeftNeighbour()
    {
        var blockPosition = new Vector3D<int>(0, 64, 8);
        var resultList = new List<Vector2D<int>>();

        Chunk.GetNeighbouringChunksForBlock(blockPosition, resultList);

        resultList.Should().ContainSingle();
        resultList[0].Should().Be(new Vector2D<int>(-1, 0));
    }

    [Test]
    public void GetNeighbouringChunksForBlock_RightEdge_ReturnsRightNeighbour()
    {
        var blockPosition = new Vector3D<int>(15, 64, 8);
        var resultList = new List<Vector2D<int>>();

        Chunk.GetNeighbouringChunksForBlock(blockPosition, resultList);

        resultList.Should().ContainSingle();
        resultList[0].Should().Be(new Vector2D<int>(1, 0));
    }

    [Test]
    public void GetNeighbouringChunksForBlock_FrontEdge_ReturnsFrontNeighbour()
    {
        var blockPosition = new Vector3D<int>(8, 64, 15);
        var resultList = new List<Vector2D<int>>();

        Chunk.GetNeighbouringChunksForBlock(blockPosition, resultList);

        resultList.Should().ContainSingle();
        resultList[0].Should().Be(new Vector2D<int>(0, 1));
    }

    [Test]
    public void GetNeighbouringChunksForBlock_BackEdge_ReturnsBackNeighbour()
    {
        var blockPosition = new Vector3D<int>(8, 64, 0);
        var resultList = new List<Vector2D<int>>();

        Chunk.GetNeighbouringChunksForBlock(blockPosition, resultList);

        resultList.Should().ContainSingle();
        resultList[0].Should().Be(new Vector2D<int>(0, -1));
    }

    [Test]
    public void GetNeighbouringChunksForBlock_Corner_ReturnsTwoNeighbours()
    {
        var blockPosition = new Vector3D<int>(0, 64, 0);
        var resultList = new List<Vector2D<int>>();

        Chunk.GetNeighbouringChunksForBlock(blockPosition, resultList);

        resultList.Should().HaveCount(2);
        resultList.Should().Contain(new Vector2D<int>(-1, 0));
        resultList.Should().Contain(new Vector2D<int>(0, -1));
    }

    [Test]
    public void ChunkData_Constructor_InitializesWithFillBlock()
    {
        var chunkData = new ChunkData(new Vector2D<int>(0, 0), fillBlockId: 5);

        // Check a few random positions
        chunkData.GetBlock(new Vector3D<int>(0, 0, 0)).Should().Be(5);
        chunkData.GetBlock(new Vector3D<int>(8, 64, 8)).Should().Be(5);
        chunkData.GetBlock(new Vector3D<int>(15, 255, 15)).Should().Be(5);
    }

    [Test]
    public void ChunkData_SetBlock_UpdatesBlockCorrectly()
    {
        var chunkData = new ChunkData(new Vector2D<int>(0, 0), fillBlockId: 0);

        chunkData.SetBlock(new Vector3D<int>(5, 10, 7), 3);

        chunkData.GetBlock(new Vector3D<int>(5, 10, 7)).Should().Be(3);
    }

    [Test]
    public void ChunkData_GetBlock_ReturnsCorrectBlock()
    {
        var chunkData = new ChunkData(new Vector2D<int>(0, 0), fillBlockId: 1);
        chunkData.SetBlock(new Vector3D<int>(3, 50, 12), 7);

        var block = chunkData.GetBlock(new Vector3D<int>(3, 50, 12));

        block.Should().Be(7);
    }

    [Test]
    public void ChunkData_Position_ReturnsCorrectPosition()
    {
        var position = new Vector2D<int>(3, -2);
        var chunkData = new ChunkData(position, fillBlockId: 0);

        chunkData.Position.Should().Be(position);
    }

    [Test]
    public void ChunkData_MultipleSetAndGet_WorksCorrectly()
    {
        var chunkData = new ChunkData(new Vector2D<int>(0, 0), fillBlockId: 0);

        // Set multiple blocks
        chunkData.SetBlock(new Vector3D<int>(0, 0, 0), 1);
        chunkData.SetBlock(new Vector3D<int>(15, 255, 15), 2);
        chunkData.SetBlock(new Vector3D<int>(8, 128, 8), 3);

        // Verify all blocks
        chunkData.GetBlock(new Vector3D<int>(0, 0, 0)).Should().Be(1);
        chunkData.GetBlock(new Vector3D<int>(15, 255, 15)).Should().Be(2);
        chunkData.GetBlock(new Vector3D<int>(8, 128, 8)).Should().Be(3);
    }

    [Test]
    public void ChunkData_OverwriteBlock_UpdatesCorrectly()
    {
        var chunkData = new ChunkData(new Vector2D<int>(0, 0), fillBlockId: 0);
        chunkData.SetBlock(new Vector3D<int>(5, 10, 5), 1);

        chunkData.SetBlock(new Vector3D<int>(5, 10, 5), 2);

        chunkData.GetBlock(new Vector3D<int>(5, 10, 5)).Should().Be(2);
    }

    [Test]
    public void ChunkData_AllPositions_AreAccessible()
    {
        var chunkData = new ChunkData(new Vector2D<int>(0, 0), fillBlockId: 0);
        var testBlockId = 42;

        // Test corners and edges
        var positions = new[]
        {
            new Vector3D<int>(0, 0, 0),
            new Vector3D<int>(15, 0, 0),
            new Vector3D<int>(0, 0, 15),
            new Vector3D<int>(15, 0, 15),
            new Vector3D<int>(0, 255, 0),
            new Vector3D<int>(15, 255, 0),
            new Vector3D<int>(0, 255, 15),
            new Vector3D<int>(15, 255, 15),
        };

        foreach (var pos in positions)
        {
            chunkData.SetBlock(pos, testBlockId);
            chunkData.GetBlock(pos).Should().Be(testBlockId, $"position {pos} should be accessible");
        }
    }

    [TestCase(0, 0, 0, 0)]
    [TestCase(1, 0, 0, 1)]
    [TestCase(0, 1, 0, 16)]
    [TestCase(0, 0, 1, 4096)]
    [TestCase(15, 255, 15, 1044495)]
    public void ChunkData_BlockIndexing_WorksConsistently(int x, int y, int z, int expectedMinIndex)
    {
        var chunkData = new ChunkData(new Vector2D<int>(0, 0), fillBlockId: 0);

        // Set and get should work consistently regardless of internal indexing
        chunkData.SetBlock(new Vector3D<int>(x, y, z), 99);
        var retrieved = chunkData.GetBlock(new Vector3D<int>(x, y, z));

        retrieved.Should().Be(99);
    }

    [Test]
    public void BlockToChunkPosition_NegativeCoordinates_HandlesCorrectly()
    {
        var testCases = new[]
        {
            (new Vector3D<int>(-1, 0, 0), new Vector2D<int>(-1, 0)),
            (new Vector3D<int>(-16, 0, 0), new Vector2D<int>(-1, 0)),
            (new Vector3D<int>(-17, 0, 0), new Vector2D<int>(-2, 0)),
            (new Vector3D<int>(0, 0, -1), new Vector2D<int>(0, -1)),
            (new Vector3D<int>(0, 0, -16), new Vector2D<int>(0, -1)),
            (new Vector3D<int>(0, 0, -17), new Vector2D<int>(0, -2)),
        };

        foreach (var (blockPos, expectedChunkPos) in testCases)
        {
            var result = Chunk.BlockToChunkPosition(blockPos);
            result.Should().Be(expectedChunkPos, $"block {blockPos} should map to chunk {expectedChunkPos}");
        }
    }

    [Test]
    public void ChunkWorldCenter_CalculatesCorrectly()
    {
        var chunkData = new ChunkData(new Vector2D<int>(2, 3), fillBlockId: 0);
        var chunk = new Chunk(chunkData, null, null, null);

        var center = chunk.ChunkWorldCenter;

        center.X.Should().Be(32); // 2 * 16
        center.Y.Should().Be(48); // 3 * 16
    }

    [Test]
    public void Chunk_Position_ReturnsDataPosition()
    {
        var position = new Vector2D<int>(5, -3);
        var chunkData = new ChunkData(position, fillBlockId: 0);
        var chunk = new Chunk(chunkData, null, null, null);

        chunk.Position.Should().Be(position);
    }

    [Test]
    public void Chunk_Data_ReturnsChunkData()
    {
        var chunkData = new ChunkData(new Vector2D<int>(0, 0), fillBlockId: 0);
        var chunk = new Chunk(chunkData, null, null, null);

        chunk.Data.Should().BeSameAs(chunkData);
    }

    [Test]
    public void LocalChunkToWorldPosition_RoundTrip_WorksCorrectly()
    {
        var worldBlock = new Vector3D<int>(37, 100, -23);

        var chunkPos = Chunk.BlockToChunkPosition(worldBlock);
        var localPos = Client.Blocks.Block.WorldBlockToLocalChunkPosition(worldBlock);
        var reconstructed = Chunk.LocalChunkToWorldPosition(chunkPos, localPos);

        reconstructed.Should().Be(worldBlock);
    }
}