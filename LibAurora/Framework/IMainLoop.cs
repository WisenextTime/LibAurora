namespace LibAurora.Framework;

public interface IMainLoop
{
	void Initialize();

	void Ready();
	void Update(double elapsedTime);
	void Render();
}