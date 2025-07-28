using Silk.NET.OpenGL;

namespace Client;

public class BufferObject<TDataType> : IDisposable where TDataType : unmanaged
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

    public unsafe void UpdateData(TDataType[] data, BufferUsageARB usage = BufferUsageARB.DynamicDraw)
    {
        Bind();
        _gl.BufferData(_target,
            (nuint)(data.Length * sizeof(TDataType)),
            new ReadOnlySpan<TDataType>(data),
            usage);
    }

    public void Bind()
    {
        _gl.BindBuffer(_target, _handle);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_handle);
    }
}