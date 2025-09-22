using System.Numerics;
using Client.Items;
using Client.UiSystem.Components;
using Client.UiSystem.Elements;

namespace Client.UiSystem.Screens;

public class HudScreen : UiScreen
{
    private readonly PlayerInventory _playerInventory;
    private readonly ItemTextures _itemTextures;
    private readonly Health _playerHealth;

    public HudScreen(PlayerInventory playerInventory, ItemTextures itemTextures, Health playerHealth)
    {
        _playerInventory = playerInventory;
        _itemTextures = itemTextures;
        _playerHealth = playerHealth;
    }

    public override void Initialize()
    {
        rootElement = new Panel();
        rootElement.Position = new Vector2(0f, 0f);
        rootElement.Size = new Vector2(WindowDimensions.Width, WindowDimensions.Height);

        var hotbar = new HotbarUi(_playerInventory, _itemTextures, new Vector2(0f, -50f));
        rootElement.AddChild(hotbar);

        var healthBar = new HealthBar(_playerHealth, new Vector2(-375f, -75f));
        rootElement.AddChild(healthBar);
    }
}