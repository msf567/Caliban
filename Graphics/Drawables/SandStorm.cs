using Caliban.Graphics.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System.Runtime.InteropServices;

namespace Caliban.Graphics.Drawables;

public sealed class SandStorm : IDrawable
{
    private const int MaxParticles = 10000;

    /// <summary>x, y, speed, drift, phase, size, alpha, pad.</summary>
    private const int FloatsPerParticle = 8;

    /// <summary>Compute-shader work-group size (must match sandstorm.comp).</summary>
    private const int LocalSize = 64;

    /// <summary>
    /// Time constant (seconds) controlling how smoothly the wind direction
    /// eases toward its target. Larger values give slower, gentler wind
    /// changes that ramp up and down instead of snapping.
    /// </summary>
    private const float WindDirectionSmoothTime = 20f;

    private Shader? _shader;
    private Shader? _computeShader;

    private int _vao;

    /// <summary>
    /// Single GPU buffer that holds all particle state. It is bound both as a
    /// shader-storage buffer (for the compute shader that simulates the
    /// grains) and as an array buffer (for the vertex shader that draws them).
    /// </summary>
    private int _particleBuffer;

    private Vector2 _resolution = new(1f, 1f);

    private readonly Random _random = new();

    private bool _spawned;

    /// <summary>How hard the storm can pull the cursor per second.</summary>
    public float MaxStrength { get; set; } = 600f;

    /// <summary>Base grain speed (pixels/second) at full strength.</summary>
    public float WindSpeed { get; set; } = 3500f;

    /// <summary>Strength the storm eases towards while it is active.</summary>
    private float _targetStrength;

    /// <summary>Current storm strength in the 0..1 range.</summary>
    private float _strength;

    /// <summary>Wind direction; only the X component is used (blows left/right).</summary>
    private Vector2 _direction = new(1f, 0f);

    /// <summary>Accumulator of sub-pixel cursor movement.</summary>
    private float _buildUp;

    /// <summary>Seconds left to ignore the storm (e.g. after a user click).</summary>
    private float _holdOn;

    /// <summary>Total elapsed time (used for gentle grain turbulence).</summary>
    private float _time;

    /// <summary>A single wind-borne sand grain (matches the SSBO layout).</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct Particle
    {
        public float X; // position in pixels
        public float Y;
        public float Speed; // per-grain horizontal speed multiplier
        public float Drift; // steady vertical drift (pixels/second)
        public float Phase; // phase offset for the turbulence wobble
        public float Size; // point size in pixels
        public float Alpha; // opacity
        public float Pad; // padding to keep an 8-float (32-byte) stride
    }

    public SandStorm()
    {
        // The storm stays dormant until the host triggers it: the transport
        // layer (see SandStormClient) calls Begin() when a SANDSTORM_START
        // message arrives, mirroring how the game drives the other clients.
    }

    /// <summary>Starts (or restarts) the storm building towards full power.</summary>
    public void Begin()
    {
        _targetStrength = 1f;
    }

    /// <summary>Eases the storm back down to nothing.</summary>
    public void End()
    {
        _targetStrength = 0f;
    }

    /// <summary>
    /// Pauses the cursor dragging for a short moment, mirroring the Unity
    /// <c>GlobalMouseDown</c> behaviour so a genuine user click is not fought.
    /// </summary>
    public void OnMouseDown()
    {
        _holdOn = 0.25f;
    }

