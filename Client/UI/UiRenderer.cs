using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.UI;
public class UiRenderer
{
    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly Texture _texture;
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    private readonly BufferObject<float> _vbo;
    private readonly BufferObject<uint> _ebo;
    private readonly VertexArrayObject<float, uint> _vao;

    private readonly float[] _vertices;
    private readonly uint[] _indices;

    public UiRenderer(GL gl, Shader shader, Texture texture, int screenWidth, int screenHeight)
    {
        _gl = gl;
        _shader = shader;
        _texture = texture;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        (_vertices, _indices) = CreateHotbarMeshData();
        _vbo = new BufferObject<float>(_gl, _vertices, BufferTargetARB.ArrayBuffer);
        _ebo = new BufferObject<uint>(_gl, _indices, BufferTargetARB.ElementArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);
        
        // Configure vertex attributes: position (2 floats) + texture coords (2 floats) = 4 floats per vertex
        _vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 4, 0); // Position
        _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 4, 2); // Texture coords
    }

    public void Render(int screenWidth, int screenHeight)
    {
        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, 0, 100);
        
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Disable(EnableCap.DepthTest);
        
        _shader.Use();
        _shader.SetUniform("uProjection", projectionMatrix);
        _shader.SetUniform("uTexture", 0); // Texture unit 0
        
        _texture.Bind(TextureUnit.Texture0);
        _vao.Bind();
        _gl.DrawElements(PrimitiveType.Triangles, (uint)_indices.Length, DrawElementsType.UnsignedInt, ReadOnlySpan<uint>.Empty);

        _gl.Enable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.Blend); 
        
        _gl.Enable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.Blend);
    }

    private Tuple<float[], uint[]> CreateHotbarMeshData()
    {
        var vertices = new List<float>();
        var indices = new List<uint>();

        const int slotCount = 9;
        const float yPosition = 950f;
        const float baseXPosition = 500f;
        const float spacing = 20f;
        const float slotWidth = 75f;
        const float slotHeight = 75f;

        var screenCenter = _screenWidth / 2f;
        var totalHotbarWidth = (slotCount * slotWidth) + (spacing * (slotCount - 1));
        var baseX = screenCenter - totalHotbarWidth / 2f;

        var indicesOffset = 0u;
        for (var i = 0; i < slotCount; i++)
        {
            var xOffset = i * (slotWidth + spacing);  // Include slot width
            var x = baseX + xOffset; 
            vertices.AddRange(CreateQuad(x, yPosition, slotWidth, slotHeight, 0f, 0f, 1f, 1f));
            indices.AddRange(CreateIndices(ref indicesOffset));
            indicesOffset += 4;
        }

        return new Tuple<float[], uint[]>(vertices.ToArray(), indices.ToArray());
    }
    
    private static float[] CreateQuad(float x, float y, float width, float height, float u1, float v1, float u2, float v2)
    {
        return
        [
            x, y, u1, v1,                           // top left
            x + width, y, u2, v1,                   // top right
            x + width, y + height, u2, v2,          // bottom right
            x, y + height, u1, v2                   // bottom left (fixed v coordinate)
        ];
    }

    private static uint[] CreateIndices(ref uint indexOffset)
    {
        return [indexOffset, indexOffset + 1, indexOffset + 2, indexOffset + 2, indexOffset + 3, indexOffset];
    }
}