using Silk.NET.OpenGL;

namespace Client.UI.Text;

public class CharacterMap
{
    private static readonly Dictionary<string, CharacterMap> _characterMaps = new();
    
    public const float BaseFontSize = 16f;
    
    private readonly Dictionary<char, Character> _map = new();

    public static CharacterMap LoadForFont(string fontName)
    {
        if (_characterMaps.TryGetValue(fontName, out var cachedCharacterMap))
        {
            return cachedCharacterMap;
        }

        var characterMap = new CharacterMap(fontName);
        _characterMaps.Add(fontName, characterMap);
        return characterMap;
    }

    public CharacterMap(string fontName)
    {
        Initialize(fontName);
    }

    public Character GetCharacter(char c)
    {
        return _map[c];
    }

    private void Initialize(string fontName)
    {
        FontLoader.LoadFace(Path.Combine("..", "..", "..", "Resources", "Fonts", $"{fontName}.ttf"));
        
        // Loads the first 128 characters as opengl textures
        for (int characterIndex = 0; characterIndex < 128; characterIndex++)
        {
            unsafe
            {
                var c = (char)characterIndex;
                FontLoader.LoadChar(c);
                var character = new Character(OpenGl.Context,
                    FontLoader.LoadedCharWidth,
                    FontLoader.LoadedCharHeight,
                    FontLoader.LoadedCharLeft,
                    FontLoader.LoadedCharTop,
                    FontLoader.LoadedCharAdvanceX.ToInt32(),
                    FontLoader.LoadedCharBuffer
                );
                _map.Add(c, character);
            }
        }
        
        Console.WriteLine($"[Character map]: Loaded {_map.Count} characters for font {fontName}");
        FontLoader.ClearFreeTypeResources();
    }
}