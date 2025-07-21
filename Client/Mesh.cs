namespace Client;

public struct Mesh
{
    public float[] Vertices { get; private set; }
    public uint[] Indices { get; private set; }

    public Mesh(float[] vertices, uint[] indices)
    {
        Vertices = vertices;
        Indices = indices;
    }
}