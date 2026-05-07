using System;
using FontStashSharp.Interfaces;
using Veldrid;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace LibAurora.Graphics.Fonts;

/// <summary>
/// Manages a 4096x4096 texture array atlas for storing font glyphs.
/// Supports up to <see cref="PageCount"/> layers. When all layers are exhausted,
/// a reset is triggered on the next allocation to recycle layers.
/// </summary>
public class FontAtlas : ITexture2DManager, IDisposable
{
	public const int PageWidth = 4096;
	public const int PageHeight = 4096;
	public const int PageCount = 4;
	private readonly Texture _atlas;

	private readonly GraphicsDevice _device;
	private readonly bool[] _layerFree = new bool[PageCount];
	private bool _disposed;
	private bool _resetPending;

	/// <summary>
	/// Creates a new font atlas with the specified graphics context.
	/// </summary>
	/// <param name="graphics">The graphics context used to create GPU resources.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="graphics"/> is null.</exception>
	public FontAtlas(IGraphics graphics)
	{
		ArgumentNullException.ThrowIfNull(graphics);

		_device = graphics.Device;
		var factory = graphics.Factory;

		_atlas = factory.CreateTexture(new TextureDescription(
			PageWidth, PageHeight, 1, PageCount, 1,
			PixelFormat.R8_G8_B8_A8_UNorm,
			TextureUsage.Sampled,
			TextureType.Texture2D));

		View = factory.CreateTextureView(_atlas);

		for (var i = 0; i < PageCount; i++)
			_layerFree[i] = true;
	}

	/// <summary>The texture view that provides shader access to all atlas layers.</summary>
	public TextureView View { get; }

	/// <summary>Releases the atlas texture and view.</summary>
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;

		View.Dispose();
		_atlas.Dispose();
	}

	/// <summary>
	/// Allocates a texture layer in the atlas. If the previous allocation cycle exhausted
	/// all layers, resets them for reuse. Returns null when all layers are currently in use;
	/// FontStashSharp is expected to retry.
	/// </summary>
	/// <param name="width">Requested texture width (currently unused).</param>
	/// <param name="height">Requested texture height (currently unused).</param>
	public object CreateTexture(int width, int height)
	{
		if (_resetPending)
		{
			for (var i = 0; i < PageCount; i++)
				_layerFree[i] = true;
			_resetPending = false;
		}

		for (var i = 0; i < PageCount; i++)
		{
			if (!_layerFree[i]) continue;
			_layerFree[i] = false;
			return new PageHandle { Layer = i, Width = width, Height = height };
		}

		_resetPending = true;
		return null!;
	}

	/// <summary>Gets the size of an allocated texture region in the atlas.</summary>
	public Point GetTextureSize(object texture)
	{
		var h = (PageHandle)texture;
		return new Point(h.Width, h.Height);
	}

	/// <summary>Uploads glyph pixel data to the specified region of an atlas layer.</summary>
	public unsafe void SetTextureData(object texture, Rectangle bounds, byte[] data)
	{
		var h = (PageHandle)texture;
		fixed (byte* ptr = data)
		{
			_device.UpdateTexture(_atlas, (nint)ptr, (uint)data.Length,
				(uint)bounds.X, (uint)bounds.Y, (uint)h.Layer,
				(uint)bounds.Width, (uint)bounds.Height, 1, 0, 0);
		}
	}
}
internal struct PageHandle
{
	public int Layer;
	public int Width;
	public int Height;
}