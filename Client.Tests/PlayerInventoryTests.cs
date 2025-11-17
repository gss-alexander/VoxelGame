using Client;
using FluentAssertions;

namespace Client.Tests;

public class PlayerInventoryTests
{
    [Test]
    public void Constructor_InitializesHotbarWith9Slots()
    {
        var inventory = new PlayerInventory();

        inventory.Hotbar.SlotCount.Should().Be(9);
    }

    [Test]
    public void Constructor_InitializesStorageWith27Slots()
    {
        var inventory = new PlayerInventory();

        inventory.Storage.SlotCount.Should().Be(27);
    }

    [Test]
    public void Constructor_InitializesSelectedHotbarSlotToZero()
    {
        var inventory = new PlayerInventory();

        inventory.SelectedHotbarSlot.Should().Be(0);
    }

    [Test]
    public void SelectedHotbarSlot_SetValue_UpdatesValue()
    {
        var inventory = new PlayerInventory();

        inventory.SelectedHotbarSlot = 5;

        inventory.SelectedHotbarSlot.Should().Be(5);
    }

    [Test]
    public void SelectedHotbarSlot_SetValue_FiresEvent()
    {
        var inventory = new PlayerInventory();
        var eventFired = false;
        inventory.OnSelectedHotbarSlotChanged += () => eventFired = true;

        inventory.SelectedHotbarSlot = 3;

        eventFired.Should().BeTrue();
    }

    [Test]
    public void CurrentHeldSlot_NoItem_ReturnsNull()
    {
        var inventory = new PlayerInventory();

        inventory.CurrentHeldSlot.Should().BeNull();
    }

    [Test]
    public void CurrentHeldSlot_WithItem_ReturnsSlot()
    {
        var inventory = new PlayerInventory();
        inventory.Hotbar.AddItem("wood", 10);
        inventory.SelectedHotbarSlot = 0;

        var slot = inventory.CurrentHeldSlot;

        slot.Should().NotBeNull();
        slot!.ItemId.Should().Be("wood");
        slot.Count.Should().Be(10);
    }

    [Test]
    public void CurrentHeldSlot_ChangesWithSelectedSlot()
    {
        var inventory = new PlayerInventory();
        inventory.Hotbar.AddItemToSlot(0, "wood", 10);
        inventory.Hotbar.AddItemToSlot(3, "stone", 5);

        inventory.SelectedHotbarSlot = 0;
        inventory.CurrentHeldSlot!.ItemId.Should().Be("wood");

        inventory.SelectedHotbarSlot = 3;
        inventory.CurrentHeldSlot!.ItemId.Should().Be("stone");
    }

    [Test]
    public void TryAddItem_EmptyInventory_AddsToHotbar()
    {
        var inventory = new PlayerInventory();

        var result = inventory.TryAddItem("wood", 10);

        result.Should().BeTrue();
        inventory.Hotbar.GetSlot(0)!.ItemId.Should().Be("wood");
        inventory.Hotbar.GetSlot(0)!.Count.Should().Be(10);
        inventory.Storage.GetSlot(0).Should().BeNull();
    }

    [Test]
    public void TryAddItem_HotbarHasCapacity_AddsToHotbar()
    {
        var inventory = new PlayerInventory();
        inventory.Hotbar.AddItem("wood", 30);

        var result = inventory.TryAddItem("wood", 20);

        result.Should().BeTrue();
        inventory.Hotbar.GetSlot(0)!.Count.Should().Be(50);
    }

    [Test]
    public void TryAddItem_HotbarFull_AddsToStorage()
    {
        var inventory = new PlayerInventory();
        // Fill hotbar completely
        for (int i = 0; i < 9; i++)
        {
            inventory.Hotbar.AddItemToSlot(i, $"item{i}", 64);
        }

        var result = inventory.TryAddItem("wood", 10);

        result.Should().BeTrue();
        inventory.Storage.GetSlot(0)!.ItemId.Should().Be("wood");
        inventory.Storage.GetSlot(0)!.Count.Should().Be(10);
    }

