﻿using System.Drawing;
using System.Numerics;
using Client.Blocks;
using Client.Chunks;
using Client.Crafting;
using Client.Items;
using Client.Items.Dropping;
using Client.UI;
using Client.UI.Text;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Client;

public class Game
{
    private GL _gl;

    private Shader _shader;

    private Camera _camera = new();

    private ChunkSystem _chunkSystem;
    
    private Vector2D<int> _frameBufferSize;

    private IKeyboard _primaryKeyboard;
    private IMouse _primaryMouse;

    private IWindow _window;

    private ImGuiController _imGuiController;
    private CrosshairRenderer _crosshairRenderer;

    private VoxelRaycaster _voxelRaycaster;
    private Player _player;
    private BlockSelector _blockSelector;

    private UiRenderer _uiRenderer;

    private BoundingBoxRenderer _boundingBoxRenderer;
    private Shader _lineShader;

    private BlockSpriteRenderer _blockSpriteRenderer;
    private Inventory _inventory;

    private BlockData[] _blockData;
    private BlockTextures _blockTextures;
    private BlockDatabase _blockDatabase;

    private TextRenderer _textRenderer;

    private BlockBreaking _blockBreaking;
    private BlockPlacement _blockPlacement;

    private PlayerInventory _playerInventory;

    private ItemDropRenderer _itemDropRenderer;

    private ItemDroppingSystem _itemDroppingSystem;
    private ItemTextures _itemTextures;

    private HotbarRenderer _hotbarRenderer;

    private bool _playerControlsEnabled = false;

