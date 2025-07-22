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

    public static readonly Face[] Faces =
    {
        new(FaceDirection.Back, [
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 0.8f,
            0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.8f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.8f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.8f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.8f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 0.8f
        ]),
        new(FaceDirection.Front, [
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.8f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.8f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 0.8f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 0.8f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f, 0.8f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.8f,
        ]),
        new(FaceDirection.Left, [
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.8f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.8f,
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,
        ]),
        new(FaceDirection.Right, [
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.8f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,
            0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.8f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,
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