    public void Load()
    {
        _shader = Shader.FromFiles(
            Path.Combine("Shaders", "sandstorm.vert"),
            Path.Combine("Shaders", "sandstorm.frag"));

        _computeShader = Shader.FromComputeFile(
            Path.Combine("Shaders", "sandstorm.comp"));

        _vao = GL.GenVertexArray();
        _particleBuffer = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _particleBuffer);
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            MaxParticles * FloatsPerParticle * sizeof(float),
            IntPtr.Zero,
            BufferUsageHint.DynamicDraw);

        int stride = FloatsPerParticle * sizeof(float);

        // location 0: vec2 position (offset 0)
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);

        // location 1: float size (offset 5 floats)
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // location 2: float alpha (offset 6 floats)
        GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        GL.BindVertexArray(0);

        // Let the vertex shader control the point size via gl_PointSize.
        GL.Enable(EnableCap.ProgramPointSize);
    }

    public void Resize(Vector2 resolution)
    {
        _resolution = resolution;

        // Seed the buffer with grains the first time we know the surface size.
        // This is the only time particle data is uploaded from the CPU; from
        // then on the compute shader owns it entirely.
        if (!_spawned)
        {
            Particle[] particles = new Particle[MaxParticles];
            for (int i = 0; i < MaxParticles; i++)
                SpawnAnywhere(ref particles[i]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _particleBuffer);
            GL.BufferSubData(
                BufferTarget.ArrayBuffer,
                IntPtr.Zero,
                MaxParticles * FloatsPerParticle * sizeof(float),
                particles);

            _spawned = true;
        }
    }

    public void Update(FrameEventArgs args)
    {
        float dt = (float)args.Time;

        _time += dt;

        // While held, the storm keeps blowing visually but does not fight the
        // cursor.
        if (_holdOn > 0f)
            _holdOn -= dt;

        // Let the wind direction wander slowly and continuously using
        // Perlin-style noise. Instead of taking the hard sign of the noise
        // (which snapped the wind between full-left and full-right), we map it
        // to a smooth -1..1 target. The (noise - 0.5) * 4 mapping keeps the
        // wind saturated near ±1 most of the time – so the storm still feels
        // strong – yet lets it pass gently through zero when it reverses.
        float noise = PerlinNoise(_time / 20f, 6f);
        float targetDirection = Math.Clamp((noise - 0.5f) * 4f, -1f, 1f);

        // Ease the wind towards that target with a frame-rate independent
        // exponential smooth, so it ramps up and down at the same gentle pace
        // no matter how fast we are rendering.
        float windEase = 1f - MathF.Exp(-dt / WindDirectionSmoothTime);
        _direction.X += (targetDirection - _direction.X) * windEase;

        // Ease the current strength towards the target.
        _strength += (_targetStrength - _strength) * MathF.Min(1f, dt * 0.5f);

        // Drag the OS cursor sideways in whichever direction the wind blows.
        // The pull now scales with how strongly the wind is currently blowing
        // (|wind|), so it eases off smoothly as the wind slackens or reverses
        // instead of yanking at a constant rate right up until it flips.
        if (_holdOn <= 0f)
        {
            float wind = -_direction.X;
            _buildUp += _strength * MaxStrength * MathF.Abs(wind) * dt;

            int step = Math.Sign(wind);
            if (step != 0)
            {
                while (_buildUp > 1f)
                {
                    _buildUp -= 1f;
                    NudgeCursor(step, 0);
                }
            }
        }

        SimulateParticlesOnGpu(dt);
    }

    public void Draw(float x, float y)
    {
        if (_shader is null)
            throw new InvalidOperationException(
                "Load() must be called before Draw().");

        // Number of grains on screen scales with the storm strength, exactly
        // like Unity's rateOverTime = Strength * MaxParticles.
        int count = (int)(Math.Clamp(_strength, 0f, 1f) * MaxParticles);
        if (count <= 0)
            return;

        _shader.Use();
        _shader.SetVector2("uResolution", _resolution);

        // The buffer already holds up-to-date positions written by the compute
        // shader, so we just draw straight from it – no CPU upload needed.
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Points, 0, count);
    }

    public void Dispose()
    {
        if (_particleBuffer != 0)
        {
            GL.DeleteBuffer(_particleBuffer);
            _particleBuffer = 0;
        }

        if (_vao != 0)
        {
            GL.DeleteVertexArray(_vao);
            _vao = 0;
        }

        _shader?.Dispose();
        _shader = null;

        _computeShader?.Dispose();
        _computeShader = null;
    }

    /// <summary>
    /// Dispatches the compute shader that advances every sand grain on the
    /// GPU, replacing the old per-grain CPU for-loop. The grains blow the
    /// same way the cursor is dragged so the effect feels coherent.
    /// </summary>
    private void SimulateParticlesOnGpu(float dt)
    {
        if (_computeShader is null)
            return;

        // Pass the continuous wind value (not just its sign) to the GPU so the
        // grains smoothly slow, stop and reverse as the wind eases through
        // zero, instead of instantly flipping horizontal direction.
        float windDirX = -_direction.X;

        _computeShader.Use();
        _computeShader.SetFloat("uDt", dt);
        _computeShader.SetFloat("uTime", _time);
        _computeShader.SetFloat("uStrength", _strength);
        _computeShader.SetFloat("uWindDirX", windDirX);
        _computeShader.SetFloat("uWindSpeed", WindSpeed);
        _computeShader.SetVector2("uResolution", _resolution);
        _computeShader.SetInt("uCount", MaxParticles);

        // Expose the particle buffer to the compute shader at binding 0.
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, _particleBuffer);

        int groups = (MaxParticles + LocalSize - 1) / LocalSize;
        GL.DispatchCompute(groups, 1, 1);

        // Make the compute writes visible to the subsequent vertex fetch.
        GL.MemoryBarrier(
            MemoryBarrierFlags.VertexAttribArrayBarrierBit |
            MemoryBarrierFlags.ShaderStorageBarrierBit);
    }

    private void SpawnAnywhere(ref Particle p)
    {
        p.X = RandomRange(0f, _resolution.X);
        p.Y = RandomRange(0f, _resolution.Y);
        Randomize(ref p);
    }

    private void Randomize(ref Particle p)
    {
        p.Speed = RandomRange(0.6f, 1.5f);
        p.Drift = RandomRange(-25f, 25f);
        p.Phase = RandomRange(0f, MathF.Tau);
        p.Size = RandomRange(1.5f, 3.5f);
        p.Alpha = RandomRange(0.35f, 0.9f);
        p.Pad = 0f;
    }

    private float RandomRange(float min, float max)
    {
        return min + (float)_random.NextDouble() * (max - min);
    }

    /// <summary>
    /// Moves the operating-system mouse cursor by the given pixel delta.
    /// This is the Graphics equivalent of Unity's
    /// <c>Cursor.Position = ...</c> nudge and is intentionally kept inside
    /// this class so the sandstorm fully owns its input behaviour.
    /// </summary>
    private static void NudgeCursor(int dx, int dy)
    {
        if (GetCursorPos(out POINT p))
            SetCursorPos(p.X + dx, p.Y + dy);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos(int x, int y);


    /// <summary>
    /// Small Perlin-style value noise in the 0..1 range, standing in for
    /// Unity's <c>Mathf.PerlinNoise</c> so the wind wanders smoothly.
    /// </summary>
    private static float PerlinNoise(float x, float y)
    {
        float xi = MathF.Floor(x);
        float yi = MathF.Floor(y);

        float xf = x - xi;
        float yf = y - yi;

        float u = xf * xf * (3f - 2f * xf);
        float v = yf * yf * (3f - 2f * yf);

        float a = Hash(xi, yi);
        float b = Hash(xi + 1f, yi);
        float c = Hash(xi, yi + 1f);
        float d = Hash(xi + 1f, yi + 1f);

        float ab = a + (b - a) * u;
        float cd = c + (d - c) * u;

        return ab + (cd - ab) * v;
    }

    private static float Hash(float x, float y)
    {
        float n = MathF.Sin(x * 127.1f + y * 311.7f) * 43758.5453f;
        return n - MathF.Floor(n);
    }
}