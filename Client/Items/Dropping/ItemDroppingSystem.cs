using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.Items.Dropping;

public class ItemDroppingSystem
{
    private readonly GL _gl;
    private readonly ItemDatabase _itemDatabase;
    private readonly ItemTextures _itemTextures;
    private readonly Shader _droppedItemShader;
    private readonly Func<Vector3, bool> _isBlockSolidFunc;

    private readonly List<DroppedItem> _droppedItems = new();

    public ItemDroppingSystem(GL gl, ItemDatabase itemDatabase, ItemTextures itemTextures, Shader droppedItemShader, Func<Vector3, bool> isBlockSolidFunc)
    {
        _gl = gl;
        _itemDatabase = itemDatabase;
        _itemTextures = itemTextures;
        _droppedItemShader = droppedItemShader;
        _isBlockSolidFunc = isBlockSolidFunc;
    }

    public void DropItem(Vector3 worldPosition, string itemId)
    {
        var itemData = _itemDatabase.Get(itemId);
        var droppedItem = new DroppedItem(_gl, _droppedItemShader, _itemTextures, itemData, worldPosition, _isBlockSolidFunc);
        _droppedItems.Add(droppedItem);
        Console.WriteLine($"[Item Dropping System]: Dropping item {itemId} at position {worldPosition}");
    }

    public void Update(float deltaTime)
    {
        foreach (var droppedItem in _droppedItems)
        {
            droppedItem.Update(deltaTime);
        }
    }

    public void RenderDroppedItems(Matrix4x4 view, Matrix4x4 projection)
    {
        foreach (var droppedItem in _droppedItems)
        {
            droppedItem.Render(view, projection);
        }
    }
}