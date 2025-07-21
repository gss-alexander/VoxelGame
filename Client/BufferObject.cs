using Silk.NET.OpenGL;

namespace Client;

public class BufferObject<TDataType> where TDataType : unmanaged
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly BufferTargetARB _target;

    public unsafe BufferObject(GL gl, TDataType[] data, BufferTargetARB target)
    {
        _gl = gl;
        _target = target;
        
        _handle = _gl.GenBuffer();
        Bind();
        _gl.BufferData(target,
            (nuint)(data.Length * sizeof(TDataType)),
            new ReadOnlySpan<TDataType>(data),
            BufferUsageARB.StaticDraw);
    }

    public void Bind()
    {
        _gl.BindBuffer(_target, _handle);
    }
}