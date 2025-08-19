using Client.Blocks;
using Client.Chunks;
using Client.Diagnostics;
using Client.Items;

namespace Client.Debug;

public class DebugMenu
{
    public bool FreeCamEnabled { get; private set; }
    
    private readonly Camera _camera;
    private readonly BlockDatabase _blockDatabase;
    private readonly BlockSelector _blockSelector;
    private readonly ItemDatabase _itemDatabase;
    private readonly VoxelRaycaster _voxelRaycaster;
    private readonly PlayerInventory _playerInventory;
    private readonly TimeAverageTracker _deltaTimeAverage;
    private readonly TimeAverageTracker _updateTimeAverage;
    private readonly TimeAverageTracker _renderTimeAverage;
    private readonly ChunkSystem _chunkSystem;
    private readonly Player _player;

    private int _selectedItemIndex;

    public DebugMenu(Camera camera, BlockDatabase blockDatabase, BlockSelector blockSelector, ItemDatabase itemDatabase,
        VoxelRaycaster voxelRaycaster, PlayerInventory playerInventory, TimeAverageTracker deltaTimeAverage,
        TimeAverageTracker updateTimeAverage, TimeAverageTracker renderTimeAverage, ChunkSystem chunkSystem,
        Player player)
    {
        _camera = camera;
        _blockDatabase = blockDatabase;
        _blockSelector = blockSelector;
        _itemDatabase = itemDatabase;
        _voxelRaycaster = voxelRaycaster;
        _playerInventory = playerInventory;
        _deltaTimeAverage = deltaTimeAverage;
        _updateTimeAverage = updateTimeAverage;
        _renderTimeAverage = renderTimeAverage;
        _chunkSystem = chunkSystem;
        _player = player;
    }

    public void Draw()
    {
        ImGuiNET.ImGui.Begin("Debug");
        
        DrawAverages();
        DrawChunkData();
        DrawPositionData();
        DrawLookingAt();
        ImGuiNET.ImGui.Separator();
        DrawItemSelection();
        ImGuiNET.ImGui.Separator();
        DrawToggles();

        ImGuiNET.ImGui.End(); 
    }

    private void DrawToggles()
    {
        var freecamEnabled = FreeCamEnabled;
        ImGuiNET.ImGui.Checkbox("Freecam", ref freecamEnabled);
        FreeCamEnabled = freecamEnabled;
    }

    private void DrawAverages()
    {
        ImGuiNET.ImGui.Text($"FPS: {1.0 / _deltaTimeAverage.AverageTime:F1}");
        ImGuiNET.ImGui.Text($"Average update time: {_updateTimeAverage.AverageTime}");
        ImGuiNET.ImGui.Text($"Average render time: {_renderTimeAverage.AverageTime}");
    }

    private void DrawPositionData()
    {
        ImGuiNET.ImGui.Text($"Player position: {_player.Position}");
        ImGuiNET.ImGui.Text($"Player chunk position: {Chunk.WorldToChunkPosition(_camera.Position)}");
    }

    private void DrawChunkData()
    {
        ImGuiNET.ImGui.Text($"Visible chunks: {_chunkSystem.VisibleChunkCount}");
    }

    private void DrawLookingAt()
    {
        var raycastHit = _voxelRaycaster.Cast(_camera.Position, _camera.Direction, 10f);
        if (raycastHit.HasValue)
        {
            ImGuiNET.ImGui.Text($"Looking at block pos: {raycastHit.Value.Position}");
            ImGuiNET.ImGui.Text($"Looking at block face: {raycastHit.Value.Face}");
        }

        else
        {
            ImGuiNET.ImGui.Text($"Looking at block pos: NaN");
            ImGuiNET.ImGui.Text($"Looking at block face: NaN");
        }
    }

    private void DrawItemSelection()
    {
        var availableItems = _itemDatabase.All;
        if (ImGuiNET.ImGui.BeginCombo("Item", availableItems[_selectedItemIndex].DisplayName))
        {
            for (var i = 0; i < availableItems.Length; i++)
            {
                bool isSelected = (_selectedItemIndex == i);
                if (ImGuiNET.ImGui.Selectable(availableItems[i].DisplayName, isSelected))
                {
                    _selectedItemIndex = i;
                }

                if (isSelected)
                {
                    ImGuiNET.ImGui.SetItemDefaultFocus();
                }
            }
            ImGuiNET.ImGui.EndCombo();
        }

        if (ImGuiNET.ImGui.Button("+1"))
        {
            _playerInventory.Storage.AddItem(availableItems[_selectedItemIndex].ExternalId, 1);
        }
        if (ImGuiNET.ImGui.Button("+16"))
        {
            _playerInventory.Storage.AddItem(availableItems[_selectedItemIndex].ExternalId, 16);
        }
    }
}