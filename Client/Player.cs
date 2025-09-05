using System.Numerics;
using Client.Inputs;
using Client.Sound;
using Silk.NET.Maths;

namespace Client;

public class Player
{
    private readonly ActionContext _actionContext;
    private readonly SoundPlayer _soundPlayer;

    public Vector3 Position
    {
        get => _entity.Position;
        set => _entity.Position = value;
    }
    public Vector3 Size => _entity.Size;

    private const float JumpSpeed = 6f;
    private const float GroundMovementSpeed = 5f;
    private const float FlyingMovementSpeed = 10f;
    private const float FlyingElevationChangeSpeed = 6f;
    private const float FootstepFrequencyInSeconds = 0.4f;

    private readonly Entity _entity;

    private bool _isFlying;
    private float _timeUntilNextFootstepInSeconds;

    public Player(Vector3 startingPosition, Func<Vector3, bool> isBlockSolidFunc, ActionContext actionContext, SoundPlayer soundPlayer)
    {
        _actionContext = actionContext;
        _soundPlayer = soundPlayer;
        _entity = new Entity(startingPosition, new(0.6f, 1.8f, 0.6f), isBlockSolidFunc);
    }

    public void Update(float deltaTime, Vector2 movementInput)
    {
        var velocity = _entity.Velocity;
        
        velocity.X = movementInput.X * HorizontalSpeed;
        velocity.Z = movementInput.Y * HorizontalSpeed;

        var jumpDoublePressed = _actionContext.IsDoublePressed(InputAction.Jump, 0.5f);

        if (_actionContext.IsHeld(InputAction.Jump) && _entity.IsGrounded)
        {
            velocity.Y = JumpSpeed;
            _entity.IsGrounded = false;
        }

        else if (jumpDoublePressed && !_isFlying && !_entity.IsGrounded)
        {
            _isFlying = true;
             velocity.Y = 0f;
             jumpDoublePressed = false;
        }

        _entity.GravityEnabled = !_isFlying;
        if (_isFlying)
        {
            velocity = UpdateFlying(jumpDoublePressed, deltaTime, velocity);
        }
        
        _entity.Velocity = velocity;
        _entity.Update(deltaTime);
        
        UpdateFootstepSounds(deltaTime, movementInput != Vector2.Zero);
    }

    private float HorizontalSpeed => _isFlying ? FlyingMovementSpeed : GroundMovementSpeed;
    
    private Vector3 UpdateFlying(bool doubleJump, float deltaTime, Vector3 currentVelocity)
    {
        if (_entity.IsGrounded)
        {
            _isFlying = false;
            return currentVelocity;
        }

        if (doubleJump)
        {
            _isFlying = false;
            return currentVelocity;
        }

        if (_actionContext.IsHeld(InputAction.Jump))
        {
            currentVelocity.Y = FlyingElevationChangeSpeed;
        }
        
        else if (_actionContext.IsHeld(InputAction.Crouch))
        {
            currentVelocity.Y = -FlyingElevationChangeSpeed;
        }

        else
        {
            currentVelocity.Y = 0f;
        }

        return currentVelocity;
    }

    private void UpdateFootstepSounds(float deltaTime, bool isMoving)
    {
        if (!isMoving || !_entity.IsGrounded)
        {
            _timeUntilNextFootstepInSeconds = FootstepFrequencyInSeconds;
            return;
        }

        if (isMoving && _timeUntilNextFootstepInSeconds <= 0f)
        {
            _soundPlayer.PlaySound("step");
            _timeUntilNextFootstepInSeconds = FootstepFrequencyInSeconds;
        }
        else
        {
            _timeUntilNextFootstepInSeconds -= deltaTime;
        }
    }
}