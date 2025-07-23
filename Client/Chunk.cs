using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client;

public class Chunk
{
    public Vector2D<int> Position => _position;
    
    public const int Size = 16;
    public const int Height = 256; // New height constant

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

    public static Vector2D<int> BlockToChunkPosition(Vector3D<int> blockPosition)
    {
        return new Vector2D<int>(
            blockPosition.X >= 0 ? blockPosition.X / Size : (blockPosition.X + 1) / Size - 1,
            blockPosition.Z >= 0 ? blockPosition.Z / Size : (blockPosition.Z + 1) / Size - 1
        );
    }

    public Chunk(int chunkX, int chunkY)
    {
        _position = new Vector2D<int>(chunkX, chunkY);
        _blocks = new BlockType[Size * Height * Size];
    }

    public void Initialize(GL gl)
    {
        _mesh = GenerateMesh();
        
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
    
    public void RegenerateMesh()
    {
        if (!_isInitialized) return;
    
        _mesh = GenerateMesh();
    
        _vbo.UpdateData(_mesh.Vertices);
        _ebo.UpdateData(_mesh.Indices);
    }

    public void GenerateChunkData(FastNoiseLite noise)
    {
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);

        for (var x = 0; x < Size; x++)
        {
            for (var z = 0; z < Size; z++)
            {
                var worldX = x + (_position.X * Size);
                var worldZ = z + (_position.Y * Size);

                // Generate multiple octaves of noise for natural variation
                var heightNoise = GenerateHeightNoise(noise, worldX, worldZ);
            
                // Add ridged noise for mountain features
                var ridgedNoise = GenerateRidgedNoise(noise, worldX, worldZ);
            
                // Combine noises with different influences
                var combinedNoise = (heightNoise * 0.7f) + (ridgedNoise * 0.3f);
            
                // Apply exponential scaling for dramatic terrain variation
                var heightValue = ApplyHeightCurve(combinedNoise);
            
                // Calculate final height
                var seaLevel = 64;
                var maxMountainHeight = 200;
                var height = (int)Math.Clamp(seaLevel + (heightValue * maxMountainHeight), 1, Height - 1);

                // Generate terrain layers
                for (var y = 0; y < height - 1; y++)
                {
                    SetBlock(x, y, z, BlockType.Cobblestone, false);
                }
                
                // Make top block dirt
                SetBlock(x, height - 1, z, BlockType.Grass, false);
            }
        }
    }

    private float GenerateHeightNoise(FastNoiseLite noise, int worldX, int worldZ)
    {
        var noiseValue = 0f;
        var amplitude = 1f;
        var frequency = 0.008f; // Base frequency for large landforms
        var maxValue = 0f;

        // Generate 6 octaves for detailed terrain
        for (int octave = 0; octave < 6; octave++)
        {
            noise.SetFrequency(frequency);
            noiseValue += noise.GetNoise(worldX, worldZ) * amplitude;
            maxValue += amplitude;

            amplitude *= 0.5f; // Each octave contributes half as much
            frequency *= 2.1f; // Slightly irregular frequency multiplication for more organic feel
        }

        // Normalize to [-1, 1] range
        return noiseValue / maxValue;
    }

    private float GenerateRidgedNoise(FastNoiseLite noise, int worldX, int worldZ)
    {
        noise.SetFrequency(0.004f); // Lower frequency for large mountain ridges
        var ridgeNoise = noise.GetNoise(worldX, worldZ);
    
        // Create ridged effect by inverting and sharpening
        ridgeNoise = 1f - MathF.Abs(ridgeNoise);
        ridgeNoise = MathF.Pow(ridgeNoise, 1.5f); // Sharpen the ridges
    
        // Add some detail ridges
        noise.SetFrequency(0.015f);
        var detailRidges = 1f - MathF.Abs(noise.GetNoise(worldX, worldZ));
        detailRidges = MathF.Pow(detailRidges, 2f) * 0.3f;
    
        return (ridgeNoise * 0.8f + detailRidges * 0.2f) * 2f - 1f; // Scale to [-1, 1]
    }

    private float ApplyHeightCurve(float noiseValue)
    {
        // Clamp noise to prevent extreme values
        noiseValue = Math.Clamp(noiseValue, -1f, 1f);
    
        if (noiseValue >= 0)
        {
            // Positive values: exponential curve for dramatic mountains
            // Using power of 1.8 creates good balance between flat areas and mountains
            return MathF.Pow(noiseValue, 1.9f);
        }
        else
        {
            // Negative values: gentler curve for valleys and low areas
            return -MathF.Pow(-noiseValue, 1.2f) * 0.3f; // Scale down valleys
        }
    }

    public void SetBlock(int x, int y, int z, BlockType block, bool regenerateMesh = true)
    {
        var position = PositionToBlockIndex(x, y, z);
        _blocks[position] = block;
        if (regenerateMesh)
        {
            RegenerateMesh();
        }
    }

    public BlockType GetBlock(int x, int y, int z)
    {
        var position = PositionToBlockIndex(x, y, z);
        return _blocks[position];
    }

    public bool IsBlockSolid(int x, int y, int z)
    {
        if (x >= Size || x < 0 || y >= Height || y < 0 || z >= Size || z < 0) // Updated bounds check
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
            for (var y = 0; y < Height; y++) // Updated to use Height
            {
                for (var z = 0; z < Size; z++)
                {
                    var blockType = GetBlock(x, y, z);
                    if (blockType == BlockType.Air)
                    {
                        continue;
                    }


                    foreach (var face in BlockData.Faces)
                    {
                        if (IsFaceBlockSolid(x, y, z, face.Direction))
                        {
                            continue;
                        }
                        
                        var textureIndex = GetTextureIndex(blockType, face.Direction);
                        
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

    private static float GetTextureIndex(BlockType blockType, BlockData.FaceDirection faceDirection)
    {
        return blockType switch
        {
            BlockType.Air => 0f,
            BlockType.Dirt => 0f,
            BlockType.Cobblestone => 1f,
            BlockType.Grass => faceDirection switch
            {
                BlockData.FaceDirection.Top => 3f,
                BlockData.FaceDirection.Bottom => 0f,
                _ => 2f
            },
            _ => throw new NotImplementedException()
        };
    }
    
    private static int PositionToBlockIndex(int x, int y, int z)
    {
        return (x + (y * Size)) + (z * Size * Height);
    }
}