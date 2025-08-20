using System.Numerics;
using Client.UI.Text;
using Client.UILib.Core.TextRendering;
using Silk.NET.OpenGL;

namespace Client.UILib;

public class Text
{
    // Text-specific properties
    public string Content { get; set; } = string.Empty;
    public float FontSize { get; set; } = 1;
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;
    public Vector3 TextColor { get; set; } = new(1f, 1f, 1f);
    public Font Font { get; set; } = Fonts.Default;

    // Generic properties
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Pivot { get; set; }

    private readonly MeshRenderer _meshRenderer;
    private readonly Shader _shader;

    public Text(Vector2 position, Vector2 size, Vector2 pivot)
    {
        Position = position;
        Size = size;
        Pivot = pivot;

        _meshRenderer = new MeshRenderer(OpenGl.Context, Mesh.Empty, BufferUsageARB.DynamicDraw);
        _meshRenderer.SetVertexAttribute(0, 4, VertexAttribPointerType.Float, 4, 0);
        _meshRenderer.Unbind();
        
        _shader = Shaders.GetShader("text");
    }

    public void Render()
    {
        if (string.IsNullOrEmpty(Content))
        {
            return;
        }
        
        OpenGl.Context.Disable(EnableCap.DepthTest);
        OpenGl.Context.Enable(EnableCap.Blend);
        OpenGl.Context.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        _shader.Use();
        _shader.SetUniform("text", 0);
        _shader.SetUniform("textColor", TextColor);
        
        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(
            0,
            ApplicationData.WindowDimensions.X,
            ApplicationData.WindowDimensions.Y,
            0,
            0,
            100
        );
        _shader.SetUniform("projection", projectionMatrix);
        
        RenderText();
        
        OpenGl.Context.Enable(EnableCap.DepthTest);
        OpenGl.Context.Disable(EnableCap.Blend);
    }

    private void RenderText()
    {
        var position = GetRealPosition();
        var currentX = CalculateStartingX();
        foreach (var letter in Content)
        {
            var character = Font.GetCharacter(letter);
            var characterMesh = GenerateCharacterMesh(currentX, position.Y, character);
            character.Bind();
            _meshRenderer.UpdateMesh(characterMesh);
            _meshRenderer.Render();
            currentX += (character.Advance >> 6) * FontSize;
        }
        _meshRenderer.Unbind();
    }

    private Mesh GenerateCharacterMesh(float x, float y, Character character)
    {
        var xPosition = x + character.Bearing.X * FontSize;
        var yPosition = y - character.Bearing.Y * FontSize; 

        var width = character.Size.X * FontSize;
        var height = character.Size.Y * FontSize;

        return UiGeometry.CreateQuad(xPosition, yPosition, width, height);
    }
    
    private float CalculateStartingX()
    {
        var totalWidth = CalculateTotalTextWidth();
        var position = GetRealPosition();

        return Alignment switch
        {
            TextAlignment.Center => position.X - totalWidth / 2f,
            TextAlignment.Right => position.X - totalWidth,
            TextAlignment.Left => position.X,
            _ => throw new NotImplementedException()
        };
    }

    private float CalculateTotalTextWidth()
    {
        var totalWidth = 0f;
        foreach (var letter in Content)
        {
            var character = Font.GetCharacter(letter);
            totalWidth += (character.Advance >> 6) * FontSize;
        }
        return totalWidth;
    }

    private Vector2 GetRealPosition()
    {
        var offset = new Vector2(Size.X * Pivot.X, Size.Y * Pivot.Y);
        return Position - offset;
    }
}