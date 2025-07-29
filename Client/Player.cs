using System.Numerics;
using Silk.NET.Maths;

namespace Client;

public class Player
{
    public Vector3 Position
    {
        get => _entity.Position;
        set => _entity.Position = value;
    }
    public Vector3 Size => _entity.Size;

    private const float JumpSpeed = 6f;
    private const float MovementSpeed = 5f;

    private readonly Entity _entity;

    public Player(Vector3 startingPosition, Func<Vector3, bool> isBlockSolidFunc)
    {
        _entity = new Entity(startingPosition, new(0.6f, 1.8f, 0.6f), isBlockSolidFunc);
    }

    public void Update(float deltaTime, Vector2 movementInput, bool isJumpPressed)
    {
        var velocity = _entity.Velocity;
        
        velocity.X = movementInput.X * MovementSpeed;
        velocity.Z = movementInput.Y * MovementSpeed;

        if (isJumpPressed && _entity.IsGrounded)
        {
            velocity.Y = JumpSpeed;
            _entity.IsGrounded = false;
        }
        
        _entity.Velocity = velocity;
        _entity.Update(deltaTime);
    }
}