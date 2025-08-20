using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.UILib;

public class Image
{
    // Generic properties
    public Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            UpdateMesh();
        }
    }

    public Vector2 Pivot
    {
        get => _pivot;
        set
        {
            _pivot = value;
            UpdateMesh();
        }
    }

    public Vector2 Size
    {
        get => _size;
        set
        {
            _size = value;
            UpdateMesh();
        }
    }

    public Vector3 Color
    {
        get => _color;
        set
        {
            _color = value;
            UpdateMesh();
        }
    }
    
    // Image-specific properties
    public Texture? Texture { get; set; } = null;
    
    private readonly MeshRenderer _meshRenderer;
    private readonly Shader _shader;

    private Vector2 _position;
    private Vector2 _pivot;
    private Vector2 _size;
    private Vector3 _color;

    public Image(Vector2 position, Vector2 pivot, Vector2 size, Vector3 color)
    {
        _position = position;
        _pivot = pivot;
        _size = size;
        _color = color;
        
        _meshRenderer = new MeshRenderer(OpenGl.Context, GenerateMesh(), BufferUsageARB.DynamicDraw);
        _meshRenderer.SetVertexAttribute(0, 2, VertexAttribPointerType.Float, 7, 0);
        _meshRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 7, 2);
        _meshRenderer.SetVertexAttribute(2, 3, VertexAttribPointerType.Float, 7, 4);
        _meshRenderer.Unbind();
        
        _shader = Shaders.GetShader("ui");
    }

    public void Render()
    {
        if (Texture == null)
        {
            return;
        }
        
        OpenGl.Context.Enable(EnableCap.Blend);
        OpenGl.Context.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        OpenGl.Context.Disable(EnableCap.DepthTest);
        
        Texture.Bind();
        _shader.Use();
        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(
            0,
            ApplicationData.WindowDimensions.X,
            ApplicationData.WindowDimensions.Y,
            0,
            0,
            100
        );
        _shader.SetUniform("uProjection", projectionMatrix);
        _shader.SetUniform("uTexture", 0);
        _meshRenderer.Render();
        
        _meshRenderer.Unbind();
        OpenGl.Context.Enable(EnableCap.DepthTest);
        OpenGl.Context.Disable(EnableCap.Blend);
    }

    private void UpdateMesh()
    {
        _meshRenderer.UpdateMesh(GenerateMesh());
    }

    private Mesh GenerateMesh()
    {
        // The pivot is a range from 0.0 - 1.0
        // 0.5, 0.5 = center
        // 0.0, 1.0 = bottom left
        // 1.0, 0.5 = center right
        // etc...
        var offset = new Vector2(_size.X * _pivot.X, _size.Y * _pivot.Y);
        var actualPosition = _position - offset;

        return UiGeometry.CreateQuadColored(actualPosition.X, actualPosition.Y, _size.X, _size.Y, _color);
    }
}