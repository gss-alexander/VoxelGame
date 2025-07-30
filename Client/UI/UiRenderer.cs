using System.Numerics;
using Client.Blocks;
using Client.Items;
using Client.UI.Text;
using Silk.NET.Input;
using Silk.NET.OpenGL;

namespace Client.UI;
public class UiRenderer
{
    public bool AllowPlayerMovement => _state == UiState.Gameplay;
    
    public enum UiState
    {
        Gameplay,
        Inventory
    }
    
    private readonly HotbarRenderer _hotbarRenderer;
    private readonly InventoryRenderer _inventoryRenderer;
    private readonly DraggableItemRenderer _draggableItemRenderer;
    private readonly PlayerInventory _playerInventory;

    private UiState _state = UiState.Gameplay;

    public UiRenderer(HotbarRenderer hotbarRenderer, InventoryRenderer inventoryRenderer, DraggableItemRenderer draggableItemRenderer, PlayerInventory playerInventory)
    {
        _hotbarRenderer = hotbarRenderer;
        _inventoryRenderer = inventoryRenderer;
        _draggableItemRenderer = draggableItemRenderer;
        _playerInventory = playerInventory;
    }

    public void OnMouseClicked(MouseButton button, Vector2 screenPosition)
    {
        if (_draggableItemRenderer.CurrentItemStack != null && button == MouseButton.Left)
        {
            TryStopDraggingItem(screenPosition);
        }
        
        else if (_draggableItemRenderer.CurrentItemStack == null && button == MouseButton.Left)
        {
            TryStartDraggingItem(screenPosition);
        }
    }

    private void TryStopDraggingItem(Vector2 screenPosition)
    {
        var clickedHotbarSlotIndex = _hotbarRenderer.GetClickedSlotIndex(screenPosition);
        if (clickedHotbarSlotIndex >= 0)
        {
            var slot = _playerInventory.Hotbar.GetSlot(clickedHotbarSlotIndex);
            if (slot == null)
            {
                // No other item in slot, so just add to it
                _playerInventory.Hotbar.AddItemToSlot(clickedHotbarSlotIndex,
                    _draggableItemRenderer.CurrentItemStack.ItemId, _draggableItemRenderer.CurrentItemStack.Count);
                _draggableItemRenderer.StopDragging();
            }

            else
            {
                // Existing item, replace dragging item with the one in the slot
                var previousItemId = _draggableItemRenderer.CurrentItemStack.ItemId;
                var previousItemCount = _draggableItemRenderer.CurrentItemStack.Count;
                    
                _draggableItemRenderer.SetDragging(slot.ItemId, slot.Count);
                _playerInventory.Hotbar.RemoveItemFromSlot(clickedHotbarSlotIndex, slot.Count);
                    
                _playerInventory.Hotbar.AddItemToSlot(clickedHotbarSlotIndex, previousItemId, previousItemCount);
            }
        }
        
        var clickedInventorySlotIndex = _inventoryRenderer.GetClickedSlotIndex(screenPosition);
        if (clickedInventorySlotIndex >= 0)
        {
            var slot = _playerInventory.Storage.GetSlot(clickedInventorySlotIndex);
            if (slot == null)
            {
                // No other item in slot, so just add to it
                _playerInventory.Storage.AddItemToSlot(clickedInventorySlotIndex,
                    _draggableItemRenderer.CurrentItemStack.ItemId, _draggableItemRenderer.CurrentItemStack.Count);
                _draggableItemRenderer.StopDragging();
            }

            else
            {
                // Existing item, replace dragging item with the one in the slot
                var previousItemId = _draggableItemRenderer.CurrentItemStack.ItemId;
                var previousItemCount = _draggableItemRenderer.CurrentItemStack.Count;
                    
                _draggableItemRenderer.SetDragging(slot.ItemId, slot.Count);
                _playerInventory.Storage.RemoveItemFromSlot(clickedInventorySlotIndex, slot.Count);
                    
                _playerInventory.Storage.AddItemToSlot(clickedInventorySlotIndex, previousItemId, previousItemCount);
            }
        }
    }

    private void TryStartDraggingItem(Vector2 screenPosition)
    {
        var clickedHotbarSlotIndex = _hotbarRenderer.GetClickedSlotIndex(screenPosition);
        if (clickedHotbarSlotIndex >= 0)
        {
            var slot = _playerInventory.Hotbar.GetSlot(clickedHotbarSlotIndex);
            if (slot != null)
            {
                _draggableItemRenderer.SetDragging(slot.ItemId, slot.Count);
                _playerInventory.Hotbar.RemoveItemFromSlot(clickedHotbarSlotIndex, slot.Count);
            }
        }

        var clickedInventorySlotIndex = _inventoryRenderer.GetClickedSlotIndex(screenPosition);
        if (clickedInventorySlotIndex >= 0)
        {
            var slot = _playerInventory.Storage.GetSlot(clickedInventorySlotIndex);
            if (slot != null)
            {
                _draggableItemRenderer.SetDragging(slot.ItemId, slot.Count);
                _playerInventory.Storage.RemoveItemFromSlot(clickedInventorySlotIndex, slot.Count);
            }
        }
    }

    public void ToggleInventory()
    {
        _state = _state == UiState.Gameplay ? UiState.Inventory : UiState.Gameplay;
    }

    public void Update(Vector2 mousePosition)
    {
        if (_draggableItemRenderer.CurrentItemStack != null)
        {
            _draggableItemRenderer.Update(mousePosition);
        }
    }

    public void Render()
    {
        switch (_state)
        {
            case UiState.Gameplay:
                _hotbarRenderer.Render();
                break;
            case UiState.Inventory:
                _hotbarRenderer.Render();
                _inventoryRenderer.Render();
                if (_draggableItemRenderer.CurrentItemStack != null)
                {
                    _draggableItemRenderer.Render();
                }
                break;
            default:
                break;
        }
    }
}