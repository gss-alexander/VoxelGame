using Silk.NET.OpenGL;
using StbImageSharp;

namespace Client;

public class Texture
{
    private readonly uint _handle;
    private readonly GL _gl;

    public unsafe Texture(GL gl, string path)
    {
        _gl = gl;

        _handle = _gl.GenTexture();
        Bind();

        var imageResult = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);
        fixed (byte* ptr = imageResult.Data)
        {
            _gl.TexImage2D(TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba,
                (uint)imageResult.Width,
                (uint)imageResult.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                ptr
            );
        }
        
        SetParameters();
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(textureSlot);
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
    }

    private void SetParameters()
    {
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.LinearMipmapLinear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        
        _gl.GenerateMipmap(TextureTarget.Texture2D);
    }
}