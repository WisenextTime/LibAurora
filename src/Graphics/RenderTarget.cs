using System;
using Veldrid;
namespace LibAurora.Graphics;

/// <summary>
/// Off-screen render target wrapping a color texture, optional depth texture, and framebuffer.
/// The format matches the main swapchain so that pipelines are interchangeable between targets.
/// </summary>
public class RenderTarget : IDisposable
{
	private bool _disposed;

	internal RenderTarget(
		Framebuffer framebuffer,
		Texture colorTexture,
		TextureView colorView,
		Texture? depthTexture,
		uint width,
		uint height)
	{
		Framebuffer = framebuffer;
		ColorTexture = colorTexture;
		ColorView = colorView;
		DepthTexture = depthTexture;
		Width = width;
		Height = height;
	}

	/// <summary>The underlying Veldrid framebuffer.</summary>
	public Framebuffer Framebuffer { get; }

	/// <summary>The color texture backing this render target.</summary>
	public Texture ColorTexture { get; }

	/// <summary>A default texture view over the full color texture.</summary>
	public TextureView ColorView { get; }

	/// <summary>The depth-stencil texture, or null if created without depth.</summary>
	public Texture? DepthTexture { get; }

	/// <summary>Width of the render target in pixels.</summary>
	public uint Width { get; }

	/// <summary>Height of the render target in pixels.</summary>
	public uint Height { get; }

	/// <inheritdoc />
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		Framebuffer.Dispose();
		ColorView.Dispose();
		ColorTexture.Dispose();
		DepthTexture?.Dispose();
	}

	/// <summary>Binds this render target to the command list and sets the full viewport.</summary>
	public void Bind(CommandList cl)
	{
		cl.SetFramebuffer(Framebuffer);
		cl.SetFullViewports();
	}

	/// <summary>Clears the color attachment to the given color.</summary>
	public void Clear(CommandList cl, RgbaFloat color)
	{
		cl.ClearColorTarget(0, color);
	}

	/// <summary>Binds this target and clears it in one call.</summary>
	public void ClearAndBind(CommandList cl, RgbaFloat clearColor)
	{
		Bind(cl);
		Clear(cl, clearColor);
	}
}