using Silk.NET.OpenGL;

namespace Client.Items;

public class ItemTextures
{
    private readonly TextureArray _textures;
    private readonly Dictionary<string, uint> _textureIndexMap = new();

    public ItemTextures(GL gl, ItemDatabase itemDatabase)
    {
        _textures = LoadTextures(itemDatabase.All, gl);
    }

    private TextureArray LoadTextures(ItemData[] items, GL gl)
    {
        var basePath = Path.Combine("..", "..", "..", "Resources", "Textures", "Items");
        var builder = new TextureArrayBuilder(16, 16);
        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var texturePath = Path.Combine(basePath, item.Texture);
            if (!File.Exists(texturePath))
            {
                texturePath = Path.Combine(basePath, "missing.png");
                Console.WriteLine($"[Item Textures]: MISSING TEXTURE FOR ITEM {item.ExternalId}");
            }
            builder = builder.AddTexture(texturePath);
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