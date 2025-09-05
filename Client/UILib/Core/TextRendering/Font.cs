using Client.UI.Text;

namespace Client.UILib.Core.TextRendering;

public class Font
{
    private readonly string _path;
    private readonly Dictionary<char, Character> _map = new();
    
    public Font(string path)
    {
        _path = path;
        Initialize();
    }

    public Character GetCharacter(char c)
    {
        return _map[c];
    }

    private void Initialize()
    {
        FontLoader.LoadFace(_path);
        
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
        
        Console.WriteLine($"[Font]: Loaded {_map.Count} characters for font {_path}");
        FontLoader.ClearFreeTypeResources();
    }
}