    public unsafe void Load(IWindow window)
    {
        
        _window = window;
        
        var inputContext = window.CreateInput();
        _primaryKeyboard = inputContext.Keyboards.First();
        _primaryKeyboard.KeyDown += OnKeyDown;

        for (int i = 0; i < inputContext.Mice.Count; i++)
        {
            inputContext.Mice[i].Cursor.CursorMode = CursorMode.Raw;
            inputContext.Mice[i].MouseMove += OnMouseMove;
            inputContext.Mice[i].Scroll += OnMouseWheel;
            inputContext.Mice[i].Click += OnMouseClicked;
        }

        _primaryMouse = inputContext.Mice.First();

        _gl = window.CreateOpenGL();
        
        _blockData = BlockDataLoader.Load(Path.Combine("..", "..", "..", "Resources", "Data", "blocks.yaml"));
        _blockDatabase = new BlockDatabase(_blockData);
        _blockTextures = new BlockTextures(_gl, _blockDatabase);

        _chunkSystem = new ChunkSystem(_gl, _blockTextures, _blockDatabase);

        _shader = new Shader(_gl,
            GetShaderPath("shader.vert"),
            GetShaderPath("shader.frag")
        );

        _frameBufferSize = window.Size;

        _imGuiController = new ImGuiController(_gl, window, inputContext);

        _crosshairRenderer = new CrosshairRenderer();
        _crosshairRenderer.Initialize(_gl, window.Size.X, window.Size.Y);

        _voxelRaycaster = new VoxelRaycaster(_chunkSystem.IsBlockSolid);
        _player = new Player(new Vector3(0f, 100f, 0f), worldPos =>
        {
            var blockPos = Block.WorldToBlockPosition(worldPos);
            return _chunkSystem.IsBlockSolid(blockPos);
        });

        _blockSelector = new BlockSelector(_blockDatabase);

        _blockSpriteRenderer = new BlockSpriteRenderer(_gl, _blockTextures);

        _playerInventory = new PlayerInventory();
        _inventory = new Inventory();
        _inventory.Hotbar.Add(0, (_blockDatabase.GetInternalId("cobblestone"), 54));
        _inventory.Hotbar.Add(1, (_blockDatabase.GetInternalId("dirt"), 25));
        
        var characterMap = new CharacterMap(_gl);
        var textShader = new Shader(_gl, GetShaderPath("text.vert"), GetShaderPath("text.frag"));
        _textRenderer = new TextRenderer(_gl, textShader, characterMap);
        
        var items = ItemLoader.Load();
        var itemDatabase = new ItemDatabase(items);
        itemDatabase.RegisterBlockItems(_blockDatabase.GetAll().Select(b => b.data).ToArray());
        _itemTextures = new ItemTextures(_gl, itemDatabase, _blockDatabase, _blockSpriteRenderer);
        var itemDropShader = new Shader(_gl, GetShaderPath("itemDrop.vert"),  GetShaderPath("itemDrop.frag"));
        _itemDroppingSystem = new ItemDroppingSystem(_gl, itemDatabase, _itemTextures, itemDropShader, worldPos =>
        {
            var blockPos = Block.WorldToBlockPosition(worldPos);
            return _chunkSystem.IsBlockSolid(blockPos);
        }, _blockDatabase, _blockTextures);

        var uiTexture = new Texture(_gl, GetTexturePath("hotbar_slot_background.png"));
        _hotbarRenderer = new HotbarRenderer(_gl, _playerInventory, window.Size.AsFloatVector(), _itemTextures,
            uiTexture, _textRenderer);

        var inventoryRenderer = new InventoryRenderer(_gl, _playerInventory, window.Size.AsFloatVector(), _itemTextures,
            uiTexture, _textRenderer);
        var draggableItemRenderer =
            new DraggableItemRenderer(_gl, _textRenderer, window.Size.AsFloatVector(), _itemTextures);

        var craftingRecipes = CraftingRecipesLoader.Load();
        var craftingGrid = new CraftingGrid(3, 3, craftingRecipes);
        var craftingGridUi = new CraftingGridUi(_gl, craftingGrid, window.Size.AsFloatVector(), uiTexture, _itemTextures);
        _uiRenderer = new UiRenderer(_hotbarRenderer, inventoryRenderer, draggableItemRenderer, _playerInventory, craftingGridUi);

        var blockBreakingShader =
            new Shader(_gl, GetShaderPath("blockBreaking.vert"), GetShaderPath("blockBreaking.frag"));
        var blockBreakingTextureArray = new TextureArrayBuilder(16, 16)
            .AddTexture(Path.Combine("..", "..", "..", "Resources", "Textures", "Misc", "BlockBreaking", "1.png"))
            .AddTexture(Path.Combine("..", "..", "..", "Resources", "Textures", "Misc", "BlockBreaking", "2.png"))
            .AddTexture(Path.Combine("..", "..", "..", "Resources", "Textures", "Misc", "BlockBreaking", "3.png"))
            .AddTexture(Path.Combine("..", "..", "..", "Resources", "Textures", "Misc", "BlockBreaking", "4.png"))
            .AddTexture(Path.Combine("..", "..", "..", "Resources", "Textures", "Misc", "BlockBreaking", "5.png"))
            .Build(_gl);
        _blockBreaking = new BlockBreaking(_gl, blockBreakingShader, blockBreakingTextureArray);
        _blockPlacement = new BlockPlacement(_playerInventory, itemDatabase, _blockDatabase, (blockPos, blockId) =>
        {
            _chunkSystem.PlaceBlock(blockPos, blockId);
        });
        
        _playerInventory.Hotbar.AddItem("dirt", 45);
        _playerInventory.Hotbar.AddItem("coal", 14);
        _playerInventory.Storage.AddItem("glass", 52);
        _playerInventory.Storage.AddItem("sand", 23);
        _playerInventory.Storage.AddItem("log", 5);
        _playerInventory.Storage.AddItem("plank", 47);
        _playerInventory.Storage.AddItem("stick", 19);
    }

    private static string GetTexturePath(string name)
    {
        return Path.Combine("..", "..", "..", "Resources", "Textures", name);
    }

    private static string GetShaderPath(string name)
    {
        return Path.Combine("..", "..", "..", "Resources", "Shaders", name);
    }

    private float _mouseClickCooldownInSeconds = 0.1f;
    private float _currentMouseClickCooldown;

    private bool _isFirstUpdate = true;

    private bool _lastLeftClickStatus;
    private bool _lastRightClickStatus;

