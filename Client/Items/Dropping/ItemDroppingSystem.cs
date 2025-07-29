using System.Numerics;
using Client.Blocks;
using Silk.NET.OpenGL;

namespace Client.Items.Dropping;

public class ItemDroppingSystem
{
    private readonly GL _gl;
    private readonly ItemDatabase _itemDatabase;
    private readonly ItemTextures _itemTextures;
    private readonly Shader _droppedItemShader;
    private readonly Func<Vector3, bool> _isBlockSolidFunc;
    private readonly BlockDatabase _blockDatabase;
    private readonly BlockTextures _blockTextures;

    private readonly List<DroppedItem> _droppedItems = new();
    private readonly List<string> _pickedUpItems = new();
    private readonly List<DroppedItem> _droppedItemsToClear = new();

    public ItemDroppingSystem(GL gl, ItemDatabase itemDatabase, ItemTextures itemTextures, Shader droppedItemShader,
        Func<Vector3, bool> isBlockSolidFunc, BlockDatabase blockDatabase, BlockTextures blockTextures)
    {
        _gl = gl;
        _itemDatabase = itemDatabase;
        _itemTextures = itemTextures;
        _droppedItemShader = droppedItemShader;
        _isBlockSolidFunc = isBlockSolidFunc;
        _blockDatabase = blockDatabase;
        _blockTextures = blockTextures;
    }

    public void DropItem(Vector3 worldPosition, string itemId)
    {
        var itemData = _itemDatabase.Get(itemId);
        var droppedItem = new DroppedItem(CreateRendererForItem(itemData), itemData, worldPosition, _isBlockSolidFunc);
        _droppedItems.Add(droppedItem);
        Console.WriteLine($"[Item Dropping System]: Dropping item {itemId} at position {worldPosition}");
    }

    private IWorldRenderable CreateRendererForItem(ItemData itemData)
    {
        return itemData.GetType() == typeof(BlockItemData)
            ? new BlockRenderer(_gl, (BlockItemData)itemData, _blockDatabase, _blockTextures)
            : new ItemDropRenderer(_gl, _droppedItemShader, _itemTextures, itemData);
    }

    public List<string> PickUpItems(Vector3 origin, float distance)
    {
        _pickedUpItems.Clear();
        _droppedItemsToClear.Clear();
        foreach (var droppedItem in _droppedItems)
        {
            if (Vector3.Distance(origin, droppedItem.Position) <= distance)
            {
                _pickedUpItems.Add(droppedItem.ItemId);
                _droppedItemsToClear.Add(droppedItem);
            }
        }

        foreach (var droppedItemToClear in _droppedItemsToClear)
        {
            _droppedItems.Remove(droppedItemToClear);
        }

        return _pickedUpItems;
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