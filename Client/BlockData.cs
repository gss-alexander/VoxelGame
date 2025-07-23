namespace Client;

public static class BlockData
{
    public enum FaceDirection
    {
        Back,
        Front,
        Left,
        Right,
        Top,
        Bottom
    }

    public readonly struct Face
    {
        public readonly FaceDirection Direction;
        public readonly float[] Vertices;
        
        public Face(FaceDirection direction, float[] vertices)
        {
            Direction = direction;
            Vertices = vertices;
        }
    }

    public static Face[] Faces =
    {
        new(FaceDirection.Back, [
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,  // Bottom-left
            0.5f, -0.5f, -0.5f,  1.0f, 1.0f, 0.8f,   // Bottom-right
            0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 0.8f,   // Top-right
            0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 0.8f,   // Top-right
            -0.5f,  0.5f, -0.5f,  0.0f, 0.0f, 0.8f,  // Top-left
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f   // Bottom-left
        ]),
        new(FaceDirection.Front, [
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 0.8f,  // Bottom-left
            0.5f, -0.5f,  0.5f,  1.0f, 1.0f, 0.8f,   // Bottom-right
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,   // Top-right
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,   // Top-right
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 0.8f,  // Top-left
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 0.8f,  // Bottom-left
        ]),
        new(FaceDirection.Left, [
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 0.8f,  // Top-front
            -0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 0.8f,  // Top-back
            -0.5f, -0.5f, -0.5f,  1.0f, 1.0f, 0.8f,  // Bottom-back
            -0.5f, -0.5f, -0.5f,  1.0f, 1.0f, 0.8f,  // Bottom-back
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 0.8f,  // Bottom-front
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 0.8f,  // Top-front
        ]),
        new(FaceDirection.Right, [
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,   // Top-front
            0.5f,  0.5f, -0.5f,  0.0f, 0.0f, 0.8f,   // Top-back
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,   // Bottom-back
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,   // Bottom-back
            0.5f, -0.5f,  0.5f,  1.0f, 1.0f, 0.8f,   // Bottom-front
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,   // Top-front
        ]),
        new(FaceDirection.Bottom, [
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.6f,
            0.5f, -0.5f, -0.5f,  1.0f, 1.0f, 0.6f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.6f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.6f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.6f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.6f,
        ]),
        new(FaceDirection.Top, [
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 1.0f
        ])
    };
}