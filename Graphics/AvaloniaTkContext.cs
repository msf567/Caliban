using Avalonia.OpenGL;
using OpenTK;

namespace Caliban.Graphics;

/// <summary>
/// Bridges OpenTK's GL bindings onto Avalonia's managed GL context.
///
/// Avalonia exposes the current context's function pointers via
/// <see cref="GlInterface.GetProcAddress"/>; OpenTK's <c>GL.LoadBindings</c>
/// consumes any <see cref="IBindingsContext"/>, so this thin wrapper lets us keep
/// programming against OpenTK's <c>GL.*</c> API while Avalonia owns the context.
/// </summary>
internal sealed class AvaloniaTkContext(GlInterface gl) : IBindingsContext
{
    public nint GetProcAddress(string procName) => gl.GetProcAddress(procName);
}
