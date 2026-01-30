using Arch.Core;
namespace LibAurora.Ecs;

public interface IUpdatableSystem
{
	void Update(double delta, World? world);
}

public interface IRenderableSystem
{
	void Draw(World? world);
}