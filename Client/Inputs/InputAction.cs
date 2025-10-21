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
    ToggleInventory,
    
    // Hotbar
    HotbarSelect1,
    HotbarSelect2,
    HotbarSelect3,
    HotbarSelect4,
    HotbarSelect5,
    HotbarSelect6,
    HotbarSelect7,
    HotbarSelect8,
    HotbarSelect9
}