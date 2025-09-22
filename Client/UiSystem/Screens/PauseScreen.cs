using System.Numerics;
using Client.UiSystem.Elements;

namespace Client.UiSystem.Screens;

public class PauseScreen : UiScreen
{
    private readonly GameController _gameController;

    public PauseScreen(GameController gameController)
    {
        _gameController = gameController;
    }
    
    public override void Initialize()
    {
        rootElement = new Panel();
        rootElement.Position = Vector2.Zero;
        rootElement.Size = new Vector2(WindowDimensions.Width, WindowDimensions.Height);
        rootElement.Anchor = UiElement.AnchorMode.LeftTop;
        rootElement.ZOrder = 0;

        var background = new Image();
        background.Sprite = Textures.GetTexture(Textures.TextureCategory.Ui, "pause_menu_background");
        background.Alpha = 0.6f;
        background.Color = Vector3.One;
        background.Size = new Vector2(WindowDimensions.Width, WindowDimensions.Height);
        background.ZOrder = 0;
        rootElement.AddChild(background);

        CreateContinueButton();
        CreateQuitButton();
    }

    private void CreateContinueButton()
    {
        var button = new Button();
        button.Size = new Vector2(400f, 100f);
        button.Position = new Vector2(0f, 400f);
        button.Anchor = UiElement.AnchorMode.CenterTop;
        button.Pivot = UiElement.PivotMode.CenterMiddle;
        button.OnClick += () => IsActive = false;
        button.ZOrder = 1;
        rootElement.AddChild(button);
        
        var image = new Image();
        image.Sprite = Textures.GetTexture(Textures.TextureCategory.Ui, "pause_menu_button");
        image.Size = button.Size;
        image.Anchor = UiElement.AnchorMode.LeftTop;
        button.AddChild(image);

        var label = new Text();
        label.FontSize = 12f;
        label.Content = "Continue";
        label.Position = Vector2.Zero;
        label.Size = button.Size;
        label.ZOrder = 0;
        label.Anchor = UiElement.AnchorMode.CenterMiddle;
        label.Pivot = UiElement.PivotMode.CenterMiddle;
        label.HorizontalAlign = Text.HorizontalAlignment.Center;
        label.VerticalAlign = Text.VerticalAlignment.Middle;
        button.AddChild(label);
    }
    
    private void CreateQuitButton()
    {        
        var button = new Button();
        button.Size = new Vector2(400f, 100f);
        button.Position = new Vector2(0f, 600f);
        button.Anchor = UiElement.AnchorMode.CenterTop;
        button.Pivot = UiElement.PivotMode.CenterMiddle;
        button.OnClick += () => _gameController.QuitGame();
        button.ZOrder = 1;
        rootElement.AddChild(button);

        var image = new Image();
        image.Sprite = Textures.GetTexture(Textures.TextureCategory.Ui, "pause_menu_button");
        image.Size = button.Size;
        image.Anchor = UiElement.AnchorMode.LeftTop;
        button.AddChild(image);
        
        var label = new Text();
        label.FontSize = 12f;
        label.Content = "Quit";
        label.Position = Vector2.Zero;
        label.Size = button.Size;
        label.ZOrder = 0;
        label.Anchor = UiElement.AnchorMode.CenterMiddle;
        label.Pivot = UiElement.PivotMode.CenterMiddle;
        label.HorizontalAlign = Text.HorizontalAlignment.Center;
        label.VerticalAlign = Text.VerticalAlignment.Middle;
        button.AddChild(label);
    }
}