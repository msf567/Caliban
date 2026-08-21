using Caliban.Graphics.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Caliban.Graphics.Drawables;

/// <summary>
/// A solid red circle rendered entirely in the vertex shader as a triangle
/// fan (1 center vertex + 65 perimeter vertices). Its screen position is
/// supplied by the host via <see cref="Draw"/>.
/// </summary>
public sealed class RedCircle : IDrawable
{
    // 1 center vertex + 64 segments + 1 vertex to close the fan.
    private const int VertexCount = 66;

    private Shader? _shader;
    private int _vao;

    private Vector2 _resolution = Vector2.One;

    public void Load()
    {
        _shader = Shader.FromFiles(
            Path.Combine("Shaders", "circle.vert"),
            Path.Combine("Shaders", "circle.frag"));

        _vao = GL.GenVertexArray();
    }

    public void Resize(Vector2 resolution)
    {
        _resolution = resolution;
    }

    public void Update(FrameEventArgs args)
    {
        // The circle is currently static; state updates (animation, growth,
        // ...) would live here so it behaves like a small sub-program.
    }

    public void Draw(float x, float y)
    {
        if (_shader is null)
            throw new InvalidOperationException(
                "Load() must be called before Draw().");

        _shader.Use();
        _shader.SetVector2("uResolution", _resolution);
        _shader.SetVector2("uCenter", new Vector2(x, y));

        GL.BindVertexArray(_vao);

        GL.DrawArrays(
            PrimitiveType.TriangleFan,
            0,
            VertexCount);
    }

    public void Dispose()
    {
        if (_vao != 0)
        {
            GL.DeleteVertexArray(_vao);
            _vao = 0;
        }

        _shader?.Dispose();
        _shader = null;
    }
}