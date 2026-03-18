using Veldrid;
namespace LibAurora.Graphics;

public interface IGraphics
{
	public GraphicsDevice Device { get; }
	public CommandList CommandList { get; }
	public ResourceFactory Factory { get; }
	public void BeginFrame();
	public void EndFrame();
}