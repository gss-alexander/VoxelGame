using System.Numerics;

namespace Client.UiSystem.Components;

public class Panel : UiElement
{
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
        return false;
    }
}