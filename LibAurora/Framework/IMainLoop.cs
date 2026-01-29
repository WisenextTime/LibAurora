using LibAurora.Graphics;
namespace LibAurora.Framework;

public interface IMainLoop : IUpdatable,IRenderable
{
	void Initialize();
	void Ready();
}