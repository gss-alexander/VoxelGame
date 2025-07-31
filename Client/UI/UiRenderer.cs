using System.Numerics;
using Client.Blocks;
using Client.Crafting;
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
        Inventory,
        CraftingGrid
    }
    
    private readonly HotbarRenderer _hotbarRenderer;
    private readonly InventoryRenderer _inventoryRenderer;
    private readonly DraggableItemRenderer _draggableItemRenderer;
    private readonly PlayerInventory _playerInventory;
    private readonly CraftingGridUi _craftingGridUi;

    private UiState _state = UiState.Gameplay;

    public UiRenderer(HotbarRenderer hotbarRenderer, InventoryRenderer inventoryRenderer, DraggableItemRenderer draggableItemRenderer, PlayerInventory playerInventory, CraftingGridUi craftingGridUi)
    {
        _hotbarRenderer = hotbarRenderer;
        _inventoryRenderer = inventoryRenderer;
        _draggableItemRenderer = draggableItemRenderer;
        _playerInventory = playerInventory;
        _craftingGridUi = craftingGridUi;
    }

    public void OnMouseClicked(MouseButton button, Vector2 screenPosition)
    {
        if (_draggableItemRenderer.CurrentItemStack != null)
        {
            TryStopDraggingItem(screenPosition, button);
        }
        
        else if (_draggableItemRenderer.CurrentItemStack == null)
        {
            TryStartDraggingItem(screenPosition, button);
        }
    }

    private void TryStopDraggingItem(Vector2 screenPosition, MouseButton button)
    {
        // todo: this method is so stupid it is unreal I ever made it. Yet it works.
        
        var clickedHotbarSlotIndex = _hotbarRenderer.GetClickedSlotIndex(screenPosition);
        if (clickedHotbarSlotIndex >= 0)
        {
            var slot = _playerInventory.Hotbar.GetSlot(clickedHotbarSlotIndex);
            if (slot == null)
            {
                // No other item in slot, so just add to it
                if (button == MouseButton.Left)
                {
                    _playerInventory.Hotbar.AddItemToSlot(clickedHotbarSlotIndex,
                        _draggableItemRenderer.CurrentItemStack.ItemId, _draggableItemRenderer.CurrentItemStack.Count);
                    _draggableItemRenderer.StopDragging();
                }
                else if (button == MouseButton.Right)
                {
                    _playerInventory.Hotbar.AddItemToSlot(clickedHotbarSlotIndex,
                        _draggableItemRenderer.CurrentItemStack.ItemId, 1);
                    _draggableItemRenderer.CurrentItemStack.Count -= 1;
                    if (_draggableItemRenderer.CurrentItemStack.Count == 0)
                    {
                        _draggableItemRenderer.StopDragging();
                    }
                }
            }

            else
            {
                // Existing item, replace dragging item with the one in the slot
                if (button == MouseButton.Left)
                {
                    if (slot.ItemId != _draggableItemRenderer.CurrentItemStack.ItemId)
                    {
                        var previousItemId = _draggableItemRenderer.CurrentItemStack.ItemId;
                        var previousItemCount = _draggableItemRenderer.CurrentItemStack.Count;
                            
                        _draggableItemRenderer.SetDragging(slot.ItemId, slot.Count);
                        _playerInventory.Hotbar.RemoveItemFromSlot(clickedHotbarSlotIndex, slot.Count);
                            
                        _playerInventory.Hotbar.AddItemToSlot(clickedHotbarSlotIndex, previousItemId, previousItemCount);
                    }

                    else
                    {
                        _playerInventory.Hotbar.AddItemToSlot(clickedHotbarSlotIndex,
                            _draggableItemRenderer.CurrentItemStack.ItemId,
                            _draggableItemRenderer.CurrentItemStack.Count);
                        _draggableItemRenderer.StopDragging();
                    }
                    
                }
                else if (button == MouseButton.Right && slot.ItemId == _draggableItemRenderer.CurrentItemStack.ItemId)
                {
                    _playerInventory.Hotbar.AddItemToSlot(clickedHotbarSlotIndex, _draggableItemRenderer.CurrentItemStack.ItemId, 1);
                    _draggableItemRenderer.CurrentItemStack.Count -= 1;
                }
            }
        }
        
        var clickedInventorySlotIndex = _inventoryRenderer.GetClickedSlotIndex(screenPosition);
        if (clickedInventorySlotIndex >= 0)
        {
            var slot = _playerInventory.Storage.GetSlot(clickedInventorySlotIndex);
            if (slot == null)
            {
                if (button == MouseButton.Left)
                {
                    // No other item in slot, so just add to it
                    _playerInventory.Storage.AddItemToSlot(clickedInventorySlotIndex,
                        _draggableItemRenderer.CurrentItemStack.ItemId, _draggableItemRenderer.CurrentItemStack.Count);
                    _draggableItemRenderer.StopDragging();
                }
                
                else if (button == MouseButton.Right)
                {
                    _playerInventory.Storage.AddItemToSlot(clickedInventorySlotIndex,
                        _draggableItemRenderer.CurrentItemStack.ItemId, 1);
                    _draggableItemRenderer.CurrentItemStack.Count -= 1;
                    if (_draggableItemRenderer.CurrentItemStack.Count == 0)
                    {
                        _draggableItemRenderer.StopDragging();
                    }
                }
            }

            else
            {
                // Existing item, replace dragging item with the one in the slot
                
                if (button == MouseButton.Left)
                {
                    if (slot.ItemId != _draggableItemRenderer.CurrentItemStack.ItemId)
                    {
                        var previousItemId = _draggableItemRenderer.CurrentItemStack.ItemId;
                        var previousItemCount = _draggableItemRenderer.CurrentItemStack.Count;
                    
                        _draggableItemRenderer.SetDragging(slot.ItemId, slot.Count);
                        _playerInventory.Storage.RemoveItemFromSlot(clickedInventorySlotIndex, slot.Count);
                    
                        _playerInventory.Storage.AddItemToSlot(clickedInventorySlotIndex, previousItemId, previousItemCount);
                    }

                    else
                    {
                        _playerInventory.Storage.AddItemToSlot(clickedInventorySlotIndex,
                            _draggableItemRenderer.CurrentItemStack.ItemId,
                            _draggableItemRenderer.CurrentItemStack.Count);
                        _draggableItemRenderer.StopDragging();
                    }
                    
                }
                else if (button == MouseButton.Right && slot.ItemId == _draggableItemRenderer.CurrentItemStack.ItemId)
                {
                    _playerInventory.Storage.AddItemToSlot(clickedInventorySlotIndex, _draggableItemRenderer.CurrentItemStack.ItemId, 1);
                    _draggableItemRenderer.CurrentItemStack.Count -= 1;
                }
            }
        }
        
        var clickedCraftingGridSlotIndex = _craftingGridUi.GetClickedSlotIndex(screenPosition);
        if (clickedCraftingGridSlotIndex >= 0)
        {
            var itemStack = _craftingGridUi.GetStackAtSlotIndex(clickedCraftingGridSlotIndex);
            if (itemStack == null)
            {
                if (button == MouseButton.Left)
                {
                    _craftingGridUi.AddItemStackToSlot(clickedCraftingGridSlotIndex,
                        new ItemStack(_draggableItemRenderer.CurrentItemStack.ItemId,
                            _draggableItemRenderer.CurrentItemStack.Count));
                    _draggableItemRenderer.StopDragging();
                }
                else if (button == MouseButton.Right)
                {
                    _craftingGridUi.AddItemStackToSlot(clickedCraftingGridSlotIndex,
                        new ItemStack(_draggableItemRenderer.CurrentItemStack.ItemId, 1));
                    _draggableItemRenderer.CurrentItemStack.Count -= 1;
                    if (_draggableItemRenderer.CurrentItemStack.Count == 0)
                    {
                        _draggableItemRenderer.StopDragging();
                    }
                }
            }

            else
            {
                if (button == MouseButton.Left)
                {
                    var oldItem = itemStack.Value.ItemId;
                    var oldCount = itemStack.Value.Amount;
                
                    _craftingGridUi.AddItemStackToSlot(clickedCraftingGridSlotIndex,
                        new ItemStack(_draggableItemRenderer.CurrentItemStack.ItemId,
                            _draggableItemRenderer.CurrentItemStack.Count));
                
                    _draggableItemRenderer.SetDragging(oldItem, oldCount);
                }
                else if (button == MouseButton.Right && itemStack.Value.ItemId == _draggableItemRenderer.CurrentItemStack.ItemId)
                {
                    _craftingGridUi.AddItemStackToSlot(clickedCraftingGridSlotIndex, new ItemStack(_draggableItemRenderer.CurrentItemStack.ItemId, 1));
                    _draggableItemRenderer.CurrentItemStack.Count -= 1;
                }
            }
        }
    }

    private void TryStartDraggingItem(Vector2 screenPosition, MouseButton button)
    {
        if (button == MouseButton.Left)
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
        
            var clickedCraftingGridSlotIndex = _craftingGridUi.GetClickedSlotIndex(screenPosition);
            if (clickedCraftingGridSlotIndex >= 0)
            {
                var itemStack = _craftingGridUi.GetStackAtSlotIndex(clickedCraftingGridSlotIndex);
                if (itemStack != null)
                {
                    _draggableItemRenderer.SetDragging(itemStack.Value.ItemId, itemStack.Value.Amount);
                    _craftingGridUi.ClearStackAtSlot(clickedCraftingGridSlotIndex);
                }
            }
        }
        
        if (_craftingGridUi.IsResultSlotClicked(screenPosition))
        {
            var craftingResult = _craftingGridUi.CraftingGrid.Result;
            if (craftingResult.HasValue)
            {
                if (button == MouseButton.Left || (button == MouseButton.Right && craftingResult.Value.Amount == 1))
                {
                    var resultItemId = craftingResult.Value.ItemId;
                    var resultItemCount = craftingResult.Value.Amount;
                    _draggableItemRenderer.SetDragging(resultItemId, resultItemCount);
                    _craftingGridUi.CraftingGrid.FinalizeCrafting();
                }
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
                _craftingGridUi.Render();
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