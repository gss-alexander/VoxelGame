using System.Text.Json;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace Client.Persistence;

public class Vector3DIntKeyConverter : JsonConverter<Vector3D<int>>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(Vector3D<int>);
    }

    public override Vector3D<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        var parts = value.Split(',');
        return new Vector3D<int>(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
    }

    public override void Write(Utf8JsonWriter writer, Vector3D<int> value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{value.X},{value.Y},{value.Z}");
    }

    public override Vector3D<int> ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        var parts = value.Split(',');
        return new Vector3D<int>(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, Vector3D<int> value, JsonSerializerOptions options)
    {
        writer.WritePropertyName($"{value.X},{value.Y},{value.Z}");
    }
}