namespace Client.Blocks;

public class BlockDatabase
{
    private readonly BlockData[] _blocks;
    private readonly Dictionary<string, int> _externalToInternalIdMapping = new();
    private readonly int _maxBlockId;

    public BlockDatabase(BlockData[] blocks)
    {
        _maxBlockId = blocks.Length;
        _blocks = new BlockData[_maxBlockId + 1];
        
        for (var i = 0; i < blocks.Length; i++)
        {
            var block = blocks[i];
            _blocks[i] = block;
            _externalToInternalIdMapping.Add(block.ExternalId, i);
        }
        
        _blocks[_maxBlockId] = new BlockData
        {
            DisplayName = "Air",
            ExternalId = "air",
            IsSolid = false,
            IsTransparent = false,
            Strength = 0f
        };
        _externalToInternalIdMapping.Add("air", _maxBlockId);
    }

    public (int id, BlockData data)[] GetAll()
    {
        var result = new (int, BlockData)[_blocks.Length];
        for (var i = 0; i < _blocks.Length; i++)
        {
            result[i] = (i, _blocks[i]);
        }
        return result;
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
}