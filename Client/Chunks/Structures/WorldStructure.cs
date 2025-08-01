using Silk.NET.Maths;

namespace Client.Chunks.Structures;

public struct WorldStructure
{
    public StructureType Type { get; }
    public Vector2D<int> HorizontalLocalPosition { get; }
    public bool IsPlaced { get; }
    
    public WorldStructure(StructureType type, Vector2D<int> horizontalLocalPosition, bool isPlaced)
    {
        Type = type;
        HorizontalLocalPosition = horizontalLocalPosition;
        IsPlaced = isPlaced;
    }
}