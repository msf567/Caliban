using System.Collections.Concurrent;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Caliban.Core.Debug;
using Caliban.Graphics.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Caliban.Graphics;

/// <summary>
/// The in-process OpenGL render surface, hosted inside the transparent overlay
/// window. It binds OpenTK's <c>GL.*</c> API on top of Avalonia's GL context and
/// acts as a lightweight host for <see cref="IDrawable"/> instances: each frame
/// it clears the transparent framebuffer and drives the load/update/draw
/// lifecycle of every spawned drawable.
///
/// This replaces the former OpenTK <c>GameWindow</c>. Because Avalonia composites
/// this control's framebuffer through DWM (rather than presenting a native
/// fullscreen GL swapchain), the overlay is never promoted to the GPU
/// independent-flip / MPO present path that was bypassing DWM alpha compositing
/// and rendering the overlay opaque on Windows 11 hybrid-GPU laptops.
/// </summary>
internal sealed class OverlayGlControl : OpenGlControlBase
{
    // Spawned from any thread (startup / transport), but the actual GL setup must
    // happen on the render thread where the context is current, so requests are
    // queued and drained in OnOpenGlRender.
    private readonly ConcurrentQueue<(string id, IDrawable drawable)> _spawnRequests = new();
    private readonly Dictionary<string, IDrawable> _gameObjects = new();
    private readonly Stopwatch _clock = Stopwatch.StartNew();

    private double _lastSeconds;
    private Vector2i _lastSize = new(-1, -1);

    /// <summary>
    /// Thread-safe: queues a drawable to be loaded and driven on the render
    /// thread. A duplicate id is ignored (first spawn wins), mirroring the old
    /// <c>Dictionary.TryAdd</c> behaviour.
    /// </summary>
    public void Spawn(string id, IDrawable drawable) => _spawnRequests.Enqueue((id, drawable));

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        // Bind OpenTK's GL entry points to Avalonia's current context. Must happen
        // here, where the context is guaranteed current.
        GL.LoadBindings(new AvaloniaTkContext(gl));

        D.Write($"OpenGL: {GL.GetString(StringName.Version)}");
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        (int width, int height) = GetPixelSize();
        Vector2i size = new(width, height);

        GL.Viewport(0, 0, width, height);

        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);

        // The drawables' fragment shaders emit *straight* (non-premultiplied)
        // alpha. Avalonia composites this control's framebuffer as PREmultiplied,
        // so blend the colour channels with SrcAlpha/OneMinusSrcAlpha (producing
        // premultiplied rgb from a cleared-to-zero surface) while accumulating the
        // alpha channel with One/OneMinusSrcAlpha. This yields a correctly
        // premultiplied result without touching any shader.
        GL.BlendFuncSeparate(
            BlendingFactorSrc.SrcAlpha,
            BlendingFactorDest.OneMinusSrcAlpha,
            BlendingFactorSrc.One,
            BlendingFactorDest.OneMinusSrcAlpha);

        GL.ClearColor(0f, 0f, 0f, 0f);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        // Load any drawables requested since the last frame now that the context
        // is current.
        ProcessSpawnRequests(size);

        // Propagate size changes to every drawable.
        if (size != _lastSize)
        {
            Vector2 resolution = new(width, height);
            foreach (IDrawable drawable in _gameObjects.Values)
                drawable.Resize(resolution);

            _lastSize = size;
        }

        double now = _clock.Elapsed.TotalSeconds;
        double dt = now - _lastSeconds;
        _lastSeconds = now;
        FrameEventArgs frame = new(dt);

        float centerY = height * 0.5f;
        int x = 0;
        foreach (IDrawable drawable in _gameObjects.Values)
        {
            drawable.Update(frame);
            drawable.Draw(x, centerY);
            x += 300;
        }

        // The control is not a free-running loop; ask for the next frame so the
        // overlay keeps rendering continuously (~display refresh).
        RequestNextFrameRendering();
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        foreach (IDrawable drawable in _gameObjects.Values)
        {
            try { drawable.Dispose(); }
            catch (Exception ex) { D.Write("[OverlayGlControl] Drawable dispose failed: " + ex.Message); }
        }

        _gameObjects.Clear();
        base.OnOpenGlDeinit(gl);
    }

    private void ProcessSpawnRequests(Vector2i size)
    {
        while (_spawnRequests.TryDequeue(out (string id, IDrawable drawable) request))
        {
            if (!_gameObjects.TryAdd(request.id, request.drawable))
                continue;

            try
            {
                request.drawable.Load();
                request.drawable.Resize(new Vector2(size.X, size.Y));
            }
            catch (Exception ex)
            {
                _gameObjects.Remove(request.id);
                D.Write($"[OverlayGlControl] Failed to spawn '{request.id}': {ex.Message}");
            }
        }
    }

    // Computes the framebuffer size in physical pixels from the control's bounds
    // and render scaling, guarding against a zero-sized viewport.
    private (int width, int height) GetPixelSize()
    {
        double scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
        int width = (int)(Bounds.Width * scaling);
        int height = (int)(Bounds.Height * scaling);
        return (width < 1 ? 1 : width, height < 1 ? 1 : height);
    }
}
