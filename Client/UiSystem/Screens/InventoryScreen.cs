using System.Numerics;
using Client.Items;
using Client.UiSystem.Components;
using Client.UiSystem.Elements;

namespace Client.UiSystem.Screens;

public class InventoryScreen : UiScreen
{
    private readonly PlayerInventory _inventory;
    private readonly ItemTextures _itemTextures;
    
    private readonly List<UiElement> _slotElements = new();
    private readonly Image _backgroundPanel = new();

    public InventoryScreen(PlayerInventory inventory, ItemTextures itemTextures)
    {
        _inventory = inventory;
        _itemTextures = itemTextures;
    }

    public override void Initialize()
    {
        rootElement = new Panel();
        rootElement.Position = new Vector2(0f, 0f);
        rootElement.Size = new Vector2(WindowDimensions.Width, WindowDimensions.Height);
        
        var backgroundDim = new Image();
        backgroundDim.Sprite = Textures.GetTexture(Textures.TextureCategory.Ui, "pause_menu_background");
        backgroundDim.Alpha = 0.7f;
        backgroundDim.Color = Vector3.One;
        backgroundDim.Size = new Vector2(WindowDimensions.Width, WindowDimensions.Height);
        backgroundDim.ZOrder = 0;
        rootElement.AddChild(backgroundDim);

        _backgroundPanel.Sprite = Textures.GetTexture(Textures.TextureCategory.Ui, "inventory_panel");
        _backgroundPanel.Anchor = UiElement.AnchorMode.CenterMiddle;
        _backgroundPanel.Pivot = UiElement.PivotMode.CenterMiddle;
        _backgroundPanel.Size = new Vector2(900f, 800f);
        _backgroundPanel.Position = new(0f, -50);
        rootElement.AddChild(_backgroundPanel);
        
        UpdateSlotElements();
        _inventory.Hotbar.OnChanged += UpdateSlotElements;
        _inventory.Storage.OnChanged += UpdateSlotElements;
    }

    private void UpdateSlotElements()
    {
        foreach (var element in _slotElements)
        {
            _backgroundPanel.RemoveChild(element);
        }
        _slotElements.Clear();
        
        for (var i = 0; i < _inventory.Hotbar.SlotCount; i++)
        {
            var slot = _inventory.Hotbar.GetSlotInternal(i);
            var size = new Vector2(800f / 9f, 88f);
            var position = new Vector2((850f / 9f) * i + 30f, -40f);
                
            var slotElement = new InventorySlot(slot, position, size * 0.8f, _itemTextures);
            _backgroundPanel.AddChild(slotElement);
            _slotElements.Add(slotElement);
        }
        
        for (var i = 0; i < _inventory.Storage.SlotCount; i++)
        {
            var slot = _inventory.Storage.GetSlotInternal(i);
            var row = (int)MathF.Floor((float)i / 9);
            var size = new Vector2(800f / 9f, 88f);
            var position = new Vector2((850f / 9f) * (i % 9) + 30f, -140f - (90f * row));
            
            var slotElement = new InventorySlot(slot, position, size * 0.8f, _itemTextures);
            _backgroundPanel.AddChild(slotElement);
            _slotElements.Add(slotElement);
        }
    }
}