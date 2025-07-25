using System.Numerics;
using Silk.NET.OpenGL;

namespace Client;

public class BlockSpriteRenderer
{
    private readonly GL _gl;
    private readonly Dictionary<BlockType, uint> _blockTextures;
    private readonly int _spriteSize;

    private uint _framebuffer;
    private uint _colorTexture;
    private uint _depthBuffer;

    private Matrix4x4 _viewMatrix;
    private Matrix4x4 _projectionMatrix;

    public BlockSpriteRenderer(GL gl, int spriteSize = 128)
    {
        _gl = gl;
        _spriteSize = spriteSize;
        _blockTextures = new Dictionary<BlockType, uint>();

        CreateFramebuffer();
        SetupCamera();
    }

    public uint GetBlockTexture(BlockType blockType, Shader blockShader, TextureArray textureArray)
    {
        if (_blockTextures.TryGetValue(blockType, out var existingTexture))
        {
            return existingTexture;
        }

        var textureId = RenderBlockToTexture(blockType, blockShader, textureArray);
        _blockTextures[blockType] = textureId;
        return textureId;
    }

    private uint RenderBlockToTexture(BlockType blockType, Shader blockShader, TextureArray textureArray)
    {
        // save the current opengl state
        var oldViewport = new int[4];
        _gl.GetInteger(GetPName.Viewport, oldViewport);
        
        // switch to the new framebuffer for the off-screen rendering
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
        _gl.Viewport(0, 0, (uint)_spriteSize, (uint)_spriteSize);
        
        // Clear it with a transparent background
        _gl.ClearColor(0f, 0f, 0f, 0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.Enable(EnableCap.DepthTest);
        
        // Create and render a temporary block model
        var tempBlock = new BlockModel(_gl, blockType, blockShader, textureArray, Vector3.Zero, _ => false, 1f);
        blockShader.Use();
        blockShader.SetUniform("uView", _viewMatrix);
        blockShader.SetUniform("uProjection", _projectionMatrix);
        tempBlock.Render();

        var resultTexture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, resultTexture);
        _gl.CopyTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, 0, 0, (uint)_spriteSize, (uint)_spriteSize,
            0);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge); 
        
        
        // restore the previous opengl state
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.Viewport(oldViewport[0], oldViewport[1], (uint)oldViewport[2], (uint)oldViewport[3]);

        return resultTexture;
    }

    private void SetupCamera()
    {
        var cameraPosition = new Vector3(1.5f, 1.5f, 1.5f);
        var targetPosition = new Vector3(0f, 0f, 0f);
        var upVector = Vector3.UnitY;

        _viewMatrix = Matrix4x4.CreateLookAt(cameraPosition, targetPosition, upVector);
        _viewMatrix *= Matrix4x4.CreateScale(1f, -1f, 1f);
        _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathUtil.DegreesToRadians(45f), 1.0f, 0.1f, 10f);
    }

    private void CreateFramebuffer()
    {
        // create the framebuffer - an off-screen canvas
        _framebuffer = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
        
        // create color texture that will store rendered block image
        _colorTexture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _colorTexture);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)_spriteSize, (uint)_spriteSize, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, ReadOnlySpan<byte>.Empty);
        
        // set texture params
        _gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
        
        // attach the color texture to the framebuffer
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, _colorTexture, 0);
        
        // create a depth buffer for 3d rendering
        _depthBuffer = _gl.GenRenderbuffer();
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthBuffer);
        _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, (uint)_spriteSize,
            (uint)_spriteSize);
        _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer, _depthBuffer);
        
        // ensure that the buffer is complete
        if (_gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            throw new Exception("Framebuffer not complete");
        }
        
        // unbind the framebuffer to return to default screen rendering
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
}