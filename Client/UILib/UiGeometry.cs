using System.Numerics;

namespace Client.UILib;

public static class UiGeometry
{
    public static Mesh CreateQuadColored(float x, float y, float width, float height, Vector3 color)
    {
        float[] vertices =
        [
            // top left
            x, y, 0.0f, 0.0f, color.X, color.Y, color.Z, 
            
            // top right
            x + width, y, 1.0f, 0.0f, color.X, color.Y, color.Z, 
            
            // bottom right
            x + width, y + height, 1.0f, 1.0f, color.X, color.Y, color.Z, 
            
            // bottom left
            x, y + height, 0.0f, 1.0f, color.X, color.Y, color.Z 
        ];

        uint[] indices =
        [
            0, 1, 2, 2, 3, 0
        ];
        
        return new Mesh(vertices, indices);
    }

    public static Mesh CreateQuad(float x, float y, float width, float height)
    {
        float[] vertices =
        [
            x, y, 0.0f, 0.0f,                   // top left
            x + width, y, 1.0f, 0.0f,           // top right
            x + width, y + height, 1.0f, 1.0f,  // bottom right
            x, y + height, 0.0f, 1.0f,          // bottom left
        ];

        uint[] indices =
        [
            0, 1, 2, 2, 3, 0
        ];
        
        return new Mesh(vertices, indices);
    }
}