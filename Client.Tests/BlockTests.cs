using System.Numerics;
using FluentAssertions;
using Silk.NET.Maths;

namespace Client.Tests;

public class BlockTests
{
    [TestCase(0f, 0f, 0f, 0, 0, 0)]
    [TestCase(-1.8f, 2f, 0f, -2, 2, 0)]
    [TestCase(-0.57f, 1.2f, 0f, -1, 1, 0)]
    [TestCase(1.76f, -0.49f, 0f, 2, 0, 0)]
    public void WorldToBlockPosition_CalculatesCorrectly(float wX, float wY, float wZ, int eX, int eY, int eZ)
    {
        var worldPosition = new Vector3(wX, wY, wZ);
        var expectedBlockPosition = new Vector3D<int>(eX, eY, eZ);

        var blockPosition = Block.WorldToBlockPosition(worldPosition);

        blockPosition.Should().Be(expectedBlockPosition);
    }
}