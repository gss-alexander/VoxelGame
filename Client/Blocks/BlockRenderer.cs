using System.Numerics;
using Client.Items;
using Silk.NET.OpenGL;

namespace Client.Blocks;

public class BlockRenderer : IWorldRenderable
{
    private readonly BlockTextures _blockTextures;
    private readonly MeshRenderer _meshRenderer;
    private readonly Shader _shader;
    
    public BlockRenderer(GL gl, BlockItemData blockItemData, BlockDatabase blockDatabase, BlockTextures blockTextures)
    {
        _blockTextures = blockTextures;
        _shader = Shaders.GetShader(gl, "shader");

        var mesh = BlockMeshGenerator.Generate(blockItemData, blockDatabase, blockTextures);
        _meshRenderer = new MeshRenderer(gl, mesh);
        _meshRenderer.SetVertexAttribute(0, 3, VertexAttribPointerType.Float, 7, 0); // Position
        _meshRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 7, 3); // UV
        _meshRenderer.SetVertexAttribute(2, 1, VertexAttribPointerType.Float, 7, 5); // Texture Index
        _meshRenderer.SetVertexAttribute(3, 1, VertexAttribPointerType.Float, 7, 6); // Brightness
        _meshRenderer.Unbind();
    }
    
    public void Render(Matrix4x4 view, Matrix4x4 projection, Vector3 position, float scale, float rotation = 0)
    {
        _blockTextures.Textures.Bind();
        
        _shader.Use();
        _shader.SetUniform("uTextureArray", 0);
        _shader.SetUniform("uProjection", projection);
        _shader.SetUniform("uView", view);
        _shader.SetUniform("uModel",
            Matrix4x4.CreateScale(scale) *
            Matrix4x4.CreateRotationY(rotation) *
            Matrix4x4.CreateTranslation(position)
        );
        
        _meshRenderer.Render();
    }
}