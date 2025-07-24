namespace Client;

public enum BlockType
{
    Air = 0,
    Dirt = 1,
    Cobblestone = 2,
    Grass = 3,
    Sand = 4,
    Log = 5,
    Leaves = 6,
    Glass = 7,
}

public static class BlockTypeExtensions
{
    public static bool IsTransparent(this BlockType blockType)
    {
        return blockType switch
        {
            BlockType.Glass => true,
            _ => false
        };
    }

    public static float GetTextureIndex(this BlockType blockType, BlockData.FaceDirection faceDirection)
    {
        return blockType switch
        {
            
            BlockType.Air => 0f,
            BlockType.Dirt => 0f,
            BlockType.Cobblestone => 1f,
            BlockType.Grass => faceDirection switch
            {
                BlockData.FaceDirection.Top => 3f,
                BlockData.FaceDirection.Bottom => 0f,
                _ => 2f
            },
            BlockType.Sand => 4f,
            BlockType.Log => faceDirection switch
            {
                BlockData.FaceDirection.Top => 5f,
                BlockData.FaceDirection.Bottom => 5f,
                _ => 6f
            },
            BlockType.Leaves => 7f,
            BlockType.Glass => 8f,
            _ => throw new NotImplementedException()
        };
    }
}