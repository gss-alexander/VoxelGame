﻿using System.Diagnostics;
using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Client;

public class BlockDrops
{
    private readonly GL _gl;
    private readonly Shader _droppedBlockShader;
    private readonly TextureArray _blockTextures;

    private readonly List<BlockModel> _blockModels = new();

    public BlockDrops(GL gl, Shader droppedBlockShader, TextureArray blockTextures)
    {
        _gl = gl;
        _droppedBlockShader = droppedBlockShader;
        _blockTextures = blockTextures;
    }
    
    public void CreateDroppedBlock(Vector3 worldPosition, BlockType type, Func<Vector3, bool> isDroppedFunc)
    {
        if (type == BlockType.Air)
        {
            throw new InvalidOperationException($"Cannot drop air block");
        }

        var blockModel = new BlockModel(_gl, type, _droppedBlockShader, _blockTextures, worldPosition, isDroppedFunc);
        _blockModels.Add(blockModel);
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