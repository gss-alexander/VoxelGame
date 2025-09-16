using System.Numerics;

namespace Client.UiSystem;

public abstract class UiElement
{
    public enum AnchorMode
    {
        LeftTop,
        LeftBottom,
        LeftMiddle,
        RightTop,
        RightBottom,
        RightMiddle,
        CenterTop,
        CenterBottom,
        CenterMiddle
    }

    public Vector2 Position { get; set; }
    
    public Vector2 AbsolutePosition => 
        (Parent?.AbsolutePosition ?? Vector2.Zero) + 
        CalculateAnchorPoint() + 
        Position;

    public int ZOrder { get; set; }
    
    public Vector2 Size { get; set; }
    public AnchorMode Anchor { get; set; } 
    public bool Visible { get; set; } = true;
    public List<UiElement> Children { get; } = new();
    public UiElement? Parent { get; set; }

    public abstract void Update(float deltaTime);
    public abstract void Render(float deltaTime);
    public abstract bool HandleInput(Vector2 mousePosition, bool isClicked);

    public void AddChild(UiElement element)
    {
        Children.Add(element);
        element.Parent = this;
    }

    public void RemoveChild(UiElement element)
    {
        if (Children.Remove(element))
        {
            element.Parent = null;
        }
    }

    public bool IsPointInside(Vector2 point)
    {
        var pos = AbsolutePosition;
        return point.X >= pos.X && point.X <= pos.X + Size.X &&
               point.Y >= pos.Y && point.Y <= pos.Y + Size.Y;
    }

    private Vector2 CalculateAnchorPoint()
    {
        var parentSize = Parent?.Size ?? new Vector2(WindowDimensions.Width, WindowDimensions.Height);
        var anchorMultiplier = Anchor switch
        {
            AnchorMode.LeftTop => new Vector2(0f, 0f),
            AnchorMode.LeftMiddle => new Vector2(0f, 0.5f),
            AnchorMode.LeftBottom => new Vector2(0f, 1f),
            AnchorMode.CenterTop => new Vector2(0.5f, 0f),
            AnchorMode.CenterMiddle => new Vector2(0.5f, 0.5f),
            AnchorMode.CenterBottom => new Vector2(0.5f, 1f),
            AnchorMode.RightTop => new Vector2(1f, 0f),
            AnchorMode.RightMiddle => new Vector2(1f, 0.5f),
            AnchorMode.RightBottom => new Vector2(1f, 1f),
            _ => throw new NotImplementedException()
        };

        return parentSize * anchorMultiplier;
    }
}