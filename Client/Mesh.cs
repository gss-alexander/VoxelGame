namespace Client;

public struct Mesh
{
    public static readonly Mesh Empty = new([], []);
    
    public float[] Vertices { get; private set; }
    public uint[] Indices { get; private set; }

    public Mesh(float[] vertices, uint[] indices)
    {
        Vertices = vertices;
        Indices = indices;
    }
}