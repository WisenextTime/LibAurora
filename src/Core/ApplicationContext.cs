using LibAurora.Audio;
using LibAurora.Graphics;
using LibAurora.Input;
namespace LibAurora.Core;

/// <summary>
/// Holds all application services returned by <see cref="IApplication.Run"/>.
/// Graphics may be null for headless configurations.
/// </summary>
public class ApplicationContext
{
	/// <summary>The running application instance.</summary>
	public required IApplication Application { get; init; }

	/// <summary>The graphics device. Null in headless mode.</summary>
	public IGraphics? Graphics { get; init; }

	/// <summary>The input provider.</summary>
	public required IInput Input { get; init; }

	/// <summary>The audio provider.</summary>
	public required IAudio Audio { get; init; }
}