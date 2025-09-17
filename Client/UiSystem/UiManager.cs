using Client.Inputs;
using Client.Items;
using Client.UiSystem.Elements;
using Client.UiSystem.Screens;

namespace Client.UiSystem;

public class UiManager
{
    private readonly ActionContext _actionContext;
    private readonly Dictionary<Screen, UiScreen> _screens = new();

    private readonly List<UiElement> _uiElementCollector = new();
    
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

        // Inventory
        var inventoryScreen = new InventoryScreen(inventory, itemTextures);
        inventoryScreen.Initialize();
        inventoryScreen.OnOpen += () =>
        {
            _actionContext.SetCursorLocked(false);
            _actionContext.MovementBlocked = true;
        };
        inventoryScreen.OnClose += () =>
        {
            _actionContext.SetCursorLocked(true);
            _actionContext.MovementBlocked = false;
        };
        _screens.Add(Screen.Inventory, inventoryScreen);
    }

    public void TogglePauseMenu()
    {
        _screens[Screen.PauseMenu].IsActive = !_screens[Screen.PauseMenu].IsActive;
    }

    public void TryToggleInventoryMenu()
    {
        if (_screens[Screen.PauseMenu].IsActive) return;

        _screens[Screen.Inventory].IsActive = !_screens[Screen.Inventory].IsActive;
    }

    public void Update(float deltaTime)
    {
        _uiElementCollector.Clear();
        
        if (_actionContext.IsPressed(InputAction.UiClick))
        {
            foreach (var screen in _screens.Values)
            {
                if (!screen.IsActive) continue;
                screen.CollectElements(_uiElementCollector);
            }

            var sortedElements = _uiElementCollector.OrderByDescending(e => e.ZOrder);
            foreach (var sortedElement in sortedElements)
            {
                if (sortedElement.Visible && sortedElement.HandleInput(_actionContext.MousePosition, true))
                {
                    break;
                }
            }
        }
        else
        {
            foreach (var screen in _screens.Values)
            {
                if (!screen.IsActive) continue;
                screen.CollectElements(_uiElementCollector);
            }

            foreach (var element in _uiElementCollector)
            {
                element.HandleInput(_actionContext.MousePosition, false);
            }
        }

        foreach (var screen in _screens.Values)
        {
            screen.Update(deltaTime);
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