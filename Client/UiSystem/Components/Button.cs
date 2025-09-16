using System.Numerics;

namespace Client.UiSystem.Components;

public class Button : UiElement
{
    public event Action OnClick;
    
    public override void Update(float deltaTime)
    {
        
    }

    public override void Render(float deltaTime)
    {
        foreach (var child in Children)
        {
            child.Render(deltaTime);
        }
    }

    public override bool HandleInput(Vector2 mousePosition, bool isClicked)
    {
        if (!IsPointInside(mousePosition)) return false;
        
        if (isClicked)
        {
            OnClick.Invoke();
        }

        return true;
    }
}