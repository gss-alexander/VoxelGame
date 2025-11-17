using System.Numerics;
using Client;
using Client.Persistence;
using FluentAssertions;
using Silk.NET.Maths;

namespace Client.Tests;

public class PersistenceTests
{
    [Test]
    public void WorldData_Constructor_WithName_InitializesCorrectly()
    {
        var worldData = new WorldData("TestWorld");

        worldData.Name.Should().Be("TestWorld");
    }

    [Test]
    public void WorldData_Constructor_Default_CreatesInstance()
    {
        var worldData = new WorldData();

        worldData.Should().NotBeNull();
        worldData.ModifiedBlocks.Should().NotBeNull();
        worldData.Inventory.Should().NotBeNull();
    }

    [Test]
    public void WorldData_AddModifiedBlock_AddsBlock()
    {
        var worldData = new WorldData("TestWorld");
        var blockPosition = new Vector3D<int>(10, 64, 20);

        worldData.AddModifiedBlock(blockPosition, "stone");

        worldData.ModifiedBlocks.Should().ContainKey(blockPosition);
        worldData.ModifiedBlocks[blockPosition].Should().Be("stone");
    }

    [Test]
    public void WorldData_AddModifiedBlock_MultipleBlocks_AddsAll()
    {
        var worldData = new WorldData("TestWorld");

        worldData.AddModifiedBlock(new Vector3D<int>(0, 0, 0), "dirt");
        worldData.AddModifiedBlock(new Vector3D<int>(5, 10, 15), "stone");
        worldData.AddModifiedBlock(new Vector3D<int>(-10, 20, -5), "wood");

        worldData.ModifiedBlocks.Should().HaveCount(3);
        worldData.ModifiedBlocks[new Vector3D<int>(0, 0, 0)].Should().Be("dirt");
        worldData.ModifiedBlocks[new Vector3D<int>(5, 10, 15)].Should().Be("stone");
        worldData.ModifiedBlocks[new Vector3D<int>(-10, 20, -5)].Should().Be("wood");
    }

    [Test]
    public void WorldData_SerializeDeserialize_EmptyWorld_RoundTrips()
    {
        var original = new WorldData("EmptyWorld");

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        deserialized.Name.Should().Be(original.Name);
        deserialized.ModifiedBlocks.Should().BeEmpty();
    }

    [Test]
    public void WorldData_SerializeDeserialize_WithPlayerPosition_RoundTrips()
    {
        var original = new WorldData("TestWorld")
        {
            PlayerPosition = new Vector3(100.5f, 64.0f, -50.25f),
            CameraPitch = 0.5f,
            CameraYaw = 1.2f
        };

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        deserialized.PlayerPosition.Should().Be(original.PlayerPosition);
        deserialized.CameraPitch.Should().Be(original.CameraPitch);
        deserialized.CameraYaw.Should().Be(original.CameraYaw);
    }

    [Test]
    public void WorldData_SerializeDeserialize_WithModifiedBlocks_RoundTrips()
    {
        var original = new WorldData("TestWorld");
        original.AddModifiedBlock(new Vector3D<int>(10, 20, 30), "stone");
        original.AddModifiedBlock(new Vector3D<int>(-5, 64, 100), "dirt");
        original.AddModifiedBlock(new Vector3D<int>(0, 0, 0), "wood");

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        deserialized.ModifiedBlocks.Should().HaveCount(3);
        deserialized.ModifiedBlocks[new Vector3D<int>(10, 20, 30)].Should().Be("stone");
        deserialized.ModifiedBlocks[new Vector3D<int>(-5, 64, 100)].Should().Be("dirt");
        deserialized.ModifiedBlocks[new Vector3D<int>(0, 0, 0)].Should().Be("wood");
    }

    [Test]
    public void WorldData_SerializeDeserialize_WithInventory_RoundTrips()
    {
        var original = new WorldData("TestWorld");
        original.Inventory.Hotbar.AddItemToSlot(0, "wood", 10);
        original.Inventory.Hotbar.AddItemToSlot(3, "stone", 25);
        original.Inventory.Storage.AddItemToSlot(5, "dirt", 64);
        original.Inventory.SelectedHotbarSlot = 3;

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        deserialized.Inventory.Hotbar.GetSlot(0)!.ItemId.Should().Be("wood");
        deserialized.Inventory.Hotbar.GetSlot(0)!.Count.Should().Be(10);
        deserialized.Inventory.Hotbar.GetSlot(3)!.ItemId.Should().Be("stone");
        deserialized.Inventory.Hotbar.GetSlot(3)!.Count.Should().Be(25);
        deserialized.Inventory.Storage.GetSlot(5)!.ItemId.Should().Be("dirt");
        deserialized.Inventory.Storage.GetSlot(5)!.Count.Should().Be(64);
        deserialized.Inventory.SelectedHotbarSlot.Should().Be(3);
    }

