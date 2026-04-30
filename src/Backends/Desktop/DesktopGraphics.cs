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
	public void BeginFrame()
	{
		if (_isRendering) throw new InvalidOperationException("Rendering already begun.");
		_isRendering = true;
		CommandList.Begin();
		if (Device.SwapchainFramebuffer == null) return;
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