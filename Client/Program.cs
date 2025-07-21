using Client;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

var game = new Game();

var windowOptions = WindowOptions.Default with
{
    Size = new Vector2D<int>(800, 600),
    Title = "Silk.NET application!"
};

var window = Window.Create(windowOptions);
window.Load += () =>
{
    game.Load(window);
};
window.Update += game.Update;
window.Render += game.Render;
window.FramebufferResize += game.OnFrameBufferResize;


window.Run();