    [Test]
    public void WorldData_SerializeDeserialize_CompleteWorld_RoundTrips()
    {
        var original = new WorldData("CompleteWorld")
        {
            PlayerPosition = new Vector3(123.45f, 67.89f, -98.76f),
            CameraPitch = 0.75f,
            CameraYaw = 2.5f
        };

        original.AddModifiedBlock(new Vector3D<int>(10, 20, 30), "stone");
        original.AddModifiedBlock(new Vector3D<int>(-100, 128, 200), "diamond");
        original.Inventory.Hotbar.AddItemToSlot(0, "pickaxe", 1);
        original.Inventory.Hotbar.AddItemToSlot(8, "torch", 64);
        original.Inventory.Storage.AddItemToSlot(0, "coal", 32);
        original.Inventory.SelectedHotbarSlot = 0;

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        // Verify all properties
        deserialized.Name.Should().Be("CompleteWorld");
        deserialized.PlayerPosition.Should().Be(original.PlayerPosition);
        deserialized.CameraPitch.Should().Be(0.75f);
        deserialized.CameraYaw.Should().Be(2.5f);
        deserialized.ModifiedBlocks.Should().HaveCount(2);
        deserialized.ModifiedBlocks[new Vector3D<int>(10, 20, 30)].Should().Be("stone");
        deserialized.ModifiedBlocks[new Vector3D<int>(-100, 128, 200)].Should().Be("diamond");
        deserialized.Inventory.Hotbar.GetSlot(0)!.ItemId.Should().Be("pickaxe");
        deserialized.Inventory.Hotbar.GetSlot(8)!.ItemId.Should().Be("torch");
        deserialized.Inventory.Storage.GetSlot(0)!.ItemId.Should().Be("coal");
        deserialized.Inventory.SelectedHotbarSlot.Should().Be(0);
    }

    [Test]
    public void WorldData_SerializeDeserialize_NegativeCoordinates_RoundTrips()
    {
        var original = new WorldData("NegativeTest");
        original.AddModifiedBlock(new Vector3D<int>(-100, -50, -200), "bedrock");
        original.PlayerPosition = new Vector3(-25.5f, -10.0f, -75.25f);

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        deserialized.ModifiedBlocks[new Vector3D<int>(-100, -50, -200)].Should().Be("bedrock");
        deserialized.PlayerPosition.Should().Be(original.PlayerPosition);
    }

    [Test]
    public void WorldData_SerializeDeserialize_LargeBlockCoordinates_RoundTrips()
    {
        var original = new WorldData("LargeCoords");
        original.AddModifiedBlock(new Vector3D<int>(10000, 255, 10000), "sky_block");
        original.AddModifiedBlock(new Vector3D<int>(-10000, 0, -10000), "void_block");

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        deserialized.ModifiedBlocks[new Vector3D<int>(10000, 255, 10000)].Should().Be("sky_block");
        deserialized.ModifiedBlocks[new Vector3D<int>(-10000, 0, -10000)].Should().Be("void_block");
    }

    [Test]
    public void WorldData_SerializeDeserialize_FullInventory_RoundTrips()
    {
        var original = new WorldData("FullInv");

        // Fill hotbar
        for (int i = 0; i < 9; i++)
        {
            original.Inventory.Hotbar.AddItemToSlot(i, $"item_{i}", i * 5);
        }

        // Fill storage
        for (int i = 0; i < 27; i++)
        {
            original.Inventory.Storage.AddItemToSlot(i, $"storage_{i}", i * 2);
        }

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        // Verify hotbar
        for (int i = 0; i < 9; i++)
        {
            var slot = deserialized.Inventory.Hotbar.GetSlot(i);
            if (i * 5 > 0)
            {
                slot!.ItemId.Should().Be($"item_{i}");
                slot.Count.Should().Be(i * 5);
            }
        }

        // Verify storage
        for (int i = 0; i < 27; i++)
        {
            var slot = deserialized.Inventory.Storage.GetSlot(i);
            if (i * 2 > 0)
            {
                slot!.ItemId.Should().Be($"storage_{i}");
                slot.Count.Should().Be(i * 2);
            }
        }
    }

    [Test]
    public void WorldData_Serialize_ProducesValidJson()
    {
        var worldData = new WorldData("JsonTest")
        {
            PlayerPosition = new Vector3(1, 2, 3)
        };

        var json = worldData.Serialize();

        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("JsonTest");
        json.Should().Contain("PlayerPosition");
    }

