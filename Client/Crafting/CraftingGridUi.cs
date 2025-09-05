using System.Numerics;
using Client.Items;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client.Crafting;

public class CraftingGridUi
{
    public CraftingGrid CraftingGrid => _craftingGrid;
    
    private readonly CraftingGrid _craftingGrid;
    private readonly Vector2 _screenSize;
    private readonly Texture _slotBackgroundTexture;
    private readonly ItemTextures _itemTextures;
    private readonly Shader _uiShader;
    private readonly Shader _uiSpriteShader;

    private const float SlotWidth = 75f;
    private const float SlotHeight = 75f;
    private const float SlotSpacing = 20f;
    private const float BaseYPosition = 50f;

    private readonly Dictionary<int, Vector2D<int>> _slotIndexGridPositionMap = new();

    private readonly MeshRenderer _backgroundMeshRenderer;
    private readonly MeshRenderer _itemSpritesRenderer;
    private readonly MeshRenderer _resultItemRenderer;

    public CraftingGridUi( CraftingGrid craftingGrid, Vector2 screenSize, Texture slotBackgroundTexture, ItemTextures itemTextures)
    {
        _craftingGrid = craftingGrid;
        _screenSize = screenSize;
        _slotBackgroundTexture = slotBackgroundTexture;
        _itemTextures = itemTextures;

        _backgroundMeshRenderer = new MeshRenderer(GenerateBackgroundMesh());
        _backgroundMeshRenderer.SetVertexAttribute(0, 2, VertexAttribPointerType.Float, 7, 0);
        _backgroundMeshRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 7, 2);
        _backgroundMeshRenderer.SetVertexAttribute(2, 3, VertexAttribPointerType.Float, 7, 4);
        _backgroundMeshRenderer.Unbind();
        
