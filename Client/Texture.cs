using Silk.NET.OpenGL;
using StbImageSharp;

namespace Client;

public class Texture
{
    private readonly uint _handle;

    public unsafe Texture(string path)
    {
        _handle = OpenGl.Context.GenTexture();
        Bind();

        var imageResult = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);
        fixed (byte* ptr = imageResult.Data)
        {
            OpenGl.Context.TexImage2D(TextureTarget.Texture2D,
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

    public unsafe Texture(byte[] data, uint width, uint height)
    {
        _handle = OpenGl.Context.GenTexture();
        Bind();

        fixed (byte* ptr = data)
        {
            OpenGl.Context.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba,
                width,
                height,
                0,
                GLEnum.Rgba,
                PixelType.UnsignedByte,
                ptr
            );
        }
        
        SetParameters();
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        OpenGl.Context.ActiveTexture(textureSlot);
        OpenGl.Context.BindTexture(TextureTarget.Texture2D, _handle);
    }

    private void SetParameters()
    {
        OpenGl.Context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) GLEnum.ClampToEdge);
        OpenGl.Context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) GLEnum.ClampToEdge);
        OpenGl.Context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.Nearest);
        OpenGl.Context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Nearest);
        OpenGl.Context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        OpenGl.Context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        
        OpenGl.Context.GenerateMipmap(TextureTarget.Texture2D);
    }
}