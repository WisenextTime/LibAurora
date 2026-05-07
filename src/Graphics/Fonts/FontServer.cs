using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using FontStashSharp;
using FontStashSharp.RichText;
using LibAurora.Resources;
using Veldrid;

namespace LibAurora.Graphics.Fonts;

/// <summary>
/// Static entry point for font rendering. Manages initialization, font caching,
/// text drawing, and resource lifecycle.
/// </summary>
public static class FontServer
{
	private static readonly Dictionary<int, SpriteFontBase> _fonts = [];
	private static FontSystem? _system;
	private static FontRenderer? _renderer;

	/// <summary>The underlying <see cref="FontSystem"/> instance. Throws if <see cref="Init"/> has not been called.</summary>
	public static FontSystem System =>
		_system ?? throw new InvalidOperationException("FontServer not initialized");

	/// <summary>The underlying <see cref="FontRenderer"/> instance. Throws if <see cref="Init"/> has not been called.</summary>
	public static FontRenderer Renderer =>
		_renderer ?? throw new InvalidOperationException("FontServer not initialized");

	/// <summary>
	/// Initializes the font server with the specified graphics device and optional default font stream.
	/// Must be called before any drawing operations.
	/// </summary>
	/// <param name="graphics">The graphics context to use for rendering.</param>
	/// <param name="fontStream">Optional stream of a default font to load on initialization.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="graphics"/> is null.</exception>
	public static void Init(IGraphics graphics, Stream? fontStream = null)
	{
		ArgumentNullException.ThrowIfNull(graphics);

		var shaders = ResourceManager.LoadResource<Shader[]>("ase://LibAurora.Shaders.Font.glsl");
		_system = new FontSystem(new FontSystemSettings
		{
			TextureWidth = FontAtlas.PageWidth,
			TextureHeight = FontAtlas.PageHeight,
		});
		_renderer = new FontRenderer(graphics, shaders);
		if (fontStream is not null)
			CreateFont(fontStream);
	}

	/// <summary>Begins a font rendering batch. Must be paired with <see cref="End"/>.</summary>
	public static void Begin() => Renderer.Begin();

	/// <summary>Ends the current font rendering batch and submits draw commands to the GPU.</summary>
	public static void End() => Renderer.End();

	/// <summary>Draws a plain text string at the specified position with the given parameters.</summary>
	/// <param name="text">The text string to draw.</param>
	/// <param name="position">Top-left position in screen coordinates.</param>
	/// <param name="fontSize">The font size in points.</param>
	/// <param name="color">Text color.</param>
	/// <param name="rotation">Rotation angle in radians around <paramref name="origin"/>.</param>
	/// <param name="origin">Rotation origin point relative to the text.</param>
	/// <param name="scale">Scale factor applied to the text.</param>
	public static void DrawText(string text, Vector2 position, int fontSize, RgbaFloat color,
		float rotation = 0f, Vector2 origin = default, Vector2 scale = default)
	{
		if (!_fonts.TryGetValue(fontSize, out var font))
		{
			font = GetFont(fontSize);
			_fonts[fontSize] = font;
		}

		font.DrawText(Renderer, text, position, new FSColor(color.ToVector4()), rotation, origin, scale);
	}

	/// <summary>Draws rich text with markup formatting at the specified position.</summary>
	/// <param name="text">The rich text string to draw.</param>
	/// <param name="position">Top-left position in screen coordinates.</param>
	/// <param name="fontSize">The font size in points.</param>
	/// <param name="defaultColor">Default color for text without color markup.</param>
	/// <param name="verticalSpacing">Vertical spacing between lines.</param>
	/// <param name="width">Maximum width of the text area before wrapping.</param>
	/// <param name="height">Maximum height of the text area before ellipsis.</param>
	/// <param name="autoEllipsis">Method for applying ellipsis when text overflows.</param>
	public static void DrawRichText(string text, Vector2 position, int fontSize, RgbaFloat defaultColor,
		int verticalSpacing = 0, int width = int.MaxValue, int height = int.MaxValue,
		AutoEllipsisMethod autoEllipsis = AutoEllipsisMethod.Word)
	{
		if (!_fonts.TryGetValue(fontSize, out var font))
		{
			font = GetFont(fontSize);
			_fonts[fontSize] = font;
		}

		var richText = new RichTextLayout
		{
			Font = font,
			Text = text,
			VerticalSpacing = verticalSpacing,
			Width = width,
			Height = height,
			AutoEllipsisMethod = autoEllipsis
		};
		richText.Draw(Renderer, position, new FSColor(defaultColor.ToVector4()));
	}

	/// <summary>Creates and registers a font from a stream. Returns the created <see cref="Font"/> instance.</summary>
	public static Font CreateFont(Stream fontStream) => new(System, fontStream);

	/// <summary>Adds raw font data to the font system.</summary>
	public static void AddFont(byte[] data) => System.AddFont(data);

	/// <summary>Loads a font file from the specified path and adds it to the font system.</summary>
	public static void AddFont(string path) => AddFont(File.ReadAllBytes(path));

	/// <summary>Gets a sized font instance from the font system. The result is cached per font size.</summary>
	public static SpriteFontBase GetFont(float size) => System.GetFont(size);

	/// <summary>Releases all font resources. After calling this, <see cref="Init"/> must be called again before reuse.</summary>
	public static void Dispose()
	{
		_system?.Dispose();
		_renderer?.Dispose();
		_system = null;
		_renderer = null;
		_fonts.Clear();
	}
}