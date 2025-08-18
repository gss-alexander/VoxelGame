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

    public Mesh(ReadOnlySpan<float> vertices, ReadOnlySpan<uint> indices)
    {
        Vertices = vertices.ToArray();
        Indices = indices.ToArray();
    }

    public Mesh(IEnumerable<Mesh> individualMeshes)
    {
        var vertices = new List<float>();
        var indices = new List<uint>();
        foreach (var mesh in individualMeshes)
        {
            vertices.AddRange(mesh.Vertices);
            indices.AddRange(mesh.Indices);
        }

        Vertices = vertices.ToArray();
        Indices = indices.ToArray();
    }
}