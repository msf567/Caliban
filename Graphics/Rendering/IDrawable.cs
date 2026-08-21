using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Caliban.Graphics.Rendering;

/// <summary>
/// A self-contained visual "subprogram" (for example, a red circle or a
/// sandstorm) that owns its own GPU resources, advances its own state on
/// <see cref="Update"/> and renders itself on <see cref="Draw"/>.
///
/// The host window keeps a collection of these and drives them each frame,
/// so new effects can be spawned simply by adding another implementation.
/// </summary>
public interface IDrawable : IDisposable
{
    /// <summary>
    /// Creates the GPU resources (shaders, buffers, ...) for this drawable.
    /// Called once, after a valid OpenGL context is available.
    /// </summary>
    void Load();

    /// <summary>
    /// Notifies the drawable that the render surface size changed so it can
    /// keep pixel-space calculations correct.
    /// </summary>
    void Resize(Vector2 resolution);

    /// <summary>
    /// Advances the drawable's internal state (animation, physics, ...).
    /// Kept synchronous and inline for now; may run async in the future.
    /// </summary>
    void Update(FrameEventArgs args);

    /// <summary>
    /// Draws the drawable at the given pixel position (top-left origin).
    /// </summary>
    void Draw(float x, float y);
}