using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Caliban.Core.World;
using Caliban.Graphics.Drawables;
using Caliban.Graphics.Transport;

namespace Caliban.Graphics;

/// <summary>
/// The Avalonia application that replaces the former OpenTK <c>GameWindow</c>
/// host. It owns the classic-desktop lifetime, shows the single transparent
/// overlay window, spawns the drawables into it, and starts the transport client
/// that drives the overlay from the rest of Caliban.
/// </summary>
public sealed class GraphicsApplication : Application
{
    // Held for the lifetime of the app so the socket client is not collected.
    private AppTransportClient? _transport;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            OverlayWindow window = new();
            desktop.MainWindow = window;

            // Spawn the same drawables the old App.Run() set up. They are queued
            // and loaded on the GL render thread once the context is current.
            window.Spawn("sandstorm", new SandStorm());
            window.Spawn("rock1", new Rock(1, Biome.DESERT));

            // Start the socket client; it wires itself to the window on connect
            // and forwards host messages into the overlay.
            _transport = new AppTransportClient(window);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
