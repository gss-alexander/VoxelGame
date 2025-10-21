using Client.Inputs;
using Client.Items;

namespace Client;

public class PlayerInventory
{
    public event Action OnSelectedHotbarSlotChanged;
    public ItemStorage.Slot? CurrentHeldSlot => Hotbar.GetSlot(SelectedHotbarSlot);

    public int SelectedHotbarSlot
    {
        get => _selectedHotbarSlot;
        set
        {
            _selectedHotbarSlot = value;
            OnSelectedHotbarSlotChanged?.Invoke();
        }
    }
    public ItemStorage Hotbar { get; }
    public ItemStorage Storage { get; }

    private int _selectedHotbarSlot;

    public PlayerInventory()
    {
        Hotbar = new ItemStorage(9, 64);
        Storage = new ItemStorage(27, 64);
    }

    public static void Copy(PlayerInventory source, PlayerInventory destination)
    {
        CopyStorage(source.Hotbar, destination.Hotbar);
        CopyStorage(source.Storage, destination.Storage);
        destination.SelectedHotbarSlot = source.SelectedHotbarSlot;
    }

    private static void CopyStorage(ItemStorage source, ItemStorage destination)
    {
        for (var slotIndex = 0; slotIndex < source.SlotCount; slotIndex++)
        {
            var sourceSlot = source.GetSlot(slotIndex);
            if (sourceSlot != null)
            {
                destination.AddItemToSlot(slotIndex, sourceSlot.ItemId, sourceSlot.Count);
            }
        }
    }

    // Tries to add an item to first the hotbar, then the storage if there is no capacity in the hotbar.
    public bool TryAddItem(string itemId, int count)
    {
        Console.WriteLine($"[PlayerInventory]: Trying to add {count} of item {itemId} to inventory");
        if (Hotbar.CanAdd(itemId, count))
        {
            Hotbar.AddItem(itemId, count);
            Console.WriteLine($"[PlayerInventory]: Added {count} of item {itemId} to hotbar");
            return true;
        }
        
        if (Storage.CanAdd(itemId, count))
        {
            Storage.AddItem(itemId, count);
            Console.WriteLine($"[PlayerInventory]: Added {count} of item {itemId} to storage");
            return true;
        }

        Console.WriteLine($"[PlayerInventory]: Failed to add {count} of item {itemId} to inventory");
        return false;
    }

    public void UpdateKeyHotbarSelection(ActionContext actionContext)
    {
        if (actionContext.IsPressed(InputAction.HotbarSelect1))
        {
            SelectedHotbarSlot = 0;
        }
        else if (actionContext.IsPressed(InputAction.HotbarSelect2))
        {
            SelectedHotbarSlot = 1;
        }
        else if (actionContext.IsPressed(InputAction.HotbarSelect3))
        {
            SelectedHotbarSlot = 2;
        }
        else if (actionContext.IsPressed(InputAction.HotbarSelect4))
        {
            SelectedHotbarSlot = 3;
        }
        else if (actionContext.IsPressed(InputAction.HotbarSelect5))
        {
            SelectedHotbarSlot = 4;
        }
        else if (actionContext.IsPressed(InputAction.HotbarSelect6))
        {
            SelectedHotbarSlot = 5;
        }
        else if (actionContext.IsPressed(InputAction.HotbarSelect7))
        {
            SelectedHotbarSlot = 6;
        }
        else if (actionContext.IsPressed(InputAction.HotbarSelect8))
        {
            SelectedHotbarSlot = 7;
        }
        else if (actionContext.IsPressed(InputAction.HotbarSelect9))
        {
            SelectedHotbarSlot = 8;
        }
    }

    public void CycleSelectedHotbarSlot(int direction)
    {
        SelectedHotbarSlot += direction;
        if (SelectedHotbarSlot > Hotbar.SlotCount - 1)
        {
            SelectedHotbarSlot = 0;
        }
        else if (SelectedHotbarSlot < 0)
        {
            SelectedHotbarSlot = Hotbar.SlotCount - 1;
        }
    }
}