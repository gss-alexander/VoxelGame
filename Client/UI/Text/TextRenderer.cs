using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.UI.Text;

public class TextRenderer
{
    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly CharacterMap _characterMap;

    private readonly BufferObject<float> _vbo;
    private readonly BufferObject<uint> _ebo;
    private readonly VertexArrayObject<float, uint> _vao;

    public TextRenderer(GL gl, Shader shader, CharacterMap characterMap)
    {
        _gl = gl;
        _shader = shader;
        _characterMap = characterMap;

        _gl.BindVertexArray(0);
        _vbo = new BufferObject<float>(_gl, [], BufferTargetARB.ArrayBuffer, BufferUsageARB.DynamicDraw);
        _ebo = new BufferObject<uint>(_gl, [], BufferTargetARB.ElementArrayBuffer, BufferUsageARB.DynamicDraw); // ebo will not be used as we are using drawArrays for now
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);
        _vao.VertexAttributePointer(0, 4, VertexAttribPointerType.Float, 4, 0);
    }

    public void RenderText(string text, float x, float y, float scale, Vector3 color, int screenWidth, int screenHeight)
    {
        _gl.Disable(EnableCap.DepthTest); 
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        _gl.ActiveTexture(TextureUnit.Texture0);
        _shader.Use();
        _shader.SetUniform("text", 0);  // <- Add this line
        _shader.SetUniform("textColor", color);
        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, 0, screenHeight, 0, 100);
        _shader.SetUniform("projection", projectionMatrix);
        _vao.Bind();

        foreach (var c in text)
        {
            var ch = _characterMap.GetCharacter(c);

            var xPos = x + ch.Bearing.X * scale;
            var yPos = y - (ch.Size.Y - ch.Bearing.Y) * scale;

            var w = ch.Size.X * scale;
            var h = ch.Size.Y * scale;
            float[] vertices =
            {
                xPos, yPos + h, 0.0f, 0.0f,      // Top-left
                xPos, yPos, 0.0f, 1.0f,          // Bottom-left  
                xPos + w, yPos, 1.0f, 1.0f,      // Bottom-right
    
                xPos, yPos + h, 0.0f, 0.0f,      // Top-left
                xPos + w, yPos, 1.0f, 1.0f,      // Bottom-right
                xPos + w, yPos + h, 1.0f, 0.0f   // Top-right
            };
            
            ch.Bind(); // <--- Added this
            _vbo.UpdateData(vertices);
            _gl.DrawArrays(GLEnum.Triangles, 0, 6);
            x += (ch.Advance >> 6) * scale; // bitshift by 6 to get value in pixels (2^6 = 64)
        }
        
        _gl.Enable(EnableCap.DepthTest); 
    }
}