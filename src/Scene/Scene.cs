namespace LibAurora.Scene;

/// <summary>
/// Abstract base for all scenes. A scene represents a logical unit of update/draw/input logic.
/// </summary>
public abstract class Scene
{
	/// <summary>Whether this scene receives Update and HandleInput calls.</summary>
	public bool Enabled { get; set; } = true;

	/// <summary>Whether this scene receives Draw calls.</summary>
	public bool Visible { get; set; } = true;

	/// <summary>Called once per frame with the delta time in seconds.</summary>
	public abstract void Update(double delta);

	/// <summary>Called once per frame with an alpha blend factor for interpolation.</summary>
	public abstract void Draw(double alpha);

	/// <summary>Called to handle input events. Returns true if the event was consumed.</summary>
	public virtual bool HandleInput() => false;
}