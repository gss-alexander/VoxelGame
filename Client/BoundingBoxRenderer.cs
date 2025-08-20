using System.Numerics;
using Silk.NET.OpenGL;

namespace Client;
public class BoundingBoxRenderer
{
    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly BufferObject<float> _vbo;
    private readonly BufferObject<uint> _ebo;
    private readonly VertexArrayObject<float, uint> _vao;

    private static readonly float[] LineVertices = {
        // Bottom face vertices
        -0.5f, -0.5f, -0.5f, // 0: bottom-left-back
         0.5f, -0.5f, -0.5f, // 1: bottom-right-back
         0.5f, -0.5f,  0.5f, // 2: bottom-right-front
        -0.5f, -0.5f,  0.5f, // 3: bottom-left-front
        
        // Top face vertices
        -0.5f,  0.5f, -0.5f, // 4: top-left-back
         0.5f,  0.5f, -0.5f, // 5: top-right-back
         0.5f,  0.5f,  0.5f, // 6: top-right-front
        -0.5f,  0.5f,  0.5f  // 7: top-left-front
    };

    private static readonly uint[] LineIndices = {
        // Bottom face
        0, 1, 1, 2, 2, 3, 3, 0,
        // Top face
        4, 5, 5, 6, 6, 7, 7, 4,
        // Vertical edges
        0, 4, 1, 5, 2, 6, 3, 7
    };

    public BoundingBoxRenderer(GL gl)
    {
        _gl = gl;
        _shader = Shaders.GetShader( "line");

        _gl.BindVertexArray(0);
        _vbo = new BufferObject<float>(_gl, LineVertices, BufferTargetARB.ArrayBuffer);
        _ebo = new BufferObject<uint>(_gl, LineIndices, BufferTargetARB.ElementArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);
        
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 3, 0); // Position only
    }

    public void RenderBoundingBox(Vector3 position, Vector3 size, Matrix4x4 view, Matrix4x4 projection, Vector3 color = default)
    {
        unsafe
        {
            if (color == default)
                color = new Vector3(1.0f, 0.0f, 0.0f); // Default red color

            _shader.Use();
        
            // Create model matrix with position and scale
            var model = Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(position);
        
            _shader.SetUniform("uModel", model);
            _shader.SetUniform("uView", view);
            _shader.SetUniform("uProjection", projection);
            _shader.SetUniform("uColor", color);

            _vao.Bind();
        
            _gl.DrawElements(PrimitiveType.Lines, (uint)LineIndices.Length, DrawElementsType.UnsignedInt, null);
        }
    }
}