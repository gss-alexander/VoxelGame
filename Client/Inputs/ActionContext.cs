using Silk.NET.Input;

namespace Client.Inputs;

public class ActionContext
{
    private struct ActionState
    {
        public bool Pressed;
        public bool Held;
        public bool Released;
    }
    
    private readonly IKeyboard _keyboard;
    private readonly IMouse _mouse;

    private readonly Dictionary<InputAction, ActionState> _actions = new();
    private readonly Dictionary<InputAction, float> _lastPressTime = new();
    private readonly Dictionary<InputAction, bool> _doublePressed = new();
    private float _currentTime;
    
    public ActionContext(IKeyboard keyboard, IMouse mouse)
    {
        _keyboard = keyboard;
        _mouse = mouse;

        foreach (var inputAction in Enum.GetValues<InputAction>())
        {
            _actions.Add(inputAction, new ActionState
            {
                Pressed = false,
                Held = false,
                Released = false
            });
            _lastPressTime.Add(inputAction, -1f);
            _doublePressed.Add(inputAction, false);
        }
    }

    public void CollectInputs(float deltaTime)
    {
        _currentTime += deltaTime;
        
        foreach (var key in _doublePressed.Keys.ToList())
        {
            _doublePressed[key] = false;
        }
        
        UpdateKeyboardAction(InputAction.Jump, Key.Space);
        UpdateKeyboardAction(InputAction.Crouch, Key.ControlLeft);
        UpdateKeyboardAction(InputAction.MoveForward, Key.W);
        UpdateKeyboardAction(InputAction.MoveBackward, Key.S);
        UpdateKeyboardAction(InputAction.MoveLeft, Key.A);
        UpdateKeyboardAction(InputAction.MoveRight, Key.D);
        UpdateKeyboardAction(InputAction.DebugAction, Key.Q);
    }

    private void UpdateKeyboardAction(InputAction action, Key associatedKey)
    {
        UpdateAction(action, _keyboard.IsKeyPressed(associatedKey));
    }

    private void UpdateMouseAction(InputAction action, MouseButton associatedMouseButton)
    {
        UpdateAction(action, _mouse.IsButtonPressed(associatedMouseButton));
    }

    private void UpdateAction(InputAction action, bool isPressed)
    {
        if (isPressed)
        {
            var currentState = _actions[action];
            
            if (!currentState.Held)
            {
                if (_lastPressTime[action] > 0 && 
                    _currentTime - _lastPressTime[action] <= 0.3f) // Default 300ms window
                {
                    _doublePressed[action] = true;
                }
                
                _lastPressTime[action] = _currentTime;
                _actions[action] = currentState with { Pressed = true, Held = true };
            }
            
            else if (currentState.Pressed)
            {
                _actions[action] = currentState with { Pressed = false };
            }
        }
        else
        {
            var currentState = _actions[action];
            
            if (currentState is { Released: false, Held: true, Pressed: false })
            {
                _actions[action] = currentState with { Released = true, Held = false };
            }
            
            else if (currentState is { Released: true, Held: false })
            {
                _actions[action] = currentState with { Released = false };
            }
        }
    }

    public bool IsHeld(InputAction action)
    {
        return _actions[action].Held;
    }

    public bool IsPressed(InputAction action)
    {
        return _actions[action].Pressed;
    }

    public bool IsReleased(InputAction action)
    {
        return _actions[action].Released;
    }

    public bool IsDoublePressed(InputAction action, float windowInSeconds)
    {
        return _doublePressed[action] && 
               _lastPressTime[action] > 0 && 
               _currentTime - _lastPressTime[action] <= windowInSeconds;
    }
}