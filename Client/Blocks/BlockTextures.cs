using Silk.NET.OpenGL;

namespace Client.Blocks;

public class BlockTextures
{
    public TextureArray Textures { get; }

    private readonly Dictionary<string, uint> _textureArrayMapping = new();
    private readonly Dictionary<int, Dictionary<BlockGeometry.FaceDirection, uint>> _blockFaceTextures = new();
    
    public BlockTextures(GL gl, BlockDatabase blockDatabase)
    {
        Textures = LoadTexturesToArray(gl);
        MapBlocksToTextures(blockDatabase);
    }
    
    public uint GetBlockTextureIndex(int blockId, BlockGeometry.FaceDirection faceDirection)
    {
        return _blockFaceTextures[blockId][faceDirection];
    }

    private void MapBlocksToTextures(BlockDatabase blockDatabase)
    {
        var allBlocks = blockDatabase.GetAll();
        for (var i = 0; i < allBlocks.Length; i++)
        {
            var (id, data) = allBlocks[i];
            _blockFaceTextures.Add(id, new Dictionary<BlockGeometry.FaceDirection, uint>());
            _blockFaceTextures[id].Add(BlockGeometry.FaceDirection.Top,
                _textureArrayMapping[data.Textures.GetTextureForFace(BlockGeometry.FaceDirection.Top)]);
            _blockFaceTextures[id].Add(BlockGeometry.FaceDirection.Bottom,
                _textureArrayMapping[data.Textures.GetTextureForFace(BlockGeometry.FaceDirection.Bottom)]);
            _blockFaceTextures[id].Add(BlockGeometry.FaceDirection.Right,
                _textureArrayMapping[data.Textures.GetTextureForFace(BlockGeometry.FaceDirection.Right)]);
            _blockFaceTextures[id].Add(BlockGeometry.FaceDirection.Left,
                _textureArrayMapping[data.Textures.GetTextureForFace(BlockGeometry.FaceDirection.Left)]);
            _blockFaceTextures[id].Add(BlockGeometry.FaceDirection.Front,
                _textureArrayMapping[data.Textures.GetTextureForFace(BlockGeometry.FaceDirection.Front)]);
            _blockFaceTextures[id].Add(BlockGeometry.FaceDirection.Back,
                _textureArrayMapping[data.Textures.GetTextureForFace(BlockGeometry.FaceDirection.Back)]);
        }
    }

    private TextureArray LoadTexturesToArray(GL gl)
    {
        var textureArrayBuilder = new TextureArrayBuilder(16, 16);
        
        var textureDirectoryPath = Path.Combine("..", "..", "..", "Resources", "Textures", "Blocks");
        var textureFiles = Directory.GetFiles(textureDirectoryPath, "*.png");
        for (var i = 0; i < textureFiles.Length; i++)
        {
            var texturePath = textureFiles[i];
            textureArrayBuilder = textureArrayBuilder.AddTexture(texturePath);
            
            var fileName = Path.GetFileName(texturePath);
            _textureArrayMapping.Add(fileName, (uint)i);
            
            Console.WriteLine($"[Block textures]: Loaded texture \"{fileName}\" with texture index {i}");
        }

        return textureArrayBuilder.Build(gl);
    }
}