    [Test]
    public void WorldData_SerializeDeserialize_EmptyInventorySlots_RoundTrips()
    {
        var original = new WorldData("SparseInv");
        // Add items with gaps
        original.Inventory.Hotbar.AddItemToSlot(0, "wood", 10);
        original.Inventory.Hotbar.AddItemToSlot(8, "stone", 5);
        original.Inventory.Storage.AddItemToSlot(13, "dirt", 20);

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        deserialized.Inventory.Hotbar.GetSlot(0)!.ItemId.Should().Be("wood");
        deserialized.Inventory.Hotbar.GetSlot(1).Should().BeNull();
        deserialized.Inventory.Hotbar.GetSlot(8)!.ItemId.Should().Be("stone");
        deserialized.Inventory.Storage.GetSlot(13)!.ItemId.Should().Be("dirt");
        deserialized.Inventory.Storage.GetSlot(0).Should().BeNull();
    }

    [Test]
    public void WorldData_SerializeDeserialize_ZeroPlayerPosition_RoundTrips()
    {
        var original = new WorldData("ZeroPos")
        {
            PlayerPosition = new Vector3(0, 0, 0),
            CameraPitch = 0,
            CameraYaw = 0
        };

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        deserialized.PlayerPosition.Should().Be(Vector3.Zero);
        deserialized.CameraPitch.Should().Be(0);
        deserialized.CameraYaw.Should().Be(0);
    }

    [Test]
    public void WorldData_SerializeDeserialize_ManyModifiedBlocks_RoundTrips()
    {
        var original = new WorldData("ManyBlocks");

        // Add 100 modified blocks
        for (int i = 0; i < 100; i++)
        {
            original.AddModifiedBlock(new Vector3D<int>(i, i * 2, i * 3), $"block_{i}");
        }

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        deserialized.ModifiedBlocks.Should().HaveCount(100);
        for (int i = 0; i < 100; i++)
        {
            deserialized.ModifiedBlocks[new Vector3D<int>(i, i * 2, i * 3)].Should().Be($"block_{i}");
        }
    }

    [Test]
    public void WorldData_SerializeDeserialize_SpecialCharactersInBlockId_RoundTrips()
    {
        var original = new WorldData("SpecialChars");
        original.AddModifiedBlock(new Vector3D<int>(0, 0, 0), "block:with:colons");
        original.AddModifiedBlock(new Vector3D<int>(1, 1, 1), "block_with_underscores");
        original.AddModifiedBlock(new Vector3D<int>(2, 2, 2), "block-with-dashes");

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        deserialized.ModifiedBlocks[new Vector3D<int>(0, 0, 0)].Should().Be("block:with:colons");
        deserialized.ModifiedBlocks[new Vector3D<int>(1, 1, 1)].Should().Be("block_with_underscores");
        deserialized.ModifiedBlocks[new Vector3D<int>(2, 2, 2)].Should().Be("block-with-dashes");
    }

    [Test]
    public void WorldData_SerializeDeserialize_ExtremeFloatValues_RoundTrips()
    {
        var original = new WorldData("ExtremeFloats")
        {
            PlayerPosition = new Vector3(float.MaxValue / 2, float.MinValue / 2, 0),
            CameraPitch = float.MaxValue / 2,
            CameraYaw = float.MinValue / 2
        };

        var serialized = original.Serialize();
        var deserialized = WorldData.Deserialize(serialized);

        deserialized.PlayerPosition.X.Should().BeApproximately(float.MaxValue / 2, 1e30f);
        deserialized.PlayerPosition.Y.Should().BeApproximately(float.MinValue / 2, 1e30f);
        deserialized.CameraPitch.Should().BeApproximately(float.MaxValue / 2, 1e30f);
        deserialized.CameraYaw.Should().BeApproximately(float.MinValue / 2, 1e30f);
    }

    [Test]
    public void WorldStorage_Initialize_CreatesDirectory()
    {
        // This test verifies that Initialize() can be called without errors
        // It creates the directory if it doesn't exist
        var action = () => WorldStorage.Initialize();

        action.Should().NotThrow();
    }

    [Test]
    public void WorldStorage_StoreAndLoad_EmptyWorld_RoundTrips()
    {
        WorldStorage.Initialize();
        var testWorldName = $"TestWorld_{Guid.NewGuid()}";
        var original = new WorldData(testWorldName);

        try
        {
            WorldStorage.StoreWorld(original);
            var loaded = WorldStorage.LoadFromName(testWorldName);

            loaded.Name.Should().Be(original.Name);
        }
        finally
        {
            // Cleanup - note: WorldStorage doesn't provide a delete method,
            // so we'll just verify the world exists
            WorldStorage.DoesWorldExist(testWorldName).Should().BeTrue();
        }
    }

