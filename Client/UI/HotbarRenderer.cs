﻿using System.Numerics;
using Client.Items;
using Silk.NET.OpenGL;

namespace Client.UI;

public class HotbarRenderer
{
    private const float BarYPosition = 950f;
    private const float SlotXSpacing = 20f;
    private const float SlotWidth = 75f;
    private const float SlotHeight = 75f;

    private readonly GL _gl;
    private readonly ItemStorage _hotbar;
    private readonly Vector2 _screenSize;
    private readonly ItemTextures _itemTextures;
    private readonly Texture _slotBackgroundTexture;
    private readonly MeshRenderer _slotBackgroundRenderer;
    private readonly MeshRenderer _itemSpritesRenderer;

    private readonly Shader _uiSpriteShader;
    private readonly Shader _uiShader;

    public HotbarRenderer(GL gl, ItemStorage hotbar, Vector2 screenSize, ItemTextures itemTextures, Texture slotBackgroundTexture)
    {
        _gl = gl;
        _hotbar = hotbar;
        _screenSize = screenSize;
        _itemTextures = itemTextures;
        _slotBackgroundTexture = slotBackgroundTexture;
        
        _slotBackgroundRenderer = new MeshRenderer(gl, GenerateBackgroundMesh());
        _slotBackgroundRenderer.SetVertexAttribute(0, 2, VertexAttribPointerType.Float, 4, 0);
        _slotBackgroundRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 4, 2);
        _slotBackgroundRenderer.Unbind();

        _itemSpritesRenderer = new MeshRenderer(gl, Mesh.Empty, BufferUsageARB.DynamicDraw);
        _itemSpritesRenderer.SetVertexAttribute(0, 2, VertexAttribPointerType.Float, 5, 0);
        _itemSpritesRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 5, 2);
        _itemSpritesRenderer.SetVertexAttribute(2, 1, VertexAttribPointerType.Float, 5, 4);
        
        _uiSpriteShader = Shaders.GetShader(gl, "itemUiSprite");
        _uiShader = Shaders.GetShader(gl, "ui");
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
        
        _gl.Enable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.Blend);
    }

    private void RenderBackground(Matrix4x4 projection)
    {
        _slotBackgroundTexture.Bind();
        
        _uiShader.Use();
        _uiShader.SetUniform("uProjection", projection);
        _uiShader.SetUniform("uTexture", 0);
        
        _slotBackgroundRenderer.Render();
        _slotBackgroundRenderer.Unbind();
    }

    private void RenderItems(Matrix4x4 projection)
    {
        _itemTextures.Use();
        
        _uiSpriteShader.Use();
        _uiSpriteShader.SetUniform("uProjection", projection);
        _uiSpriteShader.SetUniform("uTextureArray", 0);
        
        _itemSpritesRenderer.UpdateMesh(GenerateItemsMesh());
        _itemSpritesRenderer.Render();
        _itemSpritesRenderer.Unbind();
    }

    private Mesh GenerateItemsMesh()
    {
        var vertices = new List<float>();
        var indices = new List<uint>();
        
        var screenCenter = _screenSize.X / 2f;
        var totalHotbarWidth = (_hotbar.SlotCount * SlotWidth) + (SlotXSpacing * (_hotbar.SlotCount - 1));
        var baseXPosition = screenCenter - totalHotbarWidth / 2f;
        
        var indicesOffset = 0u;
        for (var i = 0; i < _hotbar.SlotCount; i++)
        {
            var slot = _hotbar.GetSlot(i);
            if (slot == null) continue;
            
            var xOffset = i * (SlotWidth + SlotXSpacing);
            var xPosition = baseXPosition + xOffset;

            var textureIndex = _itemTextures.GetTextureIndexForItem(slot.ItemId);

            vertices.AddRange(CreateQuad(xPosition, BarYPosition, textureIndex));
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

        var screenCenter = _screenSize.X / 2f;
        var totalHotbarWidth = (_hotbar.SlotCount * SlotWidth) + (SlotXSpacing * (_hotbar.SlotCount - 1));
        var baseXPosition = screenCenter - totalHotbarWidth / 2f;

        var indicesOffset = 0u;
        for (var i = 0; i < _hotbar.SlotCount; i++)
        {
            var xOffset = i * (SlotWidth + SlotXSpacing);
            var xPosition = baseXPosition + xOffset;

            vertices.AddRange(CreateQuad(xPosition, BarYPosition));
            indices.AddRange([
                indicesOffset, 1 + indicesOffset, 2 + indicesOffset,
                2 + indicesOffset, 3 + indicesOffset, indicesOffset
            ]);
            indicesOffset += 4;
        }

        return new Mesh(vertices.ToArray(), indices.ToArray());

        float[] CreateQuad(float x, float y)
        {
            return
            [
                x, y, 0.0f, 0.0f,
                x + SlotWidth, y, 1.0f, 0.0f,
                x + SlotWidth, y + SlotHeight, 1.0f, 1.0f,
                x, y + SlotHeight, 0.0f, 1.0f,
            ];
        }
    }
}