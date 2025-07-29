namespace Client.Items;
public abstract class ItemData
{
    public string ExternalId { get; set; }
    public string DisplayName { get; set; }
    public string Texture { get; set; }
}

public class MaterialItemData : ItemData
{
}

public class ToolItemData : ItemData
{
    public int Durability { get; set; }
    public int MiningLevel { get; set; }
    public float MiningSpeed { get; set; }
}

public class FuelItemData : ItemData
{
    public int BurnTime { get; set; }
}

public class FoodItemData : ItemData
{
    public int HungerRestore { get; set; }
}

public class BlockItemData : ItemData
{
}