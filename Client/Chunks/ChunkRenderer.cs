using Client.Blocks;
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

    public ChunkRenderer(GL gl, BlockTextures blockTextures, BlockDatabase blockDatabase)
    {
        _gl = gl;
        _opaqueMeshRenderer = CreateMeshRenderer();
        _transparentMeshRenderer = CreateMeshRenderer();

        const int maxFaces = Chunk.Size * Chunk.Height * Chunk.Size * 6;

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

    public void SetMeshes(Mesh opaque, Mesh transparent)
    {
        _opaqueMeshRenderer.UpdateMesh(opaque);
        _transparentMeshRenderer.UpdateMesh(transparent);
        HasTransparentBlocks = _transparentMeshRenderer.VertexCount > 0;
    }
}