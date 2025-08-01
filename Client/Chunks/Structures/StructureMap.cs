using Silk.NET.Maths;

namespace Client.Chunks.Structures;

// A 2d map representing points where structures should be placed
public class StructureMap
{
    private readonly Dictionary<Vector2D<int>, StructureType> _structures = new();

    public StructureType? TryGetStructure(Vector2D<int> horizontalPosition)
    {
        if (_structures.TryGetValue(horizontalPosition, out var structure))
        {
            return structure;
        }

        return null;
    }

    public void SetStructure(Vector2D<int> horizontalPosition, StructureType structure)
    {
        _structures[horizontalPosition] = structure;
    }
}