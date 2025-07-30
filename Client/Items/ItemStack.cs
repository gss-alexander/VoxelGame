namespace Client.Items;

public struct ItemStack
{
    public string ItemId { get; set; }
    public int Amount { get; set; }
    
    public ItemStack(string itemId, int amount)
    {
        ItemId = itemId;
        Amount = amount;
    }
}