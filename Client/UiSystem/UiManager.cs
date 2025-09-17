using Client.Inputs;
using Client.Items;
using Client.UiSystem.Elements;
using Client.UiSystem.Screens;

namespace Client.UiSystem;

public class UiManager
{
    private readonly ActionContext _actionContext;
    private readonly Dictionary<Screen, UiScreen> _screens = new();
    
    public UiManager(ActionContext actionContext, GameController gameController, PlayerInventory inventory, ItemTextures itemTextures)
    {
        _actionContext = actionContext;
        
        // Pause screen
        var pauseScreen = new PauseScreen(gameController);
        pauseScreen.Initialize();
        pauseScreen.OnOpen += () =>
        {
            _actionContext.SetCursorLocked(false);
            _actionContext.MovementBlocked = true;
        };
        pauseScreen.OnClose += () =>
        {
            _actionContext.SetCursorLocked(true);
            _actionContext.MovementBlocked = false;
        };
        _screens.Add(Screen.PauseMenu, pauseScreen);

        // HUD
        var hudScreen = new HudScreen(inventory, itemTextures);
        hudScreen.Initialize();
        _screens.Add(Screen.Hud, hudScreen);
        hudScreen.IsActive = true;
    }

    public void TogglePauseMenu()
    {
        _screens[Screen.PauseMenu].IsActive = !_screens[Screen.PauseMenu].IsActive;
    }

    public void Update(float deltaTime)
    {
        if (_actionContext.IsReleased(InputAction.UiClick))
        {
            var elements = new List<UiElement>();
            foreach (var screen in _screens.Values)
            {
                if (!screen.IsActive) continue;
                screen.CollectElements(elements);
            }

            var sortedElements = elements.OrderBy(e => e.ZOrder);
            foreach (var sortedElement in sortedElements)
            {
                if (sortedElement.Visible && sortedElement.HandleInput(_actionContext.MousePosition, true))
                {
                    break;
                }
            }
        }
    }

    public void Render(float deltaTime)
    {
        foreach (var screen in _screens.Values)
        {
            if (!screen.IsActive) continue;
            
            screen.Render(deltaTime);
        }
    }
}