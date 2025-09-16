using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Client.Items;

// Reads items from YAML file on disk
public static class ItemLoader
{
    private static readonly string FilePath = Path.Combine("..", "..", "..", "Resources", "Data", "items.yaml");

    private class ItemConfig
    {
        public Dictionary<string, Dictionary<string, object>> Items { get; set; } = new();
    }
    
    public static ItemData[] Load()
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var fileContent = File.ReadAllText(FilePath);
        var config = deserializer.Deserialize<ItemConfig>(fileContent);

        var items = new List<ItemData>();
        foreach (var kvp in config.Items)
        {
            var itemId = kvp.Key;
            var itemData = kvp.Value;
            
            var itemType = itemData.ContainsKey("type") ? itemData["type"].ToString() : "material";
            
            ItemData item = itemType switch
            {
                "tool" => CreateToolItem(itemData),
                "fuel" => CreateFuelItem(itemData),
                "food" => CreateFoodItem(itemData),
                _ => CreateMaterialItem(itemData)
            };
            
            item.ExternalId = itemId;
            items.Add(item);
        }

        Console.WriteLine($"Loaded {items.Count} items from data");
        return items.ToArray();
    }
    
    private static ToolItemData CreateToolItem(Dictionary<string, object> data)
    {
        return new ToolItemData
        {
            DisplayName = data["display_name"].ToString(),
            Texture = data["texture"].ToString(),
            Type = ToolTypeExtensions.FromString(data["tool_type"].ToString()),
            Durability = Convert.ToInt32(data["durability"]),
            MiningSpeed = Convert.ToSingle(data["mining_speed"]),
        };
    }
    
    private static FuelItemData CreateFuelItem(Dictionary<string, object> data)
    {
        return new FuelItemData
        {
            DisplayName = data["display_name"].ToString(),
            Texture = data["texture"].ToString(),
            BurnTime = Convert.ToInt32(data["burn_time"])
        };
    }
    
    private static FoodItemData CreateFoodItem(Dictionary<string, object> data)
    {
        return new FoodItemData
        {
            DisplayName = data["display_name"].ToString(),
            Texture = data["texture"].ToString(),
            HungerRestore = Convert.ToInt32(data["hunger_restore"]),
        };
    }
    
    private static MaterialItemData CreateMaterialItem(Dictionary<string, object> data)
    {
        return new MaterialItemData
        {
            DisplayName = data["display_name"].ToString(),
            Texture = data["texture"].ToString()
        };
    }
}