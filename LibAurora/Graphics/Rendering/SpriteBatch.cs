using System;
using System.Collections.Generic;
using System.Numerics;
using LibAurora.Graphics.Shaders;
using Raylib_cs;
namespace LibAurora.Graphics.Rendering;

public class SpriteBatch
{
	/// <summary>
	/// The switch of frustum culling,
	/// </summary>
	public bool EnableFrustumCulling = true;
	
	/// <summary>
	/// How many pxs away from the edge of window will be culling.
	/// </summary>
	public float CullingBuffer = 100f;
	
	private struct SpriteInstance()
	{
		public Texture2D Texture;
		public Rectangle Source;
		public Rectangle Destination;
		public Vector2 Origin;
		public float Rotation;
		public Color Tint;
		public int LayerDepth;
		
		public bool IsNPatchMode = false;
		public NPatchInfo NPatchInfo;

		public uint ShaderId = 0;
	}
	private readonly List<SpriteInstance> _sprites = [];
	private bool _batching;

	private Rectangle _renderingArea;
	private bool _cameraMode;

	private readonly Vector2[] _screenCorners = new Vector2[4];
	private int _screenWidth;
	private int _screenHeight;
	private float _cullingBuffer;

	private readonly static Comparison<SpriteInstance> _comparer = (a, b) =>
	{
		var layerCompare = a.LayerDepth.CompareTo(b.LayerDepth);
		if (layerCompare != 0) return layerCompare;
		var shaderCompare = a.ShaderId.CompareTo(b.ShaderId);
		if (shaderCompare != 0) return shaderCompare;
		var textureCompare = a.Texture.Id.CompareTo(b.Texture.Id);
		return textureCompare != 0 ? textureCompare : a.Destination.Y.CompareTo(b.Destination.Y);

	};

	/// <summary>
	/// Begin the drawing of this batch.
	/// </summary>
	/// <param name="camera">The camera of this batch</param>
	/// <exception cref="InvalidOperationException">The SpriteBatch has already begun.</exception>
	public void BeginDraw(Camera2D? camera = null)
	{
		if (_batching)
			throw new InvalidOperationException("SpriteBatch is already batching");
		_batching = true;
		_sprites.Clear();
		if (EnableFrustumCulling)
		{
			var currentWidth = Raylib.GetScreenWidth();
			var currentHeight = Raylib.GetScreenHeight();
			if (currentHeight != _screenHeight || currentWidth != _screenWidth)
			{
				_screenWidth = Raylib.GetScreenWidth();
				_screenHeight = Raylib.GetScreenHeight();
				_screenCorners[0] = Vector2.Zero;
				_screenCorners[1] = new Vector2(_screenWidth, 0);
				_screenCorners[2] = new Vector2(0, _screenHeight);
				_screenCorners[3] = new Vector2(_screenWidth, _screenHeight);
			}
			if (camera.HasValue)
			{
				_renderingArea = GetRectangle(camera.Value);
				_cullingBuffer = CullingBuffer / (camera.Value.Zoom == 0 ? 1 : camera.Value.Zoom);
			}
			else
			{
				_renderingArea = new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
				_cullingBuffer = CullingBuffer;
			}
			_renderingArea.X -= _cullingBuffer;
			_renderingArea.Y -= _cullingBuffer;
			_renderingArea.Width += _cullingBuffer * 2;
			_renderingArea.Height += _cullingBuffer * 2;
		}
		if (camera.HasValue)
		{
			Raylib.BeginMode2D(camera.Value);
			_cameraMode = true;
		}
		else _cameraMode = false;
	}

