using System.Numerics;
using Veldrid;
namespace LibAurora.Graphics;

/// <summary>
/// Abstract base for all renderers. Wraps an <see cref="IGraphics"/> instance
/// and provides convenient access to the Veldrid device, command list, and factory.
/// Subclasses implement <see cref="Begin"/> and <see cref="End"/> to record draw commands.
/// </summary>
public interface IRenderer
{
	/// <summary>The Veldrid graphics device.</summary>
	public GraphicsDevice GraphicsDevice { get; }

	/// <summary>The command list for recording draw commands.</summary>
	public CommandList CommandList { get; }

	/// <summary>The resource factory for creating GPU resources.</summary>
	public ResourceFactory Factory { get; }

	/// <summary>The current window size as a <see cref="Vector2"/>.</summary>
	public Vector2 WindowSize { get; }

	/// <summary>Called at the start of a render pass to set up state.</summary>
	public void Begin();

	/// <summary>Called at the end of a render pass to finalize and submit commands.</summary>
	public void End();
}