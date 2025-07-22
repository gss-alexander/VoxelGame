namespace Client;

public static class BlockData
{
    public static readonly float[] Vertices =
    {
        //X      Y      Z      U    V    Brightness

        // Back face (South - facing negative Z) - Side face
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 0.8f,
        0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.8f,
        0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.8f,
        0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.8f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.8f,
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 0.8f,

        // Front face (North - facing positive Z) - Side face
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.8f,
        0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.8f,
        0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 0.8f,
        0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 0.8f,
        -0.5f,  0.5f,  0.5f,  0.0f, 1.0f, 0.8f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.8f,

        // Left face (West - facing negative X) - Side face
        -0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,
        -0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.8f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.8f,
        -0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,

        // Right face (East - facing positive X) - Side face
        0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,
        0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.8f,
        0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,
        0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.8f,
        0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.8f,
        0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.8f,

        // Bottom face (Down - facing negative Y) - Darkest
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.6f,
        0.5f, -0.5f, -0.5f,  1.0f, 1.0f, 0.6f,
        0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.6f,
        0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.6f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.6f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.6f,

        // Top face (Up - facing positive Y) - Brightest
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 1.0f,
        0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 1.0f,
        0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 1.0f,
        0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 1.0f
    };
    
    public static readonly uint[] Indices =
    {
        0, 1, 3,
        1, 2, 3
    };
}