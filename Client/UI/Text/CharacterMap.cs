using Silk.NET.OpenGL;

namespace Client.UI.Text;

public class CharacterMap
{
    private readonly GL _gl;
    private readonly Dictionary<char, Character> _map = new();

    public CharacterMap(GL gl)
    {
        _gl = gl;
        Initialize();
    }

    public Character GetCharacter(char c)
    {
        return _map[c];
    }

    private void Initialize()
    {
        FontLoader.LoadFace(Path.Combine("..", "..", "..", "Resources", "Fonts", "Roboto-VariableFont_wdth,wght.ttf"));
        
        // Loads the first 128 characters as opengl textures
        for (int characterIndex = 0; characterIndex < 128; characterIndex++)
        {
            unsafe
            {
                var c = (char)characterIndex;
                FontLoader.LoadChar(c);
                var character = new Character(_gl,
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
        
        Console.WriteLine($"[Character map]: Loaded {_map.Count} characters");
        FontLoader.ClearFreeTypeResources();
    }
}