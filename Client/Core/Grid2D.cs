namespace Client.Core;

public class Grid2D<T>
{
    private readonly int _size;

    private readonly T[] _grid;
    
    public Grid2D(int size)
    {
        _size = size;
        _grid = new T[_size * _size];
    }

    public T Get(int x, int y)
    {
        return _grid[PositionToGridIndex(x, y)];
    }

    public void Set(int x, int y, T value)
    {
        _grid[PositionToGridIndex(x, y)] = value;
    }

    public bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < _size && y >= 0 && y < _size;
    }

    private int PositionToGridIndex(int x, int y)
    {
        return x + y * _size;
    }
}