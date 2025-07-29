using Client.Blocks;

namespace Client.Items;

public static class BlockMeshGenerator
{
    private static readonly Dictionary<string, Mesh> MeshCache = new();
    
    public static Mesh Generate(ItemData item, BlockDatabase blockDatabase, BlockTextures blockTextures)
    {
        if (MeshCache.TryGetValue(item.ExternalId, out var cachedMesh))
        {
            return cachedMesh;
        }
        
        var blockId = blockDatabase.GetInternalId(item.ExternalId);
        
        var vertices = new List<float>();
        var indices = new List<uint>();
        
        var faces = BlockGeometry.Faces;
        var indicesOffset = 0u;
        foreach (var face in faces)
        {
            var textureIndex = blockTextures.GetBlockTextureIndex(blockId, face.Direction);
            for (var vertexIndex = 0; vertexIndex < face.Vertices.Length; vertexIndex += 6)
            {
                var vX = face.Vertices[vertexIndex];
                var vY = face.Vertices[vertexIndex + 1];
                var vZ = face.Vertices[vertexIndex + 2];
                var vU = face.Vertices[vertexIndex + 3];
                var vV = face.Vertices[vertexIndex + 4];
                var brightness = face.Vertices[vertexIndex + 5];
        
                vertices.Add(vX);
                vertices.Add(vY);
                vertices.Add(vZ);
                vertices.Add(vU);
                vertices.Add(vV);
                vertices.Add(textureIndex);
                vertices.Add(brightness); // full brightness - todo: handle this better
            }

            foreach (var index in face.Indices)
            {
                indices.Add(index + indicesOffset);
            }

            indicesOffset += 4;
        }

        var mesh = new Mesh(vertices.ToArray(), indices.ToArray());
        MeshCache.Add(item.ExternalId, mesh);
        return mesh;
    }
}