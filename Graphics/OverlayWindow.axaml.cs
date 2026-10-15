using System.Runtime.InteropServices;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Caliban.Core.Debug;
using Caliban.Core.Transport;
using Caliban.Graphics.Drawables;
using Caliban.Graphics.Rendering;

namespace Caliban.Graphics;

/// <summary>
/// The borderless, fullscreen, always-on-top, no-taskbar transparent window that
/// hosts the OpenGL overlay surface. It replaces the old OpenTK <c>GameWindow</c>
/// (whose borderless/topmost/tool-window styling is now declared in XAML) and, on
/// Windows, additionally makes itself click-through so mouse input passes to
/// whatever is underneath. It also drives the transport heartbeat and routes host
/// messages to the drawables.
/// </summary>
public partial class OverlayWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const long WS_EX_TRANSPARENT = 0x00000020;
    private const long WS_EX_TOOLWINDOW = 0x00000080;
    private const long WS_EX_LAYERED = 0x00080000;
    private const long WS_EX_APPWINDOW = 0x00040000;

    // The storm is driven directly by host messages; captured on spawn so the
    // message handler does not have to look it up each time.
    private SandStorm? _sandStorm;

    // Seconds since the last HEARTBEAT was received from the host; the overlay
    // closes itself if the host goes away (mirrors the old frame-based timeout).
    private int _secondsSinceHeartbeat;
    private DispatcherTimer? _heartbeatTimer;

    /// <summary>The connected transport client, used to send heartbeats back to the host.</summary>
    public ClientApp? TransportClient { get; set; }

    public OverlayWindow()
    {
        InitializeComponent();
    }

    /// <summary>Queues a drawable into the GL surface (thread-safe).</summary>
    public void Spawn(string id, IDrawable drawable)
    {
        GlControl.Spawn(id, drawable);

        if (drawable is SandStorm storm)
            _sandStorm = storm;
    }

    /// <summary>
    /// Entry point for the transport client (called on a socket thread). Marshals
    /// the message onto the UI thread before touching any window/overlay state.
    /// </summary>
    public void OnMessageReceived(byte[] message)
    {
        Dispatcher.UIThread.Post(() => HandleMessage(message));
    }

    private void HandleMessage(byte[] message)
    {
        Message m = Messages.Parse(message);
        switch (m.Type)
        {
            case MessageType.SANDSTORM_START:
                _sandStorm?.Begin();
                break;

            case MessageType.HEARTBEAT:
                _secondsSinceHeartbeat = 0;
                break;

            case MessageType.HOOKS_L_CLICK:
                _sandStorm?.OnMouseDown();
                break;

            case MessageType.GAME_CLOSE:
                _sandStorm?.End();
                break;

            case MessageType.GRAPHICS_SCENE:
                HandleGraphicsScene(m);
                break;

            case MessageType.APP_CLOSE:
                Close();
                break;
        }
    }

    private static void HandleGraphicsScene(Message m)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(m.Value))
            {
                D.Write("Warning: Graphics scene payload was empty.");
                return;
            }

            GraphicsSceneDescription? scene = JsonSerializer.Deserialize<GraphicsSceneDescription>(
                m.Value,
                GraphicsSceneDescription.JsonOptions);

            if (scene == null)
            {
                D.Write("Warning: Deserialized scene returned null.");
                return;
            }

            D.Write(scene.Seed);
            if (scene.Features != null)
            {
                foreach (var feature in scene.Features)
                    D.Write(feature.Type.ToString());
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
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        ApplyNativeOverlayStyles();

        // Send a heartbeat every second and close if the host stops answering,
        // mirroring the old OnUpdateFrame timeout.
        _heartbeatTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _heartbeatTimer.Tick += OnHeartbeatTick;
        _heartbeatTimer.Start();
    }

    protected override void OnClosed(EventArgs e)
    {
        _heartbeatTimer?.Stop();
        _heartbeatTimer = null;
        base.OnClosed(e);
    }

    private void OnHeartbeatTick(object? sender, EventArgs e)
    {
        _secondsSinceHeartbeat++;
        if (_secondsSinceHeartbeat > 5)
        {
            D.Write("Closing due to missed heartbeats.");
            Close();
            return;
        }

        TransportClient?.SendMessageToHost(MessageType.HEARTBEAT, "");
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape)
        {
            D.Write("Closing due to [Esc].");
            Close();
        }
    }

    // Avalonia has no built-in "click-through" flag, so on Windows we extend the
    // native HWND styles: WS_EX_LAYERED | WS_EX_TRANSPARENT makes the (still
    // topmost, per-pixel-alpha) window pass mouse input through to whatever sits
    // behind it, while WS_EX_TOOLWINDOW (and clearing WS_EX_APPWINDOW) keeps the
    // overlay out of the taskbar and Alt-Tab, matching the old GameWindow setup.
    private void ApplyNativeOverlayStyles()
    {
        if (!OperatingSystem.IsWindows())
            return;

        try
        {
            IntPtr hwnd = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (hwnd == IntPtr.Zero)
                return;

            long exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
            exStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW;
            exStyle &= ~WS_EX_APPWINDOW;
            SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(exStyle));
        }
        catch
        {
            // Best-effort: click-through is a nicety; failure must never break the overlay.
        }
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
}
