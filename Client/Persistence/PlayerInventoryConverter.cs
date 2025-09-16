using System.Text.Json;
using System.Text.Json.Serialization;
using Client.Items;

namespace Client.Persistence;

public class PlayerInventoryConverter : JsonConverter<PlayerInventory>
{
    public override PlayerInventory? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");

        var inventory = new PlayerInventory();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName token");

            string? propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "SelectedHotbarSlot":
                    inventory.SelectedHotbarSlot = reader.GetInt32();
                    break;
                    
                case "Hotbar":
                    DeserializeItemStorage(ref reader, inventory.Hotbar, options);
                    break;
                    
                case "Storage":
                    DeserializeItemStorage(ref reader, inventory.Storage, options);
                    break;
                    
                default:
                    reader.Skip();
                    break;
            }
        }

        return inventory;
    }

    public override void Write(Utf8JsonWriter writer, PlayerInventory value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteNumber("SelectedHotbarSlot", value.SelectedHotbarSlot);
        
        writer.WritePropertyName("Hotbar");
        SerializeItemStorage(writer, value.Hotbar, options);
        
        writer.WritePropertyName("Storage");
        SerializeItemStorage(writer, value.Storage, options);
        
        writer.WriteEndObject();
    }

    private void SerializeItemStorage(Utf8JsonWriter writer, ItemStorage storage, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("SlotCount", storage.SlotCount);
        
        writer.WriteStartArray("Slots");
        for (int i = 0; i < storage.SlotCount; i++)
        {
            var slot = storage.GetSlotInternal(i); // You'll need to make this method public or internal
            JsonSerializer.Serialize(writer, slot, options);
        }
        writer.WriteEndArray();
        
        writer.WriteEndObject();
    }

    private void DeserializeItemStorage(ref Utf8JsonReader reader, ItemStorage storage, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token for ItemStorage");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName token");

            string? propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "SlotCount":
                    reader.GetInt32(); // Read but don't use since slots are already initialized
                    break;
                    
                case "Slots":
                    if (reader.TokenType != JsonTokenType.StartArray)
                        throw new JsonException("Expected StartArray token for Slots");
                    
                    int slotIndex = 0;
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        var slot = JsonSerializer.Deserialize<ItemStorage.Slot>(ref reader, options);
                        if (slot != null && slotIndex < storage.SlotCount)
                        {
                            var targetSlot = storage.GetSlotInternal(slotIndex); // You'll need to make this method public or internal
                            targetSlot.ItemId = slot.ItemId;
                            targetSlot.Count = slot.Count;
                        }
                        slotIndex++;
                    }
                    break;
                    
                default:
                    reader.Skip();
                    break;
            }
        }
    }
}