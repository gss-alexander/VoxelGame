using Client.Blocks;

namespace Client;

public class Inventory
{
    public Dictionary<int, (BlockType blockType, int count)> Hotbar { get; } = new();

    public void AddBlock(BlockType block, int count = 1)
    {
        var lastSlot = 0;
        foreach (var hotbarSlot in Hotbar)
        {
            var (slotIndex, element) = hotbarSlot;
            if (element.blockType == block)
            {
                Hotbar[slotIndex] = (block, element.count + count);
                return; // stop here if already in inventory
            }

            lastSlot++;
        }
        
        Hotbar.Add(lastSlot, (block, count));
    }
}