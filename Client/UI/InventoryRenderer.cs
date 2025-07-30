using System.Numerics;
using Client.Items;
using Client.UI.Text;
using Silk.NET.OpenGL;

namespace Client.UI;

public class InventoryRenderer
{
    private const float BarYPosition = 550f;
    private const float SlotSpacing = 20f;
    private const float SlotWidth = 75f;
    private const float SlotHeight = 75f;

    private readonly GL _gl;
    private readonly PlayerInventory _inventory;
    private readonly Vector2 _screenSize;
    private readonly ItemTextures _itemTextures;
    private readonly Texture _slotBackgroundTexture;
    private readonly TextRenderer _textRenderer;
    private readonly MeshRenderer _slotBackgroundRenderer;
    private readonly MeshRenderer _itemSpritesRenderer;

    private readonly Shader _uiSpriteShader;
    private readonly Shader _uiShader;

    private int SlotCount => _inventory.Storage.SlotCount;

    public InventoryRenderer(GL gl, PlayerInventory inventory, Vector2 screenSize, ItemTextures itemTextures,
        Texture slotBackgroundTexture, TextRenderer textRenderer)
    {
        _gl = gl;
        _inventory = inventory;
        _screenSize = screenSize;
        _itemTextures = itemTextures;
        _slotBackgroundTexture = slotBackgroundTexture;
        _textRenderer = textRenderer;

        _slotBackgroundRenderer = new MeshRenderer(gl, Mesh.Empty, BufferUsageARB.DynamicDraw);
        _slotBackgroundRenderer.SetVertexAttribute(0, 2, VertexAttribPointerType.Float, 7, 0);
        _slotBackgroundRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 7, 2);
        _slotBackgroundRenderer.SetVertexAttribute(2, 3, VertexAttribPointerType.Float, 7, 4);
        _slotBackgroundRenderer.Unbind();

