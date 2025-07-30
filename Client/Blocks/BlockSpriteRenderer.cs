using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.Blocks;

public class BlockSpriteRenderer
{
    public const int SpriteSize = 256;
    
    private readonly GL _gl;
    private readonly BlockTextures _blockTextures;

    private uint _framebuffer;
    private uint _colorTexture;
    private uint _depthBuffer;

    private Matrix4x4 _viewMatrix;
    private Matrix4x4 _projectionMatrix;

    public BlockSpriteRenderer(GL gl, BlockTextures blockTextures)
    {
        _gl = gl;
        _blockTextures = blockTextures;

        CreateFramebuffer();
        SetupCamera();
    }

    public byte[] GetBlockTextureData(int blockId)
    {
        var blockShader = Shaders.GetShader(_gl, "shader");
        var data = RenderBlockToData(blockId, blockShader, _blockTextures);
        return data;
    }

    public byte[] RenderBlockToData(int blockId, Shader blockShader, BlockTextures blockTextures)
    {
        // save the current opengl state
        var oldViewport = new int[4];
        _gl.GetInteger(GetPName.Viewport, oldViewport);
        
        // switch to the new framebuffer for the off-screen rendering
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
        _gl.ReadBuffer(ReadBufferMode.ColorAttachment0); 
        _gl.Viewport(0, 0, (uint)SpriteSize, (uint)SpriteSize);
        
        _gl.ClearColor(0f, 0f, 0f, 0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.Enable(EnableCap.DepthTest);
        
        var tempBlock = new BlockModel(_gl, blockTextures, blockId, blockShader, Vector3.Zero, _ => false, 1f);
        blockShader.Use();
        blockShader.SetUniform("uView", _viewMatrix);
        blockShader.SetUniform("uProjection", _projectionMatrix);
        tempBlock.Render();

        var pixelData = new byte[SpriteSize * SpriteSize * 4]; // RGBA = 4 bytes per pixel
        unsafe
        {
            fixed (byte* ptr = pixelData)
            {
                _gl.ReadPixels(0, 0, (uint)SpriteSize, (uint)SpriteSize, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }
        }
    
        // restore the previous opengl state
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.Viewport(oldViewport[0], oldViewport[1], (uint)oldViewport[2], (uint)oldViewport[3]);

        return pixelData;
    }

    private void SetupCamera()
    {
        var cameraPosition = new Vector3(0.2f, 0.25f, 0.2f);
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
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)SpriteSize, (uint)SpriteSize, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, ReadOnlySpan<byte>.Empty);
        
        // set texture params
        _gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
        
        // attach the color texture to the framebuffer
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, _colorTexture, 0);
        
        // create a depth buffer for 3d rendering
        _depthBuffer = _gl.GenRenderbuffer();
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthBuffer);
        _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, (uint)SpriteSize,
            (uint)SpriteSize);
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