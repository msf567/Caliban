using System.Drawing;
using Caliban.Core.World;
using Caliban.Graphics.Rendering;
using Caliban.Graphics.Utility;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Caliban.Graphics.Drawables;

public sealed class Rock : IDrawable
{
    private const int MaxInnerPoints = 8;
    private const int MaxOuterPoints = 12;

    private readonly Random _random;

    private Shader? _shader;
    private int _vao;

    private Vector2 _resolution = Vector2.One;

    private Vector2[] _innerPoints = [];
    private Vector2[] _outerPoints = [];

    // Three colors for every inner Voronoi cell.
    private Vector3[] _cellColors = [];

    private float _radius = 200.0f;

    private readonly int _seed;
    private readonly Biome _biome;

    public Rock(int seed, Biome biome)
    {
        _seed = seed;
        _biome = biome;

        _random = new Random(seed);
    }

    public void Load()
    {
        _shader = Shader.FromFiles(
            Path.Combine("Shaders", "rock.vert"),
            Path.Combine("Shaders", "rock.frag"));

        _vao = GL.GenVertexArray();

        GenerateRock();
    }

    private void GenerateRock()
    {
        _innerPoints = GenerateInnerPoints();
        _outerPoints = GenerateOuterPoints();

        GenerateCellPalettes();
    }

    private Vector2[] GenerateInnerPoints()
    {
        var points = new List<Vector2>();

        // Central site.
        points.Add(new Vector2(
            RandomRange(-0.15f, 0.15f),
            RandomRange(-0.15f, 0.15f)));

        while (points.Count < MaxInnerPoints)
        {
            var point = new Vector2(
                RandomRange(-1.0f, 1.0f),
                RandomRange(-1.0f, 1.0f));

            // Keep sites inside the rough rock area.
            if (point.X * point.X + point.Y * point.Y > 0.75f)
                continue;

            // Keep sites separated.
            bool tooClose = false;

            foreach (var existing in points)
            {
                if ((point - existing).LengthSquared < 0.035f)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
                points.Add(point);
        }

        return points.ToArray();
    }

    private Vector2[] GenerateOuterPoints()
    {
        var points = new Vector2[MaxOuterPoints];

        for (int i = 0; i < MaxOuterPoints; i++)
        {
            float t = i / (float)MaxOuterPoints;
            float angle = t * MathF.Tau;

            float radius = RandomRange(1.05f, 1.30f);

            float x = MathF.Cos(angle) * radius;
            float y = MathF.Sin(angle) * radius;

            x *= 1.15f;
            y *= 0.85f;

            points[i] = new Vector2(x, y);
        }

        return points;
    }

    private void GenerateCellPalettes()
    {
        Vector3[]? palette = PaletteManager.GetPalette("Rock", Biome.DESERT)?.Colors;

        _cellColors = new Vector3[_innerPoints.Length * 3];

        if (palette == null || palette.Length < 3)
            return;

        for (int cell = 0; cell < _innerPoints.Length; cell++)
        {
            // Pick a random starting index that leaves room for 3 colors.
            int start = _random.Next(0, palette.Length - 2);

            int offset = cell * 3;

            _cellColors[offset + 0] = palette[start + 0];
            _cellColors[offset + 1] = palette[start + 1];
            _cellColors[offset + 2] = palette[start + 2];
        }
    }

    private float RandomRange(float min, float max)
    {
        return min + (float)_random.NextDouble() * (max - min);
    }

    public void Resize(Vector2 resolution)
    {
        _resolution = resolution;
    }

    public void Update(FrameEventArgs args)
    {
    }

    public void Draw(float x, float y)
    {
        if (_shader is null)
        {
            throw new InvalidOperationException(
                "Load() must be called before Draw().");
        }

        _shader.Use();

        _shader.SetVector2(
            "uResolution",
            _resolution);

        _shader.SetVector2(
            "uCenter",
            new Vector2(x, y));

        _shader.SetFloat(
            "uRadius",
            _radius);

        _shader.SetInt(
            "uInnerPointCount",
            _innerPoints.Length);

        _shader.SetInt(
            "uOuterPointCount",
            _outerPoints.Length);

        // -----------------------------------------------------
        // Voronoi points
        // -----------------------------------------------------

        for (int i = 0; i < _innerPoints.Length; i++)
        {
            _shader.SetVector2(
                $"uInnerPoints[{i}]",
                _innerPoints[i]);
        }

        for (int i = 0; i < _outerPoints.Length; i++)
        {
            _shader.SetVector2(
                $"uOuterPoints[{i}]",
                _outerPoints[i]);
        }

        // -----------------------------------------------------
        // Cell palettes
        //
        // Three Vector3s per cell.
        // -----------------------------------------------------

        for (int i = 0; i < _cellColors.Length; i++)
        {
            _shader.SetVector3(
                $"uCellColors[{i}]",
                _cellColors[i]);
        }

        GL.BindVertexArray(_vao);

        GL.DrawArrays(
            PrimitiveType.TriangleStrip,
            0,
            4);
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