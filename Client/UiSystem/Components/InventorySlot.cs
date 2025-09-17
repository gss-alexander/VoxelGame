using System.Numerics;
using Client.Items;
using Client.UiSystem.Elements;

namespace Client.UiSystem.Components;

public class InventorySlot : Image
{
    private readonly ItemTextures _itemTextures;

    public ItemStorage.Slot? Slot
    {
        get => _slot;
        set
        {
            _slot = value;
            if (_slot != null)
            {
                Update();
            }
        }
    }

    private readonly Text _countLabel = new();

    private ItemStorage.Slot? _slot;
    
    public InventorySlot(Vector2 position, Vector2 size, ItemTextures itemTextures)
    {
        _itemTextures = itemTextures;
        Size = size;
        Position = position;
        Anchor = AnchorMode.LeftBottom;
        Pivot = PivotMode.LeftBottom;
        
        _countLabel.Anchor = AnchorMode.RightTop;
        _countLabel.Pivot = PivotMode.RightMiddle;
        _countLabel.HorizontalAlign = Text.HorizontalAlignment.Right;
        _countLabel.FontSize = 5f;
        _countLabel.Color = Vector3.Zero;
        _countLabel.Position = new Vector2(-5f, 10f);
        AddChild(_countLabel);
    }

    private void Update()
    {
        _countLabel.Content = Slot?.Count.ToString() ?? string.Empty;
        Sprite = Slot != null ? _itemTextures.GetTextureForItem(Slot.ItemId) : null;
    }
}