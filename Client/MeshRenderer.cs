using Silk.NET.OpenGL;

namespace Client;

public class MeshRenderer : IDisposable
{
    public int VertexCount => _mesh.Vertices.Length;

    private readonly GL _gl;
    
    private readonly BufferObject<float> _vbo;
    private readonly BufferObject<uint> _ebo;
    private readonly VertexArrayObject<float, uint> _vao;

    private Mesh _mesh;

    public static MeshRenderer Empty(GL gl) => new(gl, Mesh.Empty);
    
    public MeshRenderer(GL gl, Mesh mesh, BufferUsageARB usage = BufferUsageARB.StaticDraw)
    {
        _gl = gl;
        _mesh = mesh;
        
        _vbo = new BufferObject<float>(_gl, mesh.Vertices, BufferTargetARB.ArrayBuffer, usage);
        _ebo = new BufferObject<uint>(_gl, mesh.Indices, BufferTargetARB.ElementArrayBuffer, usage);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);
    }

    public void SetVertexAttribute(int index, int count, VertexAttribPointerType pointerType, int vertexSize,
        int offset)
    {
        _vao.VertexAttributePointer((uint)index, count, pointerType, (uint)vertexSize, offset);
    }

    public void Unbind()
    {
        _gl.BindVertexArray(0);
    }

    public void Render()
    {
        _vao.Bind();
        _gl.DrawElements(PrimitiveType.Triangles, 
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
        Unbind();
    }
        
    public void Dispose()
    {
        _ebo.Dispose();
        _vbo.Dispose();
        _vao.Dispose();
    }
}