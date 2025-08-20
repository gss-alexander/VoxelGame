using Silk.NET.OpenGL;

namespace Client;

// Should not really be a static class...
public static class Shaders
{
    private static readonly Dictionary<string, Shader> ShaderCache = new();
    
    public static Shader GetShader(string name)
    {
        if (ShaderCache.TryGetValue(name, out var cachedShader))
        {
            return cachedShader;
        }

        var shader = LoadShader(name);
        ShaderCache.Add(name, shader);
        return shader;
    }

    private static Shader LoadShader(string name)
    {
        return new Shader(GetShaderPath($"{name}.vert"), GetShaderPath($"{name}.frag"));
    }
    
    private static string GetShaderPath(string name)
    {
        return Path.Combine("..", "..", "..", "Resources", "Shaders", name);
    }
}