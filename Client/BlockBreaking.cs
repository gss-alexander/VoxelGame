using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client;

public class BlockBreaking
{
    private readonly GL _gl;
    private readonly Shader _shader;
    public float DestructionAmount => _currentDestruction;
    
    private const float DestructionPerSecond = 10.0f;

    private float _currentDestruction;
    private Vector3D<int>? _currentTarget;

    private readonly BufferObject<float> _vbo;
    private readonly BufferObject<uint> _ebo;
    private readonly VertexArrayObject<float, uint> _vao;

    public BlockBreaking(GL gl, Shader shader)
    {
        _gl = gl;
        _shader = shader;
        
        _vbo = new BufferObject<float>(gl, [], BufferTargetARB.ArrayBuffer);
        _ebo = new BufferObject<uint>(gl, [], BufferTargetARB.ElementArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(gl, _vbo, _ebo);
        UpdateMeshData(0);
    }

    public void UpdateDestruction(float deltaTime, Vector3D<int>? target, bool isDestroying)
    {
        if (!isDestroying)
        {
            _currentDestruction = 0f;
            return;
        }

        if (!target.HasValue)
        {
            _currentDestruction = 0f;
            return;
        }

        if (_currentTarget.HasValue && target.Value != _currentTarget.Value)
        {
            _currentDestruction = 0f;
        }

        _currentTarget = target;
        
        _currentDestruction += DestructionPerSecond * deltaTime;
    }

    public void Render()
    {
        if (!_currentTarget.HasValue) return;
        
        _shader.Use();

        var pos = _currentTarget.Value;
        var model = Matrix4x4.CreateTranslation(new Vector3(pos.X, pos.Y, pos.Z));
        _shader.SetUniform("uModel", model);
            
        _vao.Bind();
        
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, ReadOnlySpan<float>.Empty);
    }

    private void UpdateMeshData(float textureIndex)
    {
        float[] vertices =
        [
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 1.0f, textureIndex,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 1.0f, textureIndex,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 1.0f, textureIndex,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 1.0f, textureIndex
        ];
        uint[] indices =
        [
            0, 1, 2, 2, 3, 0
        ];
        
        _vbo.UpdateData(vertices);
        _ebo.UpdateData(indices);
    }
}