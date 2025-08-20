using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.Clouds;

public class CloudRenderer
{
    private readonly MeshRenderer _meshRenderer;
    private readonly Shader _cloudShader;
    
    public CloudRenderer(GL gl, Mesh cloudsMesh)
    {
        _cloudShader = Shaders.GetShader("cloud");
        
        _meshRenderer = new MeshRenderer(gl, cloudsMesh);
        _meshRenderer.SetVertexAttribute(0, 3, VertexAttribPointerType.Float, 4, 0);
        _meshRenderer.SetVertexAttribute(1, 1, VertexAttribPointerType.Float, 4, 3);
        _meshRenderer.Unbind();
    }

    public void Render(Vector3 worldPosition, Matrix4x4 view, Matrix4x4 projection)
    {
        var model = Matrix4x4.CreateTranslation(worldPosition);
        
        _cloudShader.Use();
        _cloudShader.SetUniform("uModel", model);
        _cloudShader.SetUniform("uView", view);
        _cloudShader.SetUniform("uProjection", projection);
        _meshRenderer.Render();
    }
}