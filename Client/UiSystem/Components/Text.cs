using System.Numerics;
using Client.UI.Text;
using Silk.NET.OpenGL;

namespace Client.UiSystem.Components;

public class Text : UiElement
{
    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    public float Alpha { get; set; } = 1f;
    public Vector3 Color { get; set; } = Vector3.One;
    public float FontSize { get; set; } = 14f;
    
    public string Content
    {
        get => _content;
        set => _content = value;
    }

    public TextAlignment Alignment
    {
        get => _alignment;
        set => _alignment = value;
    }
    
    private readonly MeshRenderer _meshRenderer;
    private readonly Shader _shader;
    private readonly CharacterMap _characterMap;

    private string _content = string.Empty;
    private TextAlignment _alignment;

    public Text()
    {
        _shader = Shaders.GetShader("ui_text");

        _meshRenderer = new MeshRenderer(Mesh.Empty, BufferUsageARB.DynamicDraw);
        _meshRenderer.SetVertexAttribute(0, 2, VertexAttribPointerType.Float, 4, 0);
        _meshRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 4, 2);
        _meshRenderer.Unbind();

        _characterMap = new CharacterMap(OpenGl.Context);
    }
    
    public override void Update(float deltaTime)
    {
    }

    public override void Render(float deltaTime)
    {
        var scale = FontSize / CharacterMap.BaseFontSize;
        
        float totalWidth = 0f;
        foreach (var c in Content)
        {
            var character = _characterMap.GetCharacter(c);
            totalWidth += (character.Advance >> 6) * scale;
        }

        float startX = Alignment switch
        {
            TextAlignment.Center => AbsolutePosition.X - totalWidth / 2f,
            TextAlignment.Right => AbsolutePosition.X - totalWidth,
            TextAlignment.Left => AbsolutePosition.X,
            _ => throw new NotImplementedException()
        };
        
        OpenGl.Context.Disable(EnableCap.DepthTest); 
        OpenGl.Context.Enable(EnableCap.Blend);
        OpenGl.Context.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        _shader.Use();
        _shader.SetUniform("uTexture", 0);
        _shader.SetUniform("uColor", Color);
        _shader.SetUniform("uAlpha", Alpha);
        var projectionMatrix =
            Matrix4x4.CreateOrthographicOffCenter(0, WindowDimensions.Width, WindowDimensions.Height, 0, 0, 100);
        _shader.SetUniform("uProjection", projectionMatrix);

        var currentX = startX;
        foreach (var ch in Content)
        {
            var character = _characterMap.GetCharacter(ch);

            var xPos = currentX + character.Bearing.X * scale;
            var yPos = AbsolutePosition.Y - character.Bearing.Y * scale;

            var width = character.Size.X * scale;
            var height = character.Size.Y * scale;

            var mesh = GenerateCharacterMesh(xPos, yPos, width, height);
            _meshRenderer.UpdateMesh(mesh);
            
            character.Bind();
            _meshRenderer.Render();

            currentX += (character.Advance >> 6) * scale;
        }
        
        _meshRenderer.Unbind();
        
        OpenGl.Context.Enable(EnableCap.DepthTest);
        OpenGl.Context.Disable(EnableCap.Blend);
    }

    public override bool HandleInput(Vector2 mousePosition, bool isClicked)
    {
        return false;
    }

    private Mesh GenerateCharacterMesh(float x, float y, float width, float height)
    {
        float[] vertices =
        [
            x, y,                   0.0f, 0.0f,
            x + width, y,           1.0f, 0.0f, 
            x + width, y + height,  1.0f, 1.0f,
            x, y + height,          0.0f, 1.0f
        ];

        uint[] indices =
        [
            0, 1, 2, 
            2, 3, 0
        ];

        return new Mesh(vertices, indices);
    }
}