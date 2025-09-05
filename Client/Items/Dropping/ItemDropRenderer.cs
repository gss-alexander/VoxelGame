using System.Numerics;
using Client.Items.MeshGeneration;
using Silk.NET.OpenGL;

namespace Client.Items.Dropping;

public class ItemDropRenderer : IWorldRenderable
{
    private readonly Shader _shader;
    private readonly ItemTextures _itemTextures;

    private readonly MeshRenderer _meshRenderer;

    public ItemDropRenderer(Shader shader, ItemTextures itemTextures, ItemData itemData)
    {
        _shader = shader;
        _itemTextures = itemTextures;

        var mesh = SpriteMeshGenerator.Generate(itemData, itemTextures);
        _meshRenderer = new MeshRenderer(mesh);
        _meshRenderer.SetVertexAttribute(0, 3, VertexAttribPointerType.Float, 6, 0);
        _meshRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 6, 3);
        _meshRenderer.SetVertexAttribute(2, 1, VertexAttribPointerType.Float, 6, 5);
        _meshRenderer.Unbind();
    }

    public void Render(Matrix4x4 view, Matrix4x4 projection, Vector3 position, float scale, float rotation = 0f)
    {
        _itemTextures.Use(ItemTextures.ItemTextureType.Item);
        
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