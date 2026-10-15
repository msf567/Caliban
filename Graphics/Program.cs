using Avalonia;
using Avalonia.OpenGL;
using Caliban.Core.Debug;

namespace Caliban.Graphics;

internal static class Program
{
    // Avalonia requires an STA entry point.
    [STAThread]
    public static void Main(string[] args)
    {
        // Debug mode is toggled by passing "debug" as a launch parameter,
        // mirroring the rest of Caliban (e.g. CalibanProgram sets
        // D.debugMode = _args.Contains("debug")).
        D.debugMode = args.Contains("debug");

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<GraphicsApplication>()
        .UsePlatformDetect()
        .With(new Win32PlatformOptions
        {
            // Force a *native* desktop-GL (WGL) context instead of Avalonia's
            // default ANGLE/GLES backend: the overlay's shaders are desktop GLSL
            // (#version 330/430 core) and the sandstorm uses a compute shader, so
            // we request an OpenGL 4.3 core profile (falling back to 4.0).
            RenderingMode = new[] { Win32RenderingMode.Wgl },
            WglProfiles = new List<GlVersion>
            {
                new(GlProfileType.OpenGL, 4, 3),
                new(GlProfileType.OpenGL, 4, 0)
            }
        })
        .LogToTrace();
}
