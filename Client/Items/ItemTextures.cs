using Client.Blocks;
using Silk.NET.OpenGL;

namespace Client.Items;

public class ItemTextures
{
    private readonly TextureArray _textures;
    private readonly Dictionary<string, uint> _textureIndexMap = new();

    public ItemTextures(GL gl, ItemDatabase itemDatabase, BlockDatabase blockDatabase, BlockSpriteRenderer blockSpriteRenderer)
    {
        _textures = LoadTextures(itemDatabase.All, gl, blockDatabase, blockSpriteRenderer);
    }

    private TextureArray LoadTextures(ItemData[] items, GL gl, BlockDatabase blockDatabase, BlockSpriteRenderer blockSpriteRenderer)
    {
        var basePath = Path.Combine("..", "..", "..", "Resources", "Textures", "Items");
        var builder = new TextureArrayBuilder(16, 16);
        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if (item.GetType() == typeof(BlockItemData))
            {
                var blockId = blockDatabase.GetInternalId(item.ExternalId);
                var data = blockSpriteRenderer.GetBlockTextureData(blockId);
                builder = builder.AddTextureFromMemory(data);
            }

            else
            {
                var texturePath = Path.Combine(basePath, item.Texture);
                if (!File.Exists(texturePath))
                {
                    texturePath = Path.Combine(basePath, "missing.png");
                    Console.WriteLine($"[Item Textures]: MISSING TEXTURE FOR ITEM {item.ExternalId}");
                }
                builder = builder.AddTexture(texturePath);
            }
            
            _textureIndexMap.Add(item.ExternalId, (uint)i);
        }

        Console.WriteLine($"[Item Textures]: Loaded {items.Length} item textures");
        return builder.Build(gl);
    }
    
    public uint GetTextureIndexForItem(string itemId)
    {
        return _textureIndexMap[itemId];
    }

    public void Use()
    {
        _textures.Bind();
    }
}