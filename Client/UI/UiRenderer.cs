using System.Numerics;
using Client.Blocks;
using Client.Items;
using Client.UI.Text;
using Silk.NET.OpenGL;

namespace Client.UI;
public class UiRenderer
{
    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly Texture _texture;
    private readonly BlockSpriteRenderer _blockSpriteRenderer;
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    private readonly BufferObject<float> _vbo;
    private readonly BufferObject<uint> _ebo;
    private readonly VertexArrayObject<float, uint> _vao;
    
    private readonly BufferObject<float> _blockVbo;
    private readonly BufferObject<uint> _blockEbo;
    private readonly VertexArrayObject<float, uint> _blockVao; 

    private readonly float[] _vertices;
    private readonly uint[] _indices;

    private readonly TextRenderer _textRenderer;
    private readonly PlayerInventory _inventory;

    public UiRenderer(GL gl, Shader shader, Texture texture, BlockSpriteRenderer blockSpriteRenderer, int screenWidth, int screenHeight, TextRenderer textRenderer, PlayerInventory inventory)
    {
        _gl = gl;
        _shader = shader;
        _texture = texture;
        _blockSpriteRenderer = blockSpriteRenderer;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _textRenderer = textRenderer;
        _inventory = inventory;

        (_vertices, _indices) = CreateHotbarMeshData();
        _vbo = new BufferObject<float>(_gl, _vertices, BufferTargetARB.ArrayBuffer);
        _ebo = new BufferObject<uint>(_gl, _indices, BufferTargetARB.ElementArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);
        
        // Configure vertex attributes: position (2 floats) + texture coords (2 floats) = 4 floats per vertex
        _vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 4, 0); // Position
        _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 4, 2); // Texture coords
        _gl.BindVertexArray(0);

        var blockVertices = CreateQuad(0, 0, 50, 50, 0f, 0f, 1f, 1f);
        var blockIndices = new uint[] { 0, 1, 2, 2, 3, 0 };

        _blockVbo = new BufferObject<float>(_gl, blockVertices, BufferTargetARB.ArrayBuffer);
        _blockEbo = new BufferObject<uint>(_gl, blockIndices, BufferTargetARB.ElementArrayBuffer);
        _blockVao = new VertexArrayObject<float, uint>(_gl, _blockVbo, _blockEbo);
        
        _blockVao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 4, 0);
        _blockVao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 4, 2);
    }

    public void Render(int screenWidth, int screenHeight, Shader blockShader, ItemTextures itemTextures)
    {
        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, 0, 100);
        
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Disable(EnableCap.DepthTest);
        
        _shader.Use();
        _shader.SetUniform("uProjection", projectionMatrix);
        _shader.SetUniform("uTexture", 0); // Texture unit 0
        _shader.SetUniform("uModel", Matrix4x4.Identity);
        
        _texture.Bind(TextureUnit.Texture0);
        _vao.Bind();
        _gl.DrawElements(PrimitiveType.Triangles, (uint)_indices.Length, DrawElementsType.UnsignedInt, ReadOnlySpan<uint>.Empty);

        RenderItems(screenWidth, screenHeight, itemTextures);

        _gl.Enable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.Blend);
    }

    private void RenderItems(int screenWidth, int screenHeight, ItemTextures itemTextures)
    {
        var slotCount = _inventory.Hotbar.SlotCount;
        
        const float slotWidth = 75f;
        const float slotHeight = 75f;
        const float spacing = 20f;
        const float blockSize = 100f; // Size of the block sprite
        const float yPosition = 700f;

        var screenCenter = screenWidth / 2f;
        var totalHotbarWidth = (slotCount * slotWidth) + (spacing * (slotCount - 1));
        var baseX = screenCenter - totalHotbarWidth / 2f - 250f;

        _blockVao.Bind();

        for (var i = 0; i < slotCount; i++)
        {
            var slot = _inventory.Hotbar.GetSlot(i);
            if (slot == null) continue; // empty slot

            // Calculate position (center the block sprite in the slot)
            var slotX = baseX + i * (slotWidth + spacing);
            var blockX = slotX + (slotWidth - blockSize) / 2f;
            var blockY = yPosition + (slotHeight - blockSize) / 2f;

            // Get the sprite texture for this block type
            var spriteTexture = itemTextures.GetTextureIndexForItem(slot.ItemId);
            
            // Bind the sprite texture
            _gl.BindTexture(TextureTarget.Texture2D, spriteTexture);
            
            // Update the model matrix to position the block sprite
            var modelMatrix = Matrix4x4.CreateScale(12f) * Matrix4x4.CreateTranslation(blockX, blockY, 0);
            _shader.SetUniform("uModel", modelMatrix);
            
            // Draw the block sprite
            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, ReadOnlySpan<uint>.Empty);
        } 
        
        for (var i = 0; i < slotCount; i++)
        {
            var slot = _inventory.Hotbar.GetSlot(i);
            if (slot == null) continue; // empty slot
        
            // Calculate position (center the block sprite in the slot)
            var slotX = baseX + i * (slotWidth + spacing);
            var blockX = slotX + (slotWidth - blockSize) / 2f;
            _textRenderer.RenderText(slot.Count.ToString(), blockX + 280, yPosition - 615, 0.5f, new Vector3(1.0f, 1.0f, 1.0f), screenWidth, screenHeight);
        } 
    }


    private Tuple<float[], uint[]> CreateHotbarMeshData()
    {
        var vertices = new List<float>();
        var indices = new List<uint>();

        var slotCount = _inventory.Hotbar.SlotCount;
        
        const float yPosition = 950f;
        const float baseXPosition = 500f;
        const float spacing = 20f;
        const float slotWidth = 75f;
        const float slotHeight = 75f;

        var screenCenter = _screenWidth / 2f;
        var totalHotbarWidth = (slotCount * slotWidth) + (spacing * (slotCount - 1));
        var baseX = screenCenter - totalHotbarWidth / 2f;


        var indicesOffset = 0u;
        for (var i = 0; i < slotCount; i++)
        {
            var xOffset = i * (slotWidth + spacing);  // Include slot width
            var x = baseX + xOffset; 
            vertices.AddRange(CreateQuad(x, yPosition, slotWidth, slotHeight, 0f, 0f, 1f, 1f));
            indices.AddRange(CreateIndices(ref indicesOffset));
            indicesOffset += 4;
        }

        return new Tuple<float[], uint[]>(vertices.ToArray(), indices.ToArray());
    }
    
    private static float[] CreateQuad(float x, float y, float width, float height, float u1, float v1, float u2, float v2)
    {
        return
        [
            x, y, u1, v1,                           // top left
            x + width, y, u2, v1,                   // top right
            x + width, y + height, u2, v2,          // bottom right
            x, y + height, u1, v2                   // bottom left (fixed v coordinate)
        ];
    }

    private static uint[] CreateIndices(ref uint indexOffset)
    {
        return [indexOffset, indexOffset + 1, indexOffset + 2, indexOffset + 2, indexOffset + 3, indexOffset];
    }
}