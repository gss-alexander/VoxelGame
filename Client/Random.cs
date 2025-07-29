using System.Numerics;

namespace Client;

public static class Random
{
    private static readonly System.Random _random = new System.Random(); 
    
    public static int Next() => _random.Next();
    public static int Next(int maxValue) => _random.Next(maxValue);
    public static int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    public static float Range(float min, float max) => 
        (float)(_random.NextDouble() * (max - min) + min); 
    
    public static bool NextBool() => _random.NextDouble() >= 0.5;
}