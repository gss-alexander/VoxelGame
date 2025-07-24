﻿namespace Client;

public class BlockSelector
{
    public BlockType CurrentBlock => _availableBlocks[_currentSelectedBlockIndex];
    
    private static readonly BlockType[] _availableBlocks = new[]
    {
        BlockType.Dirt,
        BlockType.Grass,
        BlockType.Cobblestone
    };
    
    private int _currentSelectedBlockIndex;

    public void Cycle(int direction)
    {
        _currentSelectedBlockIndex += direction;
        if (_currentSelectedBlockIndex < 0)
        {
            _currentSelectedBlockIndex = _availableBlocks.Length - 1;
        }

        if (_currentSelectedBlockIndex > _availableBlocks.Length - 1)
        {
            _currentSelectedBlockIndex = 0;
        }
    }
}