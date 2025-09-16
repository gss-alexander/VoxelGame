namespace Client.Blocks;

public class BlockDatabase
{
    public (int id, BlockData data)[] GetAll()
    {
        return _blocks.Select(kvp => (kvp.Key, kvp.Value)).ToArray();
    }
    
    private readonly Dictionary<int, BlockData> _blocks = new();
    private readonly bool[] _isSolidBlockArray;

    private readonly Dictionary<string, int> _externalToInternalIdMapping = new();

    public BlockDatabase(BlockData[] blocks)
    {
        for (var i = 0; i < blocks.Length; i++)
        {
            var block = blocks[i];
            _blocks.Add(i, block);
            _externalToInternalIdMapping.Add(block.ExternalId, i);
        }
        
        // Manually add air block - todo: look into just not having an air block...
        _blocks.Add(blocks.Length, new BlockData
        {
            DisplayName = "Air",
            ExternalId = "air",
            IsSolid = false,
            IsTransparent = false,
            Strength = 0f
        });
        _externalToInternalIdMapping.Add("air", blocks.Length);

        _isSolidBlockArray = new bool[_blocks.Count];
        foreach (var (id, blockData) in _blocks)
        {
            _isSolidBlockArray[id] = blockData.IsSolid;
        }
    }

    public int GetInternalId(string externalId)
    {
        return _externalToInternalIdMapping[externalId];
    }

    public string GetExternalId(int id)
    {
        return _blocks[id].ExternalId;
    }

    public BlockData GetById(int id)
    {
        return _blocks[id];
    }

    public bool IsBlockSolid(int id)
    {
        return _isSolidBlockArray[id];
    }
}