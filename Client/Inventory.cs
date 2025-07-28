using Client.Blocks;

namespace Client;

public class Inventory
{
    public Dictionary<int, (int blockId, int count)> Hotbar { get; } = new();

    public void AddBlock(int blockId, int count = 1)
    {
        var lastSlot = 0;
        foreach (var hotbarSlot in Hotbar)
        {
            var (slotIndex, element) = hotbarSlot;
            if (element.blockId == blockId)
            {
                Hotbar[slotIndex] = (blockId, element.count + count);
                return; // stop here if already in inventory
            }

            lastSlot++;
        }
        
        Hotbar.Add(lastSlot, (blockId, count));
    }
}