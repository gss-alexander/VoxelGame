using System.Numerics;
using Silk.NET.OpenGL;

namespace Client;

public class Shader
{
    private readonly Dictionary<string, int> _uniformLocationCache = new();
    
    private readonly uint _handle;
    
    public Shader(string vertexShaderPath, string fragmentShaderPath)
    {
        var vertexShader = LoadShader(ShaderType.VertexShader, ReadShaderSourceFromFilesystem(vertexShaderPath));
        var fragmentShader = LoadShader(ShaderType.FragmentShader, ReadShaderSourceFromFilesystem(fragmentShaderPath));
        _handle = OpenGl.Context.CreateProgram();
        OpenGl.Context.AttachShader(_handle, vertexShader);
        OpenGl.Context.AttachShader(_handle, fragmentShader);
        OpenGl.Context.LinkProgram(_handle);
        OpenGl.Context.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out var status);
        if (status != (int)GLEnum.True)
        {
            throw new Exception($"Failed to link program: {OpenGl.Context.GetProgramInfoLog(_handle)}");
        }
        OpenGl.Context.DetachShader(_handle, vertexShader);
        OpenGl.Context.DetachShader(_handle, fragmentShader);
        OpenGl.Context.DeleteShader(vertexShader);
        OpenGl.Context.DeleteShader(fragmentShader);
    }

    public void Use()
    {
        OpenGl.Context.UseProgram(_handle);
    }

    public void SetUniform(string name, Matrix4x4 value)
    {
        unsafe
        {
            var location = GetUniformLocation(name);
            OpenGl.Context.UniformMatrix4(location, 1, false, (float*)&value);
        }
    }

    public void SetUniform(string name, float value)
    {
        var location = GetUniformLocation(name);
        OpenGl.Context.Uniform1(location, value);
    }
    
    public void SetUniform(string name, int value)
    {
        var location = GetUniformLocation(name);
        OpenGl.Context.Uniform1(location, value);
    }
    
    public void SetUniform(string name, Vector2 value)
    {
        var location = GetUniformLocation(name);
        OpenGl.Context.Uniform2(location, value);
    }

    public void SetUniform(string name, Vector3 value)
    {
        var location = GetUniformLocation(name);
        OpenGl.Context.Uniform3(location, value);
    }

    private int GetUniformLocation(string name)
    {
        if (_uniformLocationCache.TryGetValue(name, out var cachedLocation))
        {
            return cachedLocation;
        }
        
        var location = OpenGl.Context.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader");
        }
        
        _uniformLocationCache.Add(name, location);
        
        return location;
    }

    private uint LoadShader(ShaderType type, string source)
    {
        var shader = OpenGl.Context.CreateShader(type);
        OpenGl.Context.ShaderSource(shader, source);
        OpenGl.Context.CompileShader(shader);
        OpenGl.Context.GetShader(shader, ShaderParameterName.CompileStatus, out var status);
        if (status != (int)GLEnum.True)
        {
            throw new Exception($"Failed to compile shader of type {type}: {OpenGl.Context.GetShaderInfoLog(shader)}");
        }

        return shader;
    }

    private static string ReadShaderSourceFromFilesystem(string path)
    {
        if (!File.Exists(path))
        {
            throw new Exception($"Could not find shader source at path: {path}");
        }

        return File.ReadAllText(path);
    }
}