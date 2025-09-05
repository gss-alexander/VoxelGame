using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.UI.Text;

public class TextRenderer
{
    private readonly Shader _shader;
    private readonly CharacterMap _characterMap;
    private readonly MeshRenderer _meshRenderer;

    public TextRenderer(Shader shader, CharacterMap characterMap)
    {
        _shader = shader;
        _characterMap = characterMap;

        _meshRenderer = new MeshRenderer(Mesh.Empty, BufferUsageARB.DynamicDraw);
        _meshRenderer.SetVertexAttribute(0, 4, VertexAttribPointerType.Float, 4, 0);
        _meshRenderer.Unbind();
    }

    public void RenderText(string text, float x, float y, float scale, Vector3 color, int screenWidth, int screenHeight, TextAlignment alignment = TextAlignment.Left)
    {
        if (string.IsNullOrEmpty(text)) return;

        float totalWidth = 0;
        foreach (var c in text)
        {
            var ch = _characterMap.GetCharacter(c);
            totalWidth += (ch.Advance >> 6) * scale;
        }

        float startX = alignment switch
        {
            TextAlignment.Center => x - totalWidth / 2f,
            TextAlignment.Right => x - totalWidth,
            _ => x // Left alignment (default)
        };

        OpenGl.Context.Disable(EnableCap.DepthTest); 
        OpenGl.Context.Enable(EnableCap.Blend);
        OpenGl.Context.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    
        _shader.Use();
        _shader.SetUniform("text", 0);
        _shader.SetUniform("textColor", color);
    
        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, 0, 100);
        _shader.SetUniform("projection", projectionMatrix);

        var currentX = startX;
    
        foreach (var c in text)
        {
            var ch = _characterMap.GetCharacter(c);

            var xPos = currentX + ch.Bearing.X * scale;
            var yPos = y - (ch.Size.Y - ch.Bearing.Y) * scale;

            var w = ch.Size.X * scale;
            var h = ch.Size.Y * scale;

            var mesh = GenerateCharacterMesh(xPos, yPos, w, h);
        
            ch.Bind();
            _meshRenderer.UpdateMesh(mesh);
            _meshRenderer.Render();
        
            currentX += (ch.Advance >> 6) * scale;
        }
    
        _meshRenderer.Unbind();
        OpenGl.Context.Enable(EnableCap.DepthTest);
        OpenGl.Context.Disable(EnableCap.Blend);
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