    [Test]
    public void TryAddItem_BothFull_ReturnsFalse()
    {
        var inventory = new PlayerInventory();

        // Fill hotbar
        for (int i = 0; i < 9; i++)
        {
            inventory.Hotbar.AddItemToSlot(i, $"item{i}", 64);
        }

        // Fill storage
        for (int i = 0; i < 27; i++)
        {
            inventory.Storage.AddItemToSlot(i, $"storageItem{i}", 64);
        }

        var result = inventory.TryAddItem("wood", 10);

        result.Should().BeFalse();
    }

    [Test]
    public void TryAddItem_HotbarPartiallyFull_AddsToHotbar()
    {
        var inventory = new PlayerInventory();
        inventory.Hotbar.AddItemToSlot(0, "stone", 64);
        inventory.Hotbar.AddItemToSlot(1, "dirt", 64);

        var result = inventory.TryAddItem("wood", 10);

        result.Should().BeTrue();
        inventory.Hotbar.GetSlot(2)!.ItemId.Should().Be("wood");
    }

    [Test]
    public void TryAddItem_ItemInStorageNotHotbar_AddsToHotbar()
    {
        var inventory = new PlayerInventory();
        inventory.Storage.AddItem("wood", 30);

        var result = inventory.TryAddItem("wood", 10);

        result.Should().BeTrue();
        // Should add to hotbar, not merge with storage
        inventory.Hotbar.GetSlot(0)!.ItemId.Should().Be("wood");
        inventory.Hotbar.GetSlot(0)!.Count.Should().Be(10);
    }

    [Test]
    public void CycleSelectedHotbarSlot_ForwardFromZero_SelectsNext()
    {
        var inventory = new PlayerInventory();
        inventory.SelectedHotbarSlot = 0;

        inventory.CycleSelectedHotbarSlot(1);

        inventory.SelectedHotbarSlot.Should().Be(1);
    }

    [Test]
    public void CycleSelectedHotbarSlot_ForwardFromLast_WrapsToZero()
    {
        var inventory = new PlayerInventory();
        inventory.SelectedHotbarSlot = 8;

        inventory.CycleSelectedHotbarSlot(1);

        inventory.SelectedHotbarSlot.Should().Be(0);
    }

    [Test]
    public void CycleSelectedHotbarSlot_BackwardFromZero_WrapsToLast()
    {
        var inventory = new PlayerInventory();
        inventory.SelectedHotbarSlot = 0;

        inventory.CycleSelectedHotbarSlot(-1);

        inventory.SelectedHotbarSlot.Should().Be(8);
    }

    [Test]
    public void CycleSelectedHotbarSlot_BackwardFromMiddle_SelectsPrevious()
    {
        var inventory = new PlayerInventory();
        inventory.SelectedHotbarSlot = 5;

        inventory.CycleSelectedHotbarSlot(-1);

        inventory.SelectedHotbarSlot.Should().Be(4);
    }

    [Test]
    public void CycleSelectedHotbarSlot_MultipleSteps_WorksCorrectly()
    {
        var inventory = new PlayerInventory();
        inventory.SelectedHotbarSlot = 6;

        inventory.CycleSelectedHotbarSlot(3);

        inventory.SelectedHotbarSlot.Should().Be(0);
    }

    [Test]
    public void Copy_EmptyInventory_CopiesCorrectly()
    {
        var source = new PlayerInventory();
        var destination = new PlayerInventory();

        PlayerInventory.Copy(source, destination);

        destination.SelectedHotbarSlot.Should().Be(source.SelectedHotbarSlot);
    }

    [Test]
    public void Copy_WithItems_CopiesAllItems()
    {
        var source = new PlayerInventory();
        source.Hotbar.AddItemToSlot(0, "wood", 10);
        source.Hotbar.AddItemToSlot(3, "stone", 25);
        source.Storage.AddItemToSlot(5, "dirt", 40);
        source.SelectedHotbarSlot = 3;

        var destination = new PlayerInventory();
        PlayerInventory.Copy(source, destination);

        destination.Hotbar.GetSlot(0)!.ItemId.Should().Be("wood");
        destination.Hotbar.GetSlot(0)!.Count.Should().Be(10);
        destination.Hotbar.GetSlot(3)!.ItemId.Should().Be("stone");
        destination.Hotbar.GetSlot(3)!.Count.Should().Be(25);
        destination.Storage.GetSlot(5)!.ItemId.Should().Be("dirt");
        destination.Storage.GetSlot(5)!.Count.Should().Be(40);
        destination.SelectedHotbarSlot.Should().Be(3);
    }

