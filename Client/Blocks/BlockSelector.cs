namespace Client.Blocks;

// only for debug
public class BlockSelector
{
    private readonly BlockDatabase _blockDatabase;
    public int CurrentBlock => _availableBlocks[_currentSelectedBlockIndex];
    
    private readonly List<int> _availableBlocks;
    
    private int _currentSelectedBlockIndex;
    
    public BlockSelector(BlockDatabase blockDatabase)
    {
        _blockDatabase = blockDatabase;
        _availableBlocks = new List<int>();
        foreach (var block in _blockDatabase.GetAll())
        {
            if (block.data.ExternalId == "air") continue;
            _availableBlocks.Add(block.id);
        }
    }

    public void Cycle(int direction)
    {
        _currentSelectedBlockIndex += direction;
        if (_currentSelectedBlockIndex < 0)
        {
            _currentSelectedBlockIndex = _availableBlocks.Count - 1;
        }

        if (_currentSelectedBlockIndex > _availableBlocks.Count - 1)
        {
            _currentSelectedBlockIndex = 0;
        }
    }
}