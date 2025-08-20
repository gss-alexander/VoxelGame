using Silk.NET.OpenGL;

namespace Client;

public static class OpenGl
{
    public static GL Context
    {
        get
        {
            if (_context == null)
            {
                throw new InvalidOperationException("The OpenGL context has not been set");
            }

            return _context;
        }
        set
        {
            if (_context != null)
            {
                throw new InvalidOperationException("The OpenGL context has already been set");
            }

            _context = value;
        }
    }
    
    private static GL? _context;
}