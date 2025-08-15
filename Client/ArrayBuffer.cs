namespace Client;

public class ArrayBuffer<T>
{
    public int Size { get; private set; }
    
    private readonly T[] _buffer;
    private readonly int _capacity;

    public ArrayBuffer(int capacity)
    {
        _buffer = new T[capacity];
        _capacity = capacity;
    }

    public void Clear()
    {
        Size = 0;
    }

    public void Write(T data)
    {
        _buffer[Size] = data;
        Size += 1;
    }

    public ReadOnlySpan<T> Read()
    {
        return new ReadOnlySpan<T>(_buffer, 0, Size);
    }
}