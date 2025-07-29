using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.Items;

public class ItemDropRenderer
{
    private readonly GL _gl;
    private readonly Mesh _mesh;
    private readonly Shader _shader;
    private readonly ItemTextures _itemTextures;

    private readonly MeshRenderer _meshRenderer;

    public ItemDropRenderer(GL gl, Mesh mesh, Shader shader, ItemTextures itemTextures)
    {
        _gl = gl;
        _mesh = mesh;
        _shader = shader;
        _itemTextures = itemTextures;

        _meshRenderer = new MeshRenderer(_gl, _mesh);
        _meshRenderer.SetVertexAttribute(0, 3, VertexAttribPointerType.Float, 6, 0);
        _meshRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 6, 3);
        _meshRenderer.SetVertexAttribute(2, 1, VertexAttribPointerType.Float, 6, 5);
    }

    public void Render(Matrix4x4 view, Matrix4x4 projection)
    {
        _itemTextures.Use();
        
        _shader.Use();
        _shader.SetUniform("uProjection", projection);
        _shader.SetUniform("uView", view);
        _shader.SetUniform("uModel", Matrix4x4.CreateScale(0.03f) * Matrix4x4.CreateTranslation(0f, 1f, 0f));
        _shader.SetUniform("uTextureArray", 0);
        
        _meshRenderer.Render();
    }
}