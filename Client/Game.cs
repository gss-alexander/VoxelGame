using System.Drawing;
using System.Numerics;
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
            "C:\\dev\\personal\\VoxelGame\\Client\\Shaders\\shader.vert",
            "C:\\dev\\personal\\VoxelGame\\Client\\Shaders\\shader.frag");

        _textureArray = new TextureArrayBuilder(16, 16)
            .AddTexture("C:\\dev\\personal\\VoxelGame\\Client\\Textures\\dirt.png")
            .AddTexture("C:\\dev\\personal\\VoxelGame\\Client\\Textures\\cobblestone.png")
            .Build(_gl);

        _frameBufferSize = window.Size;

        _imGuiController = new ImGuiController(_gl, window, inputContext);

        _crosshairRenderer = new CrosshairRenderer();
        _crosshairRenderer.Initialize(_gl, window.Size.X, window.Size.Y);

        _voxelRaycaster = new VoxelRaycaster(_chunkSystem.IsBlockSolid);
    }

    private float _mouseClickCooldownInSeconds = 0.1f;
    private float _currentMouseClickCooldown;

    public void Update(double deltaTime)
    {
        _chunkSystem.UpdateChunkVisibility(_camera.Position, 5);

        if (_currentMouseClickCooldown <= 0f)
        {
            if (_primaryMouse.IsButtonPressed(MouseButton.Left))
            {
                var raycastHit = _voxelRaycaster.Cast(_camera.Position, _camera.Direction, 10f);
                if (raycastHit.HasValue)
                {
                    _chunkSystem.DestroyBlock(raycastHit.Value.Position);
                    _currentMouseClickCooldown = _mouseClickCooldownInSeconds;
                }
            }

            if (_primaryMouse.IsButtonPressed(MouseButton.Right))
            {
                var raycastHit = _voxelRaycaster.Cast(_camera.Position, _camera.Direction, 10f);
                if (raycastHit.HasValue)
                {
                    _chunkSystem.PlaceBlock(Block.GetFaceNeighbour(raycastHit.Value.Position, raycastHit.Value.Face), BlockType.Dirt);
                    _currentMouseClickCooldown = _mouseClickCooldownInSeconds;
                }
            }
        }

        else
        {
            _currentMouseClickCooldown -= (float)deltaTime;
        }
        
        var moveSpeed = 5f * (float)deltaTime;
        if (_primaryKeyboard.IsKeyPressed(Key.W))
        {
            _camera.Position += moveSpeed * _camera.Front;
        }
        
        if (_primaryKeyboard.IsKeyPressed(Key.S))
        {
            _camera.Position -= moveSpeed * _camera.Front;
        }
        
        if (_primaryKeyboard.IsKeyPressed(Key.A))
        {
            _camera.Position -= Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * moveSpeed;
        }
        
        if (_primaryKeyboard.IsKeyPressed(Key.D))
        {
            _camera.Position += Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * moveSpeed;
        }

        var cursorMode = _primaryKeyboard.IsKeyPressed(Key.Tab) ? CursorMode.Normal : CursorMode.Raw;
        _primaryMouse.Cursor.CursorMode = cursorMode;
    }

    public unsafe void Render(double deltaTime)
    {
        _imGuiController.Update((float)deltaTime);
        
        _gl.Enable(EnableCap.DepthTest);
        _gl.ClearColor(0.47f, 0.742f, 1f, 1.0f);
        _gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    
        
        _textureArray.Bind(TextureUnit.Texture0);
        _shader.Use();
        _shader.SetUniform("uTextureArray", 0);

        var model = Matrix4x4.Identity;
        var view = Matrix4x4.CreateLookAt(_camera.Position, _camera.Position + _camera.Front, _camera.Up);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(DegreesToRadians(75.0f),
            (float)_frameBufferSize.X / _frameBufferSize.Y, 0.1f, 100.0f);
    
        _shader.SetUniform("uModel", model);
        _shader.SetUniform("uView", view);
        _shader.SetUniform("uProjection", projection);

        _chunkSystem.RenderChunks();
        
        ImGuiNET.ImGui.Begin("Debug");
        ImGuiNET.ImGui.Text($"FPS: {1.0 / deltaTime:F1}");
        ImGuiNET.ImGui.Text($"Visible chunks: {_chunkSystem.VisibleChunkCount}");
        ImGuiNET.ImGui.Text($"Player position: {_camera.Position}");
        ImGuiNET.ImGui.Text($"Player chunk position: {Chunk.WorldToChunkPosition(_camera.Position)}");
        var raycastHit = _voxelRaycaster.Cast(_camera.Position, _camera.Direction, 10f);
        if (raycastHit.HasValue)
        {
            ImGuiNET.ImGui.Text($"Looking at block pos: {raycastHit.Value.Position}");
            ImGuiNET.ImGui.Text($"Looking at block face: {raycastHit.Value.Face}");
        }
        ImGuiNET.ImGui.End(); 
        
        _crosshairRenderer.Render();
        
        _imGuiController.Render();
    }

    private static float DegreesToRadians(float degrees)
    {
        return MathF.PI / 180f * degrees;
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
            direction.X = MathF.Cos(DegreesToRadians(_camera.Yaw)) * MathF.Cos(DegreesToRadians(_camera.Pitch));
            direction.Y = MathF.Sin(DegreesToRadians(_camera.Pitch));
            direction.Z = MathF.Sin(DegreesToRadians(_camera.Yaw)) * MathF.Cos(DegreesToRadians(_camera.Pitch));
            _camera.Direction = direction;
            _camera.Front = Vector3.Normalize(direction);
        }
    }
    
    private void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    {
    }
}