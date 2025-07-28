using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client.UI.Text;

public class Character
{
    public uint TextureId { get; } // ID handle of the glyph texture
    public Vector2D<int> Size { get; } // Size of the glyph
    public Vector2D<int> Bearing { get; } // Offset from baseline to left/top of glyph
    public int Advance { get; } // Offset to advance to next glyph

    private readonly GL _gl;
    private readonly uint _handle;

    public unsafe Character(GL gl, uint width, uint height, int left, int top, int advance, byte* data)
    {
        _gl = gl;
        
        Size = new Vector2D<int>((int)width, (int)height);
        Bearing = new Vector2D<int>(left, top);
        Advance = advance;
        
        // since we are only using 1 byte for color of texture, so this is needed.
        _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        
        TextureId = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, TextureId);
        _gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            InternalFormat.Red,
            width,
            height,
            0,
            GLEnum.Red,
            PixelType.UnsignedByte,
            data
        );
        
        _gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
    }

    public void Bind()
    {
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, TextureId);
    }
}