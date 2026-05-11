using System.Collections.Generic;
using Myra.Graphics2D;
using Veldrid;
using Point = System.Drawing.Point;
namespace LibAurora.Graphics;

/// <summary>
/// Wraps a Veldrid <see cref="Texture"/> and <see cref="TextureView"/> with pre-built
/// <see cref="ResourceSet"/> entries keyed by <see cref="TextureFiltering"/>.
/// Each instance gets a unique <see cref="Id"/> for batch grouping.
/// </summary>
public readonly struct TextureHandle(Texture texture, TextureView view, Dictionary<TextureFiltering, ResourceSet> resourceSets)
{
	private static uint _nextId;

	/// <summary>Unique identifier used for batch grouping.</summary>
	public uint Id { get; } = _nextId++;

	/// <summary>The underlying Veldrid <see cref="Texture"/>.</summary>
	public Texture Texture { get; init; } = texture;

	/// <summary>The texture view used for sampling.</summary>
	public TextureView View { get; init; } = view;

	/// <summary>Pre-built <see cref="ResourceSet"/> entries keyed by <see cref="TextureFiltering"/> mode.</summary>
	public Dictionary<TextureFiltering, ResourceSet> ResourceSets { get; init; } = resourceSets;

	/// <summary>Texture dimensions in pixels.</summary>
	public Point Size => new((int)Texture.Width, (int)Texture.Height);
}