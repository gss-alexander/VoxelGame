using System.Numerics;
using Silk.NET.OpenGL;

namespace Client;

public class Shader
{
    private readonly GL _gl;
    private readonly uint _handle;
    
    public Shader(GL gl, string vertexShaderPath, string fragmentShaderPath)
    {
        _gl = gl;

        var vertexShader = LoadShader(ShaderType.VertexShader, ReadShaderSourceFromFilesystem(vertexShaderPath));
        var fragmentShader = LoadShader(ShaderType.FragmentShader, ReadShaderSourceFromFilesystem(fragmentShaderPath));
        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vertexShader);
        _gl.AttachShader(_handle, fragmentShader);
        _gl.LinkProgram(_handle);
        _gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out var status);
        if (status != (int)GLEnum.True)
        {
            throw new Exception($"Failed to link program: {_gl.GetProgramInfoLog(_handle)}");
        }
        _gl.DetachShader(_handle, vertexShader);
        _gl.DetachShader(_handle, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    public void Use()
    {
        _gl.UseProgram(_handle);
    }

    public void SetUniform(string name, Matrix4x4 value)
    {
        unsafe
        {
            var location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader");
            }

            _gl.UniformMatrix4(location, 1, false, (float*)&value);
        }
    }

    public void SetUniform(string name, float value)
    {
        var location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader");
        }
        _gl.Uniform1(location, value);
    }
    
    public void SetUniform(string name, int value)
    {
        var location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader");
        }
        _gl.Uniform1(location, value);
    }

    private uint LoadShader(ShaderType type, string source)
    {
        var shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out var status);
        if (status != (int)GLEnum.True)
        {
            throw new Exception($"Failed to compile shader of type {type}: {_gl.GetShaderInfoLog(shader)}");
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