    [Test]
    public void WorldStorage_StoreAndLoad_WithData_RoundTrips()
    {
        WorldStorage.Initialize();
        var testWorldName = $"TestWorldData_{Guid.NewGuid()}";
        var original = new WorldData(testWorldName)
        {
            PlayerPosition = new Vector3(10, 20, 30),
            CameraPitch = 0.5f,
            CameraYaw = 1.0f
        };
        original.AddModifiedBlock(new Vector3D<int>(1, 2, 3), "stone");
        original.Inventory.Hotbar.AddItemToSlot(0, "wood", 10);

        try
        {
            WorldStorage.StoreWorld(original);
            var loaded = WorldStorage.LoadFromName(testWorldName);

            loaded.Name.Should().Be(original.Name);
            loaded.PlayerPosition.Should().Be(original.PlayerPosition);
            loaded.CameraPitch.Should().Be(original.CameraPitch);
            loaded.CameraYaw.Should().Be(original.CameraYaw);
            loaded.ModifiedBlocks[new Vector3D<int>(1, 2, 3)].Should().Be("stone");
            loaded.Inventory.Hotbar.GetSlot(0)!.ItemId.Should().Be("wood");
            loaded.Inventory.Hotbar.GetSlot(0)!.Count.Should().Be(10);
        }
        finally
        {
            WorldStorage.DoesWorldExist(testWorldName).Should().BeTrue();
        }
    }

    [Test]
    public void WorldStorage_DoesWorldExist_NonExistentWorld_ReturnsFalse()
    {
        WorldStorage.Initialize();
        var nonExistentWorld = $"NonExistent_{Guid.NewGuid()}";

        var exists = WorldStorage.DoesWorldExist(nonExistentWorld);

        exists.Should().BeFalse();
    }

    [Test]
    public void WorldStorage_DoesWorldExist_ExistingWorld_ReturnsTrue()
    {
        WorldStorage.Initialize();
        var testWorldName = $"ExistingWorld_{Guid.NewGuid()}";
        var worldData = new WorldData(testWorldName);
        WorldStorage.StoreWorld(worldData);

        var exists = WorldStorage.DoesWorldExist(testWorldName);

        exists.Should().BeTrue();
    }

    [Test]
    public void WorldStorage_LoadFromName_NonExistentWorld_ThrowsException()
    {
        WorldStorage.Initialize();
        var nonExistentWorld = $"NonExistent_{Guid.NewGuid()}";

        var action = () => WorldStorage.LoadFromName(nonExistentWorld);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage($"Could not find file for world {nonExistentWorld}");
    }

    [Test]
    public void WorldStorage_StoreWorld_OverwritesExisting()
    {
        WorldStorage.Initialize();
        var testWorldName = $"OverwriteTest_{Guid.NewGuid()}";

        // Store first version
        var original = new WorldData(testWorldName)
        {
            PlayerPosition = new Vector3(10, 20, 30)
        };
        WorldStorage.StoreWorld(original);

        // Store second version with different data
        var updated = new WorldData(testWorldName)
        {
            PlayerPosition = new Vector3(100, 200, 300)
        };
        WorldStorage.StoreWorld(updated);

        // Load and verify it has the updated data
        var loaded = WorldStorage.LoadFromName(testWorldName);
        loaded.PlayerPosition.Should().Be(new Vector3(100, 200, 300));
    }

    [Test]
    public void WorldStorage_StoreAndLoad_ComplexWorld_RoundTrips()
    {
        WorldStorage.Initialize();
        var testWorldName = $"ComplexWorld_{Guid.NewGuid()}";
        var original = new WorldData(testWorldName)
        {
            PlayerPosition = new Vector3(123.45f, 67.89f, -12.34f),
            CameraPitch = 1.57f,
            CameraYaw = 3.14f
        };

        // Add multiple modified blocks
        for (int i = 0; i < 10; i++)
        {
            original.AddModifiedBlock(new Vector3D<int>(i, i * 10, i * 20), $"block_{i}");
        }

        // Add inventory items
        for (int i = 0; i < 9; i++)
        {
            original.Inventory.Hotbar.AddItemToSlot(i, $"hotbar_{i}", i + 1);
        }
        original.Inventory.SelectedHotbarSlot = 5;

        try
        {
            WorldStorage.StoreWorld(original);
            var loaded = WorldStorage.LoadFromName(testWorldName);

            loaded.Name.Should().Be(original.Name);
            loaded.PlayerPosition.Should().Be(original.PlayerPosition);
            loaded.CameraPitch.Should().Be(original.CameraPitch);
            loaded.CameraYaw.Should().Be(original.CameraYaw);
            loaded.ModifiedBlocks.Should().HaveCount(10);
            for (int i = 0; i < 10; i++)
            {
                loaded.ModifiedBlocks[new Vector3D<int>(i, i * 10, i * 20)].Should().Be($"block_{i}");
            }
            loaded.Inventory.SelectedHotbarSlot.Should().Be(5);
        }
        finally
        {
            WorldStorage.DoesWorldExist(testWorldName).Should().BeTrue();
        }
    }
}