    public void Update(double deltaTime)
    {
        // this is here because the actual mouse click event is extremely slow...
        var isLeftClickPressed = _primaryMouse.IsButtonPressed(MouseButton.Left);
        if (isLeftClickPressed != _lastLeftClickStatus && isLeftClickPressed)
        {
            _uiRenderer.OnMouseClicked(MouseButton.Left, _primaryMouse.Position);
        }
        _lastLeftClickStatus = isLeftClickPressed;
        
        var isRightClickPressed = _primaryMouse.IsButtonPressed(MouseButton.Right);
        if (isRightClickPressed != _lastRightClickStatus && isRightClickPressed)
        {
            _uiRenderer.OnMouseClicked(MouseButton.Right, _primaryMouse.Position);
        }
        _lastRightClickStatus = isRightClickPressed;
        
        _playerControlsEnabled = _uiRenderer.AllowPlayerMovement;
        _primaryMouse.Cursor.CursorMode = _playerControlsEnabled ? CursorMode.Raw : CursorMode.Normal;
        
        _chunkSystem.UpdateChunkVisibility(_camera.Position, 4);

        if (_isFirstUpdate)
        {
            _isFirstUpdate = false;
            
            // set player position to the top of the chunk.
            for (var y = 0; y < Chunk.Height; y++)
            {
                if (!_chunkSystem.IsBlockSolid(new Vector3D<int>(0, y, 0)))
                {
                    _player.Position = new Vector3(0f, y + 1.5f, 0f);
                    break;
                }
            }
        }

        if (_playerControlsEnabled)
        {
            var raycast = _voxelRaycaster.Cast(_camera.Position, _camera.Direction, 10f);
            if (raycast.HasValue)
            {
                var blockType = _chunkSystem.GetBlock(raycast.Value.Position);
                var block = _blockDatabase.GetById(blockType);
                _blockBreaking.SetLookingAtBlock(block, raycast.Value.Position);
            }
            else
            {
                _blockBreaking.ClearLookingAtBlock();
            }
            _blockBreaking.UpdateDestruction((float)deltaTime, _primaryMouse.IsButtonPressed(MouseButton.Left));
            if (_blockBreaking.ShouldBreak)
            {
                var hit = raycast.Value;
                var blockId = _chunkSystem.GetBlock(hit.Position);
                var block = _blockDatabase.GetById(blockId);
                if (block.Drops != null)
                {
                    foreach (var drop in block.Drops)
                    {
                        var randomRoll = Random.Range(0f, 1.0f);
                        if (randomRoll <= drop.Probability)
                        {
                            var randomOffset = new Vector3(Random.Range(-0.1f, 0.1f), 0f, Random.Range(-0.1f, 0.1f));
                            _itemDroppingSystem.DropItem(Block.GetCenterPosition(hit.Position) + randomOffset, drop.Item);
                        }
                    }
                }
            
                _chunkSystem.DestroyBlock(hit.Position);
            }
        
            if (_currentMouseClickCooldown <= 0f)
            {
                if (_blockPlacement.Update(raycast, _primaryMouse.IsButtonPressed(MouseButton.Right)))
                {
                    _currentMouseClickCooldown = _mouseClickCooldownInSeconds;
                }
            }

            else
            {
                _currentMouseClickCooldown -= (float)deltaTime;
            }
        }


        var movementInput = _playerControlsEnabled ? GetMovementInputWithCamera() : Vector3.Zero;
        _player.Update((float)deltaTime, new Vector2(movementInput.X, movementInput.Z), _primaryKeyboard.IsKeyPressed(Key.Space));
        _camera.Position = _player.Position + new Vector3(0f, _player.Size.Y * 0.5f, 0f); 
        
        // var cursorMode = _primaryKeyboard.IsKeyPressed(Key.Tab) ? CursorMode.Normal : CursorMode.Raw;
        // _primaryMouse.Cursor.CursorMode = cursorMode;
        
        
        var pickedUpItems = _itemDroppingSystem.PickUpItems(_player.Position, 1.5f);
        foreach (var pickedUpItem in pickedUpItems)
        {
            Console.WriteLine($"[Item Pickup]: Picked up {pickedUpItem}");
            _playerInventory.TryAddItem(pickedUpItem, 1);
        }
        
        _itemDroppingSystem.Update((float)deltaTime);
        
        _uiRenderer.Update(_primaryMouse.Position);
    }

    public unsafe void Render(double deltaTime)
    {
        _imGuiController.Update((float)deltaTime);
        
        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.ClearColor(0.47f, 0.742f, 1f, 1.0f);
        _gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    
        
        _blockTextures.Textures.Bind(TextureUnit.Texture0);
        _shader.Use();
        _shader.SetUniform("uTextureArray", 0);

        var model = Matrix4x4.Identity;
        var view = Matrix4x4.CreateLookAt(_camera.Position, _camera.Position + _camera.Front, _camera.Up);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathUtil.DegreesToRadians(75.0f),
            (float)_frameBufferSize.X / _frameBufferSize.Y, 0.1f, 1000.0f);
    
        _shader.SetUniform("uModel", model);
        _shader.SetUniform("uView", view);
        _shader.SetUniform("uProjection", projection);

        // WORLD RENDERING - START
        
        _chunkSystem.RenderChunks();
        _chunkSystem.RenderTransparency(_camera.Position);
        
        _itemDroppingSystem.RenderDroppedItems(view, projection);
        