        _itemSpritesRenderer = new MeshRenderer(Mesh.Empty, BufferUsageARB.DynamicDraw);
        _itemSpritesRenderer.SetVertexAttribute(0, 2, VertexAttribPointerType.Float, 5, 0);
        _itemSpritesRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 5, 2);
        _itemSpritesRenderer.SetVertexAttribute(2, 1, VertexAttribPointerType.Float, 5, 4);
        _itemSpritesRenderer.Unbind();
        
        _resultItemRenderer = new MeshRenderer(Mesh.Empty, BufferUsageARB.DynamicDraw);
        _resultItemRenderer.SetVertexAttribute(0, 2, VertexAttribPointerType.Float, 5, 0);
        _resultItemRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 5, 2);
        _resultItemRenderer.SetVertexAttribute(2, 1, VertexAttribPointerType.Float, 5, 4);
        _resultItemRenderer.Unbind();
        
        _uiSpriteShader = Shaders.GetShader("itemUiSprite");
        _uiShader = Shaders.GetShader("ui");
    }
    
    public void Render()
    {
        OpenGl.Context.Enable(EnableCap.Blend);
        OpenGl.Context.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        OpenGl.Context.Disable(EnableCap.DepthTest);
        
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
        
        OpenGl.Context.Enable(EnableCap.DepthTest);
        OpenGl.Context.Disable(EnableCap.Blend);
    }

    public ItemStack? GetStackAtSlotIndex(int slotIndex)
    {
        var gridPos = _slotIndexGridPositionMap[slotIndex];
        return _craftingGrid.GetStackAtSlot(gridPos.X, gridPos.Y);
    }

    public void ClearStackAtSlot(int slotIndex)
    {
        var gridPos = _slotIndexGridPositionMap[slotIndex];
        _craftingGrid.ClearStack(gridPos.X, gridPos.Y);
    }

    public void AddItemStackToSlot(int slotIndex, ItemStack itemStack)
    {
        var gridPos = _slotIndexGridPositionMap[slotIndex];
        _craftingGrid.AddItemStack(itemStack, gridPos.X, gridPos.Y);
    }
    
    public int GetClickedSlotIndex(Vector2 screenPosition)
    {
        var screenCenter = _screenSize / 2f;
        
        var totalBackgroundWidth = (_craftingGrid.Width * SlotWidth) + (SlotSpacing * (_craftingGrid.Width - 1));
        var baseXPosition = screenCenter.X - totalBackgroundWidth / 2f;

        for (var i = 0; i < _craftingGrid.Width * _craftingGrid.Height; i++)
        {
            var row = (int)MathF.Floor((float)i / _craftingGrid.Width);
            var column = i % _craftingGrid.Width;

            var xOffset = column * (SlotWidth + SlotSpacing);
            var xPosition = baseXPosition + xOffset;

            var yOffset = row * (SlotHeight + SlotSpacing);
            var yPosition = BaseYPosition + yOffset;
            
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

    public bool IsResultSlotClicked(Vector2 screenPosition)
    {
        var screenCenter = _screenSize / 2f;

        var totalBackgroundWidth = (_craftingGrid.Width * SlotWidth) + (SlotSpacing * (_craftingGrid.Width - 1));
        var baseXPosition = screenCenter.X - totalBackgroundWidth / 2f;
        
        var outputXPosition = baseXPosition + ((_craftingGrid.Width + 1) * (SlotWidth + SlotSpacing));
        var outputYPosition = BaseYPosition + (SlotHeight + SlotSpacing);

        return (screenPosition.X >= outputXPosition &&
                screenPosition.X <= outputXPosition + SlotWidth &&
                screenPosition.Y >= outputYPosition &&
                screenPosition.Y <= outputYPosition + SlotHeight);
    }
    
    private void RenderBackground(Matrix4x4 projection)
    {
        _slotBackgroundTexture.Bind();
        
        _uiShader.Use();
        _uiShader.SetUniform("uProjection", projection);
        _uiShader.SetUniform("uTexture", 0);
        
        _backgroundMeshRenderer.Render();
        _backgroundMeshRenderer.Unbind();
    }
    
    private void RenderItems(Matrix4x4 projection)
    {
        var itemGroups = new Dictionary<ItemTextures.ItemTextureType, List<(int slot, string itemId)>>();
        var slots = _craftingGrid.GetAllSlots();
    
        for (var i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot.Item == null) continue;
        
            var textureType = _itemTextures.GetTextureTypeForItem(slot.Item.Value.ItemId);
            if (!itemGroups.ContainsKey(textureType))
                itemGroups[textureType] = new List<(int, string)>();
            itemGroups[textureType].Add((i, slot.Item.Value.ItemId));
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
        
        var craftingResult = _craftingGrid.Result;
        if (craftingResult.HasValue)
        {
            var textureType = _itemTextures.GetTextureTypeForItem(craftingResult.Value.ItemId);
            _itemTextures.Use(textureType);
            _resultItemRenderer.UpdateMesh(GenerateResultItemMesh(craftingResult.Value.ItemId));
            _resultItemRenderer.Render();
            _resultItemRenderer.Unbind();
        }
    }

    private Mesh GenerateResultItemMesh(string itemId)
    {
        var vertices = new List<float>();
        var indices = new List<uint>();
        
        var screenCenter = _screenSize / 2f;

        var totalBackgroundWidth = (_craftingGrid.Width * SlotWidth) + (SlotSpacing * (_craftingGrid.Width - 1));
        var baseXPosition = screenCenter.X - totalBackgroundWidth / 2f;
        
        var ti = _itemTextures.GetTextureIndexForItem(itemId);
        var outputXPosition = baseXPosition + ((_craftingGrid.Width + 1) * (SlotWidth + SlotSpacing));
        var outputYPosition = BaseYPosition + (SlotHeight + SlotSpacing);
        
        vertices.AddRange(CreateQuad(outputXPosition, outputYPosition, ti));
        indices.AddRange([
            0, 1, 2, 2, 3, 0
        ]);

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
    
    
    private Mesh GenerateItemsMeshForGroup(List<(int slotIndex, string itemId)> items, string? resultItemId = null)
    {
        var vertices = new List<float>();
        var indices = new List<uint>();

        var screenCenter = _screenSize / 2f;

        var totalBackgroundWidth = (_craftingGrid.Width * SlotWidth) + (SlotSpacing * (_craftingGrid.Width - 1));
        var baseXPosition = screenCenter.X - totalBackgroundWidth / 2f;

        var indicesOffset = 0u;
        foreach (var (slotIndex, itemId) in items)
        {
            var row = (int)MathF.Floor((float)slotIndex / _craftingGrid.Width);
            var column = slotIndex % _craftingGrid.Width;

            var xOffset = column * (SlotWidth + SlotSpacing);
            var xPosition = baseXPosition + xOffset;

            var yOffset = row * (SlotHeight + SlotSpacing);
            var yPosition = BaseYPosition + yOffset;

            var textureIndex = _itemTextures.GetTextureIndexForItem(itemId);

            vertices.AddRange(CreateQuad(xPosition, yPosition, textureIndex));
            indices.AddRange([
                indicesOffset, 1 + indicesOffset, 2 + indicesOffset,
                2 + indicesOffset, 3 + indicesOffset, indicesOffset
            ]);
            indicesOffset += 4;
        }

        if (resultItemId != null)
        {
            var textureIndex = _itemTextures.GetTextureIndexForItem(resultItemId);
            var outputXPosition = baseXPosition + ((_craftingGrid.Width + 1) * (SlotWidth + SlotSpacing));
            var outputYPosition = BaseYPosition + (SlotHeight + SlotSpacing);
            vertices.AddRange(CreateQuad(outputXPosition, outputYPosition, textureIndex));
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

        var screenCenter = _screenSize / 2f;

        var totalBackgroundWidth = (_craftingGrid.Width * SlotWidth) + (SlotSpacing * (_craftingGrid.Width - 1));
        var baseXPosition = screenCenter.X - totalBackgroundWidth / 2f;

        var indicesOffset = 0u;
        for (var i = 0; i < _craftingGrid.Width * _craftingGrid.Height; i++)
        {
            var row = (int)MathF.Floor((float)i / _craftingGrid.Width);
            var column = i % _craftingGrid.Width;

            var xOffset = column * (SlotWidth + SlotSpacing);
            var xPosition = baseXPosition + xOffset;

            var yOffset = row * (SlotHeight + SlotSpacing);
            var yPosition = BaseYPosition + yOffset;

            var color = new Vector3(1.0f, 1.0f, 1.0f);
            vertices.AddRange(CreateQuad(xPosition, yPosition, color));
            indices.AddRange([
                indicesOffset, 1 + indicesOffset, 2 + indicesOffset,
                2 + indicesOffset, 3 + indicesOffset, indicesOffset
            ]);
            indicesOffset += 4;
            
            _slotIndexGridPositionMap.Add(i, new Vector2D<int>(column, row));
        }
        
        // Also add the output slot to the right
        var outputXPosition = baseXPosition + ((_craftingGrid.Width + 1) * (SlotWidth + SlotSpacing));
        var outputYPosition = BaseYPosition + (SlotHeight + SlotSpacing);
        vertices.AddRange(CreateQuad(outputXPosition, outputYPosition, new Vector3(1.0f, 1.0f, 1.0f)));
        indices.AddRange([
            indicesOffset, 1 + indicesOffset, 2 + indicesOffset,
            2 + indicesOffset, 3 + indicesOffset, indicesOffset
        ]);
        indicesOffset += 4;

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