namespace Client.Chunks.Generation;

public class Heightmap
{
    private readonly int _width;
    private readonly int _height;
    
    private readonly int[] _map;

    public Heightmap(int width, int height)
    {
        _width = width;
        _height = height;
        _map = new int[width * height];
    }

    public void Set(int x, int y, int height)
    {
        _map[PositionToIndex(x, y)] = height;
    }

    public int Get(int x, int y)
    {
        return _map[PositionToIndex(x, y)];
    }
    
    private int PositionToIndex(int x, int y)
    {
        return x + y * _height;
    }
}