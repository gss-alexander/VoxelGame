using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Client.Audio;
using Client.Blocks;
using Client.Chunks;
using Client.Clouds;
using Client.Crafting;
using Client.Debug;
using Client.Diagnostics;
using Client.Inputs;
using Client.Items;
using Client.Items.Dropping;
using Client.Persistence;
using Client.Settings;
using Client.Sound;
using Client.UI;
using Client.UI.Text;
using Client.UiSystem;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Client;

public class Game
{
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

    private BlockData[] _blockData;
    private BlockTextures _blockTextures;
    private BlockDatabase _blockDatabase;

    // private TextRenderer _textRenderer;

    private BlockBreaking _blockBreaking;
    private BlockPlacement _blockPlacement;

    private PlayerInventory _playerInventory;

    private ItemDropRenderer _itemDropRenderer;

    private ItemDroppingSystem _itemDroppingSystem;
    private ItemTextures _itemTextures;

    private HotbarRenderer _hotbarRenderer;

    private ActionContext _actionContext;

    private readonly TimeAverageTracker _updateTimeAverage = new(60);
    private readonly TimeAverageTracker _renderTimeAverage = new(60);
    private readonly TimeAverageTracker _deltaTimeAverage = new(60);

    private CloudSystem _cloudSystem;

    private ItemDatabase _itemDatabase;

    private DebugMenu _debugMenu;

    private AudioContext _audioContext;

    private GraphicsSettings _graphicsSettings = new();

    private UiManager _uiManager;

    private GameController _gameController;

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

        _actionContext = new ActionContext(_primaryKeyboard, _primaryMouse);

        OpenGl.Context = window.CreateOpenGL();
        
        _blockData = BlockDataLoader.Load(Path.Combine("..", "..", "..", "Resources", "Data", "blocks.yaml"));
        _blockDatabase = new BlockDatabase(_blockData);
        _blockTextures = new BlockTextures(OpenGl.Context, _blockDatabase);

        _chunkSystem = new ChunkSystem(_blockTextures, _blockDatabase);

        _shader = new Shader(
            GetShaderPath("shader.vert"),
            GetShaderPath("shader.frag")
        );
        
        _soundPlayer = new SoundPlayer();

        OnFrameBufferResize(window.Size);

        _imGuiController = new ImGuiController(OpenGl.Context, window, inputContext);

        _crosshairRenderer = new CrosshairRenderer();
        _crosshairRenderer.Initialize(OpenGl.Context, window.Size.X, window.Size.Y);

        _voxelRaycaster = new VoxelRaycaster(_chunkSystem.IsBlockSolid);
        _player = new Player(new Vector3(0f, 100f, 0f), worldPos =>
        {
            var blockPos = Block.WorldToBlockPosition(worldPos);
            return _chunkSystem.IsBlockSolid(blockPos);
        }, _actionContext, _soundPlayer);

        _blockSelector = new BlockSelector(_blockDatabase);

        _blockSpriteRenderer = new BlockSpriteRenderer(OpenGl.Context, _blockTextures);

        _playerInventory = new PlayerInventory();
        
        var items = ItemLoader.Load();
        var itemDatabase = new ItemDatabase(items);
        itemDatabase.RegisterBlockItems(_blockDatabase.GetAll().Select(b => b.data).ToArray());
        _itemDatabase = itemDatabase;
        _itemTextures = new ItemTextures(OpenGl.Context, itemDatabase, _blockDatabase, _blockSpriteRenderer);
        var itemDropShader = new Shader(GetShaderPath("itemDrop.vert"),  GetShaderPath("itemDrop.frag"));
        _itemDroppingSystem = new ItemDroppingSystem(itemDatabase, _itemTextures, itemDropShader, worldPos =>
        {
            var blockPos = Block.WorldToBlockPosition(worldPos);
            return _chunkSystem.IsBlockSolid(blockPos);
        }, _blockDatabase, _blockTextures);

        var craftingRecipes = CraftingRecipesLoader.Load();
        var craftingGrid = new CraftingGrid(3, 3, craftingRecipes);

