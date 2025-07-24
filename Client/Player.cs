﻿using System.Numerics;
using Silk.NET.Maths;

namespace Client;

public class Player
{
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 Size { get; set; } = new Vector3(0.6f, 1.8f, 0.6f);

    private const float Gravity = 14f;
    private const float JumpSpeed = 6f;

    private const float MovementSpeed = 5f;

    private readonly Func<Vector3, bool> _isBlockSolidFunc;

    private bool _isGrounded;
    

    public Player(Vector3 startingPosition, Func<Vector3, bool> isBlockSolidFunc)
    {
        _isBlockSolidFunc = isBlockSolidFunc;
        Position = startingPosition;
        Velocity = Vector3.Zero;
    }

    public void Update(float deltaTime, Vector2 movementInput, bool isJumpPressed)
    {
        var velocity = Velocity;
        
        // Apply gravity
        velocity.Y += -Gravity * deltaTime;
        
        // Apply movement input on horizontal axis
        velocity.X = movementInput.X * MovementSpeed;
        velocity.Z = movementInput.Y * MovementSpeed;

        if (isJumpPressed && _isGrounded)
        {
            velocity.Y = JumpSpeed;
            _isGrounded = false;
        }
        
        Velocity = velocity;
        MoveWithCollision(deltaTime);
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
                _isGrounded = true;
            }

            Velocity = Velocity with { Y = 0f };
        }
        else
        {
            _isGrounded = false;
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
        var min = position;
        var max = position + Size;

        var minX = (int)MathF.Floor(min.X);
        var maxX = (int)MathF.Floor(max.X);
        var minY = (int)MathF.Floor(min.Y);
        var maxY = (int)MathF.Floor(max.Y);
        var minZ = (int)MathF.Floor(min.Z);
        var maxZ = (int)MathF.Floor(max.Z);

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                for (var z = minZ; z <= maxZ; z++)
                {
                    if (_isBlockSolidFunc(new Vector3(x, y, z)))
                    {
                        if (AABBOverlap(min, max, new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1)))
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