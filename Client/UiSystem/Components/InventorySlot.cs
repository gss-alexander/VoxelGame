using System.Numerics;
using Client.Items;
using Client.UiSystem.Elements;

namespace Client.UiSystem.Components;

public class InventorySlot : Panel
{
    
    private readonly ItemTextures _itemTextures;

    private static InventorySlot? _draggingElement;

    public ItemStorage.Slot Slot { get; }

    private readonly Text _countLabel = new();
    private readonly Image _iconImage = new();
    private readonly Panel _container = new();

    private Vector2 _originalPosition;

    private Vector2 _mousePosition;
    
    public InventorySlot(ItemStorage.Slot slot, Vector2 position, Vector2 size, ItemTextures itemTextures)
    {
        Slot = slot;
        _itemTextures = itemTextures;
        
        Size = size;
        Position = position;
        Anchor = AnchorMode.LeftBottom;
        Pivot = PivotMode.LeftBottom;
        ZOrder = 1;

        _container.Size = size;
        _container.Anchor = AnchorMode.CenterMiddle;
        _container.Pivot = PivotMode.CenterMiddle;
        AddChild(_container);

        _iconImage.Size = size;
        _iconImage.Anchor = AnchorMode.CenterMiddle;
        _iconImage.Pivot = PivotMode.CenterMiddle;
        _container.AddChild(_iconImage);
        
        _countLabel.Anchor = AnchorMode.RightTop;
        _countLabel.Pivot = PivotMode.RightMiddle;
        _countLabel.HorizontalAlign = Text.HorizontalAlignment.Right;
        _countLabel.FontSize = 5f;
        _countLabel.Color = Vector3.Zero;
        _countLabel.Position = new Vector2(-5f, 10f);
        _container.AddChild(_countLabel);

        Slot.OnChanged += UpdateContent;
        UpdateContent();

        _originalPosition = position;
    }

    private void UpdateContent()
    {
        _countLabel.Content = !Slot.IsEmpty ? Slot.Count.ToString() : string.Empty;
        _iconImage.Sprite = !Slot.IsEmpty ? _itemTextures.GetTextureForItem(Slot.ItemId) : null;
    }

    public override void Update(float deltaTime)
    {
        if (_draggingElement != null && _draggingElement == this)
        {
            _container.SetAbsolutePosition(_mousePosition);
        }
        
        foreach (var child in Children)
        {
            child.Update(deltaTime);
        }
    }

    public override bool HandleInput(Vector2 mousePosition, bool isClicked)
    {
        _mousePosition = mousePosition;
        if (!IsPointInside(mousePosition)) return false;

        if (isClicked && _draggingElement == null && !Slot.IsEmpty)
        {
            _draggingElement = this;
        }

        else if (isClicked && _draggingElement != null)
        {
            if (_draggingElement == this)
            {
                // If we click the same slot we came from then just stop dragging and reset the position
                _draggingElement = null;
                _container.Position = Vector2.Zero;
            }
            
            else if (Slot.IsEmpty)
            {
                // We assume that a slot being dragged is not empty due to the check above
                Slot.ItemId = _draggingElement.Slot.ItemId;
                Slot.Count = _draggingElement.Slot.Count;

                _draggingElement.Slot.ItemId = "null";
                _draggingElement.Slot.Count = 0;
                _draggingElement = null;
            }
            else if (Slot.ItemId == _draggingElement.Slot.ItemId)
            {
                // Since the item is the same we can add the dragging stack to this one
                Slot.Count += _draggingElement.Slot.Count;
                
                _draggingElement.Slot.ItemId = "null";
                _draggingElement.Slot.Count = 0;
                _draggingElement = null;
            }
            else
            {
                // A different type of item is in this slot. Swap out contents of slot and make this one the dragging one
                var currentItem = Slot.ItemId;
                var currentCount = Slot.Count;

                Slot.ItemId = _draggingElement.Slot.ItemId;
                Slot.Count = _draggingElement.Slot.Count;

                _draggingElement.Slot.ItemId = currentItem;
                _draggingElement.Slot.Count = currentCount;
                _draggingElement._container.Position = Vector2.Zero;

                _draggingElement = this;
                
                
                // todo: this is a broken piece of shit and does not work properly. Please fix later, but I cannot stand more UI
            }
        }
        
        return true;
    }
}