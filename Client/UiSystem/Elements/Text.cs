using System.Numerics;
using Client.UI.Text;
using Silk.NET.OpenGL;

namespace Client.UiSystem.Elements;

public class Text : UiElement
{
    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right
    }
    
    public enum VerticalAlignment
    {
        Top,
        Middle,
        Bottom
    }

    public float Alpha { get; set; } = 1f;
    public Vector3 Color { get; set; } = Vector3.One;
    public float FontSize { get; set; } = 14f;
    public VerticalAlignment VerticalAlign { get; set; } = VerticalAlignment.Top;
    public string Content { get; set; } = string.Empty;
    public HorizontalAlignment HorizontalAlign { get; set; }

    private readonly MeshRenderer _meshRenderer;
    private readonly Shader _shader;
    private readonly CharacterMap _characterMap;

    public Text()
    {
        _shader = Shaders.GetShader("ui_text");

        _meshRenderer = new MeshRenderer(Mesh.Empty, BufferUsageARB.DynamicDraw);
        _meshRenderer.SetVertexAttribute(0, 2, VertexAttribPointerType.Float, 4, 0);
        _meshRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 4, 2);
        _meshRenderer.Unbind();

        _characterMap = CharacterMap.LoadForFont("Roboto-VariableFont_wdth,wght");
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

        float textStartX = HorizontalAlign switch
        {
            HorizontalAlignment.Left => AbsolutePosition.X,
            HorizontalAlignment.Center => AbsolutePosition.X + (Size.X - totalWidth) / 2f,
            HorizontalAlignment.Right => AbsolutePosition.X + Size.X - totalWidth,
            _ => throw new NotImplementedException()
        };

        float maxAscent = 0f;
        float maxDescent = 0f;
    
        foreach (var c in Content)
        {
            var character = _characterMap.GetCharacter(c);
            float ascent = character.Bearing.Y * scale;
            float descent = (character.Size.Y - character.Bearing.Y) * scale;
        
            maxAscent = Math.Max(maxAscent, ascent);
            maxDescent = Math.Max(maxDescent, descent);
        }
    
        float textHeight = maxAscent + maxDescent;
    
        float baselineY = VerticalAlign switch
        {
            VerticalAlignment.Top => AbsolutePosition.Y + maxAscent,
            VerticalAlignment.Middle => AbsolutePosition.Y + (Size.Y - textHeight) / 2f + maxAscent,
            VerticalAlignment.Bottom => AbsolutePosition.Y + Size.Y - maxDescent,
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

        var currentX = textStartX;
        foreach (var ch in Content)
        {
            var character = _characterMap.GetCharacter(ch);

            var xPos = currentX + character.Bearing.X * scale;
            var yPos = baselineY - character.Bearing.Y * scale;

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

    private static Mesh GenerateCharacterMesh(float x, float y, float width, float height)
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