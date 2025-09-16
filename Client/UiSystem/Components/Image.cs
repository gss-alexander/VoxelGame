using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.UiSystem.Components;

public class Image : UiElement
{
    public Vector3 Color { get; set; }
    public float Alpha { get; set; }
    public Texture? Sprite { get; set; }

    private readonly MeshRenderer _meshRenderer;
    private readonly Shader _shader;

    public Image()
    {
        _shader = Shaders.GetShader("ui_image");
        
        _meshRenderer = new MeshRenderer(GenerateQuadMesh(), BufferUsageARB.DynamicDraw);
        _meshRenderer.SetVertexAttribute(0, 2, VertexAttribPointerType.Float, 4, 0);
        _meshRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 4, 2);
        _meshRenderer.Unbind();
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Render(float deltaTime)
    {
        if (Sprite == null) return;

        if (IsDirty)
        {
            _meshRenderer.UpdateMesh(GenerateQuadMesh());
            IsDirty = false;
        }
        
        OpenGl.Context.Enable(EnableCap.Blend);
        OpenGl.Context.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        OpenGl.Context.Disable(EnableCap.DepthTest);
        
        Sprite.Bind(TextureUnit.Texture0);
        
        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(
            0,
            WindowDimensions.Width,
            WindowDimensions.Height,
            0,
            0,
            100
        );
        
        _shader.Use();
        _shader.SetUniform("uProjection", projectionMatrix);
        
        _shader.SetUniform("uTexture", 0);
        _shader.SetUniform("uColor", Color);
        _shader.SetUniform("uAlpha", Alpha);
        
        _meshRenderer.Render();
        _meshRenderer.Unbind();
        
        OpenGl.Context.Enable(EnableCap.DepthTest);
        OpenGl.Context.Disable(EnableCap.Blend);

        foreach (var child in Children)
        {
            child.Render(deltaTime);
        }
    }

    public override void HandleInput(Vector2 mousePosition, bool isClicked)
    {
    }

    private Mesh GenerateQuadMesh()
    {
        float[] vertices =
        [
            AbsolutePosition.X, AbsolutePosition.Y + Size.Y, 0.0f, 1.0f, // bottom left
            AbsolutePosition.X + Size.X, AbsolutePosition.Y + Size.Y, 1.0f, 1.0f, // bottom right
            AbsolutePosition.X + Size.X, AbsolutePosition.Y, 1.0f, 0.0f, // top right
            AbsolutePosition.X, AbsolutePosition.Y, 0.0f, 0.0f, // top left
        ];

        uint[] indices = [0, 1, 2, 2, 3, 0];

        return new Mesh(vertices, indices);
    }
}