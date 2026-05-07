using System;
using System.IO;
using FontStashSharp;

namespace LibAurora.Graphics.Fonts;

/// <summary>
/// Represents a loaded font face. Supports loading from raw bytes, a file path, or a stream.
/// The underlying <see cref="FontSystem"/> is shared and not disposed by this instance.
/// </summary>
public class Font : IDisposable
{
	private readonly FontSystem _system;
	private bool _disposed;

	/// <summary>
	/// Creates a font from raw byte data and registers it with the specified font system.
	/// </summary>
	/// <param name="system">The shared <see cref="FontSystem"/> to register the font with.</param>
	/// <param name="data">Raw font file data (e.g., TTF/OTF bytes).</param>
	public Font(FontSystem system, byte[] data)
	{
		_system = system;
		system.AddFont(data);
	}

	/// <summary>Loads a font from the specified file path.</summary>
	public Font(FontSystem system, string path) : this(system, File.ReadAllBytes(path))
	{
	}

	/// <summary>Loads a font from a stream by reading all bytes into memory first.</summary>
	public Font(FontSystem system, Stream stream) : this(system, ReadAllBytes(stream))
	{
	}

	/// <summary>
	/// Disposes this font instance. The shared <see cref="FontSystem"/> is not affected.
	/// </summary>
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;

		GC.SuppressFinalize(this);
	}

	/// <summary>Gets a sized font instance from the underlying font system.</summary>
	public SpriteFontBase GetFont(float size) => _system.GetFont(size);

	private static byte[] ReadAllBytes(Stream stream)
	{
		using var ms = new MemoryStream();
		stream.CopyTo(ms);
		return ms.ToArray();
	}
}