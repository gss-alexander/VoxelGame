using System.Numerics;
using FluentAssertions;
using Silk.NET.Maths;

namespace Client.Tests;
    [TestFixture]
    public class VoxelRaycasterTests
    {
        private VoxelRaycaster _raycaster;
        private HashSet<Vector3D<int>> _solidVoxels;

        [SetUp]
        public void SetUp()
        {
            _solidVoxels = new HashSet<Vector3D<int>>();
            _raycaster = new VoxelRaycaster(pos => _solidVoxels.Contains(pos));
        }

        #region Basic Hit Detection Tests

        [Test]
        public void Cast_ShouldReturnHit_WhenRayStartsInSolidVoxel()
        {
            // Arrange
            var solidVoxel = new Vector3D<int>(0, 0, 0);
            _solidVoxels.Add(solidVoxel);
            var origin = new Vector3(0f, 0f, 0f);
            var direction = Vector3.UnitX;

            // Act
            var result = _raycaster.Cast(origin, direction, 10f);

            // Assert
            result.Should().NotBeNull();
            result.Value.Position.Should().Be(solidVoxel);
        }

        [TestCase(1f, 0f, 0f, 1, 0, 0)] // +X direction
        [TestCase(-1f, 0f, 0f, -1, 0, 0)] // -X direction
        [TestCase(0f, 1f, 0f, 0, 1, 0)] // +Y direction
        [TestCase(0f, -1f, 0f, 0, -1, 0)] // -Y direction
        [TestCase(0f, 0f, 1f, 0, 0, 1)] // +Z direction
        [TestCase(0f, 0f, -1f, 0, 0, -1)] // -Z direction
        public void Cast_ShouldHitAdjacentVoxel_WhenRayingInCardinalDirections(
            float dirX, float dirY, float dirZ, int expectedX, int expectedY, int expectedZ)
        {
            // Arrange
            var solidVoxel = new Vector3D<int>(expectedX, expectedY, expectedZ);
            _solidVoxels.Add(solidVoxel);
            var origin = new Vector3(0f, 0f, 0f); // Center of voxel (0,0,0)
            var direction = new Vector3(dirX, dirY, dirZ);

            // Act
            var result = _raycaster.Cast(origin, direction, 10f);

            // Assert
            result.Should().NotBeNull();
            result.Value.Position.Should().Be(solidVoxel);
        }

        [TestCase(1f, 1f, 0f, 2, 2, 0)] // Diagonal in XY plane
        [TestCase(1f, 1f, 1f, 1, 1, 1)] // 3D diagonal
        [TestCase(-1f, 1f, 0f, -2, 2, 0)] // Mixed diagonal
        [TestCase(1f, 0f, 1f, 2, 0, 2)] // Diagonal in XZ plane
        [TestCase(0f, 1f, 1f, 0, 2, 2)] // Diagonal in YZ plane
        public void Cast_ShouldHitVoxel_WhenRayingInDiagonalDirections(
            float dirX, float dirY, float dirZ, int expectedX, int expectedY, int expectedZ)
        {
            // Arrange
            var solidVoxel = new Vector3D<int>(expectedX, expectedY, expectedZ);
            _solidVoxels.Add(solidVoxel);
            var origin = new Vector3(0f, 0f, 0f);
            var direction = new Vector3(dirX, dirY, dirZ);

            // Act
            var result = _raycaster.Cast(origin, direction, 10f);

            // Assert
            result.Should().NotBeNull();
            result.Value.Position.Should().Be(solidVoxel);
        }

        #endregion

        #region Miss Tests

        [Test]
        public void Cast_ShouldReturnNull_WhenNoSolidVoxelsInPath()
        {
            // Arrange
            var origin = new Vector3(0f, 0f, 0f);
            var direction = Vector3.UnitX;

            // Act
            var result = _raycaster.Cast(origin, direction, 10f);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Cast_ShouldReturnNull_WhenSolidVoxelBeyondMaxDistance()
        {
            // Arrange
            var solidVoxel = new Vector3D<int>(5, 0, 0);
            _solidVoxels.Add(solidVoxel);
            var origin = new Vector3(0f, 0f, 0f);
            var direction = Vector3.UnitX;
            var maxDistance = 2f; // Too short to reach voxel at x=5

            // Act
            var result = _raycaster.Cast(origin, direction, maxDistance);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [Test]
        public void Cast_ShouldReturnFirstHit_WhenMultipleSolidVoxelsInPath()
        {
            // Arrange
            _solidVoxels.Add(new Vector3D<int>(2, 0, 0));
            _solidVoxels.Add(new Vector3D<int>(3, 0, 0));
            _solidVoxels.Add(new Vector3D<int>(4, 0, 0));
            var origin = new Vector3(0f, 0f, 0f);
            var direction = Vector3.UnitX;

            // Act
            var result = _raycaster.Cast(origin, direction, 10f);

            // Assert
            result.Should().NotBeNull();
            result.Value.Position.Should().Be(new Vector3D<int>(2, 0, 0));
        }

        #endregion

        #region Direction Vector Tests

        [Test]
        public void Cast_ShouldNormalizeDirection_WhenDirectionNotNormalized()
        {
            // Arrange
            var solidVoxel = new Vector3D<int>(1, 0, 0);
            _solidVoxels.Add(solidVoxel);
            var origin = new Vector3(0f, 0f, 0f);
            var direction = new Vector3(5f, 0f, 0f); // Non-normalized

            // Act
            var result = _raycaster.Cast(origin, direction, 10f);

            // Assert
            result.Should().NotBeNull();
            result.Value.Position.Should().Be(solidVoxel);
        }

        [TestCase(0f, 0f, 0f)]
        public void Cast_ShouldHandleZeroDirection_Gracefully(float x, float y, float z)
        {
            // Arrange
            var origin = new Vector3(0f, 0f, 0f);
            var direction = new Vector3(x, y, z);

            // Act & Assert
            Action act = () => _raycaster.Cast(origin, direction, 10f);
            act.Should().NotThrow();
        }

        #endregion

        #region Negative Coordinate Tests

        [TestCase(-1, 0, 0)]
        [TestCase(0, -1, 0)]
        [TestCase(0, 0, -1)]
        [TestCase(-2, -3, -4)]
        public void Cast_ShouldWork_WithNegativeCoordinates(int x, int y, int z)
        {
            // Arrange
            var solidVoxel = new Vector3D<int>(x, y, z);
            _solidVoxels.Add(solidVoxel);
            var origin = new Vector3(-0.5f, -0.5f, -0.5f);
            var direction = Vector3.Normalize(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));

            // Act
            var result = _raycaster.Cast(origin, direction, 10f);

            // Assert
            result.Should().NotBeNull();
            result.Value.Position.Should().Be(solidVoxel);
        }

        #endregion

        #region Distance and Precision Tests

        [TestCase(0.1f)]
        [TestCase(1f)]
        [TestCase(5f)]
        [TestCase(100f)]
        public void Cast_ShouldRespectMaxDistance(float maxDistance)
        {
            // Arrange
            var solidVoxel = new Vector3D<int>(10, 0, 0); // Far away
            _solidVoxels.Add(solidVoxel);
            var origin = new Vector3(0f, 0f, 0f);
            var direction = Vector3.UnitX;

            // Act
            var result = _raycaster.Cast(origin, direction, maxDistance);

            // Assert
            var expectedHit = maxDistance >= 9.5f; // Distance to voxel (10,0,0) from origin
            if (expectedHit)
            {
                result.Should().NotBeNull();
            }
            else
            {
                result.Should().BeNull();
            }
        }

        [TestCase(0.01f, 0.01f, 0.01f)]
        [TestCase(0.001f, 0.001f, 0.001f)]
        public void Cast_ShouldHandleSmallDirectionComponents(float x, float y, float z)
        {
            // Arrange
            var solidVoxel = new Vector3D<int>(1, 1, 1);
            _solidVoxels.Add(solidVoxel);
            var origin = new Vector3(0f, 0f, 0f);
            var direction = Vector3.Normalize(new Vector3(x, y, z));

            // Act
            var result = _raycaster.Cast(origin, direction, 100f);

            // Assert
            result.Should().NotBeNull();
            result.Value.Position.Should().Be(solidVoxel);
        }

        #endregion

        #region Axis-Aligned Ray Tests

        [TestCase(1f, 0f, 0f, 5, 0, 0)] // Pure X-axis ray
        [TestCase(0f, 1f, 0f, 0, 5, 0)] // Pure Y-axis ray
        [TestCase(0f, 0f, 1f, 0, 0, 5)] // Pure Z-axis ray
        public void Cast_ShouldWork_WithAxisAlignedRays(
            float dirX, float dirY, float dirZ, int targetX, int targetY, int targetZ)
        {
            // Arrange
            var solidVoxel = new Vector3D<int>(targetX, targetY, targetZ);
            _solidVoxels.Add(solidVoxel);
            var origin = new Vector3(0f, 0f, 0f);
            var direction = new Vector3(dirX, dirY, dirZ);

            // Act
            var result = _raycaster.Cast(origin, direction, 10f);

            // Assert
            result.Should().NotBeNull();
            result.Value.Position.Should().Be(solidVoxel);
        }

        #endregion

        #region Complex Path Tests

        [Test]
        public void Cast_ShouldTraverseCorrectPath_ThroughMultipleEmptyVoxels()
        {
            // Arrange
            var solidVoxel = new Vector3D<int>(3, 3, 3);
            _solidVoxels.Add(solidVoxel);
            var origin = new Vector3(0.1f, 0.1f, 0.1f);
            var direction = Vector3.Normalize(new Vector3(1f, 1f, 1f));

            // Act
            var result = _raycaster.Cast(origin, direction, 20f);

            // Assert
            result.Should().NotBeNull();
            result.Value.Position.Should().Be(solidVoxel);
        }

        [Test]
        public void Cast_ShouldSkipEmptyVoxels_InStraightLine()
        {
            // Arrange
            // Create a line of voxels with gaps
            _solidVoxels.Add(new Vector3D<int>(5, 0, 0)); // Target
            var origin = new Vector3(0f, 0f, 0f);
            var direction = Vector3.UnitX;

            // Act
            var result = _raycaster.Cast(origin, direction, 10f);

            // Assert
            result.Should().NotBeNull();
            result.Value.Position.Should().Be(new Vector3D<int>(5, 0, 0));
        }

        #endregion

        #region Constructor and State Tests

        [Test]
        public void Cast_ShouldCallIsVoxelSolidFunc_ForEachVoxelInPath()
        {
            // Arrange
            var calledPositions = new List<Vector3D<int>>();
            var raycaster = new VoxelRaycaster(pos =>
            {
                calledPositions.Add(pos);
                return pos.X == 2 && pos.Y == 0 && pos.Z == 0; // Only (2,0,0) is solid
            });
            var origin = new Vector3(0f, 0f, 0f);
            var direction = Vector3.UnitX;

            // Act
            var result = raycaster.Cast(origin, direction, 5f);

            // Assert
            result.Should().NotBeNull();
            result.Value.Position.Should().Be(new Vector3D<int>(2, 0, 0));
            calledPositions.Should().Contain(new Vector3D<int>(0, 0, 0));
            calledPositions.Should().Contain(new Vector3D<int>(1, 0, 0));
            calledPositions.Should().Contain(new Vector3D<int>(2, 0, 0));
        }

        #endregion

        #region Performance Edge Cases

        [Test]
        public void Cast_ShouldTerminate_WhenMaxDistanceIsZero()
        {
            // Arrange
            var origin = new Vector3(0f, 0f, 0f);
            var direction = Vector3.UnitX;

            // Act
            var result = _raycaster.Cast(origin, direction, 0f);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Cast_ShouldWork_WithVeryLargeMaxDistance()
        {
            // Arrange
            var solidVoxel = new Vector3D<int>(100, 100, 100);
            _solidVoxels.Add(solidVoxel);
            var origin = new Vector3(0f, 0f, 0f);
            var direction = Vector3.Normalize(new Vector3(1f, 1f, 1f));

            // Act
            var result = _raycaster.Cast(origin, direction, 1000f);

            // Assert
            result.Should().NotBeNull();
            result.Value.Position.Should().Be(solidVoxel);
        }

        #endregion
    }