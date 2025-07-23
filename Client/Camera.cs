using System.Numerics;
using Silk.NET.Input;

namespace Client;

public class Camera
{
    public Vector3 Position = new(0.0f, 3.0f, 0.0f);
    public Vector3 Target = Vector3.Zero;
    public Vector3 Direction;
    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Vector3.UnitY, Direction));
    public Vector3 Up => Vector3.Cross(Direction, Right);
    public Vector3 Front = new(0.0f, 0.0f, -1.0f);
    public float Yaw = -90f;
    public float Pitch = 0f;
    public float Zoom = 45f;

    public Camera()
    {
        Direction = Vector3.Normalize(Position - Target);
    }
}