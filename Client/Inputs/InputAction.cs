namespace Client.Inputs;

public enum InputAction
{
    // Movement
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    Jump,
    Crouch,
    
    // World interaction
    DropItem,
    PlaceBlock,
    DestroyBlock,
    
    // UI
    UiClick,
    TogglePause,
    ToggleInventory
}