	/// <summary>
	/// Draw a texture with transform and color(Mux).
	/// </summary>
	/// <param name="texture">the texture used for this drawing.</param>
	/// <param name="posX">the position X</param>
	/// <param name="posY">the position Y</param>
	/// <param name="tint">the color used for this drawing.(Mux)</param>
	/// <param name="rotation">the rotation</param>
	/// <param name="layer">the drawing layer (default 0)</param>
	/// <param name="origin">the rotation origin</param>
	/// <exception cref="InvalidOperationException">draw before batch beginning</exception>
	public void Draw(Texture2D texture, float posX, float posY, Color tint, Vector2 origin = default, float rotation = 0,
		int layer = 0) => Draw(
			texture, new Rectangle(0, 0, texture.Width, texture.Height), new Rectangle(posX, posY, texture.Width, texture.Height),
			origin, rotation, tint, layer);
	/// <summary>
	/// Draw a texture with transform and color(Mux).
	/// </summary>
	/// <param name="texture">the texture used for this drawing.</param>
	/// <param name="position">the position</param>
	/// <param name="color">the color used for this drawing.(Mux)</param>
	/// <param name="rotation">the rotation</param>
	/// <param name="layer">the drawing layer (default 0)</param>
	/// <param name="origin">the rotation origin</param>
	/// <exception cref="InvalidOperationException">draw before batch beginning</exception>
	public void Draw(Texture2D texture, Vector2 position, Color color, Vector2 origin = default, float rotation = 0,
		int layer = 0)
		=> Draw(texture, position.X, position.Y, color, origin, rotation, layer);
	/// <summary>
	/// Draw a part of a texture with transform and color(Mux).
	/// </summary>
	/// <param name="texture">the texture used for this drawing.</param>
	/// <param name="source">the part of texture</param>
	/// <param name="position">the position</param>
	/// <param name="tint">the color used for this drawing.(Mux)</param>
	/// <param name="rotation">the rotation</param>
	/// <param name="layer">the drawing layer (default 0)</param>
	/// <param name="origin">the rotation origin</param>
	/// <exception cref="InvalidOperationException">draw before batch beginning</exception>
	public void Draw(Texture2D texture, Rectangle source, Vector2 position, Color tint = default,Vector2 origin = default, float rotation = 0,
		int layer = 0)
		=> Draw(
			texture, source, new Rectangle(position.X, position.Y, texture.Width, texture.Height), Vector2.Zero, 0f, tint, layer);
	/// <summary>
	/// Draw a part of a texture to a target range with color(Mux)
	/// </summary>
	/// <param name="texture">the texture used for this drawing.</param>
	/// <param name="source">the part of texture</param>
	/// <param name="destin">the target range</param>
	/// <param name="origin">the origin of rotation</param>
	/// <param name="rotation">the rotation</param>
	/// <param name="tint">the color used for this drawing.(Mux)</param>
	/// <param name="layer">the drawing layer (default 0)</param>
	/// <param name="shader">the drawing shader</param>
	/// <exception cref="InvalidOperationException">draw before batch beginning</exception>
	public void Draw(Texture2D texture, Rectangle source, Rectangle destin,
		Vector2 origin, float rotation, Color tint, int layer = 0, Shader? shader = null)
	{
		if (!_batching)
			throw new InvalidOperationException("Call BeginDraw() first");
		if (EnableFrustumCulling && !IsInView(destin)) return;
		uint shaderId = 0;
		if (shader.HasValue)
		{
			ShaderService.Instance.RegisterShader(shader.Value);
			shaderId = shader.Value.Id;
		}
		_sprites.Add(
			new SpriteInstance
			{
				Texture = texture,
				Source = source,
				Destination = destin,
				Origin = origin,
				Rotation = rotation,
				Tint = tint,
				LayerDepth = layer,
				
				ShaderId = shaderId,
			});
	}
	/// <summary>
	/// Draw a texture with ninepatch mode.
	/// </summary>
	/// <param name="texture">the texture used for this drawing.</param>
	/// <param name="nPatchInfo">info od ninepatch</param>
	/// <param name="destin">the target range</param>
	/// <param name="tint">the color used for this drawing.(Mux)</param>
	/// <param name="origin">the origin of rotation</param>
	/// <param name="rotation">the rotation</param>
	/// <param name="layer">the drawing layer (default 0)</param>
	/// <param name="shader">the drawing shader</param>
	/// <exception cref="InvalidOperationException">draw before batch beginning</exception>
	public void Draw(Texture2D texture, NPatchInfo nPatchInfo, Rectangle destin, Color tint, Vector2 origin = default,
		float rotation = 0f, int layer = 0, Shader? shader = null)
	{
		if (!_batching)
			throw new InvalidOperationException("Call BeginDraw() first");
		if (EnableFrustumCulling && !IsInView(destin)) return;
		uint shaderId = 0;
		if (shader.HasValue)
		{
			ShaderService.Instance.RegisterShader(shader.Value);
			shaderId = shader.Value.Id;
		}
		_sprites.Add(
			new SpriteInstance
			{
				Texture = texture,
				Source = new Rectangle(0, 0, texture.Width, texture.Height),
				Destination = destin,
				Origin = origin,
				Rotation = rotation,
				Tint = tint,
				LayerDepth = layer,
				
				IsNPatchMode = true,
				NPatchInfo = nPatchInfo,
				
				ShaderId = shaderId,
			});
	}

	/// <summary>
	/// End and apply the drawing of this batch
	/// </summary>
	/// <exception cref="InvalidOperationException">end before batch beginning</exception>
	public void EndDraw()
	{
		if (!_batching) throw new InvalidOperationException("SpriteBatch is not batching");
		if (_sprites.Count != 0)
		{
			if (_sprites.Count > 1) _sprites.Sort(_comparer);
			uint currentShaderId = 0;

			foreach (var sprite in _sprites)
			{
				if (sprite.ShaderId != currentShaderId)
				{
					if (currentShaderId != 0)
					{
						Raylib.EndShaderMode();
					}

					if (sprite.ShaderId != 0)
					{
						Raylib.BeginShaderMode(ShaderService.Instance.GetShader(sprite.ShaderId));
					}
					currentShaderId = sprite.ShaderId;
				}
				if (sprite.IsNPatchMode)
					Raylib.DrawTextureNPatch(
						sprite.Texture,
						sprite.NPatchInfo,
						sprite.Destination,
						sprite.Origin,
						sprite.Rotation,
						sprite.Tint);
				else
					Raylib.DrawTexturePro(
						sprite.Texture,
						sprite.Source,
						sprite.Destination,
						sprite.Origin,
						sprite.Rotation,
						sprite.Tint
					);
			}
			if (currentShaderId != 0)
			{
				Raylib.EndShaderMode();
			}
			_sprites.Clear();
		}
		if (_cameraMode) Raylib.EndMode2D();
		_batching = false;
	}
	private Rectangle GetRectangle(Camera2D camera)
	{
		var min = Raylib.GetScreenToWorld2D(_screenCorners[0], camera);
		var max = min;

		for (var i = 1; i < _screenCorners.Length; i++)
		{
			var worldPos = Raylib.GetScreenToWorld2D(_screenCorners[i], camera);
			min = Vector2.Min(min, worldPos);
			max = Vector2.Max(max, worldPos);
		}

		return new Rectangle(
			min.X, min.Y,
			max.X - min.X,
			max.Y - min.Y
		);
	}

	private bool IsInView(Rectangle bounds) => Raylib.CheckCollisionRecs(_renderingArea, bounds);
}