        _itemSpritesRenderer = new MeshRenderer(gl, Mesh.Empty, BufferUsageARB.DynamicDraw);
        _itemSpritesRenderer.SetVertexAttribute(0, 2, VertexAttribPointerType.Float, 5, 0);
        _itemSpritesRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 5, 2);
        _itemSpritesRenderer.SetVertexAttribute(2, 1, VertexAttribPointerType.Float, 5, 4);
        _itemSpritesRenderer.Unbind();
        
        _uiSpriteShader = Shaders.GetShader(gl, "itemUiSprite");
        _uiShader = Shaders.GetShader(gl, "ui");
    }
    public int GetClickedSlotIndex(Vector2 screenPosition)
    {
        var columnsPerRow = _inventory.Hotbar.SlotCount;
        
        var screenCenter = _screenSize / 2f;
        var totalInventoryWidth = (columnsPerRow * SlotWidth) + (SlotSpacing * (columnsPerRow - 1));
        var baseXPosition = screenCenter.X - totalInventoryWidth / 2f;

        var amountOfRows = (int)MathF.Ceiling((float)SlotCount / columnsPerRow);
        var totalInventoryHeight = (amountOfRows * SlotHeight) + (SlotSpacing * (amountOfRows - 1));
        var baseYPosition = screenCenter.Y - totalInventoryHeight / 2f;

        for (var i = 0; i < SlotCount; i++)
        {
            var row = (int)MathF.Floor((float)i / columnsPerRow);
            var column = i % columnsPerRow;
            
            var xOffset = column * (SlotWidth + SlotSpacing);
            var xPosition = baseXPosition + xOffset;

            var yOffset = row * (SlotHeight + SlotSpacing);
            var yPosition = baseYPosition + yOffset;
            
            if (screenPosition.X >= xPosition &&
                screenPosition.X <= xPosition + SlotWidth &&
                screenPosition.Y >= yPosition &&
                screenPosition.Y <= yPosition + SlotHeight)
            {
                return i;
            }
        }

        return -1;
    }

    public void Render()
    {
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Disable(EnableCap.DepthTest);
        
        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(
            0,
            _screenSize.X,
            _screenSize.Y,
            0,
            0,
            100
        );
        
        RenderBackground(projectionMatrix);
        RenderItems(projectionMatrix);
        RenderItemAmountText();
        
        _gl.Enable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.Blend);
    }

    private void RenderBackground(Matrix4x4 projection)
    {
        _slotBackgroundTexture.Bind();
        
        _uiShader.Use();
        _uiShader.SetUniform("uProjection", projection);
        _uiShader.SetUniform("uTexture", 0);
        
        _slotBackgroundRenderer.UpdateMesh(GenerateBackgroundMesh());
        _slotBackgroundRenderer.Render();
        _slotBackgroundRenderer.Unbind();
    }

    private void RenderItems(Matrix4x4 projection)
    {
        var itemGroups = new Dictionary<ItemTextures.ItemTextureType, List<(int slot, string itemId)>>();
    
        for (var i = 0; i < SlotCount; i++)
        {
            var slot = _inventory.Storage.GetSlot(i);
            if (slot == null) continue;
        
            var textureType = _itemTextures.GetTextureTypeForItem(slot.ItemId);
            if (!itemGroups.ContainsKey(textureType))
                itemGroups[textureType] = new List<(int, string)>();
            itemGroups[textureType].Add((i, slot.ItemId));
        }
    
        foreach (var (textureType, items) in itemGroups)
        {
            _itemTextures.Use(textureType);
            _uiSpriteShader.Use();
            _uiSpriteShader.SetUniform("uProjection", projection);
            _uiSpriteShader.SetUniform("uTextureArray", 0);
        
            _itemSpritesRenderer.UpdateMesh(GenerateItemsMeshForGroup(items));
            _itemSpritesRenderer.Render();
        }
    
        _itemSpritesRenderer.Unbind();
    }

    private void RenderItemAmountText()
    {
        var columnsPerRow = _inventory.Hotbar.SlotCount;
        
        var screenCenter = _screenSize / 2f;
        var totalInventoryWidth = (columnsPerRow * SlotWidth) + (SlotSpacing * (columnsPerRow - 1));
        var baseXPosition = screenCenter.X - totalInventoryWidth / 2f;

        var amountOfRows = (int)MathF.Ceiling((float)SlotCount / columnsPerRow);
        var totalInventoryHeight = (amountOfRows * SlotHeight) + (SlotSpacing * (amountOfRows - 1));
        var baseYPosition = screenCenter.Y - totalInventoryHeight / 2f;

        for (var i = 0; i < SlotCount; i++)
        {
            var slot = _inventory.Storage.GetSlot(i);
            if (slot == null) continue;
            var row = (int)MathF.Floor((float)i / columnsPerRow);
            var column = i % columnsPerRow;
            
            var xOffset = column * (SlotWidth + SlotSpacing) + (SlotWidth / 1.3f);
            var xPosition = baseXPosition + xOffset;

            var yOffset = row * (SlotHeight + SlotSpacing);
            var yPosition = baseYPosition + yOffset;
            _textRenderer.RenderText(
                slot.Count.ToString(),
                xPosition,
                yPosition + 10f,
                0.5f,
                new Vector3(0f, 0f, 0f),
                (int)_screenSize.X,
                (int)_screenSize.Y,
                TextAlignment.Center
            );
        }
    }
    
    private Mesh GenerateItemsMeshForGroup(List<(int slotIndex, string itemId)> items)
    {
        var vertices = new List<float>();
        var indices = new List<uint>();
        
        var columnsPerRow = _inventory.Hotbar.SlotCount;
        
        var screenCenter = _screenSize / 2f;
        var totalInventoryWidth = (columnsPerRow * SlotWidth) + (SlotSpacing * (columnsPerRow - 1));
        var baseXPosition = screenCenter.X - totalInventoryWidth / 2f;

        var amountOfRows = (int)MathF.Ceiling((float)SlotCount / columnsPerRow);
        var totalInventoryHeight = (amountOfRows * SlotHeight) + (SlotSpacing * (amountOfRows - 1));
        var baseYPosition = screenCenter.Y - totalInventoryHeight / 2f;

        var indicesOffset = 0u;
        foreach (var (slotIndex, itemId) in items)
        {
            var row = (int)MathF.Floor((float)slotIndex / columnsPerRow);
            var column = slotIndex % columnsPerRow;
            
            var xOffset = column * (SlotWidth + SlotSpacing);
            var xPosition = baseXPosition + xOffset;

            var yOffset = row * (SlotHeight + SlotSpacing);
            var yPosition = baseYPosition + yOffset;
            
            var textureIndex = _itemTextures.GetTextureIndexForItem(itemId);

            vertices.AddRange(CreateQuad(xPosition, yPosition, textureIndex));
            indices.AddRange([
                indicesOffset, 1 + indicesOffset, 2 + indicesOffset,
                2 + indicesOffset, 3 + indicesOffset, indicesOffset
            ]);
            indicesOffset += 4;
        }
   
        return new Mesh(vertices.ToArray(), indices.ToArray());
   
        float[] CreateQuad(float x, float y, uint textureIndex)
        {
            return
            [
                x, y, 0.0f, 0.0f, textureIndex,
                x + SlotWidth, y, 1.0f, 0.0f, textureIndex,
                x + SlotWidth, y + SlotHeight, 1.0f, 1.0f, textureIndex,
                x, y + SlotHeight, 0.0f, 1.0f, textureIndex,
            ];
        }
    } 

    private Mesh GenerateBackgroundMesh()
    {
        var vertices = new List<float>();
        var indices = new List<uint>();

        // the number of slots that will be placed per row of the inventory. dependent on hotbar for consistency
        var columnsPerRow = _inventory.Hotbar.SlotCount;
        
        var screenCenter = _screenSize / 2f;
        var totalInventoryWidth = (columnsPerRow * SlotWidth) + (SlotSpacing * (columnsPerRow - 1));
        var baseXPosition = screenCenter.X - totalInventoryWidth / 2f;

        var amountOfRows = (int)MathF.Ceiling((float)SlotCount / columnsPerRow);
        var totalInventoryHeight = (amountOfRows * SlotHeight) + (SlotSpacing * (amountOfRows - 1));
        var baseYPosition = screenCenter.Y - totalInventoryHeight / 2f;

        var indicesOffset = 0u;
        for (var i = 0; i < SlotCount; i++)
        {
            var row = (int)MathF.Floor((float)i / columnsPerRow);
            var column = i % columnsPerRow;
            
            var xOffset = column * (SlotWidth + SlotSpacing);
            var xPosition = baseXPosition + xOffset;

            var yOffset = row * (SlotHeight + SlotSpacing);
            var yPosition = baseYPosition + yOffset;

            var color = new Vector3(1.0f, 1.0f, 1.0f);
            vertices.AddRange(CreateQuad(xPosition, yPosition, color));
            indices.AddRange([
                indicesOffset, 1 + indicesOffset, 2 + indicesOffset,
                2 + indicesOffset, 3 + indicesOffset, indicesOffset
            ]);
            indicesOffset += 4;
        }

        return new Mesh(vertices.ToArray(), indices.ToArray());

        float[] CreateQuad(float x, float y, Vector3 color)
        {
            return
            [
                x, y, 0.0f, 0.0f, color.X, color.Y, color.Z,
                x + SlotWidth, y, 1.0f, 0.0f, color.X, color.Y, color.Z,
                x + SlotWidth, y + SlotHeight, 1.0f, 1.0f, color.X, color.Y, color.Z,
                x, y + SlotHeight, 0.0f, 1.0f, color.X, color.Y, color.Z
            ];
        }
    }
}