        var blockBreakingShader =
            new Shader(GetShaderPath("blockBreaking.vert"), GetShaderPath("blockBreaking.frag"));
        var blockBreakingTextureArray = new TextureArrayBuilder(16, 16)
            .AddTexture(Path.Combine("..", "..", "..", "Resources", "Textures", "Misc", "BlockBreaking", "1.png"))
            .AddTexture(Path.Combine("..", "..", "..", "Resources", "Textures", "Misc", "BlockBreaking", "2.png"))
            .AddTexture(Path.Combine("..", "..", "..", "Resources", "Textures", "Misc", "BlockBreaking", "3.png"))
            .AddTexture(Path.Combine("..", "..", "..", "Resources", "Textures", "Misc", "BlockBreaking", "4.png"))
            .AddTexture(Path.Combine("..", "..", "..", "Resources", "Textures", "Misc", "BlockBreaking", "5.png"))
            .Build(OpenGl.Context);
        _blockBreaking = new BlockBreaking(OpenGl.Context, blockBreakingShader, blockBreakingTextureArray, _soundPlayer, _playerInventory, _itemDatabase);
        _blockPlacement = new BlockPlacement(_playerInventory, itemDatabase, _blockDatabase, (blockPos, blockId) =>
        {
            _chunkSystem.PlaceBlock(blockPos, blockId);
        }, _soundPlayer);
        
        _isWorldLoaded = TryLoadingWorld(); 
        
        _chunkSystem.StartChunkGenerationThread();
        
        _chunkSystem.ForceLoad(_player.Position, 1);

        _cloudSystem = new CloudSystem();
        _cloudSystem.GenerateClouds();

        _debugMenu = new DebugMenu(_camera, _blockDatabase, _blockSelector, _itemDatabase, _voxelRaycaster,
            _playerInventory, _deltaTimeAverage, _updateTimeAverage, _renderTimeAverage, _chunkSystem, _player, _soundPlayer, _graphicsSettings);

        _gameController = new GameController(() => _window.Close());
        
