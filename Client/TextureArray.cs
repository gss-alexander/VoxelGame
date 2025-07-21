using Silk.NET.OpenGL;

namespace Client;

public class TextureArray
{
    private readonly uint _handle;
    private readonly GL _gl;
    private readonly uint _layerCount;

    public TextureArray(GL gl, uint handle, uint layerCount)
    {
        _handle = handle;
        _gl = gl;
        _layerCount = layerCount;
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(textureSlot);
        _gl.BindTexture(TextureTarget.Texture2DArray, _handle);
    }
}