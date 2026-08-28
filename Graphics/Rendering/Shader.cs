using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Caliban.Graphics.Rendering;

/// <summary>
/// Compiles and links a GLSL program from separate vertex and fragment
/// shader files and exposes small helpers for setting uniforms.
/// </summary>
public sealed class Shader : IDisposable
{
    private readonly Dictionary<string, int> _uniformCache = new();

    public int Handle { get; }

    private Shader(int handle)
    {
        Handle = handle;
    }

    /// <summary>
    /// Loads the two shader files, compiles them and links the program.
    /// Paths are resolved relative to the application base directory so
    /// they keep working regardless of the current working directory.
    /// </summary>
    public static Shader FromFiles(string vertexPath, string fragmentPath)
    {
        string vertexSource = ReadShaderFile(vertexPath);
        string fragmentSource = ReadShaderFile(fragmentPath);

        int vertexShader = CompileShader(
            ShaderType.VertexShader,
            vertexSource);

        int fragmentShader = CompileShader(
            ShaderType.FragmentShader,
            fragmentSource);

        int program = GL.CreateProgram();

        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);

        GL.LinkProgram(program);

        CheckProgram(program);

        GL.DetachShader(program, vertexShader);
        GL.DetachShader(program, fragmentShader);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        return new Shader(program);
    }

    /// <summary>
    /// Loads, compiles and links a compute-only program from a single
    /// compute shader file. Used to run the sandstorm particle simulation
    /// entirely on the GPU.
    /// </summary>
    public static Shader FromComputeFile(string computePath)
    {
        string computeSource = ReadShaderFile(computePath);

        int computeShader = CompileShader(
            ShaderType.ComputeShader,
            computeSource);

        int program = GL.CreateProgram();

        GL.AttachShader(program, computeShader);

        GL.LinkProgram(program);

        CheckProgram(program);

        GL.DetachShader(program, computeShader);

        GL.DeleteShader(computeShader);

        return new Shader(program);
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    public void SetVector2(string name, Vector2 value)
    {
        GL.Uniform2(GetUniformLocation(name), value.X, value.Y);
    }

    public void SetVector3(string name, Vector3 value)
    {
        GL.Uniform3(GetUniformLocation(name), value.X, value.Y, value.Z);
    }

    public void SetFloat(string name, float value)
    {
        GL.Uniform1(GetUniformLocation(name), value);
    }

    public void SetInt(string name, int value)
    {
        GL.Uniform1(GetUniformLocation(name), value);
    }

    private int GetUniformLocation(string name)
    {
        if (_uniformCache.TryGetValue(name, out int location))
            return location;

        location = GL.GetUniformLocation(Handle, name);
        _uniformCache[name] = location;

        return location;
    }

    private static string ReadShaderFile(string path)
    {
        string fullPath = Path.IsPathRooted(path)
            ? path
            : Path.Combine(AppContext.BaseDirectory, path);

        return File.ReadAllText(fullPath);
    }

    private static int CompileShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);

        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(
            shader,
            ShaderParameter.CompileStatus,
            out int success);

        if (success == 0)
            throw new Exception(GL.GetShaderInfoLog(shader));

        return shader;
    }

    private static void CheckProgram(int program)
    {
        GL.GetProgram(
            program,
            GetProgramParameterName.LinkStatus,
            out int success);

        if (success == 0)
            throw new Exception(GL.GetProgramInfoLog(program));
    }

    public void Dispose()
    {
        GL.DeleteProgram(Handle);
    }
}