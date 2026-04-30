using System.Numerics;
using Veldrid;
namespace LibAurora.Graphics;

/// <summary>
/// Abstract base for all renderers. Wraps an <see cref="IGraphics"/> instance
/// and provides convenient access to the Veldrid device, command list, and factory.
/// Subclasses implement <see cref="Begin"/> and <see cref="End"/> to record draw commands.
/// </summary>
public abstract class Renderer(IGraphics graphics)
{
	/// <summary>The Veldrid graphics device.</summary>
	public GraphicsDevice GraphicsDevice => graphics.Device;

	/// <summary>The command list for recording draw commands.</summary>
	public CommandList CommandList => graphics.CommandList;

	/// <summary>The resource factory for creating GPU resources.</summary>
	public ResourceFactory Factory => graphics.Factory;

	/// <summary>The current window size as a <see cref="Vector2"/>.</summary>
	public Vector2 WindowSize => new(graphics.ViewportWidth, graphics.ViewportHeight);

	/// <summary>Called at the start of a render pass to set up state.</summary>
	public abstract void Begin();

	/// <summary>Called at the end of a render pass to finalize and submit commands.</summary>
	public abstract void End();
}