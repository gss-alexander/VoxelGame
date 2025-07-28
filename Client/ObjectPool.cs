namespace Client;

public class ObjectPool<T>
{
    private readonly Func<T> _creationFunc;
    private readonly Queue<T> _pool;

    public ObjectPool(Func<T> creationFunc)
    {
        _creationFunc = creationFunc;
        _pool = new Queue<T>(24);
    }

    public T Get()
    {
        if (_pool.Count > 0)
        {
            var next = _pool.Dequeue();
            return next;
        }

        var newItem = _creationFunc();
        return newItem;
    }

    public void Release(T item)
    {
        _pool.Enqueue(item);
    }
}