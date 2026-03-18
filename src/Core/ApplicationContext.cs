using LibAurora.Audio;
using LibAurora.Graphics;
using LibAurora.Input;
namespace LibAurora.Core;

public class ApplicationContext
{
	public required IApplication Application { get; init; }
	public IGraphics? Graphics { get; init; }
	public required IInput Input { get; init; }
	public required IAudio Audio { get; init; }
}