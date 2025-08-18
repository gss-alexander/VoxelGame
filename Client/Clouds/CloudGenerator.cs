using Client.Blocks;
using Client.Core;
using Silk.NET.Maths;

namespace Client.Clouds;

public class CloudGenerator
{
    private const float CloudScale = 10f;
    
    private readonly int _maxSize;
    
    public CloudGenerator(int maxSize)
    {
        _maxSize = maxSize;
    }

    public Mesh GenerateMesh()
    {
        var vertices = new List<float>();
        var indices = new List<uint>();
        var indicesOffset = 0u;

        var shape = GenerateCloudShape();

        for (var x = 0; x < _maxSize; x++)
        {
            for (var z = 0; z < _maxSize; z++)
            {
                if (!shape.Get(x, z)) continue;
                
                foreach (var face in BlockGeometry.Faces)
                {
                    // If the neighbour is both within bounds and it is cloud we skip this face
                    if (face.Direction != BlockGeometry.FaceDirection.Top &&
                        face.Direction != BlockGeometry.FaceDirection.Bottom)
                    {
                        var neighbourOffset = face.Direction.GetVectorOffset();
                        var neighbourPos = new Vector2D<int>(x + neighbourOffset.X, z + neighbourOffset.Z);
                        if (shape.IsWithinBounds(neighbourPos.X, neighbourPos.Y) &&
                            shape.Get(neighbourPos.X, neighbourPos.Y))
                        {
                            continue;
                        }
                    }
                    
                    for (var vertexIndex = 0; vertexIndex < face.Vertices.Length; vertexIndex += 6)
                    {
                        var vX = face.Vertices[vertexIndex] * CloudScale + (x * CloudScale);
                        var vY = face.Vertices[vertexIndex + 1] * CloudScale * 0.5f;
                        var vZ = face.Vertices[vertexIndex + 2] * CloudScale + (z * CloudScale);
                        var brightness = face.Vertices[vertexIndex + 5];
    
                        vertices.Add(vX);
                        vertices.Add(vY);
                        vertices.Add(vZ);
                        vertices.Add(brightness);
                    }

                    foreach (var index in face.Indices)
                    {
                        indices.Add(index + indicesOffset);
                    }

                    indicesOffset += 4;
                }
            }
        }

        return new Mesh(vertices.ToArray(), indices.ToArray());
    }

    private Grid2D<bool> GenerateCloudShape()
    {
        var shape = new Grid2D<bool>(_maxSize);

        var startX = Random.Next(1, _maxSize - 1);
        var startY = Random.Next(1, _maxSize - 1);
        shape.Set(startX, startY, true);
        
        var minSize = 2;
        var maxSize = (int)(_maxSize * _maxSize * 0.6);
        var targetSize = Random.Next(minSize, maxSize);
        var currentSize = 1;

        var activeEdges = new List<ValueTuple<int, int>>();
        activeEdges.Add(new ValueTuple<int, int>(startX, startY));

        while (currentSize < targetSize && activeEdges.Count > 0)
        {
            var edgeIndex = Random.Next(0, activeEdges.Count);
            var currentCell = activeEdges[edgeIndex];

            var neighbours = GetEmptyNeighbours(currentCell.Item1, currentCell.Item2, shape);
            if (neighbours.Count == 0)
            {
                activeEdges.RemoveAt(edgeIndex);
                continue;
            }

            var newCellIndex = Random.Next(0, neighbours.Count);
            var newCell = neighbours[newCellIndex];
            shape.Set(newCell.Item1, newCell.Item2, true);
            currentSize++;
            
            activeEdges.Add(newCell);

            if (Random.Range(0f, 1f) < 0.3)
            {
                activeEdges.RemoveAt(edgeIndex);
            }
        }

        return shape;
    }

    private static readonly ValueTuple<int, int>[] Directions =
    [
        new(0, 1),
        new(0, -1),
        new(1, 0),
        new(-1, 0)
    ];

    private List<ValueTuple<int, int>> GetEmptyNeighbours(int x, int y, Grid2D<bool> grid)
    {
        var neighbours = new List<ValueTuple<int, int>>();
        foreach (var direction in Directions)
        {
            var newX = x + direction.Item1;
            var newY = y + direction.Item2;

            if (grid.IsWithinBounds(newX, newY) && !grid.Get(newX, newY))
            {
                neighbours.Add(new ValueTuple<int, int>(newX, newY));
            }
        }

        return neighbours;
    }
}