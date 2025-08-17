using System.Numerics;
using System.Text.Json;
using Silk.NET.Maths;

namespace Client.Persistence;

public class WorldData
{
    public string Name { get; set; }
    public Vector3 PlayerPosition { get; set; }
    public float CameraPitch { get; set; }
    public float CameraYaw { get; set; }
    public Dictionary<Vector3D<int>, string> ModifiedBlocks { get; set; } = new();

    public WorldData()
    {
        
    }

    public WorldData(string name)
    {
        Name = name;
    }

    public void AddModifiedBlock(Vector3D<int> worldPosition, string blockId)
    {
        ModifiedBlocks.Add(worldPosition, blockId);
    }
    
    public static WorldData Deserialize(string json)
    {
        return JsonSerializer.Deserialize<WorldData>(json, GetOptions());
    } 

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, GetOptions());
    }

    private static JsonSerializerOptions GetOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new Vector3DIntKeyConverter());
        options.Converters.Add(new Vector3Converter());
        return options;
    }
}