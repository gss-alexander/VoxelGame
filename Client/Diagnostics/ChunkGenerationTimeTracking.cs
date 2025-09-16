namespace Client.Diagnostics;

public static class ChunkGenerationTimeTracking
{
    public static TimeAverageTracker MeshGenerationTime { get; } = new(16);
}