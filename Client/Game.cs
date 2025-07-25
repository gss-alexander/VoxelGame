using System.Drawing;
using System.Numerics;
using Client.UI;
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
    private TextureArray _textureArray;

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
    private BlockDrops _blockDrops;

    private BoundingBoxRenderer _boundingBoxRenderer;
    private Shader _lineShader;
    
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
        }

        _primaryMouse = inputContext.Mice.First();

        _gl = window.CreateOpenGL();

        _chunkSystem = new ChunkSystem(_gl);

        _shader = new Shader(_gl,
            GetShaderPath("shader.vert"),
            GetShaderPath("shader.frag")
        );

        _textureArray = new TextureArrayBuilder(16, 16)
            .AddTexture(GetTexturePath("dirt.png")) //0
            .AddTexture(GetTexturePath("cobblestone.png")) // 1
            .AddTexture(GetTexturePath("grass_side.png")) // 2
            .AddTexture(GetTexturePath("grass_top.png")) // 3
            .AddTexture(GetTexturePath("sand.png")) // 4
            .AddTexture(GetTexturePath("log_top.png")) // 5
            .AddTexture(GetTexturePath("log_side.png")) // 6
            .AddTexture(GetTexturePath("leaves.png")) // 7
            .AddTexture(GetTexturePath("glass.png")) // 8
            .Build(_gl);

        _frameBufferSize = window.Size;

        _imGuiController = new ImGuiController(_gl, window, inputContext);

        _crosshairRenderer = new CrosshairRenderer();
        _crosshairRenderer.Initialize(_gl, window.Size.X, window.Size.Y);

        _voxelRaycaster = new VoxelRaycaster(_chunkSystem.IsBlockSolid);
        _player = new Player(new Vector3(0f, 10f, 0f), worldPos =>
        {
            var blockPos = Block.WorldToBlockPosition(worldPos);
            return _chunkSystem.IsBlockSolid(blockPos);
        });

        _blockSelector = new BlockSelector();

        var uiShader = new Shader(_gl, GetShaderPath("ui.vert"), GetShaderPath("ui.frag"));
        var uiTexture = new Texture(_gl, GetTexturePath("hotbar_slot_background.png"));
        _uiRenderer = new UiRenderer(_gl, uiShader, uiTexture, _window.Size.X, _window.Size.Y);

        _blockDrops = new BlockDrops(_gl, _shader, _textureArray);
    }

    private static string GetTexturePath(string name)
    {
        return Path.Combine("..", "..", "..", "Textures", name);
    }

    private static string GetShaderPath(string name)
    {
        return Path.Combine("..", "..", "..", "Shaders", name);
    }

    private float _mouseClickCooldownInSeconds = 0.1f;
    private float _currentMouseClickCooldown;

    public void Update(double deltaTime)
    {
        _chunkSystem.UpdateChunkVisibility(_camera.Position, 1);

        if (_currentMouseClickCooldown <= 0f)
        {
            if (_primaryMouse.IsButtonPressed(MouseButton.Left))
            {
                var raycastHit = _voxelRaycaster.Cast(_camera.Position, _camera.Direction, 10f);
                if (raycastHit.HasValue)
                {
                    var hit = raycastHit.Value;
                    
                    // create a dropped block where the block is
                    var blockType = _chunkSystem.GetBlock(hit.Position);
                    _blockDrops.CreateDroppedBlock(Block.GetCenterPosition(hit.Position), blockType, worldPos =>
                    {
                        var blockPos = Block.WorldToBlockPosition(worldPos);
                        return _chunkSystem.IsBlockSolid(blockPos);
                    });
                    
                    // actually remove the block
                    _chunkSystem.DestroyBlock(hit.Position);
                    _currentMouseClickCooldown = _mouseClickCooldownInSeconds;
                }
            }

            if (_primaryMouse.IsButtonPressed(MouseButton.Right))
            {
                var raycastHit = _voxelRaycaster.Cast(_camera.Position, _camera.Direction, 10f);
                if (raycastHit.HasValue)
                {
                    _chunkSystem.PlaceBlock(Block.GetFaceNeighbour(raycastHit.Value.Position, raycastHit.Value.Face), _blockSelector.CurrentBlock);
                    _currentMouseClickCooldown = _mouseClickCooldownInSeconds;
                }
            }
        }

        else
        {
            _currentMouseClickCooldown -= (float)deltaTime;
        }

        var movementInput = GetMovementInputWithCamera();
        _player.Update((float)deltaTime, new Vector2(movementInput.X, movementInput.Z), _primaryKeyboard.IsKeyPressed(Key.Space));
        _camera.Position = _player.Position + new Vector3(0f, _player.Size.Y * 0.5f, 0f); 
        
        var cursorMode = _primaryKeyboard.IsKeyPressed(Key.Tab) ? CursorMode.Normal : CursorMode.Raw;
        _primaryMouse.Cursor.CursorMode = cursorMode;
        
        _blockDrops.Update((float)deltaTime);

        var pickedUpBlocks = _blockDrops.PickupDroppedBlocks(_player.Position, 1.5f);
        foreach (var pickedUpBlock in pickedUpBlocks)
        {
            Console.WriteLine($"Picked up block of type: {pickedUpBlock}");
        }
    }

    public unsafe void Render(double deltaTime)
    {
        _imGuiController.Update((float)deltaTime);
        
        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.ClearColor(0.47f, 0.742f, 1f, 1.0f);
        _gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    
        
        _textureArray.Bind(TextureUnit.Texture0);
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
        _blockDrops.Render((float)deltaTime);
        // _boundingBoxRenderer.RenderBoundingBox(_player.Position, _player.Size, view, projection,
        //     new Vector3(1.0f, 0.0f, 0.0f));
        
        // WORLD RENDERING - END
        
        // UI RENDERING - START
        
        _gl.DepthMask(false);
        _chunkSystem.RenderTransparency(_camera.Position);
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
        ImGuiNET.ImGui.Text($"Selected block: {_blockSelector.CurrentBlock}");
        ImGuiNET.ImGui.End(); 
        
        _crosshairRenderer.Render();
        _uiRenderer.Render(_window.Size.X, _window.Size.Y);
        
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
            var xOffset = (position.X - _lastMousePosition.X) * lookSensitivity;
            var yOffset = (position.Y - _lastMousePosition.Y) * lookSensitivity;
            _lastMousePosition = position;

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
    }

    
    private void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    {
        var direction = scrollWheel.Y < 0 ? -1 : 1;
        _blockSelector.Cycle(direction);
    }
}