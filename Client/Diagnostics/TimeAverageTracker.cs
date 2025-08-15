namespace Client.Diagnostics;

/// <summary>
/// A zero-allocation (after constructor call) time tracker that calculates the average time by using a ring buffer internally.
/// </summary>
public class TimeAverageTracker
{
    private readonly float[] _frameTimes;
    private readonly int _capacity;
    private int _index;
    private int _count;
    private float _sum;

    public TimeAverageTracker(int capacity)
    {
        _capacity = capacity;
        _frameTimes = new float[capacity];
        _index = 0;
        _count = 0;
        _sum = 0f;
    }

    public float AverageTime => _count == 0 ? 0f : _sum / _count;

    public void AddTime(float frameTime)
    {
        if (_count < _capacity)
        {
            _frameTimes[_index] = frameTime;
            _sum += frameTime;
            _count++;
        }
        else
        {
            _sum -= _frameTimes[_index];
            _frameTimes[_index] = frameTime;
            _sum += frameTime;
        }

        _index = (_index + 1) % _capacity;
    } 
}