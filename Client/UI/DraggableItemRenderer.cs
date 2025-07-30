using System.Numerics;
using Client.Items;
using Client.UI.Text;
using Silk.NET.OpenGL;

namespace Client.UI;

public class DraggableItemRenderer
{
    // this should really be a part of the item storage itself..
    public class DraggingItemStack
    {
        public string ItemId { get; set; }
        public int Count { get; set; }
    }
    
    public DraggingItemStack? CurrentItemStack { get; private set; }
    
    private const float Size = 75f;
    
    private readonly GL _gl;
    private readonly TextRenderer _textRenderer;
    private readonly Vector2 _screenSize;
    private readonly ItemTextures _itemTextures;
    private readonly MeshRenderer _itemSpriteRenderer;
    private readonly Shader _uiSpriteShader;

    private Vector2 _lastPosition;

    public DraggableItemRenderer(GL gl, TextRenderer textRenderer, Vector2 screenSize, ItemTextures itemTextures)
    {
        _gl = gl;
        _textRenderer = textRenderer;
        _screenSize = screenSize;
        _itemTextures = itemTextures;

        _itemSpriteRenderer = new MeshRenderer(gl, Mesh.Empty, BufferUsageARB.DynamicDraw);
        _itemSpriteRenderer.SetVertexAttribute(0, 2, VertexAttribPointerType.Float, 5, 0);
        _itemSpriteRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 5, 2);
        _itemSpriteRenderer.SetVertexAttribute(2, 1, VertexAttribPointerType.Float, 5, 4);
        _itemSpriteRenderer.Unbind();
        
        _uiSpriteShader = Shaders.GetShader(gl, "itemUiSprite");
    }

    public void SetDragging(string itemId, int count)
    {
        CurrentItemStack = new DraggingItemStack
        {
            ItemId = itemId,
            Count = count
        };
    }

    public void StopDragging()
    {
        CurrentItemStack = null;
    }

    public void Update(Vector2 mousePosition)
    {
        var mesh = GenerateMesh(mousePosition, CurrentItemStack.ItemId);
        _itemSpriteRenderer.UpdateMesh(mesh);
        _lastPosition = mousePosition;
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
        _itemTextures.Use(_itemTextures.GetTextureTypeForItem(CurrentItemStack.ItemId));
        _uiSpriteShader.Use();
        _uiSpriteShader.SetUniform("uProjection", projectionMatrix);
        _uiSpriteShader.SetUniform("uTextureArray", 0);
        
        _itemSpriteRenderer.Render();
        RenderItemAmountText(_lastPosition);
        
        _gl.Enable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.Blend);
    }
    
    private void RenderItemAmountText(Vector2 position)
    {
        // var xOffset = i * (SlotWidth + SlotXSpacing) + (SlotWidth / 1.3f);
        _textRenderer.RenderText(
            CurrentItemStack.Count.ToString(),
            position.X + (75f / 1.3f),
            position.Y + 10f,
            0.5f,
            new Vector3(0f, 0f, 0f),
            (int)_screenSize.X,
            (int)_screenSize.Y,
            TextAlignment.Center
        );
    }
    
    private Mesh GenerateMesh(Vector2 position, string itemId)
    {
        var vertices = new List<float>();
        var indices = new List<uint>();

        var x = position.X;
        var y = position.Y;
        var textureIndex = _itemTextures.GetTextureIndexForItem(itemId);
        vertices.AddRange(
        [
            x, y, 0.0f, 0.0f, textureIndex,
            x + Size, y, 1.0f, 0.0f, textureIndex,
            x + Size, y + Size, 1.0f, 1.0f, textureIndex,
            x, y + Size, 0.0f, 1.0f, textureIndex,
        ]);
        indices.AddRange([0, 1, 2, 2, 3, 0]);
   

        return new Mesh(vertices.ToArray(), indices.ToArray());
    } 
}