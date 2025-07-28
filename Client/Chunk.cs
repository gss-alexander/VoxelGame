using System.Numerics;
using Client.Chunks;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client;

public class Chunk
{
    public Vector2 ChunkWorldCenter => new Vector2(_position.X, _position.Y) * Size;
    public Vector2D<int> Position => _position;
    
    public const int Size = 16;
    public const int Height = 256;

    private Vector2D<int> _position;
    private ChunkData _data;

    // Opaque rendering
    private Mesh _opaqueMesh;
    private MeshRenderer _opaqueMeshRenderer;

    // Transparent rendering
    private Mesh _transparentMesh;
    private MeshRenderer _transparentMeshRenderer;

    private GL _gl;
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

    public Chunk(ChunkData data, Vector2D<int> position)
    {
        _data = data;
        _position = position;
    }


    public void Initialize(GL gl)
    {
        var meshes = GenerateMeshes();
        _opaqueMesh = meshes.opaque;
        _transparentMesh = meshes.transparent;
        
        _gl = gl;
        _gl.BindVertexArray(0); // Ensure that we are not binding the EBO and VBO to existing VAO
        
        _opaqueMeshRenderer = new MeshRenderer(_gl, _opaqueMesh);
        _opaqueMeshRenderer.SetVertexAttribute(0, 3, VertexAttribPointerType.Float, 7, 0); // Position
        _opaqueMeshRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 7, 3); // UV
        _opaqueMeshRenderer.SetVertexAttribute(2, 1, VertexAttribPointerType.Float, 7, 5); // Texture Index
        _opaqueMeshRenderer.SetVertexAttribute(3, 1, VertexAttribPointerType.Float, 7, 6); // Brightness
        _opaqueMeshRenderer.Unbind();

        _transparentMeshRenderer = new MeshRenderer(_gl, _transparentMesh);
        _transparentMeshRenderer.SetVertexAttribute(0, 3, VertexAttribPointerType.Float, 7, 0); // Position
        _transparentMeshRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 7, 3); // UV
        _transparentMeshRenderer.SetVertexAttribute(2, 1, VertexAttribPointerType.Float, 7, 5); // Texture Index
        _transparentMeshRenderer.SetVertexAttribute(3, 1, VertexAttribPointerType.Float, 7, 6); // Brightness
        _transparentMeshRenderer.Unbind();
        
        _isInitialized = true;
    }

    public void RenderOpaque()
    {
        if (!_isInitialized || _opaqueMesh.Vertices.Length == 0) return;
        
        _opaqueMeshRenderer.Render();
    }

    public void RenderTransparent()
    {
        if (!_isInitialized || _transparentMesh.Vertices.Length == 0) return;
        
        _transparentMeshRenderer.Render();
    }

    public bool HasTransparentBlocks()
    {
        return _transparentMesh.Vertices.Length > 0;
    }

    public Vector3 GetCenterPosition()
    {
        return new Vector3(
            _position.X * Size + Size / 2f,
            Height / 2f,
            _position.Y * Size + Size / 2f
        );
    }
    
    public void RegenerateMesh()
    {
        if (!_isInitialized) return;
    
        var meshes = GenerateMeshes();
        _opaqueMesh = meshes.opaque;
        _transparentMesh = meshes.transparent;

        _opaqueMeshRenderer.UpdateMesh(_opaqueMesh);
        _transparentMeshRenderer.UpdateMesh(_transparentMesh);
    }

    public void GenerateFlatWorld()
    {
        const int height = 6;
        for (var x = 0; x < Size; x++)
        {
            for (var z = 0; z < Size; z++)
            {
                for (var y = 0; y < height - 2; y++)
                {
                    SetBlock(x, y, z, BlockType.Cobblestone, false);
                }
                
                SetBlock(x, height - 2, z, BlockType.Dirt, false);
                SetBlock(x, height - 1, z, BlockType.Grass, false);
            }
        }
    }

    public void SetBlock(int x, int y, int z, BlockType block, bool regenerateMesh = true)
    {
        _data.SetBlock(new Vector3D<int>(x, y, z), block);
        if (regenerateMesh)
        {
            RegenerateMesh();
        }
    }

    public BlockType GetBlock(int x, int y, int z)
    {
        return _data.GetBlock(new Vector3D<int>(x, y, z));
    }

    public bool IsBlockSolid(int x, int y, int z)
    {
        if (x >= Size || x < 0 || y >= Height || y < 0 || z >= Size || z < 0)
        {
            return false;
        }
        
        var block = GetBlock(x, y, z);
        return block != BlockType.Air;
    }

    public (Mesh opaque, Mesh transparent) GenerateMeshes()
    {
        var opaqueVertices = new List<float>();
        var opaqueIndices = new List<uint>();
        var transparentVertices = new List<float>();
        var transparentIndices = new List<uint>();

        var transparentIndicesOffset = 0u;
        var opaqueIndicesOffset = 0u;
        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                for (var z = 0; z < Size; z++)
                {
                    var blockType = GetBlock(x, y, z);
                    if (blockType == BlockType.Air)
                    {
                        continue;
                    }

                    var isTransparent = blockType.IsTransparent();
                    var vertices = isTransparent ? transparentVertices : opaqueVertices;
                    var indices = isTransparent ? transparentIndices : opaqueIndices;

                    foreach (var face in BlockData.Faces)
                    {
                        if (IsFaceBlockSolid(x, y, z, face.Direction))
                        {
                            continue;
                        }

                        var textureIndex = blockType.GetTextureIndex(face.Direction);
                        
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

                        foreach (var index in face.Indices)
                        {
                            var indicesOffset = isTransparent ? transparentIndicesOffset : opaqueIndicesOffset;
                            indices.Add(index + indicesOffset);
                        }

                        if (isTransparent)
                        {
                            transparentIndicesOffset += 4;
                        }
                        else
                        {
                            opaqueIndicesOffset += 4;
                        }
                    }
                }
            }
        }

        return (
            new Mesh(opaqueVertices.ToArray(), opaqueIndices.ToArray()),
            new Mesh(transparentVertices.ToArray(), transparentIndices.ToArray())
        );
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

        
        var neighbourBlockSolid = IsBlockSolid(x + facePositionOffset.X, y + facePositionOffset.Y, z + facePositionOffset.Z);
        var neighbourIsTransparent = false;
        if (!IsPositionOutOfBounds(x + facePositionOffset.X, y + facePositionOffset.Y, z + facePositionOffset.Z))
        {
            neighbourIsTransparent =
                GetBlock(x + facePositionOffset.X, y + facePositionOffset.Y, z + facePositionOffset.Z).IsTransparent();
        }

        return neighbourBlockSolid && !neighbourIsTransparent;
    }

    private static bool IsPositionOutOfBounds(int x, int y, int z)
    {
        return x >= Size || x < 0 || y >= Height || y < 0 || z >= Size || z < 0;
    }
}