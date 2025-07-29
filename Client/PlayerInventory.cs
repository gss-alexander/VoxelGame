using Client.Items;

namespace Client;

public class PlayerInventory
{
    public ItemStorage Hotbar { get; }
    public ItemStorage Storage { get; }

    public PlayerInventory()
    {
        Hotbar = new ItemStorage(9, 64);
        Storage = new ItemStorage(32, 64);
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
}