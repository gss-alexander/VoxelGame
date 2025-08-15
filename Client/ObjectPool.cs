namespace Client;

public class ObjectPool<T>
{
    private readonly Func<T> _creationFunc;
    private readonly Action<T> _onRelease;
    private readonly Queue<T> _pool;

    public ObjectPool(Func<T> creationFunc, Action<T> onRelease)
    {
        _creationFunc = creationFunc;
        _onRelease = onRelease;
        _pool = new Queue<T>(128);
    }

    public void Prewarm(int count)
    {
        for (var i = 0; i < count; i++)
        {
            Release(_creationFunc());
        }
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
        _onRelease(item);
        _pool.Enqueue(item);
    }
}