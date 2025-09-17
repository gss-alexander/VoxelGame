namespace Client;

public static class Textures
{
    public enum TextureCategory
    {
        Blocks,
        Items,
        Ui,
        Misc
    }
    
    private static readonly Dictionary<TextureCategory, Dictionary<string, Texture>> TextureCache = new();

    public static Texture GetTexture(TextureCategory category, string name)
    {
        if (TextureCache.TryGetValue(category, out var categories))
        {
            if (categories.TryGetValue(name, out var cachedTexture))
            {
                return cachedTexture;
            }
        }
        else
        {
            TextureCache.Add(category, new Dictionary<string, Texture>());
        }

        var texture = LoadTexture(category, name);
        TextureCache[category].Add(name, texture);
        return texture;
    }

    private static Texture LoadTexture(TextureCategory category, string name)
    {
        var path = Path.Combine("..", "..", "..", "Resources", "Textures", category.ToString(), $"{name}.png");
        return new Texture(path);
    }
}