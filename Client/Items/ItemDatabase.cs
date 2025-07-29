using Client.Blocks;

namespace Client.Items;

public class ItemDatabase
{
    private readonly Dictionary<string, ItemData> _items = new();
    
    public ItemDatabase(ItemData[] items)
    {
        foreach (var item in items)
        {
            _items.Add(item.ExternalId, item);
        }
    }

    public void RegisterBlockItems(BlockData[] blocks)
    {
        foreach (var block in blocks)
        {
            var blockItemData = new BlockItemData()
            {
                DisplayName = block.DisplayName,
                ExternalId = $"block:{block.ExternalId}",
                Texture = "missing.png" // todo: needs generated block sprite texture
            };
            _items.Add(blockItemData.ExternalId, blockItemData);
        }
    }

    public T Get<T>(string itemId) where T : ItemData
    {
        return (T)_items[itemId];
    }

    public ItemData Get(string itemId)
    {
        return _items[itemId];
    }
}