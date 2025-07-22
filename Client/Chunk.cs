using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client;

public class Chunk
{
    public Vector2D<int> Position => _position;
    
    public const int Size = 16; 

    private readonly BlockType[] _blocks;
    private Vector2D<int> _position;

    private Mesh _mesh;
    private GL _gl;
    private VertexArrayObject<float, uint> _vao;
    private BufferObject<float> _vbo;
    private BufferObject<uint> _ebo;
    private bool _isInitialized;

    public static Vector2D<int> WorldToChunkPosition(Vector3 worldPosition)
    {
        var x = (int)MathF.Floor(worldPosition.X / Size);
        var y = (int)MathF.Floor(worldPosition.Z / Size);
        return new Vector2D<int>(x, y);
    }

    public Chunk(int chunkX, int chunkY)
    {
        _position = new Vector2D<int>(chunkX, chunkY);
        _blocks = new BlockType[Size * Size * Size];
        GenerateChunk();
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
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 7, 0); // Position
        _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 7, 3); // UV
        _vao.VertexAttributePointer(2, 1, VertexAttribPointerType.Float, 7, 5); // Texture Index
        _vao.VertexAttributePointer(3, 1, VertexAttribPointerType.Float, 7, 6); // Brightness 
        
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

    private void GenerateChunk()
    {
        var noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        noise.SetFrequency(0.04f); // Lower frequency for larger features
    
        for (var x = 0; x < Size; x++)
        {
            for (var z = 0; z < Size; z++)
            {
                var worldX = x + (_position.X * Size);
                var worldZ = z + (_position.Y * Size);
            
                // Sample multiple octaves for more natural variation
                var baseHeight = noise.GetNoise(worldX, worldZ);
                var detailHeight = noise.GetNoise(worldX * 4, worldZ * 4) * 0.25f;
                var combinedNoise = baseHeight + detailHeight;
            
                // Use a base terrain level and scale height more gradually
                var baseLevel = 6;
                var heightVariation = 8;
                var height = (int)Math.Clamp(baseLevel + (combinedNoise * heightVariation), 1, Size - 1);
            
                for (var y = 0; y < height; y++)
                {
                    SetBlock(x, y, z, BlockType.Cobblestone);
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
        if (x >= Size || x < 0 || y >= Size || y < 0 || z >= Size || z < 0)
        {
            return false;
        }
        
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

                    foreach (var face in BlockData.Faces)
                    {
                        if (IsFaceBlockSolid(x, y, z, face.Direction))
                        {
                            continue;
                        }
                        
                        for (var vertexIndex = 0; vertexIndex < face.Vertices.Length; vertexIndex += 6)
                        {
                            var vX = face.Vertices[vertexIndex] + x + (Size * _position.X);
                            var vY = face.Vertices[vertexIndex + 1] + y;
                            var vZ = face.Vertices[vertexIndex + 2] + z + (Size * _position.Y);
                            var vU = face.Vertices[vertexIndex + 3];
                            var vV = face.Vertices[vertexIndex + 4];
                            var brightness = face.Vertices[vertexIndex + 5];
        
                            vertices.Add(vX);
                            vertices.Add(vY);
                            vertices.Add(vZ);
                            vertices.Add(vU);
                            vertices.Add(vV);
                            vertices.Add(textureIndex);
                            vertices.Add(brightness); 
                        }
                    }
                }
            }
        }

        return new Mesh(vertices.ToArray(), indices.ToArray());
    }

    private bool IsFaceBlockSolid(int x, int y, int z, BlockData.FaceDirection face)
    {
        var facePositionOffset = face switch
        {
            BlockData.FaceDirection.Back => new Vector3D<int>(0, 0, -1),
            BlockData.FaceDirection.Front => new Vector3D<int>(0, 0, 1),
            BlockData.FaceDirection.Left => new Vector3D<int>(-1, 0, 0),
            BlockData.FaceDirection.Right => new Vector3D<int>(1, 0, 0),
            BlockData.FaceDirection.Top => new Vector3D<int>(0, 1, 0),
            BlockData.FaceDirection.Bottom => new Vector3D<int>(0, -1, 0),
            _ => throw new Exception($"No offset defined for face {face}")
        };

        return IsBlockSolid(x + facePositionOffset.X, y + facePositionOffset.Y, z + facePositionOffset.Z);
    }

    private static float GetTextureIndex(BlockType blockType)
    {
        return blockType switch
        {
            BlockType.Air => 0f,
            BlockType.Dirt => 0f,
            BlockType.Cobblestone => 1f,
            _ => throw new NotImplementedException()
        };
    }
    
    private static int PositionToBlockIndex(int x, int y, int z)
    {
        return (x + (y * Size)) + (z * Size * Size);
    }
}