using Silk.NET.Maths;

namespace Client.Chunks.Structures;

public struct WorldStructure
{
    public StructureType Type { get; }
    public Vector2D<int> HorizontalLocalPosition { get; }
    public bool IsPlaced { get; }
    public int Metadata { get; } // Used to store additional data like tree height

    public WorldStructure(StructureType type, Vector2D<int> horizontalLocalPosition, bool isPlaced, int metadata = 0)
    {
        Type = type;
        HorizontalLocalPosition = horizontalLocalPosition;
        IsPlaced = isPlaced;
        Metadata = metadata;
    }
}