    [Test]
    public void Copy_DoesNotAffectSourceWhenDestinationModified()
    {
        var source = new PlayerInventory();
        source.Hotbar.AddItemToSlot(0, "wood", 10);

        var destination = new PlayerInventory();
        PlayerInventory.Copy(source, destination);

        destination.Hotbar.RemoveItemFromSlot(0, 5);

        source.Hotbar.GetSlot(0)!.Count.Should().Be(10);
        destination.Hotbar.GetSlot(0)!.Count.Should().Be(5);
    }

    [Test]
    public void Copy_OverwritesDestinationData()
    {
        var source = new PlayerInventory();
        source.Hotbar.AddItemToSlot(0, "wood", 10);
        source.SelectedHotbarSlot = 2;

        var destination = new PlayerInventory();
        destination.Hotbar.AddItemToSlot(0, "stone", 30);
        destination.Hotbar.AddItemToSlot(1, "dirt", 40);
        destination.SelectedHotbarSlot = 5;

        PlayerInventory.Copy(source, destination);

        // Slot 0 should have both wood and stone since Copy adds to slots
        destination.Hotbar.GetSlot(0)!.ItemId.Should().Be("wood");
        destination.Hotbar.GetSlot(0)!.Count.Should().Be(10);
        destination.Hotbar.GetSlot(1)!.ItemId.Should().Be("dirt");
        destination.SelectedHotbarSlot.Should().Be(2);
    }

    [Test]
    public void Copy_WithFullInventory_CopiesAll()
    {
        var source = new PlayerInventory();

        // Fill hotbar
        for (int i = 0; i < 9; i++)
        {
            source.Hotbar.AddItemToSlot(i, $"hotbar{i}", i + 1);
        }

        // Fill storage
        for (int i = 0; i < 27; i++)
        {
            source.Storage.AddItemToSlot(i, $"storage{i}", i + 10);
        }

        source.SelectedHotbarSlot = 7;

        var destination = new PlayerInventory();
        PlayerInventory.Copy(source, destination);

        // Verify hotbar
        for (int i = 0; i < 9; i++)
        {
            destination.Hotbar.GetSlot(i)!.ItemId.Should().Be($"hotbar{i}");
            destination.Hotbar.GetSlot(i)!.Count.Should().Be(i + 1);
        }

        // Verify storage
        for (int i = 0; i < 27; i++)
        {
            destination.Storage.GetSlot(i)!.ItemId.Should().Be($"storage{i}");
            destination.Storage.GetSlot(i)!.Count.Should().Be(i + 10);
        }

        destination.SelectedHotbarSlot.Should().Be(7);
    }

    [Test]
    public void TryAddItem_MultipleAdditions_PrioritizesHotbar()
    {
        var inventory = new PlayerInventory();

        inventory.TryAddItem("wood", 10);
        inventory.TryAddItem("stone", 20);
        inventory.TryAddItem("dirt", 15);

        inventory.Hotbar.GetSlot(0)!.ItemId.Should().Be("wood");
        inventory.Hotbar.GetSlot(1)!.ItemId.Should().Be("stone");
        inventory.Hotbar.GetSlot(2)!.ItemId.Should().Be("dirt");
        inventory.Storage.GetSlot(0).Should().BeNull();
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(7)]
    [TestCase(8)]
    public void SelectedHotbarSlot_AllValidSlots_WorksCorrectly(int slotIndex)
    {
        var inventory = new PlayerInventory();
        inventory.Hotbar.AddItemToSlot(slotIndex, "wood", 10);

        inventory.SelectedHotbarSlot = slotIndex;

        inventory.CurrentHeldSlot!.ItemId.Should().Be("wood");
    }

    [Test]
    public void TryAddItem_WithStackMerging_MergesInHotbar()
    {
        var inventory = new PlayerInventory();
        inventory.Hotbar.AddItemToSlot(5, "wood", 32);

        var result = inventory.TryAddItem("wood", 16);

        result.Should().BeTrue();
        inventory.Hotbar.GetSlot(5)!.Count.Should().Be(48);
    }
}
