using System.Numerics;
using Client.Inputs;
using Client.Sound;

namespace Client.Player;

public class Player
{
    private readonly ActionContext _actionContext;
    private readonly SoundPlayer _soundPlayer;

    public Health Health { get; } = new(10f, 10f);
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
    private const float FootstepFrequencyInSeconds = 0.5f;
    private const float MinimumMovementThreshold = 0.1f;

    private readonly Entity _entity;

    private bool _isFlying;
    private float _footstepTimer;
    private bool _wasGroundedLastFrame;
    private Vector3 _lastPosition;

    private float _timeSpentFalling;
    private const float TimeBeforeFallDamage = 1.0f;

    private const float JumpBufferTimeInSeconds = 0.1f;
    private float _secondsSinceJumpPressed;

    public Player(Vector3 startingPosition, Func<Vector3, bool> isBlockSolidFunc, ActionContext actionContext, SoundPlayer soundPlayer)
    {
        _actionContext = actionContext;
        _soundPlayer = soundPlayer;
        _entity = new Entity(startingPosition, new(0.6f, 1.8f, 0.6f), isBlockSolidFunc);
        _lastPosition = startingPosition;
        _wasGroundedLastFrame = false;
    }

    public void Update(float deltaTime, Vector2 movementInput)
    {
        var velocity = _entity.Velocity;
        
        velocity.X = movementInput.X * HorizontalSpeed;
        velocity.Z = movementInput.Y * HorizontalSpeed;

        var jumpDoublePressed = _actionContext.IsDoublePressed(InputAction.Jump, 0.5f);

        _secondsSinceJumpPressed += deltaTime;
        if (_actionContext.IsPressed(InputAction.Jump))
        {
            _secondsSinceJumpPressed = 0f;
        }
        if (_secondsSinceJumpPressed <= JumpBufferTimeInSeconds && _entity.IsGrounded)
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
        
        UpdateFootstepSounds(deltaTime);
        UpdateFallDamage(deltaTime);
        
        _wasGroundedLastFrame = _entity.IsGrounded;
        _lastPosition = _entity.Position;
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

    private void UpdateFootstepSounds(float deltaTime)
    {
        bool justLanded = _entity.IsGrounded && !_wasGroundedLastFrame;
        bool isMoving = IsPlayerMoving(deltaTime);
        
        if (justLanded && isMoving)
        {
            return;
        }
        
        if (!isMoving || !_entity.IsGrounded)
        {
            _footstepTimer = 0f;
            return;
        }
        
        _footstepTimer -= deltaTime;
        if (_footstepTimer <= 0f)
        {
            _soundPlayer.PlaySound("step");
            _footstepTimer = FootstepFrequencyInSeconds;
        }
    }
    
    private bool IsPlayerMoving(float deltaTime)
    {
        var horizontalMovement = new Vector2(_entity.Position.X - _lastPosition.X, _entity.Position.Z - _lastPosition.Z);
        return horizontalMovement.Length() > MinimumMovementThreshold * deltaTime;
    }

    private void UpdateFallDamage(float deltaTime)
    {
        if (!_entity.IsGrounded)
        {
            _timeSpentFalling += deltaTime;
            return;
        }
        
        if (_entity.IsGrounded)
        {
            if (!_wasGroundedLastFrame)
            {
                if (_timeSpentFalling >= TimeBeforeFallDamage)
                {
                    // todo: better damage calculation
                    Health.Damage(1f);
                }
            }
            else
            {
                _timeSpentFalling = 0f;
            }
        }
    }
}