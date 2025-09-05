namespace Client.UILib.Core.TextRendering;

public static class Fonts
{
    private static readonly Dictionary<string, Font> FontCache = new();

    public static Font Default => Get("Roboto-VariableFont_wdth,wght");

    public static Font Get(string fontName)
    {
        if (FontCache.TryGetValue(fontName, out var cachedFont))
        {
            return cachedFont;
        }
        
        var path = BuildFontPath(fontName);
        var font = new Font(path);
        FontCache.Add(fontName, font);
        return font;
    }

    private static string BuildFontPath(string fontName)
    {
        return Path.Combine("..", "..", "..", "Resources", "Fonts", $"{fontName}.ttf");
    }
}