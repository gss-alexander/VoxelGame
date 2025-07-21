using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client;

public class Chunk
{
    public const int Size = 16; 

    private readonly BlockType[] _blocks;
    private Vector2D<int> _position;

    private Mesh _mesh;
    private GL _gl;
    private VertexArrayObject<float, uint> _vao;
    private BufferObject<float> _vbo;
    private BufferObject<uint> _ebo;
    private bool _isInitialized;

    public Chunk(int chunkX, int chunkY)
    {
        _position = new Vector2D<int>(chunkX, chunkY);
        _blocks = new BlockType[Size * Size * Size];
        Fill(BlockType.Dirt);
        RandomFill();
        _mesh = GenerateMesh();
    }

    public void Initialize(GL gl)
    {
        _gl = gl;
        
        // Create buffers and VAO for this chunk
        _ebo = new BufferObject<uint>(_gl, _mesh.Indices, BufferTargetARB.ElementArrayBuffer);
        _vbo = new BufferObject<float>(_gl, _mesh.Vertices, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);
        
        // Set up vertex attributes
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 6, 0);
        _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 6, 3);
        _vao.VertexAttributePointer(2, 1, VertexAttribPointerType.Float, 6, 5);
        
        _isInitialized = true;
    }

    public void Render()
    {
        if (!_isInitialized) return;
        
        _vao.Bind();
        _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)(_mesh.Vertices.Length / 6));
    }

    public void Fill(BlockType block)
    {
        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                for (var z = 0; z < Size; z++)
                {
                    SetBlock(x, y, z, block);
                }
            }
        }
    }

    private void RandomFill()
    {
        var rng = new Random(DateTime.Now.Millisecond);
        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                for (var z = 0; z < Size; z++)
                {
                    var randomNumber = rng.Next(0, 4);
                    if (randomNumber == 0)
                    {
                        SetBlock(x, y, z, BlockType.Air);
                    }
                    
                    else if (randomNumber == 1)
                    {
                        SetBlock(x, y, z, BlockType.Dirt);
                    }

                    else
                    {
                        SetBlock(x, y, z, BlockType.Stone);
                    }
                }
            }
        }
    }

    public void SetBlock(int x, int y, int z, BlockType block)
    {
        var position = PositionToBlockIndex(x, y, z);
        _blocks[position] = block;
    }

    public BlockType GetBlock(int x, int y, int z)
    {
        var position = PositionToBlockIndex(x, y, z);
        return _blocks[position];
    }

    public bool IsBlockSolid(int x, int y, int z)
    {
        var block = GetBlock(x, y, z);
        return block != BlockType.Air;
    }

    public Mesh GenerateMesh()
    {
        var vertices = new List<float>();
        var indices = new List<uint>();

        var indexOffset = 0u;
        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                for (var z = 0; z < Size; z++)
                {
                    var blockType = GetBlock(x, y, z);
                    if (blockType == BlockType.Air)
                    {
                        continue;
                    }

                    var textureIndex = GetTextureIndex(blockType);
                    
                    for (var vertexIndex = 0; vertexIndex < BlockData.Vertices.Length; vertexIndex += 5)
                    {
                        var vX = BlockData.Vertices[vertexIndex] + x + (Size * _position.X);
                        var vY = BlockData.Vertices[vertexIndex + 1] + y;
                        var vZ = BlockData.Vertices[vertexIndex + 2] + z + (Size *  _position.Y);
                        var vU = BlockData.Vertices[vertexIndex + 3];
                        var vV = BlockData.Vertices[vertexIndex + 4];
                        vertices.Add(vX);
                        vertices.Add(vY);
                        vertices.Add(vZ);
                        vertices.Add(vU);
                        vertices.Add(vV);
                        vertices.Add(textureIndex);
                    }

                    foreach (var index in BlockData.Indices)
                    {
                        indices.Add(index + indexOffset);
                    }

                    indexOffset += 36;
                }
            }
        }

        return new Mesh(vertices.ToArray(), indices.ToArray());
    }

    private static float GetTextureIndex(BlockType blockType)
    {
        return blockType switch
        {
            BlockType.Air => 0f,
            BlockType.Dirt => 0f,
            BlockType.Stone => 1f,
            _ => throw new NotImplementedException()
        };
    }
    
    private static int PositionToBlockIndex(int x, int y, int z)
    {
        return (x + (y * Size)) + (z * Size * Size);
    }
}