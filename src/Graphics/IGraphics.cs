using System;
using Veldrid;
namespace LibAurora.Graphics;

/// <summary>
/// Abstraction over the graphics device and frame lifecycle.
/// Provides access to the underlying Veldrid device, command list, and resource factory.
/// </summary>
public interface IGraphics
{
	/// <summary>The current viewport width in pixels.</summary>
	public uint ViewportWidth { get; }

	/// <summary>The current viewport height in pixels.</summary>
	public uint ViewportHeight { get; }

	/// <summary>The Veldrid graphics device.</summary>
	public GraphicsDevice Device { get; }

	/// <summary>The command list used to record draw commands for the current frame.</summary>
	public CommandList CommandList { get; }

	/// <summary>The resource factory for creating GPU resources.</summary>
	public ResourceFactory Factory { get; }

	/// <summary>Begins a new frame. Must be called before any draw commands.</summary>
	public void BeginFrame();

	/// <summary>Ends the current frame and presents it to the screen.</summary>
	public void EndFrame();

	/// <summary>Creates a renderer of type <typeparamref name="T"/> using the given factory function.</summary>
	public T CreateRenderer<T>(Func<IGraphics, T> factory) where T : IRenderer => factory(this);
}