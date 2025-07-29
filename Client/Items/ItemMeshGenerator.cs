using System.Drawing;
using StbImageSharp;

namespace Client.Items;

public static class ItemMeshGenerator
{
    public static Mesh Generate(ItemData item, ItemTextures itemTextures)
    {
        var solidMap = LoadImageDataForItem(item.Texture);
        return CreateGeometry(solidMap, item.ExternalId, itemTextures);
    }

    private static Mesh CreateGeometry(SpriteSolidMap solidMap, string itemId, ItemTextures itemTextures)
    {
        var vertices = new List<float>();
        var indices = new List<uint>();

        var indicesOffset = 0u;
        for (var x = 0; x < solidMap.Width; x++)
        {
            for (var y = 0; y < solidMap.Height; y++)
            {
                var isSolid = solidMap.IsSolid(x, y);
                if (!isSolid) continue;

                var textureIndex = itemTextures.GetTextureIndexForItem(itemId);
                var u = x / 16.0f;
                var v = (solidMap.Height - 1 - y) / (float)solidMap.Height; 
                AddMeshData(x, y, u, v, textureIndex, vertices, indices, ref indicesOffset);
            }
        }

        return new Mesh(vertices.ToArray(), indices.ToArray());
    }

    private static void AddMeshData(int x, int y, float u, float v, float ti, List<float> vertices, List<uint> indices, ref uint indicesOffset)
    {
        vertices.AddRange([
            // BACK
            -0.5f + x, -0.5f + y, -0.5f, u, v, ti,   // Bottom-left
             0.5f + x, -0.5f + y, -0.5f, u, v, ti,   // Bottom-right
             0.5f + x,  0.5f + y, -0.5f, u, v, ti,   // Top-right
            -0.5f + x,  0.5f + y, -0.5f, u, v, ti,   // Top-left
            
            // FRONT
            -0.5f + x, -0.5f + y,  0.5f, u, v, ti,  // Bottom-left
             0.5f + x, -0.5f + y,  0.5f, u, v, ti,  // Bottom-right
             0.5f + x,  0.5f + y,  0.5f, u, v, ti,  // Top-right
            -0.5f + x,  0.5f + y,  0.5f, u, v, ti,  // Top-left
            
            // LEFT
            -0.5f + x,  0.5f + y,  0.5f, u, v, ti, // Top-front
            -0.5f + x,  0.5f + y, -0.5f, u, v, ti, // Top-back
            -0.5f + x, -0.5f + y, -0.5f, u, v, ti,// Bottom-back
            -0.5f + x, -0.5f + y,  0.5f, u, v, ti,// Bottom-front
            
            // RIGHT
             0.5f + x,  0.5f + y,  0.5f, u, v, ti, // Top-front
             0.5f + x,  0.5f + y, -0.5f, u, v, ti,  // Top-back
             0.5f + x, -0.5f + y, -0.5f, u, v, ti, // Bottom-back
             0.5f + x, -0.5f + y,  0.5f, u, v, ti,  // Bottom-front
            
            // BOTTOM
            -0.5f + x, -0.5f + y, -0.5f, u, v, ti, // Back-left
             0.5f + x, -0.5f + y, -0.5f, u, v, ti,  // Back-right
             0.5f + x, -0.5f + y,  0.5f, u, v, ti,  // Front-right
            -0.5f + x, -0.5f + y,  0.5f, u, v, ti,  // Front-left
            
            // TOP
            -0.5f + x,  0.5f + y, -0.5f, u, v, ti,   // Back-left
             0.5f + x,  0.5f + y, -0.5f, u, v, ti,    // Back-right
             0.5f + x,  0.5f + y,  0.5f, u, v, ti,   // Front-right
            -0.5f + x,  0.5f + y,  0.5f, u, v, ti,   // Front-left
        ]);

        uint[] standardIndices =
        [
            0, 1, 2, 2, 3, 0,
            4, 5, 6, 6, 7, 4,
            8, 9, 10, 10, 11, 8,
            12, 13, 14, 14, 15, 12,
            16, 17, 18, 18, 19, 16,
            20, 21, 22, 22, 23, 20
        ];
        foreach (var index in standardIndices)
        {
            indices.Add(index + indicesOffset);
        }

        indicesOffset += 24;
    }

    private static SpriteSolidMap LoadImageDataForItem(string texture)
    {
        var path = Path.Combine("..", "..", "..", "Resources", "Textures", "Items", texture);

        using var stream = File.OpenRead(path);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        var pixels = new bool[image.Width * image.Height];

        for (var x = 0; x < image.Width; x++)
        {
            for (var y = 0; y < image.Height; y++)
            {
                var index = (y * image.Width + x) * 4; // RGBA = 4 bytes per pixel
                var alpha = image.Data[index + 3]; // Alpha is the 4th component
            
                var pixelIndex = (image.Height - 1 - y) * image.Width + x;
                pixels[pixelIndex] = alpha > 0; 
            }
        }

        return new SpriteSolidMap(image.Width, image.Height, pixels);
    }

    private struct SpriteSolidMap
    {
        public int Width { get; }
        public int Height { get; }
        public bool[] Pixels { get; }

        public SpriteSolidMap(int width, int height, bool[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public bool IsSolid(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return false;
            return Pixels[y * Width + x];
        }
    }
}