using System.Numerics;
using Client.UiSystem.Elements;
using JetBrains.FormatRipper.MachO;

namespace Client.UiSystem.Screens;

public abstract class UiScreen
{
    public event Action OnOpen;
    public event Action OnClose;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            var wasActive = _isActive;
            _isActive = value;
            
            if (_isActive && !wasActive)
            {
                OnOpen?.Invoke();
            }

            if (!_isActive && wasActive)
            {
                OnClose?.Invoke();
            }
        }
    }

    public void CollectElements(List<UiElement> elementCollection)
    {
        rootElement?.CollectElements(elementCollection);
    }
    
    protected UiElement? rootElement;

    private bool _isActive;
    
    public abstract void Initialize();

    public virtual void Update(float deltaTime) => rootElement?.Update(deltaTime);
    public virtual void Render(float deltaTime) => rootElement?.Render(deltaTime);
    public virtual void HandleInput(Vector2 mousePosition, bool clicked) =>
        rootElement?.HandleInput(mousePosition, clicked);
}
