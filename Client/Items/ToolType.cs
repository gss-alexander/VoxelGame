namespace Client.Items;

public enum ToolType
{
    Pickaxe,
    Axe,
    Shovel
}

public static class ToolTypeExtensions
{
    public static ToolType FromString(string toolTypeString)
    {
        return toolTypeString switch
        {
            "pickaxe" => ToolType.Pickaxe,
            "axe" => ToolType.Axe,
            "shovel" => ToolType.Shovel,
            _ => throw new NotImplementedException($"No tool type found for \"{toolTypeString}\"")
        };
    }
}