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
        public readonly uint[] Indices;
        
        public Face(FaceDirection direction, float[] vertices, uint[] indices)
        {
            Direction = direction;
            Vertices = vertices;
            Indices = indices;
        }
    }

    public static Face[] Faces =
    {
        new(FaceDirection.Back, [
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,  // Bottom-left
             0.5f, -0.5f, -0.5f,  1.0f, 1.0f, 0.8f,  // Bottom-right
             0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 0.8f,  // Top-right
            -0.5f,  0.5f, -0.5f,  0.0f, 0.0f, 0.8f   // Top-left
        ], [0, 1, 2, 2, 3, 0]),

        new(FaceDirection.Front, [
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 0.8f,  // Bottom-left
             0.5f, -0.5f,  0.5f,  1.0f, 1.0f, 0.8f,  // Bottom-right
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,  // Top-right
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 0.8f   // Top-left
        ], [0, 1, 2, 2, 3, 0]),

        new(FaceDirection.Left, [
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 0.8f,  // Top-front
            -0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 0.8f,  // Top-back
            -0.5f, -0.5f, -0.5f,  1.0f, 1.0f, 0.8f,  // Bottom-back
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 0.8f   // Bottom-front
        ], [0, 1, 2, 2, 3, 0]),

        new(FaceDirection.Right, [
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,  // Top-front
             0.5f,  0.5f, -0.5f,  0.0f, 0.0f, 0.8f,  // Top-back
             0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,  // Bottom-back
             0.5f, -0.5f,  0.5f,  1.0f, 1.0f, 0.8f   // Bottom-front
        ], [0, 1, 2, 2, 3, 0]),

        new(FaceDirection.Bottom, [
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.6f,  // Back-left
             0.5f, -0.5f, -0.5f,  1.0f, 1.0f, 0.6f,  // Back-right
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.6f,  // Front-right
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.6f   // Front-left
        ], [0, 1, 2, 2, 3, 0]),

        new(FaceDirection.Top, [
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 1.0f,  // Back-left
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 1.0f,  // Back-right
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 1.0f,  // Front-right
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 1.0f   // Front-left
        ], [0, 1, 2, 2, 3, 0])
    };
}