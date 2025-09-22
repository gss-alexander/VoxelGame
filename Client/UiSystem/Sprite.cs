namespace Client.UiSystem;

public class Sprite
{
    public int Width { get; }
    public int Height { get; }
    public Texture Texture { get; }

    public Sprite(int width, int height, Texture texture)
    {
        Width = width;
        Height = height;
        Texture = texture;
    }
}