using Client.Blocks;
using Silk.NET.OpenGL;

namespace Client.Items;

public class ItemTextures
{
    public enum ItemTextureType
    {
        Item,
        Block
    }
    
    private readonly Dictionary<string, uint> _textureIndexMap = new();
    private readonly Dictionary<string, ItemTextureType> _textureTypeMap = new(); 

    private readonly TextureArray _itemSpriteTextures;
    private readonly TextureArray _blockSpriteTextures;

    private readonly Dictionary<string, Texture> _textures = new();

    public ItemTextures(GL gl, ItemDatabase itemDatabase, BlockDatabase blockDatabase, BlockSpriteRenderer blockSpriteRenderer)
    {
        var items = new List<ItemData>();
        var blocks = new List<ItemData>();
        foreach (var itemData in itemDatabase.All)
        {
            if (itemData.GetType() == typeof(BlockItemData))
            {
                blocks.Add(itemData);
            }
            else
            {
                items.Add(itemData);
            }
        }
        
        _itemSpriteTextures = LoadItemSprites(gl, items);
        _blockSpriteTextures = LoadBlockSprites(gl, blocks, blockDatabase, blockSpriteRenderer);
    }
    
    public ItemTextureType GetTextureTypeForItem(string itemId)
    {
        return _textureTypeMap[itemId]; 
    } 

    private TextureArray LoadItemSprites(GL gl, List<ItemData> items)
    {
        var basePath = Path.Combine("..", "..", "..", "Resources", "Textures", "Items");
        var builder = new TextureArrayBuilder(16, 16);
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var texturePath = Path.Combine(basePath, item.Texture);
            if (!File.Exists(texturePath))
            {
                texturePath = Path.Combine(basePath, "missing.png");
                Console.WriteLine($"[Item Textures]: MISSING TEXTURE FOR ITEM {item.ExternalId}");
            }

            var texture = new Texture(texturePath);
            _textures.Add(item.ExternalId, texture);
            
            builder = builder.AddTexture(texturePath);
            Console.WriteLine($"[Item Textures]: Loaded item texture {item.ExternalId}");
            
            _textureIndexMap.Add(item.ExternalId, (uint)i);
            _textureTypeMap.Add(item.ExternalId, ItemTextureType.Item); 
        }

        Console.WriteLine($"[Item Textures]: Loaded {items.Count} item textures");
        return builder.Build(gl);
    }

    private TextureArray LoadBlockSprites(GL gl, List<ItemData> blocks, BlockDatabase blockDatabase, BlockSpriteRenderer blockSpriteRenderer)
    {
        var builder = new TextureArrayBuilder(BlockSpriteRenderer.SpriteSize, BlockSpriteRenderer.SpriteSize);
        
        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var blockId = blockDatabase.GetInternalId(block.ExternalId);
            var blockSpriteData = blockSpriteRenderer.GetBlockTextureData(blockId);

            var texture = new Texture(blockSpriteData, BlockSpriteRenderer.SpriteSize, BlockSpriteRenderer.SpriteSize);
            _textures.Add(block.ExternalId, texture);
            
            builder = builder.AddTextureFromMemory(blockSpriteData);
            _textureIndexMap.Add(block.ExternalId, (uint)i);
            _textureTypeMap.Add(block.ExternalId, ItemTextureType.Block); 
        }

        return builder.Build(gl);
    }

    public Texture GetTextureForItem(string itemId)
    {
        return _textures[itemId];
    }
    
    public uint GetTextureIndexForItem(string itemId)
    {
        return _textureIndexMap[itemId];
    }

    public void Use(ItemTextureType itemType)
    {
        (itemType == ItemTextureType.Block ? _blockSpriteTextures : _itemSpriteTextures).Bind();
    }
}