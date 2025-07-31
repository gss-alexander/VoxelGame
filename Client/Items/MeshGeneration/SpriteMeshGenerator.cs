using StbImageSharp;

namespace Client.Items.MeshGeneration;

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

        for (var x = 0; x < solidMap.Width; x++)
        {
            for (var y = 0; y < solidMap.Height; y++)
            {
                var isSolid = solidMap.IsSolid(x, y);
                if (!isSolid) continue;

                var textureIndex = itemTextures.GetTextureIndexForItem(itemId);
                var u = x / 16.0f;
                var v = (solidMap.Height - 1 - y) / (float)solidMap.Height;
                
                var adjustedX = (x - centerX) * scale;
                var adjustedY = (y - centerY) * scale;
                
                var needBack = !solidMap.IsSolid(x, y - 1);
                var needFront = !solidMap.IsSolid(x, y + 1);
                var needLeft = !solidMap.IsSolid(x - 1, y);
                var needRight = !solidMap.IsSolid(x + 1, y);
                
                AddMeshData(adjustedX, adjustedY, u, v, textureIndex, vertices, indices, scale, 
                    needBack, needFront, needLeft, needRight);
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

    private static void AddMeshData(float x, float y, float u, float v, float ti, List<float> vertices, List<uint> indices, float scale,
        bool needBack, bool needFront, bool needLeft, bool needRight)
    {
        var halfScale = scale * 0.5f;
    
        // BACK
        if (needBack)
        {
            var currentIndex = (uint)(vertices.Count / 6);
            vertices.AddRange([
                -halfScale + x, -halfScale + y, -halfScale, u, v, ti,   // Bottom-left
                halfScale + x, -halfScale + y, -halfScale, u, v, ti,   // Bottom-right
                halfScale + x,  halfScale + y, -halfScale, u, v, ti,   // Top-right
                -halfScale + x,  halfScale + y, -halfScale, u, v, ti,   // Top-left
            ]);
        
            uint[] backIndices = [0, 1, 2, 2, 3, 0];
            foreach (var index in backIndices)
            {
                indices.Add(index + currentIndex);
            }
        }
    
        // FRONT
        if (needFront)
        {
            var currentIndex = (uint)(vertices.Count / 6);
            vertices.AddRange([
                -halfScale + x, -halfScale + y,  halfScale, u, v, ti,  // Bottom-left
                halfScale + x, -halfScale + y,  halfScale, u, v, ti,  // Bottom-right
                halfScale + x,  halfScale + y,  halfScale, u, v, ti,  // Top-right
                -halfScale + x,  halfScale + y,  halfScale, u, v, ti,  // Top-left
            ]);
        
            uint[] frontIndices = [0, 1, 2, 2, 3, 0];
            foreach (var index in frontIndices)
            {
                indices.Add(index + currentIndex);
            }
        }
    
        // LEFT
        if (needLeft)
        {
            var currentIndex = (uint)(vertices.Count / 6);
            vertices.AddRange([
                -halfScale + x,  halfScale + y,  halfScale, u, v, ti, // Top-front
                -halfScale + x,  halfScale + y, -halfScale, u, v, ti, // Top-back
                -halfScale + x, -halfScale + y, -halfScale, u, v, ti,// Bottom-back
                -halfScale + x, -halfScale + y,  halfScale, u, v, ti,// Bottom-front
            ]);
        
            uint[] leftIndices = [0, 1, 2, 2, 3, 0];
            foreach (var index in leftIndices)
            {
                indices.Add(index + currentIndex);
            }
        }
    
        // RIGHT
        if (needRight)
        {
            var currentIndex = (uint)(vertices.Count / 6);
            vertices.AddRange([
                halfScale + x,  halfScale + y,  halfScale, u, v, ti, // Top-front
                halfScale + x,  halfScale + y, -halfScale, u, v, ti,  // Top-back
                halfScale + x, -halfScale + y, -halfScale, u, v, ti, // Bottom-back
                halfScale + x, -halfScale + y,  halfScale, u, v, ti,  // Bottom-front
            ]);
        
            uint[] rightIndices = [0, 1, 2, 2, 3, 0];
            foreach (var index in rightIndices)
            {
                indices.Add(index + currentIndex);
            }
        }
    
        // BOTTOM
        var bottomIndex = (uint)(vertices.Count / 6);
        vertices.AddRange([
            -halfScale + x, -halfScale + y, -halfScale, u, v, ti, // Back-left
            halfScale + x, -halfScale + y, -halfScale, u, v, ti,  // Back-right
            halfScale + x, -halfScale + y,  halfScale, u, v, ti,  // Front-right
            -halfScale + x, -halfScale + y,  halfScale, u, v, ti,  // Front-left
        ]);
    
        uint[] bottomIndices = [0, 1, 2, 2, 3, 0];
        foreach (var index in bottomIndices)
        {
            indices.Add(index + bottomIndex);
        }
    
        // TOP
        var topIndex = (uint)(vertices.Count / 6);
        vertices.AddRange([
            -halfScale + x,  halfScale + y, -halfScale, u, v, ti,   // Back-left
            halfScale + x,  halfScale + y, -halfScale, u, v, ti,    // Back-right
            halfScale + x,  halfScale + y,  halfScale, u, v, ti,   // Front-right
            -halfScale + x,  halfScale + y,  halfScale, u, v, ti,   // Front-left
        ]);
    
        uint[] topIndices = [0, 1, 2, 2, 3, 0];
        foreach (var index in topIndices)
        {
            indices.Add(index + topIndex);
        }
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