        _blockBreaking.Render(view, projection);
        // _itemDropRenderer.Render(view, projection);
        
        // WORLD RENDERING - END
        
        // UI RENDERING - START
        
        _gl.DepthMask(false);
        _gl.DepthMask(true);
        
        ImGuiNET.ImGui.Begin("Debug");
        ImGuiNET.ImGui.Text($"FPS: {1.0 / deltaTime:F1}");
        ImGuiNET.ImGui.Text($"Visible chunks: {_chunkSystem.VisibleChunkCount}");
        ImGuiNET.ImGui.Text($"Player position: {_player.Position}");
        ImGuiNET.ImGui.Text($"Player chunk position: {Chunk.WorldToChunkPosition(_camera.Position)}");
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
        ImGuiNET.ImGui.Text($"Selected block: {_blockDatabase.GetById(_blockSelector.CurrentBlock).DisplayName}");
        ImGuiNET.ImGui.End(); 
        
        _crosshairRenderer.Render();
        // _uiRenderer.Render(_window.Size.X, _window.Size.Y, _shader, _itemTextures);
        // _hotbarRenderer.Render();
        _uiRenderer.Render();
        
        _imGuiController.Render();
        
        // UI RENDERING - END
    }

    private Vector3 GetMovementInputWithCamera()
    {
        var inputVector = Vector2.Zero;
    
        if (_primaryKeyboard.IsKeyPressed(Key.W))
            inputVector.Y += 1f; // Forward
    
        if (_primaryKeyboard.IsKeyPressed(Key.S))
            inputVector.Y -= 1f; // Backward
    
        if (_primaryKeyboard.IsKeyPressed(Key.D))
            inputVector.X += 1f; // Right
    
        if (_primaryKeyboard.IsKeyPressed(Key.A))
            inputVector.X -= 1f; // Left

        // Transform input based on camera orientation
        var forward = _camera.Direction; // Forward is opposite of camera direction
        var right = -_camera.Right;
    
        // Project onto horizontal plane (remove Y component for ground movement)
        forward.Y = 0;
        right.Y = 0;
        forward = Vector3.Normalize(forward);
        right = Vector3.Normalize(right);

        if (inputVector == Vector2.Zero)
        {
            return Vector3.Zero;
        }
        
        return  Vector3.Normalize((forward * inputVector.Y + right * inputVector.X));
    } 

    public void OnFrameBufferResize(Vector2D<int> newSize)
    {
        _gl.Viewport(newSize);
        _frameBufferSize = newSize;
    }

    public void OnKeyDown(IKeyboard keyboard, Key pressedKey, int keyCode)
    {
        Console.WriteLine($"Key \"{pressedKey}\" pressed!");
        if (pressedKey == Key.Escape)
        {
            _window.Close();
        }

        if (pressedKey == Key.Tab)
        {
            _uiRenderer.ToggleInventory();
        }
    }

    private Vector2 _lastMousePosition;

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        const float lookSensitivity = 0.1f;
        if (_lastMousePosition == default)
        {
            _lastMousePosition = position;
        }
        
        else
        {
            if (_playerControlsEnabled)
            {
                var xOffset = (position.X - _lastMousePosition.X) * lookSensitivity;
                var yOffset = (position.Y - _lastMousePosition.Y) * lookSensitivity;

                _camera.Yaw += xOffset;
                _camera.Pitch -= yOffset;

                _camera.Pitch = Math.Clamp(_camera.Pitch, -89.0f, 89.0f);

                var direction = _camera.Direction;
                direction.X = MathF.Cos(MathUtil.DegreesToRadians(_camera.Yaw)) * MathF.Cos(MathUtil.DegreesToRadians(_camera.Pitch));
                direction.Y = MathF.Sin(MathUtil.DegreesToRadians(_camera.Pitch));
                direction.Z = MathF.Sin(MathUtil.DegreesToRadians(_camera.Yaw)) * MathF.Cos(MathUtil.DegreesToRadians(_camera.Pitch));
                _camera.Direction = direction;
                _camera.Front = Vector3.Normalize(direction);
            }
            
            _lastMousePosition = position;
        }
    }

    private void OnMouseClicked(IMouse mouse, MouseButton button, Vector2 position)
    {
        Console.WriteLine($"clicked with mouse {button} at {position}");
        // _uiRenderer.OnMouseClicked(button, position);
    }
    
    private void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    {
        var direction = scrollWheel.Y > 0 ? 1 : -1;
        _playerInventory.CycleSelectedHotbarSlot(direction);
    }
}