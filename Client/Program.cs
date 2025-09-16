using Client;
using Client.Persistence;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

var game = new Game();

var windowOptions = WindowOptions.Default with
{
    Size = new Vector2D<int>(1920, 1080),
    Title = $"{AppConstants.GameTitle} - v{AppConstants.MajorVersion}.{AppConstants.MinorVersion}.{AppConstants.RevisionVersion}"
};

WorldStorage.Initialize();

var window = Window.Create(windowOptions);
window.Load += () =>
{
    game.Load(window);
};
window.Update += game.Update;
window.Render += game.Render;
window.FramebufferResize += game.OnFrameBufferResize;
window.Closing += game.OnClosing;


window.Run();