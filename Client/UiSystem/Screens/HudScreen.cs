using System.Numerics;
using Client.Items;
using Client.UiSystem.Elements;

namespace Client.UiSystem.Screens;

public class HudScreen : UiScreen
{
    private readonly PlayerInventory _playerInventory;
    private readonly ItemTextures _itemTextures;

    private Image _background;
    
    public HudScreen(PlayerInventory playerInventory, ItemTextures itemTextures)
    {
        _playerInventory = playerInventory;
        _itemTextures = itemTextures;
    }

    public override void Initialize()
    {
        rootElement = new Panel();
        rootElement.Position = new Vector2(0f, 0f);
        rootElement.Size = new Vector2(WindowDimensions.Width, WindowDimensions.Height);

        _background = new Image();
        _background.Sprite = Textures.GetTexture(Textures.TextureCategory.Ui, "hotbar_background");
        _background.Size = new Vector2(800, 88f);
        _background.Position = new Vector2(0f, -100f);
        _background.Anchor = UiElement.AnchorMode.CenterBottom;
        _background.Pivot = UiElement.PivotMode.CenterMiddle;
        rootElement.AddChild(_background);

        _playerInventory.Hotbar.OnChanged += () => UpdateHotbarSlots();
        _playerInventory.OnSelectedHotbarSlotChanged += () => UpdateHotbarSlots();
        UpdateHotbarSlots();
    }

    private void UpdateHotbarSlots()
    {
        _background.DestroyAllChildren();

        var selectedHotbarSlot = _playerInventory.SelectedHotbarSlot;

        for (var i = 0; i < _playerInventory.Hotbar.SlotCount; i++)
        {
            var slot = _playerInventory.Hotbar.GetSlot(i);
            var size = new Vector2(800f / 9f, 88f);
            var position = new Vector2((800f / 9f) * i, 0f);
            
            if (slot != null)
            {
                var slotElement = new Image();
                slotElement.Sprite = _itemTextures.GetTextureForItem(slot.ItemId);
                slotElement.Size = size * 0.9f;
                slotElement.Anchor = UiElement.AnchorMode.LeftMiddle;
                slotElement.Pivot = UiElement.PivotMode.LeftMiddle;
                slotElement.Position = position;
                _background.AddChild(slotElement);

                var countLabel = new Text();
                countLabel.Anchor = UiElement.AnchorMode.RightTop;
                countLabel.Pivot = UiElement.PivotMode.RightMiddle;
                countLabel.HorizontalAlign = Text.HorizontalAlignment.Right;
                countLabel.FontSize = 5f;
                countLabel.Color = Vector3.Zero;
                countLabel.Content = slot.Count.ToString();
                countLabel.Position = new Vector2(-5f, 10f);
                slotElement.AddChild(countLabel);
            }
            
            if (i == selectedHotbarSlot)
            {
                var selectionElement = new Image();
                selectionElement.Sprite = Textures.GetTexture(Textures.TextureCategory.Ui, "selected_hotbar_slot");
                selectionElement.Size = size;
                selectionElement.Position = position;
                selectionElement.Anchor = UiElement.AnchorMode.LeftMiddle;
                selectionElement.Pivot = UiElement.PivotMode.LeftMiddle;
                
                _background.AddChild(selectionElement);
            }
        }
    }
}