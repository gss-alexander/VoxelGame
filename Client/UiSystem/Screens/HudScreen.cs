using System.Numerics;
using Client.Items;
using Client.UiSystem.Components;
using Client.UiSystem.Elements;

namespace Client.UiSystem.Screens;

public class HudScreen : UiScreen
{
    private readonly PlayerInventory _playerInventory;
    private readonly ItemTextures _itemTextures;

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

        var hotbar = new HotbarUi(_playerInventory, _itemTextures);
        rootElement.AddChild(hotbar);
    }
}