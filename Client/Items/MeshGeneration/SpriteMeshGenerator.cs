﻿using StbImageSharp;

namespace Client.Items;

public static class SpriteMeshGenerator
{
    private static readonly Dictionary<string, Mesh> MeshCache = new();
    
    public static Mesh Generate(ItemData item, ItemTextures itemTextures)
    {
        if (MeshCache.TryGetValue(item.ExternalId, out var cachedMesh))
        {
            return cachedMesh;
        }
        
        var solidMap = LoadImageDataForItem(item.Texture);
        var mesh = CreateGeometry(solidMap, item.ExternalId, itemTextures);
        MeshCache.Add(item.ExternalId, mesh);
        return mesh;
    }

    private static Mesh CreateGeometry(SpriteSolidMap solidMap, string itemId, ItemTextures itemTextures)
    {
        var vertices = new List<float>();
        var indices = new List<uint>();

        var bounds = CalculateSpriteBounds(solidMap);
        if (!bounds.HasValue)
        {
            return new Mesh(Array.Empty<float>(), Array.Empty<uint>());
        }

        var (minX, minY, maxX, maxY) = bounds.Value;
        
        // Calculate center offset to center the sprite
        var centerX = (minX + maxX) / 2.0f;
        var centerY = (minY + maxY) / 2.0f;
        
        // Calculate scaling factor to normalize to 1 unit max
        var spriteWidth = maxX - minX + 1; // +1 because bounds are inclusive
        var spriteHeight = maxY - minY + 1;
        var maxDimension = Math.Max(spriteWidth, spriteHeight);
        var scale = 1.0f / maxDimension;

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
                
                // Adjust position to center and scale the sprite
                var adjustedX = (x - centerX) * scale;
                var adjustedY = (y - centerY) * scale;
                
                AddMeshData(adjustedX, adjustedY, u, v, textureIndex, vertices, indices, ref indicesOffset, scale);
            }
        }

        return new Mesh(vertices.ToArray(), indices.ToArray());
    }

    private static (int minX, int minY, int maxX, int maxY)? CalculateSpriteBounds(SpriteSolidMap solidMap)
    {
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        bool foundSolidPixel = false;

        for (var x = 0; x < solidMap.Width; x++)
        {
            for (var y = 0; y < solidMap.Height; y++)
            {
                if (solidMap.IsSolid(x, y))
                {
                    foundSolidPixel = true;
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }
        }

        return foundSolidPixel ? (minX, minY, maxX, maxY) : null;
    }

    private static void AddMeshData(float x, float y, float u, float v, float ti, List<float> vertices, List<uint> indices, ref uint indicesOffset, float scale)
    {
        var halfScale = scale * 0.5f;
        
        vertices.AddRange([
            // BACK
            -halfScale + x, -halfScale + y, -halfScale, u, v, ti,   // Bottom-left
             halfScale + x, -halfScale + y, -halfScale, u, v, ti,   // Bottom-right
             halfScale + x,  halfScale + y, -halfScale, u, v, ti,   // Top-right
            -halfScale + x,  halfScale + y, -halfScale, u, v, ti,   // Top-left
            
            // FRONT
            -halfScale + x, -halfScale + y,  halfScale, u, v, ti,  // Bottom-left
             halfScale + x, -halfScale + y,  halfScale, u, v, ti,  // Bottom-right
             halfScale + x,  halfScale + y,  halfScale, u, v, ti,  // Top-right
            -halfScale + x,  halfScale + y,  halfScale, u, v, ti,  // Top-left
            
            // LEFT
            -halfScale + x,  halfScale + y,  halfScale, u, v, ti, // Top-front
            -halfScale + x,  halfScale + y, -halfScale, u, v, ti, // Top-back
            -halfScale + x, -halfScale + y, -halfScale, u, v, ti,// Bottom-back
            -halfScale + x, -halfScale + y,  halfScale, u, v, ti,// Bottom-front
            
            // RIGHT
             halfScale + x,  halfScale + y,  halfScale, u, v, ti, // Top-front
             halfScale + x,  halfScale + y, -halfScale, u, v, ti,  // Top-back
             halfScale + x, -halfScale + y, -halfScale, u, v, ti, // Bottom-back
             halfScale + x, -halfScale + y,  halfScale, u, v, ti,  // Bottom-front
            
            // BOTTOM
            -halfScale + x, -halfScale + y, -halfScale, u, v, ti, // Back-left
             halfScale + x, -halfScale + y, -halfScale, u, v, ti,  // Back-right
             halfScale + x, -halfScale + y,  halfScale, u, v, ti,  // Front-right
            -halfScale + x, -halfScale + y,  halfScale, u, v, ti,  // Front-left
            
            // TOP
            -halfScale + x,  halfScale + y, -halfScale, u, v, ti,   // Back-left
             halfScale + x,  halfScale + y, -halfScale, u, v, ti,    // Back-right
             halfScale + x,  halfScale + y,  halfScale, u, v, ti,   // Front-right
            -halfScale + x,  halfScale + y,  halfScale, u, v, ti,   // Front-left
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