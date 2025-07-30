using Client.Blocks;

namespace Client.Items.MeshGeneration;

public static class ItemMeshGeneration
{
    private static readonly Dictionary<string, Mesh> _meshCache = new();
    
    public static Mesh Generate(ItemData item, ItemTextures itemTextures, BlockTextures blockTextures, BlockDatabase blockDatabase)
    {
        if (_meshCache.TryGetValue(item.ExternalId, out var cachedMesh))
        {
            return cachedMesh;
        }

        var mesh = item.GetType() == typeof(BlockItemData)
            ? BlockMeshGenerator.Generate(item, blockDatabase, blockTextures)
            : SpriteMeshGenerator.Generate(item, itemTextures);
        
        _meshCache.Add(item.ExternalId, mesh);
        return mesh;
    }
}