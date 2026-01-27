namespace LibAurora.Framework;

public interface IMainLoop
{
	void Initialize();
	void Update(double elapsedTime);
	void Render();
}