        _uiManager = new UiManager(_actionContext, _gameController, _playerInventory, _itemTextures);
    }

    private SoundPlayer _soundPlayer;

    private bool _isWorldLoaded;

    private static string GetShaderPath(string name)
    {
        return Path.Combine("..", "..", "..", "Resources", "Shaders", name);
    }

    private float _mouseClickCooldownInSeconds = 0.1f;
    private float _currentMouseClickCooldown;

    private bool _isFirstUpdate = true;

    private bool _lastLeftClickStatus;
    private bool _lastRightClickStatus;

    private readonly Stopwatch _updateStopwatch = new();

    public void Update(double deltaTime)
    {
        _updateStopwatch.Restart();
        _actionContext.CollectInputs((float)deltaTime);
        _soundPlayer.Update();
        
        // this is here because the actual mouse click event is extremely slow...
        var isLeftClickPressed = _primaryMouse.IsButtonPressed(MouseButton.Left);
        if (isLeftClickPressed != _lastLeftClickStatus && isLeftClickPressed)
        {
            // _uiRenderer.OnMouseClicked(MouseButton.Left, _primaryMouse.Position);
        }
        _lastLeftClickStatus = isLeftClickPressed;
        
        var isRightClickPressed = _primaryMouse.IsButtonPressed(MouseButton.Right);
        if (isRightClickPressed != _lastRightClickStatus && isRightClickPressed)
        {
            // _uiRenderer.OnMouseClicked(MouseButton.Right, _primaryMouse.Position);
        }
        _lastRightClickStatus = isRightClickPressed;
        
        // _primaryMouse.Cursor.CursorMode = _playerControlsEnabled ? CursorMode.Raw : CursorMode.Normal;
        
        _chunkSystem.UpdateChunkVisibility(_camera.Position, _graphicsSettings.RenderDistance);

        if (_isFirstUpdate)
        {
            OnFirstUpdate();
            _isFirstUpdate = false;
        }

        if (!_actionContext.MovementBlocked)
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

            BlockData? lookingAtBlockData = null;
            if (raycast.HasValue)
            {
                var hit = raycast.Value;
                var blockId = _chunkSystem.GetBlock(hit.Position);
                lookingAtBlockData = _blockDatabase.GetById(blockId);
            }
            
            _blockBreaking.UpdateDestruction((float)deltaTime, _actionContext.IsHeld(InputAction.DestroyBlock), lookingAtBlockData);
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
                            _itemDroppingSystem.CreateDroppedItem(Block.GetCenterPosition(hit.Position) + randomOffset, drop.Item);
                        }
                    }
                }
            
                _chunkSystem.DestroyBlock(hit.Position);
                if (block.DestructionSoundId != null)
                {
                    _soundPlayer.PlaySound(block.DestructionSoundId);
                }
            }
        
            if (_currentMouseClickCooldown <= 0f)
            {
                if (_blockPlacement.Update(raycast, _actionContext.IsHeld(InputAction.PlaceBlock)))
                {
                    _currentMouseClickCooldown = _mouseClickCooldownInSeconds;
                }
            }

            else
            {
                _currentMouseClickCooldown -= (float)deltaTime;
            }

            if (_actionContext.IsPressed(InputAction.DropItem))
            {
                var currentHeldSlot = _playerInventory.Hotbar.GetSlot(_playerInventory.SelectedHotbarSlot);
                if (currentHeldSlot != null)
                {
                    var dropOrigin = _camera.Position + _camera.Direction * 0.25f;
                    _itemDroppingSystem.PlayerDropItem(dropOrigin, _camera.Direction, 7.5f, currentHeldSlot.ItemId);
                    _playerInventory.Hotbar.RemoveItemFromSlot(_playerInventory.SelectedHotbarSlot, 1);
                }
            }
        }


        if (!_debugMenu.FreeCamEnabled)
        {
            var movementInput = !_actionContext.MovementBlocked ? GetMovementInputWithCamera(true) : Vector3.Zero;
            _player.Update((float)deltaTime, new Vector2(movementInput.X, movementInput.Z));
            _camera.Position = _player.Position + new Vector3(0f, _player.Size.Y * 0.5f, 0f); 
        }
        else
        {
            const float freecamSpeed = 20f;
            var movementInput = !_actionContext.MovementBlocked ? GetMovementInputWithCamera(false) : Vector3.Zero;
            _camera.Position += movementInput * (freecamSpeed * (float)deltaTime);
        }
        
        var pickedUpItems = _itemDroppingSystem.PickUpItems(_player.Position, 1.5f);
        foreach (var pickedUpItem in pickedUpItems)
        {
            _playerInventory.TryAddItem(pickedUpItem, 1);
        }
        
        _itemDroppingSystem.Update((float)deltaTime);
        
        // _uiRenderer.Update(_primaryMouse.Position);
        
        _cloudSystem.Update((float)deltaTime);

        if (_actionContext.IsPressed(InputAction.TogglePause))
        {
            _uiManager.TogglePauseMenu();
        }

        if (_actionContext.IsPressed(InputAction.ToggleInventory))
        {
            _uiManager.TryToggleInventoryMenu();
        }
        
        _uiManager.Update((float)deltaTime);
        
        _updateStopwatch.Stop();
        _updateTimeAverage.AddTime((float)_updateStopwatch.Elapsed.TotalSeconds);
        _deltaTimeAverage.AddTime((float)deltaTime);
    }

    private readonly Stopwatch _renderStopwatch = new();
    public unsafe void Render(double deltaTime)
    {
        _renderStopwatch.Restart();
        _window.VSync = _debugMenu.UseVSync;
        _imGuiController.Update((float)deltaTime);
        
        OpenGl.Context.Enable(EnableCap.DepthTest);
        OpenGl.Context.Enable(EnableCap.Blend);
        OpenGl.Context.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        OpenGl.Context.ClearColor(0.47f, 0.742f, 1f, 1.0f);
        OpenGl.Context.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        
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

        if (_debugMenu.RenderWireframes)
        {
            OpenGl.Context.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);
        }
        
        _chunkSystem.RenderChunks();
        _chunkSystem.RenderTransparency(_camera.Position);
        
        _itemDroppingSystem.RenderDroppedItems(view, projection);
        
        _blockBreaking.Render(view, projection);
        
        _cloudSystem.Render(view, projection);
        
        if (_debugMenu.RenderWireframes)
        {
            OpenGl.Context.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
        }
        
        // WORLD RENDERING - END
        
        // UI RENDERING - START
        
        OpenGl.Context.DepthMask(false);
        OpenGl.Context.DepthMask(true);
        
        _debugMenu.Draw();
        
        _uiManager.Render((float)deltaTime);
        
        _crosshairRenderer.Render();
        // _uiRenderer.Render();
        
        _imGuiController.Render();

        _renderStopwatch.Stop();
        _renderTimeAverage.AddTime((float)_renderStopwatch.Elapsed.TotalSeconds);
        
        // UI RENDERING - END
    }


    private Vector3 GetMovementInputWithCamera(bool projectToHorizontalPlane)
    {
        var inputVector = Vector2.Zero;
    
        if (_actionContext.IsHeld(InputAction.MoveForward))
            inputVector.Y += 1f; // Forward
    
        if (_actionContext.IsHeld(InputAction.MoveBackward))
            inputVector.Y -= 1f; // Backward
    
        if (_actionContext.IsHeld(InputAction.MoveRight))
            inputVector.X += 1f; // Right
    
        if (_actionContext.IsHeld(InputAction.MoveLeft))
            inputVector.X -= 1f; // Left

        // Transform input based on camera orientation
        var forward = _camera.Direction; // Forward is opposite of camera direction
        var right = -_camera.Right;
    
        // Project onto horizontal plane (remove Y component for ground movement)
        if (projectToHorizontalPlane)
        {
            forward.Y = 0;
            right.Y = 0;
            forward = Vector3.Normalize(forward);
            right = Vector3.Normalize(right);
        }

        if (inputVector == Vector2.Zero)
        {
            return Vector3.Zero;
        }
        
        return  Vector3.Normalize((forward * inputVector.Y + right * inputVector.X));
    } 

    public void OnFrameBufferResize(Vector2D<int> newSize)
    {
        OpenGl.Context.Viewport(newSize);
        _frameBufferSize = newSize;
        WindowDimensions.Width = _frameBufferSize.X;
        WindowDimensions.Height = _frameBufferSize.Y;
    }

    public void OnKeyDown(IKeyboard keyboard, Key pressedKey, int keyCode)
    {
        if (pressedKey == Key.Tab)
        {
            // _uiRenderer.ToggleInventory();
        }
    }

    private void OnFirstUpdate()
    {
        if (!_isWorldLoaded)
        {
            // set player position to the top of the chunk.
            for (var y = 0; y < Chunk.Height; y++)
            {
                var blockPosition = Block.WorldToBlockPosition(_player.Position);
                if (!_chunkSystem.IsVirtualBlockSolid(new Vector3D<int>(blockPosition.X, y, blockPosition.Z)))
                {
                    _player.Position = _player.Position with { Y = y + 0.5f };
                    break;
                }
            }
        }
        
        OnMouseMove(_primaryMouse, _primaryMouse.Position);
    }

    private Vector2 _lastMousePosition;

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        const float lookSensitivity = 0.1f;
        if (_lastMousePosition == default)
        {
            _lastMousePosition = position;
        }
        
        if (!_actionContext.MovementBlocked)
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

    private void OnMouseClicked(IMouse mouse, MouseButton button, Vector2 position)
    {
    }
    
    private void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    {
        var direction = scrollWheel.Y > 0 ? 1 : -1;
        _playerInventory.CycleSelectedHotbarSlot(direction);
    }

    private bool TryLoadingWorld()
    {
        if (WorldStorage.DoesWorldExist("Test world"))
        {
            var worldData = WorldStorage.LoadFromName("Test world");
            _chunkSystem.ApplyPersistedBlockChanges(worldData.ModifiedBlocks);
            _player.Position = worldData.PlayerPosition;
            _camera.Pitch = worldData.CameraPitch;
            _camera.Yaw = worldData.CameraYaw;
            PlayerInventory.Copy(worldData.Inventory, _playerInventory);
            return true;
        }

        return false;
    }

    public void OnClosing()
    {
        _chunkSystem.StopChunkGenerationThread();
        
        var worldData = new WorldData("Test world");
        
        // Register all modified blocks
        foreach (var kvp in _chunkSystem.GetModifiedBlocks())
        {
            worldData.AddModifiedBlock(kvp.Key, kvp.Value);
        }
        
        worldData.PlayerPosition = _player.Position;
        worldData.CameraPitch = _camera.Pitch;
        worldData.CameraYaw = _camera.Yaw;
        worldData.Inventory = _playerInventory;
        
        WorldStorage.StoreWorld(worldData);
    }
}