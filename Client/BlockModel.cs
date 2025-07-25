using System.Numerics;
using Silk.NET.OpenGL;

namespace Client;

public class BlockModel
{
    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly TextureArray _textureArray;
    private readonly Vector3 _worldPos;

    private readonly BufferObject<float> _vbo;
    private readonly BufferObject<uint> _ebo;
    private readonly VertexArrayObject<float, uint> _vao;

    private readonly List<float> _vertices;
    
    public BlockModel(GL gl, BlockType blockType, Shader shader, TextureArray textureArray, Vector3 worldPos)
    {
        _gl = gl;
        _shader = shader;
        _textureArray = textureArray;
        _worldPos = worldPos;

        _vertices = new List<float>();
        // var indices = new List<uint>();
        
        var faces = BlockData.Faces;
        foreach (var face in faces)
        {
            var textureIndex = blockType.GetTextureIndex(face.Direction);
            for (var vertexIndex = 0; vertexIndex < face.Vertices.Length; vertexIndex += 6)
            {
                var vX = face.Vertices[vertexIndex] + worldPos.X;
                var vY = face.Vertices[vertexIndex + 1] + worldPos.Y;
                var vZ = face.Vertices[vertexIndex + 2] + worldPos.Z;
                var vU = face.Vertices[vertexIndex + 3];
                var vV = face.Vertices[vertexIndex + 4];
        
                _vertices.Add(vX);
                _vertices.Add(vY);
                _vertices.Add(vZ);
                _vertices.Add(vU);
                _vertices.Add(vV);
                _vertices.Add(textureIndex);
                _vertices.Add(1.0f); // full brightness - todo: handle this better
            }
        }

        _gl.BindVertexArray(0);
        _vbo = new BufferObject<float>(_gl, _vertices.ToArray(), BufferTargetARB.ArrayBuffer);
        _ebo = new BufferObject<uint>(_gl, [], BufferTargetARB.ElementArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);
        
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 7, 0); // Position
        _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 7, 3); // UV
        _vao.VertexAttributePointer(2, 1, VertexAttribPointerType.Float, 7, 5); // Texture Index
        _vao.VertexAttributePointer(3, 1, VertexAttribPointerType.Float, 7, 6); // Brightness
    }

    public void Render(Matrix4x4 view, Matrix4x4 projection)
    {
        _shader.Use();
        _shader.SetUniform("uView", view);
        _shader.SetUniform("uProjection", projection);
        _shader.SetUniform("uTextureArray", 0);
        var centeredModel = Matrix4x4.CreateTranslation(-_worldPos) * 
                            Matrix4x4.CreateScale(0.15f) * 
                            Matrix4x4.CreateTranslation(_worldPos); 
    
        _shader.SetUniform("uModel", centeredModel); 
        
        _textureArray.Bind();
        _vao.Bind();
        _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)(_vertices.Count / 7));
    }
}