using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client.Chunks;

// Renders chunk data
public class ChunkRenderer
{
    public bool HasTransparentBlocks { get; private set; }
    
    private readonly GL _gl;
    
    private readonly MeshRenderer _opaqueMeshRenderer;
    private readonly MeshRenderer _transparentMeshRenderer;

    public ChunkRenderer(GL gl)
    {
        _gl = gl;

        _opaqueMeshRenderer = CreateMeshRenderer();
        _transparentMeshRenderer = CreateMeshRenderer();
    }
    
    public void RenderOpaque()
    {
        _opaqueMeshRenderer.Render();
    }

    public void RenderTransparent()
    {
        _transparentMeshRenderer.Render();
    }

    private MeshRenderer CreateMeshRenderer()
    {
        _gl.BindVertexArray(0); // Ensure that we are not binding the EBO and VBO to existing VAO
        
        var renderer = MeshRenderer.Empty(_gl);
        renderer.SetVertexAttribute(0, 3, VertexAttribPointerType.Float, 7, 0); // Position
        renderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 7, 3); // UV
        renderer.SetVertexAttribute(2, 1, VertexAttribPointerType.Float, 7, 5); // Texture Index
        renderer.SetVertexAttribute(3, 1, VertexAttribPointerType.Float, 7, 6); // Brightness
        renderer.Unbind();

        return renderer;
    }

    public void RegenerateMeshes(ChunkData chunkData)
    {
        var opaqueVertices = new List<float>();
        var opaqueIndices = new List<uint>();
        var transparentVertices = new List<float>();
        var transparentIndices = new List<uint>();

        var transparentIndicesOffset = 0u;
        var opaqueIndicesOffset = 0u;
        for (var x = 0; x < Chunk.Size; x++)
        {
            for (var y = 0; y < Chunk.Height; y++)
            {
                for (var z = 0; z < Chunk.Size; z++)
                {
                    var blockPos = new Vector3D<int>(x, y, z);
                    var blockType = chunkData.GetBlock(blockPos);
                    if (blockType == BlockType.Air)
                    {
                        continue;
                    }

                    var isTransparent = blockType.IsTransparent();
                    var vertices = isTransparent ? transparentVertices : opaqueVertices;
                    var indices = isTransparent ? transparentIndices : opaqueIndices;

                    foreach (var face in BlockData.Faces)
                    {
                        if (IsFaceBlockSolid(x, y, z, face.Direction, chunkData))
                        {
                            continue;
                        }

                        var textureIndex = blockType.GetTextureIndex(face.Direction);
                        
                        for (var vertexIndex = 0; vertexIndex < face.Vertices.Length; vertexIndex += 6)
                        {
                            var vX = face.Vertices[vertexIndex] + x + (Chunk.Size * chunkData.Position.X);
                            var vY = face.Vertices[vertexIndex + 1] + y;
                            var vZ = face.Vertices[vertexIndex + 2] + z + (Chunk.Size * chunkData.Position.Y);
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

        _opaqueMeshRenderer.UpdateMesh(new Mesh(opaqueVertices.ToArray(), opaqueIndices.ToArray()));
        _transparentMeshRenderer.UpdateMesh(new Mesh(transparentVertices.ToArray(), transparentIndices.ToArray()));
        HasTransparentBlocks = _transparentMeshRenderer.VertexCount > 0;
    }
    
    private static bool IsFaceBlockSolid(int x, int y, int z, BlockData.FaceDirection face, ChunkData chunkData)
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

        var faceBlockPosition =
            new Vector3D<int>(facePositionOffset.X + x, facePositionOffset.Y + y, facePositionOffset.Z + z);
        
        var neighbourBlockSolid = IsBlockSolid(faceBlockPosition, chunkData);
        var neighbourIsTransparent = false;
        if (!IsPositionOutOfBounds(faceBlockPosition))
        {
            neighbourIsTransparent = chunkData.GetBlock(faceBlockPosition).IsTransparent();
        }

        return neighbourBlockSolid && !neighbourIsTransparent;
    }
    
    private static bool IsPositionOutOfBounds(Vector3D<int> position)
    {
        return position.X >= Chunk.Size || position.X < 0 ||
               position.Y >= Chunk.Height || position.Y < 0 ||
               position.Z >= Chunk.Size || position.Z < 0;
    }
    
    private static bool IsBlockSolid(Vector3D<int> position, ChunkData chunkData)
    {
        if (IsPositionOutOfBounds(position))
        {
            return false;
        }

        var block = chunkData.GetBlock(position);
        return block != BlockType.Air;
    }
}