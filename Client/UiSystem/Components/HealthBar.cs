using System.Numerics;
using Client.UiSystem.Elements;

namespace Client.UiSystem.Components;

public class HealthBar : Panel
{
    private readonly Health _health;

    private readonly Image[] _heartImages;

    private readonly Texture _fullHeartTexture;
    private readonly Texture _halfHeartTexture;
    private readonly Texture _emptyHeartTexture;

    public HealthBar(Health health, Vector2 position)
    {
        _health = health;
        _fullHeartTexture = Textures.GetTexture(Textures.TextureCategory.Ui, "hud_heart_full");
        _halfHeartTexture = Textures.GetTexture(Textures.TextureCategory.Ui, "hud_heart_half"); 
        _emptyHeartTexture = Textures.GetTexture(Textures.TextureCategory.Ui, "hud_heart_empty"); 

        Size = new Vector2(200f, 60f);
        Position = position;
        Anchor = AnchorMode.CenterBottom;
        Pivot = PivotMode.CenterMiddle;

        _heartImages = new Image[10];
        for (var i = 0; i < 10; i++)
        {
            CreateHeartImage(i);
        }

        _health.OnChange += UpdateHeartIcons;
        UpdateHeartIcons();
    }

    private void CreateHeartImage(int index)
    {
        var img = new Image();
        img.Sprite = _fullHeartTexture;
        img.Pivot = PivotMode.CenterMiddle;
        img.Anchor = AnchorMode.CenterMiddle;
        img.Size = new Vector2(40f, 40f);
        img.Position = new Vector2(40f * index, -50f);

        _heartImages[index] = img;
        
        AddChild(img);
    }

    private void UpdateHeartIcons()
    {
        for (var i = 0; i < 10; i++)
        {
            var img = _heartImages[i];
            float heartThreshold = i + 1f;
        
            if (_health.Value >= heartThreshold)
            {
                img.Sprite = _fullHeartTexture;
            }
            else if (_health.Value >= heartThreshold - 0.5f)
            {
                img.Sprite = _halfHeartTexture;
            }
            else
            {
                img.Sprite = _emptyHeartTexture;
            }
        }
    }
}