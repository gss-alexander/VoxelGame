using System.Numerics;
using Client.Items;
using Client.Sound;
using Silk.NET.Maths;

namespace Client.Blocks;

public class BlockPlacement
{
    private readonly PlayerInventory _playerInventory;
    private readonly ItemDatabase _itemDatabase;
    private readonly BlockDatabase _blockDatabase;
    private readonly Action<Vector3D<int>, int> _placeBlockAction;
    private readonly SoundPlayer _soundPlayer;

    public BlockPlacement(PlayerInventory playerInventory, ItemDatabase itemDatabase, BlockDatabase blockDatabase, Action<Vector3D<int>, int> placeBlockAction, SoundPlayer soundPlayer)
    {
        _playerInventory = playerInventory;
        _itemDatabase = itemDatabase;
        _blockDatabase = blockDatabase;
        _placeBlockAction = placeBlockAction;
        _soundPlayer = soundPlayer;
    }

    public bool Update(VoxelRaycaster.Hit? raycastHit, bool requestedPlacement)
    {
        if (!requestedPlacement) return false;
        if (!raycastHit.HasValue) return false;

        var selectedInventorySlot = _playerInventory.Hotbar.GetSlot(_playerInventory.SelectedHotbarSlot);
        if (selectedInventorySlot == null) return false;

        var selectedItem = selectedInventorySlot.ItemId;
        var itemData = _itemDatabase.Get(selectedItem);
        if (itemData.GetType() != typeof(BlockItemData)) return false;

        var blockId = _blockDatabase.GetInternalId(itemData.ExternalId);
        _placeBlockAction(Block.GetFaceNeighbour(raycastHit.Value.Position, raycastHit.Value.Face), blockId);
        _playerInventory.Hotbar.RemoveItemFromSlot(_playerInventory.SelectedHotbarSlot, 1);
        var blockData = _blockDatabase.GetById(blockId);
        _soundPlayer.PlaySound(blockData.PlacementSoundId ?? "blocks/placement/default");
        
        return true;
    }
}