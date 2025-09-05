using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.Clouds;

public class CloudSystem
{

    private class Cloud
    {
        public Vector3 Position { get; set; }
        public CloudRenderer Renderer { get; }

        public Cloud(Vector3 initialPosition, CloudRenderer renderer)
        {
            Position = initialPosition;
            Renderer = renderer;
        }
    }

    private readonly List<Cloud> _clouds = new();
    private readonly CloudGenerator _cloudGenerator = new(8);

    private static readonly Vector3 MovementDirection = new(-1f, 0f, 0f);
    private const float MovementSpeed = 0.5f;
    private const float Spacing = 80f;
    
    public CloudSystem()
    {
    }

    public void GenerateClouds()
    {
        for (var x = -10; x < 10; x++)
        {
            for (var z = -10; z < 10; z++)
            {
                var mesh = _cloudGenerator.GenerateMesh();
                var renderer = new CloudRenderer(mesh);
                _clouds.Add(new Cloud(new Vector3(x * Spacing, 150, z * Spacing), renderer));
            }
        }
    }

    public void Update(float deltaTime)
    {
        var movement = MovementDirection * (MovementSpeed * deltaTime);
        foreach (var cloud in _clouds)
        {
            cloud.Position += movement;
        }
    }

    public void Render(Matrix4x4 view, Matrix4x4 projection)
    {
        foreach (var cloud in _clouds)
        {
            cloud.Renderer.Render(cloud.Position, view, projection);
        }
    }
}