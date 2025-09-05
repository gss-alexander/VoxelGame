using Silk.NET.OpenGL;

namespace Client;

public class MeshRenderer : IDisposable
{
    public int VertexCount => _mesh.Vertices.Length;

    
    private readonly BufferObject<float> _vbo;
    private readonly BufferObject<uint> _ebo;
    private readonly VertexArrayObject<float, uint> _vao;

    private Mesh _mesh;

    public static MeshRenderer Empty() => new(Mesh.Empty);
    
    public MeshRenderer(Mesh mesh, BufferUsageARB usage = BufferUsageARB.StaticDraw)
    {
        _mesh = mesh;
        
        _vbo = new BufferObject<float>(OpenGl.Context, mesh.Vertices, BufferTargetARB.ArrayBuffer, usage);
        _ebo = new BufferObject<uint>(OpenGl.Context, mesh.Indices, BufferTargetARB.ElementArrayBuffer, usage);
        _vao = new VertexArrayObject<float, uint>(OpenGl.Context, _vbo, _ebo);
    }

    public void SetVertexAttribute(int index, int count, VertexAttribPointerType pointerType, int vertexSize,
        int offset)
    {
        _vao.VertexAttributePointer((uint)index, count, pointerType, (uint)vertexSize, offset);
    }

    public void Unbind()
    {
        OpenGl.Context.BindVertexArray(0);
    }

    public void Render()
    {
        _vao.Bind();
        OpenGl.Context.DrawElements(PrimitiveType.Triangles, 
            (uint)_mesh.Indices.Length, 
            DrawElementsType.UnsignedInt,
            ReadOnlySpan<float>.Empty);
    }

    public void UpdateMesh(Mesh newMesh)
    {
        _vao.Bind();
        _vbo.UpdateData(newMesh.Vertices);
        _ebo.UpdateData(newMesh.Indices);
        _mesh = newMesh;
    }
        
    public void Dispose()
    {
        _ebo.Dispose();
        _vbo.Dispose();
        _vao.Dispose();
    }
}