using System.Numerics;
using Client.Items;
using Client.UiSystem.Elements;

namespace Client.UiSystem.Components;

public class HotbarUi : Image
{
    private readonly PlayerInventory _inventory;
    private readonly ItemTextures _itemTextures;

    private readonly List<UiElement> _slotElements = new();
    private Image? _selectedSlotImage;

    public HotbarUi(PlayerInventory inventory, ItemTextures itemTextures)
    {
        _inventory = inventory;
        _itemTextures = itemTextures;
        
        Size = new Vector2(800f, 88f);
        Position = new Vector2(0f, -100f);
        Anchor = AnchorMode.CenterBottom;
        Pivot = PivotMode.CenterMiddle;
        Sprite = Textures.GetTexture(Textures.TextureCategory.Ui, "hotbar_background");

        UpdateSelectedHotbarSlotImage();
        UpdateSlotElements();

        inventory.OnSelectedHotbarSlotChanged += UpdateSelectedHotbarSlotImage;
        inventory.Hotbar.OnChanged += UpdateSlotElements;
    }

    private void UpdateSlotElements()
    {
        foreach (var element in _slotElements)
        {
            RemoveChild(element);
        }
        _slotElements.Clear();
        
        
        for (var i = 0; i < _inventory.Hotbar.SlotCount; i++)
        {
            var slot = _inventory.Hotbar.GetSlot(i);
            var size = new Vector2(800f / 9f, 88f);
            var position = new Vector2((800f / 9f) * i, 0f);
            
            if (slot != null)
            {
                var slotElement = new Image();
                slotElement.Sprite = _itemTextures.GetTextureForItem(slot.ItemId);
                slotElement.Size = size * 0.9f;
                slotElement.Anchor = AnchorMode.LeftMiddle;
                slotElement.Pivot = PivotMode.LeftMiddle;
                slotElement.Position = position;
                AddChild(slotElement);
                _slotElements.Add(slotElement);

                var countLabel = new Text();
                countLabel.Anchor = AnchorMode.RightTop;
                countLabel.Pivot = PivotMode.RightMiddle;
                countLabel.HorizontalAlign = Text.HorizontalAlignment.Right;
                countLabel.FontSize = 5f;
                countLabel.Color = Vector3.Zero;
                countLabel.Content = slot.Count.ToString();
                countLabel.Position = new Vector2(-5f, 10f);
                slotElement.AddChild(countLabel);
            }
        }
    }

    private void UpdateSelectedHotbarSlotImage()
    {
        if (_selectedSlotImage != null)
        {
            RemoveChild(_selectedSlotImage);
        }
        
        var image = new Image
        {
            Sprite = Textures.GetTexture(Textures.TextureCategory.Ui, "selected_hotbar_slot"),
            Size = SlotSize,
            Position = CalculateSlotPosition(_inventory.SelectedHotbarSlot),
            Anchor = AnchorMode.LeftMiddle,
            Pivot = PivotMode.LeftMiddle
        };
        
        _selectedSlotImage = image;
        AddChild(_selectedSlotImage);
    }

    private Vector2 CalculateSlotPosition(int slotIndex)
    {
        return new Vector2((800f / 9f) * slotIndex, 0f);
    }

    private Vector2 SlotSize => new(800f / 9f, 88f);
}