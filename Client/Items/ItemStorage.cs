namespace Client.Items;

public class ItemStorage
{
    public event Action OnChanged;
    public int SlotCount => _slots.Length;
    
    private readonly int _slotCapacity;

    public class Slot
    {
        public bool IsEmpty => ItemId == "null";
        public event Action OnChanged;
        
        public string ItemId
        {
            get => _itemId;
            set
            {
                _itemId = value;
                OnChanged?.Invoke();
            }
        }

        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                OnChanged?.Invoke();
            }
        }

        private string _itemId = "null";
        private int _count;
    }

    private readonly Slot[] _slots;

    public ItemStorage(int slotCount, int slotCapacity)
    {
        _slotCapacity = slotCapacity;
        _slots = new Slot[slotCount];
        for (var i = 0; i < slotCount; i++)
        {
            _slots[i] = new Slot();
            _slots[i].OnChanged += () => OnChanged?.Invoke();
        }
    }

    public Slot? GetSlot(int slotIndex)
    {
        var slot = GetSlotInternal(slotIndex);
        if (slot.ItemId == "null")
        {
            return null;
        }

        return slot;
    }

    public Slot GetSlotInternal(int slotIndex)
    {
        return _slots[slotIndex];
    }

    public bool HasItem(string itemId)
    {
        for (var i = 0; i < _slots.Length; i++)
        {
            var slot = _slots[i];
            if (slot.ItemId == itemId)
            {
                return true;
            }
        }

        return false;
    }

    public void AddItem(string itemId, int count)
    {
        if (count <= 0)
        {
            throw new InvalidOperationException($"Cannot add 0 or negative amount to storage");
        }

        var slot = GetSlotForAdding(itemId, count);
        if (slot == null)
        {
            throw new InvalidOperationException($"Cannot add items to storage as it exceeds capacity or has no slots");
        }

        if (slot.ItemId == "null")
        {
            slot.ItemId = itemId;
        }
        slot.Count += count;
        OnChanged?.Invoke();
    }

    public void AddItemToSlot(int slotIndex, string itemId, int count)
    {
        if (count <= 0)
        {
            throw new InvalidOperationException($"Cannot add 0 or negative amount to storage");
        }

        var slot = GetSlotInternal(slotIndex);
        if (slot.ItemId == itemId)
        {
            slot.Count += count;
        }
        else
        {
            slot.ItemId = itemId;
            slot.Count = count;
        }
        OnChanged?.Invoke();
    }

    public void RemoveItemFromSlot(int slotIndex, int amount)
    {
        if (amount <= 0)
        {
            throw new Exception($"Cannot remove negative amount of items");
        }

        var slot = GetSlot(slotIndex);
        if (slot == null)
        {
            throw new Exception($"Tried to get slot outside of bounds: {slotIndex}");
        }

        var currentItemsInSlot = slot.Count;
        if (currentItemsInSlot - amount < 0)
        {
            throw new Exception($"Tried to remove more items from slot than exists in slot.");
        }

        slot.Count -= amount;
        if (slot.Count == 0)
        {
            slot.ItemId = "null";
        }
        OnChanged?.Invoke();
    }

    public bool CanAdd(string itemId, int count)
    {
        return GetSlotForAdding(itemId, count) != null;
    }

    private Slot? GetSlotForAdding(string itemId, int count)
    {
        var existingSlotWithItemIndex = -1;
        var firstEmptySlotIndex = -1;
        for (var i = 0; i < _slots.Length; i++)
        {
            var slot = GetSlotInternal(i);
            
            // Store the first empty slot if found. If no other slot with item and capacity is found, use it.
            if (slot.ItemId == "null" && firstEmptySlotIndex == -1)
            {
                firstEmptySlotIndex = i;
                continue;
            }
            
            // Found another slot with the same item type
            if (slot.ItemId == itemId)
            {
                // Check that adding to it won't exceed capacity
                var newCount = slot.Count + count;
                if (newCount > _slotCapacity)
                {
                    continue;
                }

                // Use this slot for filling up
                existingSlotWithItemIndex = i;
                break;
            }
        }

        if (existingSlotWithItemIndex >= 0)
        {
            return _slots[existingSlotWithItemIndex];
        }

        if (firstEmptySlotIndex >= 0)
        {
            return _slots[firstEmptySlotIndex];
        }

        return null;
    }
}