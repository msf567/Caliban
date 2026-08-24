using Caliban.Graphics.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Caliban.Core.Transport;
using Caliban.Graphics.Drawables;

namespace Caliban.Graphics;

internal sealed class App : GameWindow
{
    private readonly Dictionary<string, IDrawable> _gameObjects = new();

    private bool _loaded;

    public App()
        : base(
            GameWindowSettings.Default,
            new NativeWindowSettings
            {
                Title = "Graphics",

                WindowState = WindowState.Fullscreen,
                APIVersion = new Version(4, 3),
                Profile = ContextProfile.Core,

                TransparentFramebuffer = true,
                AlphaBits = 8,

                WindowBorder = WindowBorder.Hidden,
                AutoIconify = false,

                Vsync = VSyncMode.On
            })
    {
    }

    public void ClientOnMessageReceived(byte[] _message)
    {
        Message m = Messages.Parse(_message);
        switch (m.Type)
        {
            case MessageType.SANDSTORM_START:
                ((SandStorm)_gameObjects["sandstorm"]).Begin();
                break;

            case MessageType.HOOKS_L_CLICK:
                ((SandStorm)_gameObjects["sandstorm"]).OnMouseDown();
                break;

            case MessageType.GAME_CLOSE:
                ((SandStorm)_gameObjects["sandstorm"]).End();
                break;

            case MessageType.APP_CLOSE:
                Close();
                break;
        }
    }

    public void Spawn(string id, IDrawable drawable)
    {
        _gameObjects.TryAdd(id, drawable);

        if (_loaded)
        {
            drawable.Load();
            drawable.Resize(FramebufferSize);
        }
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        Console.WriteLine($"OpenGL: {GL.GetString(StringName.Version)}");
        Console.WriteLine($"Transparent: {HasTransparentFramebuffer}");

        GL.ClearColor(0f, 0f, 0f, 0f);
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(
            BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha);

        foreach (IDrawable drawable in _gameObjects.Values)
        {
            drawable.Load();
            drawable.Resize(FramebufferSize);
        }

        unsafe
        {
            WinEx.MakeOverlayClickThrough(GLFW.GetWin32Window(WindowPtr));
        }

        _loaded = true;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);

        foreach (IDrawable drawable in _gameObjects.Values)
            drawable.Resize(FramebufferSize);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();

        foreach (IDrawable drawable in _gameObjects.Values)
            drawable.Update(e);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        Vector2 center = new(
            FramebufferSize.X * 0.5f,
            FramebufferSize.Y * 0.5f);

        foreach (IDrawable drawable in _gameObjects.Values)
            drawable.Draw(center.X, center.Y);

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        foreach (IDrawable drawable in _gameObjects.Values)
            drawable.Dispose();

        _gameObjects.Clear();

        base.OnUnload();
    }
}