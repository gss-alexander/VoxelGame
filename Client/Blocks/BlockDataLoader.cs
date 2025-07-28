using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Client.Blocks;

public static class BlockDataLoader
{
    private class BlockConfig
    {
        public Dictionary<string, BlockData> Blocks { get; set; } = new();
    }
    
    public static BlockData[] Load(string dataFilePath)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var fileContent = File.ReadAllText(dataFilePath);
        var config = deserializer.Deserialize<BlockConfig>(fileContent);

        var blocks = new List<BlockData>();
        foreach (var kvp in config.Blocks)
        {
            // the key becomes the ID
            kvp.Value.ExternalId = kvp.Key;
            blocks.Add(kvp.Value);
        }

        Console.WriteLine($"Loaded {blocks.Count} blocks from data");
        return blocks.ToArray();
    }
}