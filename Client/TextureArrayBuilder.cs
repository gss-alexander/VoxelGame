using Silk.NET.OpenGL;
using StbImageSharp;

namespace Client;

public class TextureArrayBuilder
{
    private readonly List<byte[]> _textureData = new();

    private readonly uint _width;
    private readonly uint _height;
    
    public TextureArrayBuilder(uint width, uint height)
    {
        _width = width;
        _height = height;
    }

    public TextureArrayBuilder AddTexture(string path)
    {
        var imageResult = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);
        _textureData.Add(imageResult.Data);
        return this;
    }

    public TextureArray Build(GL gl)
    {
        if (_textureData.Count == 0)
        {
            throw new InvalidOperationException("No textures added to array");
        }

        var handle = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2DArray, handle);

        gl.TexStorage3D(TextureTarget.Texture2DArray, 1, SizedInternalFormat.Rgba8, _width, _height,
            (uint)_textureData.Count);
        
        for (var i = 0; i < _textureData.Count; i++)
        {
            unsafe
            {
                var textureData = _textureData[i];
                fixed (byte* ptr = textureData)
                    gl.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, _width, _height, 1, PixelFormat.Rgba,
                        PixelType.UnsignedByte, ptr);
            }
        }

        SetParameters(gl);

        return new TextureArray(gl, handle, (uint)_textureData.Count);
    }

    private static void SetParameters(GL gl)
    {
        gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
        gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureBaseLevel, 0);
        gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMaxLevel, 8);
        
        // Removed for pixel art
        // gl.GenerateMipmap(TextureTarget.Texture2DArray); 
    }
}