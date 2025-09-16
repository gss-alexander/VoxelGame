using System.Numerics;

namespace Client;

public class Entity
{
    private const float Gravity = 16.0f;
    
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 Size { get; set; }
    public bool IsGrounded { get; set; }
    public bool GravityEnabled { get; set; }
    public float Friction { get; set; } = 0f;

    private readonly Func<Vector3, bool> _isBlockSolidFunc;
    
    public Entity(Vector3 position, Vector3 size, Func<Vector3, bool> isBlockSolidFunc, Vector3? velocity = null)
    {
        Position = position;
        Size = size;
        Velocity = velocity ?? Vector3.Zero;
        
        _isBlockSolidFunc = isBlockSolidFunc;
    }

    public void Update(float deltaTime)
    {
        ApplyGravity(deltaTime);
        ApplyFriction(deltaTime);
        MoveWithCollision(deltaTime);
    }

    private void ApplyGravity(float deltaTime)
    {
        if (!GravityEnabled) return;
        Velocity = Velocity with { Y = Velocity.Y - Gravity * deltaTime };
    }

    private void ApplyFriction(float deltaTime)
    {
        if (Friction <= 0f) return;
        if (Velocity.Length() <= 0f) return;

        var horizontalVelocity = Velocity with { Y = 0f };
        var velocityChange = horizontalVelocity * (Friction * deltaTime);

        horizontalVelocity -= velocityChange;
        Velocity = horizontalVelocity with { Y = Velocity.Y };
    }

    private void MoveWithCollision(float deltaTime)
    {
        var deltaMovement = Velocity * deltaTime;
        var newPosition = Position;

        newPosition.X += deltaMovement.X;
        if (HasCollision(newPosition))
        {
            newPosition.X -= deltaMovement.X;
            Velocity = Velocity with { X = 0f };
        }

        newPosition.Y += deltaMovement.Y;
        if (HasCollision(newPosition))
        {
            newPosition.Y -= deltaMovement.Y;

            if (deltaMovement.Y < 0)
            {
                IsGrounded = true;
            }

            Velocity = Velocity with { Y = 0f };
        }
        else
        {
            IsGrounded = false;
        }

        newPosition.Z += deltaMovement.Z;
        if (HasCollision(newPosition))
        {
            newPosition.Z -= deltaMovement.Z;
            Velocity = Velocity with { Z = 0f };
        }

        Position = newPosition;
    }
    
    private bool HasCollision(Vector3 position)
    {
        var min = position - Size / 2f;
        var max = position + Size / 2f;

        var minX = (int)MathF.Floor(min.X);
        var maxX = (int)MathF.Floor(max.X);
        var minY = (int)MathF.Floor(min.Y);
        var maxY = (int)MathF.Floor(max.Y);
        var minZ = (int)MathF.Floor(min.Z);
        var maxZ = (int)MathF.Floor(max.Z);

        for (var x = minX - 1; x <= maxX + 1; x++)
        {
            for (var y = minY - 1; y <= maxY + 1; y++)
            {
                for (var z = minZ - 1; z <= maxZ + 1; z++)
                {
                    if (_isBlockSolidFunc(new Vector3(x, y, z)))
                    {
                        // Apply 0.5f offset because voxels span -0.5f - 0.5f in world space
                        if (AABBOverlap(min, max, new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), new Vector3(x + 0.5f, y + 0.5f, z + 0.5f)))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private bool AABBOverlap(Vector3 min1, Vector3 max1, Vector3 min2, Vector3 max2)
    {
        return (min1.X < max2.X && max1.X > min2.X &&
                min1.Y < max2.Y && max1.Y > min2.Y &&
                min1.Z < max2.Z && max1.Z > min2.Z);
    }
}