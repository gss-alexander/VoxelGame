using Client.Items;

namespace Client;

public class PlayerInventory
{
    public ItemStorage.Slot? CurrentHeldSlot => Hotbar.GetSlot(SelectedHotbarSlot);
    
    public int SelectedHotbarSlot { get; set; }
    public ItemStorage Hotbar { get; }
    public ItemStorage Storage { get; }

    public PlayerInventory()
    {
        Hotbar = new ItemStorage(8, 64);
        Storage = new ItemStorage(32, 64);
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