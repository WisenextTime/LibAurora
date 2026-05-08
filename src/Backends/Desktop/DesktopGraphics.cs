using System;
using LibAurora.Debug;
using LibAurora.Graphics;
using LibAurora.Utils;
using Veldrid;
using Veldrid.Sdl2;
namespace LibAurora.Backends.Desktop;

/// <summary>
/// Desktop graphics implementation using Veldrid.
/// Auto-detects the best available graphics backend and manages the device, swapchain, and command list.
/// </summary>
public class DesktopGraphics : IGraphics
{
	private readonly GraphicsBackend[] _backends =
	[
		GraphicsBackend.Metal,
		GraphicsBackend.Direct3D11,
		GraphicsBackend.Vulkan,
		GraphicsBackend.OpenGL,
		GraphicsBackend.OpenGLES,
	];

	private CommandList? _commandList;
	private GraphicsDevice? _device;
	private bool _isRendering;

	private Sdl2Window? _window;

	/// <summary>The detected graphics backend, or null if none available.</summary>
	public GraphicsBackend? Backend;

	/// <summary>Creates a new instance and detects the best backend.</summary>
	public DesktopGraphics()
	{
		Backend = BackendCreateUtils.TestValidBackend(_backends);
		LogServer.Log($"Valid backend: {Backend}", LogLevel.Debug);
	}

	/// <inheritdoc />
	public uint ViewportWidth => (uint)(_window?.Width ?? 0);

	/// <inheritdoc />
	public uint ViewportHeight => (uint)(_window?.Height ?? 0);

	/// <inheritdoc />
	public GraphicsDevice Device =>
		_device ?? throw new InvalidOperationException("The device is not initialized.");

	/// <inheritdoc />
	public CommandList CommandList
		=> _commandList ?? throw new InvalidOperationException("The command list is not initialized.");

	/// <inheritdoc />
	public ResourceFactory Factory => Device.ResourceFactory;

	/// <inheritdoc />
	public PixelFormat? SwapchainColorFormat
	{
		get
		{
			var fb = _device?.SwapchainFramebuffer;
			var attachments = fb?.OutputDescription.ColorAttachments;
			return attachments is { Length: > 0 } ? attachments[0].Format : null;
		}
	}

	/// <inheritdoc />
	public PixelFormat? SwapchainDepthFormat
		=> _device?.SwapchainFramebuffer?.OutputDescription.DepthAttachment?.Format;

	/// <inheritdoc />
	public void BeginFrame(bool bindDefaultSwapchain = true)
	{
		if (_isRendering) throw new InvalidOperationException("Rendering already begun.");
		_isRendering = true;
		CommandList.Begin();
		if (!bindDefaultSwapchain) return;
		CommandList.SetFramebuffer(Device.SwapchainFramebuffer);
		CommandList.ClearColorTarget(0, RgbaFloat.Black);
	}

	/// <inheritdoc />
	public void EndFrame()
	{
		if (!_isRendering) throw new InvalidOperationException("Rendering not begun.");
		_isRendering = false;
		CommandList.End();
		Device.SubmitCommands(CommandList);
		Device.SwapBuffers();
	}

	/// <inheritdoc />
	public RenderTarget CreateRenderTarget(uint width, uint height, bool withDepth = false)
	{
		var colorFormat = SwapchainColorFormat ?? PixelFormat.B8_G8_R8_A8_UNorm_SRgb;
		PixelFormat? depthFormat = withDepth ? SwapchainDepthFormat ?? PixelFormat.D24_UNorm_S8_UInt : null;

		var colorDesc = TextureDescription.Texture2D(width, height, 1, 1, colorFormat,
			TextureUsage.RenderTarget | TextureUsage.Sampled);
		var colorTexture = Factory.CreateTexture(colorDesc);
		var colorView = Factory.CreateTextureView(colorTexture);

		Texture? depthTexture = null;
		if (depthFormat.HasValue)
		{
			var depthDesc = TextureDescription.Texture2D(width, height, 1, 1, depthFormat.Value,
				TextureUsage.DepthStencil);
			depthTexture = Factory.CreateTexture(depthDesc);
		}

		var fb = Factory.CreateFramebuffer(new FramebufferDescription(depthTexture, colorTexture));

		return new RenderTarget(fb, colorTexture, colorView, depthTexture, width, height);
	}

	/// <summary>Initializes the graphics device and command list for the given window.</summary>
	public void Initialize(Sdl2Window window)
	{
		InitBackend(window);
		_window = window;
		_commandList = Factory.CreateCommandList();
	}

	private void InitBackend(Sdl2Window window)
	{
		var option = new GraphicsDeviceOptions
		{
			PreferStandardClipSpaceYDirection = true,
			PreferDepthRangeZeroToOne = true,
			SwapchainDepthFormat = PixelFormat.D24_UNorm_S8_UInt,
			SyncToVerticalBlank = false,
		};
		var swapchainSource = BackendCreateUtils.GetSwapchainSource(window.SdlWindowHandle);
		_device = BackendCreateUtils.CreateGraphicDevice(Backend, window, swapchainSource, option);
	}
}