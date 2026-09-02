using Caliban.Graphics.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Caliban.Core.Transport;
using Caliban.Graphics.Drawables;
using System.Text.Json;
using Caliban.Core.Debug;

namespace Caliban.Graphics;

internal sealed class App : GameWindow
{
    private readonly Dictionary<string, IDrawable> _gameObjects = new();
    private Dictionary<int, Scene> SceneCache = new();
    private bool _loaded;
    public ClientApp? TransportClient;
    private double heartbeatTimer, heartbeatCount;

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
        heartbeatCount = 0;
        heartbeatTimer = 0;
    }

    public void ClientOnMessageReceived(byte[] _message)
    {
        Message m = Messages.Parse(_message);
        switch (m.Type)
        {
            case MessageType.SANDSTORM_START:
                ((SandStorm)_gameObjects["sandstorm"]).Begin();
                break;
            case MessageType.HEARTBEAT:
                heartbeatCount = 0;
                D.Write("Got Heartbeat!");
                break;
            case MessageType.HOOKS_L_CLICK:
                ((SandStorm)_gameObjects["sandstorm"]).OnMouseDown();
                break;

            case MessageType.GAME_CLOSE:
                ((SandStorm)_gameObjects["sandstorm"]).End();
                break;

            case MessageType.GRAPHICS_SCENE:
                try
                {
                    if (string.IsNullOrWhiteSpace(m.Value))
                    {
                        D.Write("Warning: Graphics scene payload was empty.");
                        break;
                    }

                    GraphicsSceneDescription? scene = JsonSerializer.Deserialize<GraphicsSceneDescription>(m.Value);

                    if (scene == null)
                    {
                        D.Write("Warning: Deserialized scene returned null.");
                        break;
                    }

                    D.Write(scene.Seed);
                    if (scene.Features != null)
                    {
                        foreach (var feature in scene.Features)
                        {
                            D.Write(feature.Type);
                        }
                    }
                    else
                    {
                        D.Write("Notice: Scene contains no features.");
                    }
                }
                catch (JsonException ex)
                {
                    D.Write($"JSON Parsing Error in GRAPHICS_SCENE: {ex.Message}");
                }
                catch (Exception ex)
                {
                    D.Write($"Unexpected error processing GRAPHICS_SCENE: {ex.Message}");
                }

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

        D.Write($"OpenGL: {GL.GetString(StringName.Version)}");
        D.Write($"Transparent: {HasTransparentFramebuffer}");

        unsafe
        {
            var hwnd = GLFW.GetWin32Window(WindowPtr);
            int exStyle = Core.OS.Windows.GetWindowLong(hwnd, Core.OS.Windows.GWL_EXSTYLE);

            exStyle &= ~Core.OS.Windows.WS_EX_APPWINDOW;
            exStyle |= Core.OS.Windows.WS_EX_TOOLWINDOW;

            Core.OS.Windows.SetWindowLong(hwnd, Core.OS.Windows.GWL_EXSTYLE, exStyle);
        }

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
        heartbeatCount += e.Time;
        if (heartbeatCount > 5)
        {
            D.Write("Closing due to missed heartbeats.");
            Close();
        }

        heartbeatTimer += e.Time;
        if (heartbeatTimer > 1)
        {
            heartbeatTimer -= 1;
            D.Write("Sending Heartbeat");
            TransportClient?.SendMessageToHost(MessageType.HEARTBEAT, "");
        }

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            D.Write("Closing due to [Esc].");
            Close();
        }

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

        int x = 0;
        foreach (IDrawable drawable in _gameObjects.Values)
        {
            drawable.Draw(x, center.Y);
            x += 300;
        }

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