using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.Blocks;

public class BlockDrops
{
    private readonly GL _gl;
    private readonly Shader _droppedBlockShader;
    private readonly BlockTextures _blockTextures;

    private readonly List<BlockModel> _blockModels = new();

    public BlockDrops(GL gl, Shader droppedBlockShader, BlockTextures blockTextures)
    {
        _gl = gl;
        _droppedBlockShader = droppedBlockShader;
        _blockTextures = blockTextures;
    }
    
    public void CreateDroppedBlock(Vector3 worldPosition, int blockId, Func<Vector3, bool> isDroppedFunc)
    {
        var blockModel = new BlockModel(_gl, _blockTextures, blockId, _droppedBlockShader, worldPosition, isDroppedFunc, 0.15f);
        _blockModels.Add(blockModel);
    }

    public List<int> PickupDroppedBlocks(Vector3 origin, float range)
    {
        var pickedUpBlocks = new List<BlockModel>();
        foreach (var block in _blockModels)
        {
            if (Vector3.Distance(block.Position, origin) <= range)
            {
                pickedUpBlocks.Add(block);
            }
        }

        foreach (var pickedUpBlock in pickedUpBlocks)
        {
            _blockModels.Remove(pickedUpBlock);
        }

        if (pickedUpBlocks.Count > 0)
        {
            return pickedUpBlocks.Select(pub => pub.BlockId).ToList();
        }

        return [];
    }

    public void Update(float deltaTime)
    {
        foreach (var block in _blockModels)
        {
            block.Update(deltaTime);
        }
    }

    public void Render(float deltaTime)
    {
        foreach (var block in _blockModels)
        {
            block.Render();
        }
    }
}