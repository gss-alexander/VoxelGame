using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client.Blocks;

public class BlockBreaking
{
    public bool ShouldBreak =>
        _currentlyLookingAtBlock != null && _currentlyLookingAtBlock.Strength <= _currentDestruction;
    
    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly TextureArray _breakingTextureArray;
    
    private const float DestructionPerSecond = 10.0f;
    private const int DestructionTextureCount = 5;


    private readonly BufferObject<float> _vbo;
    private readonly BufferObject<uint> _ebo;
    private readonly VertexArrayObject<float, uint> _vao;

    private BlockData? _currentlyLookingAtBlock;
    private Vector3D<int> _targetPosition;
    private int _lastTexturePos;
    private float _currentDestruction;

    public BlockBreaking(GL gl, Shader shader, TextureArray breakingTextureArray)
    {
        _gl = gl;
        _shader = shader;
        _breakingTextureArray = breakingTextureArray;

        _vbo = new BufferObject<float>(gl, [], BufferTargetARB.ArrayBuffer);
        _ebo = new BufferObject<uint>(gl, [], BufferTargetARB.ElementArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(gl, _vbo, _ebo);
        
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 6, 0);
        _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 6, 3);
        _vao.VertexAttributePointer(2, 1, VertexAttribPointerType.Float, 6, 5);
        
        UpdateMeshData(0);
    }

    public void SetLookingAtBlock(BlockData blockData, Vector3D<int> position)
    {
        _currentlyLookingAtBlock = blockData;
        if (position != _targetPosition)
        {
            _currentDestruction = 0f;
        }
        _targetPosition = position;
    }

    public void ClearLookingAtBlock()
    {
        _currentlyLookingAtBlock = null;
        _currentDestruction = 0f;
    }

    public void UpdateDestruction(float deltaTime, bool isDestroying)
    {
        if (!isDestroying)
        {
            _currentDestruction = 0f;
            return;
        }

        _currentDestruction += DestructionPerSecond * deltaTime;
        var currentTexture = GetDestructionTexture();
        if (currentTexture != _lastTexturePos)
        {
            _lastTexturePos = currentTexture;
            UpdateMeshData(_lastTexturePos);
        }
    }

    public void Render(Matrix4x4 view, Matrix4x4 projection)
    {
        if (GetDestructionTexture() == 0) return;
        
        _breakingTextureArray.Bind();
        _shader.Use();

        var model = Matrix4x4.CreateTranslation(new Vector3(_targetPosition.X, _targetPosition.Y, _targetPosition.Z));
        _shader.SetUniform("uModel", model);
        _shader.SetUniform("uView", view);
        _shader.SetUniform("uProjection", projection);
        
        _shader.SetUniform("uTextureArray", 0);
            
        _vao.Bind();
        
        _gl.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, ReadOnlySpan<float>.Empty);
    }

    private void UpdateMeshData(float textureIndex)
    {
        float[] vertices =
        [
            // top face
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, textureIndex,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f, textureIndex,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, textureIndex,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, textureIndex,
        
            // bottom face
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f, textureIndex,
            0.5f, -0.5f,  0.5f,  1.0f, 1.0f, textureIndex,
            0.5f, -0.5f, -0.5f,  1.0f, 0.0f, textureIndex,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, textureIndex,
        
            // front face
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, textureIndex,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f, textureIndex,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f, textureIndex,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f, textureIndex,
        
            // back face
            0.5f, -0.5f, -0.5f,  0.0f, 0.0f, textureIndex,
            -0.5f, -0.5f, -0.5f,  1.0f, 0.0f, textureIndex,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f, textureIndex,
            0.5f,  0.5f, -0.5f,  0.0f, 1.0f, textureIndex,
        
            // left face
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, textureIndex,
            -0.5f, -0.5f,  0.5f,  1.0f, 0.0f, textureIndex,
            -0.5f,  0.5f,  0.5f,  1.0f, 1.0f, textureIndex,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, textureIndex,
        
            // right face
            0.5f, -0.5f,  0.5f,  0.0f, 0.0f, textureIndex,
            0.5f, -0.5f, -0.5f,  1.0f, 0.0f, textureIndex,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f, textureIndex,
            0.5f,  0.5f,  0.5f,  0.0f, 1.0f, textureIndex
        ];
    
        uint[] indices =
        [
            // top face
            0, 1, 2, 2, 3, 0,
            // bottom face
            4, 5, 6, 6, 7, 4,
            // front face
            8, 9, 10, 10, 11, 8,
            // back face
            12, 13, 14, 14, 15, 12,
            // left face
            16, 17, 18, 18, 19, 16,
            // right face
            20, 21, 22, 22, 23, 20
        ];
    
        _vbo.UpdateData(vertices);
        _ebo.UpdateData(indices);
    }

    private int GetDestructionTexture()
    {
        var blockBreakingProgress = Math.Clamp(_currentDestruction / _currentlyLookingAtBlock?.Strength ?? 0f, 0f, 1f);
        if (blockBreakingProgress == 0.0) return 0;
        if (blockBreakingProgress >= 1.0) return DestructionTextureCount;
        return (int)(blockBreakingProgress * DestructionTextureCount) + 1;
    }
}