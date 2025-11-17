using Client.Items;
using FluentAssertions;

namespace Client.Tests;

public class ItemStorageTests
{
    [Test]
    public void Constructor_InitializesCorrectNumberOfSlots()
    {
        var storage = new ItemStorage(5, 64);

        storage.SlotCount.Should().Be(5);
    }

    [Test]
    public void Constructor_InitializesAllSlotsAsEmpty()
    {
        var storage = new ItemStorage(3, 64);

        for (int i = 0; i < storage.SlotCount; i++)
        {
            var slot = storage.GetSlot(i);
            slot.Should().BeNull();
        }
    }

    [Test]
    public void GetSlot_EmptySlot_ReturnsNull()
    {
        var storage = new ItemStorage(5, 64);

        var slot = storage.GetSlot(0);

        slot.Should().BeNull();
    }

    [Test]
    public void GetSlot_NonEmptySlot_ReturnsSlot()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 5);

        var slot = storage.GetSlot(0);

        slot.Should().NotBeNull();
        slot!.ItemId.Should().Be("wood");
        slot.Count.Should().Be(5);
    }

    [Test]
    public void GetSlotInternal_AlwaysReturnsSlot()
    {
        var storage = new ItemStorage(5, 64);

        var slot = storage.GetSlotInternal(0);

        slot.Should().NotBeNull();
        slot.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void HasItem_ItemExists_ReturnsTrue()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("stone", 10);

        var hasItem = storage.HasItem("stone");

        hasItem.Should().BeTrue();
    }

    [Test]
    public void HasItem_ItemDoesNotExist_ReturnsFalse()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("stone", 10);

        var hasItem = storage.HasItem("wood");

        hasItem.Should().BeFalse();
    }

    [Test]
    public void HasItem_EmptyStorage_ReturnsFalse()
    {
        var storage = new ItemStorage(5, 64);

        var hasItem = storage.HasItem("stone");

        hasItem.Should().BeFalse();
    }

    [Test]
    public void AddItem_ToEmptyStorage_AddsItemToFirstSlot()
    {
        var storage = new ItemStorage(5, 64);

        storage.AddItem("wood", 10);

        var slot = storage.GetSlot(0);
        slot.Should().NotBeNull();
        slot!.ItemId.Should().Be("wood");
        slot.Count.Should().Be(10);
    }

    [Test]
    public void AddItem_SameItemWithCapacity_AddsToExistingSlot()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 10);

        storage.AddItem("wood", 5);

        var slot = storage.GetSlot(0);
        slot!.Count.Should().Be(15);
    }

    [Test]
    public void AddItem_SameItemExceedsCapacity_UsesNewSlot()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 60);

        storage.AddItem("wood", 10);

        var slot0 = storage.GetSlot(0);
        var slot1 = storage.GetSlot(1);
        slot0!.Count.Should().Be(60);
        slot1!.ItemId.Should().Be("wood");
        slot1.Count.Should().Be(10);
    }

    [Test]
    public void AddItem_DifferentItems_UsesNextEmptySlot()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 10);

        storage.AddItem("stone", 5);

        var slot0 = storage.GetSlot(0);
        var slot1 = storage.GetSlot(1);
        slot0!.ItemId.Should().Be("wood");
        slot1!.ItemId.Should().Be("stone");
    }

    [Test]
    public void AddItem_ZeroCount_ThrowsException()
    {
        var storage = new ItemStorage(5, 64);

        var action = () => storage.AddItem("wood", 0);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add 0 or negative amount to storage");
    }

    [Test]
    public void AddItem_NegativeCount_ThrowsException()
    {
        var storage = new ItemStorage(5, 64);

        var action = () => storage.AddItem("wood", -5);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add 0 or negative amount to storage");
    }

    [Test]
    public void AddItem_StorageFull_ThrowsException()
    {
        var storage = new ItemStorage(2, 64);
        storage.AddItem("wood", 64);
        storage.AddItem("stone", 64);

        var action = () => storage.AddItem("dirt", 1);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add items to storage as it exceeds capacity or has no slots");
    }

    [Test]
    public void AddItemToSlot_EmptySlot_AddsItem()
    {
        var storage = new ItemStorage(5, 64);

        storage.AddItemToSlot(2, "wood", 10);

        var slot = storage.GetSlot(2);
        slot!.ItemId.Should().Be("wood");
        slot.Count.Should().Be(10);
    }

    [Test]
    public void AddItemToSlot_SameItem_IncreasesCount()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItemToSlot(1, "wood", 10);

        storage.AddItemToSlot(1, "wood", 5);

        var slot = storage.GetSlot(1);
        slot!.Count.Should().Be(15);
    }

    [Test]
    public void AddItemToSlot_DifferentItem_ReplacesSlot()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItemToSlot(1, "wood", 10);

        storage.AddItemToSlot(1, "stone", 5);

        var slot = storage.GetSlot(1);
        slot!.ItemId.Should().Be("stone");
        slot.Count.Should().Be(5);
    }

    [Test]
    public void AddItemToSlot_ZeroCount_ThrowsException()
    {
        var storage = new ItemStorage(5, 64);

        var action = () => storage.AddItemToSlot(0, "wood", 0);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add 0 or negative amount to storage");
    }

    [Test]
    public void RemoveItemFromSlot_PartialRemoval_DecreasesCount()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 10);

        storage.RemoveItemFromSlot(0, 5);

        var slot = storage.GetSlot(0);
        slot!.Count.Should().Be(5);
        slot.ItemId.Should().Be("wood");
    }

    [Test]
    public void RemoveItemFromSlot_CompleteRemoval_EmptiesSlot()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 10);

        storage.RemoveItemFromSlot(0, 10);

        var slot = storage.GetSlot(0);
        slot.Should().BeNull();
    }

    [Test]
    public void RemoveItemFromSlot_EmptySlot_ThrowsException()
    {
        var storage = new ItemStorage(5, 64);

        var action = () => storage.RemoveItemFromSlot(0, 5);

        action.Should().Throw<Exception>()
            .WithMessage("Tried to get slot outside of bounds: 0");
    }

    [Test]
    public void RemoveItemFromSlot_MoreThanAvailable_ThrowsException()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 5);

        var action = () => storage.RemoveItemFromSlot(0, 10);

        action.Should().Throw<Exception>()
            .WithMessage("Tried to remove more items from slot than exists in slot.");
    }

    [Test]
    public void RemoveItemFromSlot_NegativeAmount_ThrowsException()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 10);

        var action = () => storage.RemoveItemFromSlot(0, -5);

        action.Should().Throw<Exception>()
            .WithMessage("Cannot remove negative amount of items");
    }

    [Test]
    public void RemoveItemFromSlot_ZeroAmount_ThrowsException()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 10);

        var action = () => storage.RemoveItemFromSlot(0, 0);

        action.Should().Throw<Exception>()
            .WithMessage("Cannot remove negative amount of items");
    }

    [Test]
    public void CanAdd_EmptyStorage_ReturnsTrue()
    {
        var storage = new ItemStorage(5, 64);

        var canAdd = storage.CanAdd("wood", 10);

        canAdd.Should().BeTrue();
    }

    [Test]
    public void CanAdd_ExistingItemWithCapacity_ReturnsTrue()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 30);

        var canAdd = storage.CanAdd("wood", 20);

        canAdd.Should().BeTrue();
    }

    [Test]
    public void CanAdd_ExistingItemExceedsCapacity_EmptySlotAvailable_ReturnsTrue()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 60);

        var canAdd = storage.CanAdd("wood", 10);

        canAdd.Should().BeTrue();
    }

    [Test]
    public void CanAdd_StorageFull_ReturnsFalse()
    {
        var storage = new ItemStorage(2, 64);
        storage.AddItem("wood", 64);
        storage.AddItem("stone", 64);

        var canAdd = storage.CanAdd("dirt", 1);

        canAdd.Should().BeFalse();
    }

    [Test]
    public void CanAdd_ExceedsSlotCapacity_NoEmptySlots_ReturnsFalse()
    {
        var storage = new ItemStorage(2, 64);
        storage.AddItem("wood", 60);
        storage.AddItem("stone", 64);

        var canAdd = storage.CanAdd("wood", 10);

        canAdd.Should().BeFalse();
    }

    [Test]
    public void OnChanged_AddItem_FiresEvent()
    {
        var storage = new ItemStorage(5, 64);
        var eventFired = false;
        storage.OnChanged += () => eventFired = true;

        storage.AddItem("wood", 10);

        eventFired.Should().BeTrue();
    }

    [Test]
    public void OnChanged_RemoveItem_FiresEvent()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 10);
        var eventFired = false;
        storage.OnChanged += () => eventFired = true;

        storage.RemoveItemFromSlot(0, 5);

        eventFired.Should().BeTrue();
    }

    [Test]
    public void OnChanged_AddItemToSlot_FiresEvent()
    {
        var storage = new ItemStorage(5, 64);
        var eventFired = false;
        storage.OnChanged += () => eventFired = true;

        storage.AddItemToSlot(0, "wood", 10);

        eventFired.Should().BeTrue();
    }

    [Test]
    public void Slot_OnChanged_ModifyingItemId_FiresEvent()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 10);
        var slot = storage.GetSlot(0);
        var eventFired = false;
        slot!.OnChanged += () => eventFired = true;

        slot.ItemId = "stone";

        eventFired.Should().BeTrue();
    }

    [Test]
    public void Slot_OnChanged_ModifyingCount_FiresEvent()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 10);
        var slot = storage.GetSlot(0);
        var eventFired = false;
        slot!.OnChanged += () => eventFired = true;

        slot.Count = 20;

        eventFired.Should().BeTrue();
    }

    [Test]
    public void Slot_IsEmpty_NullItem_ReturnsTrue()
    {
        var storage = new ItemStorage(5, 64);
        var slot = storage.GetSlotInternal(0);

        slot.IsEmpty.Should().BeTrue();
    }

    [Test]
    public void Slot_IsEmpty_WithItem_ReturnsFalse()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItem("wood", 10);
        var slot = storage.GetSlot(0);

        slot!.IsEmpty.Should().BeFalse();
    }

    [Test]
    public void AddItem_FillsFirstSlotWithMatchingItemBeforeUsingEmpty()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItemToSlot(2, "wood", 10);

        storage.AddItem("wood", 5);

        var slot0 = storage.GetSlot(0);
        var slot2 = storage.GetSlot(2);
        slot0.Should().BeNull();
        slot2!.Count.Should().Be(15);
    }

    [Test]
    public void AddItem_UsesFirstEmptySlot()
    {
        var storage = new ItemStorage(5, 64);
        storage.AddItemToSlot(1, "stone", 64);
        storage.AddItemToSlot(3, "dirt", 64);

        storage.AddItem("wood", 10);

        var slot0 = storage.GetSlot(0);
        slot0!.ItemId.Should().Be("wood");
    }

    [TestCase(1)]
    [TestCase(32)]
    [TestCase(64)]
    public void AddItem_VariousAmounts_WorksCorrectly(int amount)
    {
        var storage = new ItemStorage(5, 64);

        storage.AddItem("wood", amount);

        var slot = storage.GetSlot(0);
        slot!.Count.Should().Be(amount);
    }

    [Test]
    public void MultipleOperations_ComplexScenario_WorksCorrectly()
    {
        var storage = new ItemStorage(5, 64);

        // Add multiple items
        storage.AddItem("wood", 30);
        storage.AddItem("stone", 20);
        storage.AddItem("wood", 25);

        // Should have wood in slot 0 (30+25=55), stone in slot 1 (20)
        storage.GetSlot(0)!.ItemId.Should().Be("wood");
        storage.GetSlot(0)!.Count.Should().Be(55);
        storage.GetSlot(1)!.ItemId.Should().Be("stone");
        storage.GetSlot(1)!.Count.Should().Be(20);

        // Remove some wood
        storage.RemoveItemFromSlot(0, 15);
        storage.GetSlot(0)!.Count.Should().Be(40);

        // Add more wood - should go to slot 0
        storage.AddItem("wood", 20);
        storage.GetSlot(0)!.Count.Should().Be(60);

        // Add more wood - should go to slot 2 (slot 0 is now 60)
        storage.AddItem("wood", 10);
        storage.GetSlot(2)!.ItemId.Should().Be("wood");
        storage.GetSlot(2)!.Count.Should().Be(10);
    }
}
