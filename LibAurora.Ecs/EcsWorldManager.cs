using LibAurora.Framework;
using LibAurora.Graphics;
namespace LibAurora.Ecs;

public class EcsWorldManager : IUpdatable, IRenderable
{
	public readonly List<Scene> Scenes = [];
	public void Update(double delta)
	{
		foreach (var scene in Scenes.Where(scene => !scene.Pause).OrderBy(scene => scene.Priority))
		{
			scene.Update(delta);
		}
	}
	public void Draw()
	{
		foreach (var scene in Scenes.Where(scene => scene.Visible).OrderBy(scene => scene.Priority))
		{
			scene.Draw();
		}
	}
	public void PushScene(Scene scene,bool pauseActiveScene = true)
	{
		if (pauseActiveScene && Scenes.Count > 0) Scenes[^1].Pause = true;
		Scenes.Add(scene);
		scene.OnPushed?.Invoke();
	}
	public Scene? PopScene(bool unpauseActiveScene = true)
	{
		if (Scenes.Count == 0) return null;
		if(unpauseActiveScene) Scenes[-1].Pause = false;
		var poppedScene = Scenes[^1];
		Scenes.RemoveAt(Scenes.Count - 1);
		poppedScene.OnPopped?.Invoke();
		return poppedScene;
	}
	public void PopToScene(Scene scene)
	{
		if (!Scenes.Contains(scene)) return;
        
		while (Scenes.Count > 0 && Scenes[^1] != scene)
		{
			PopScene();
		}
	}
	public void PauseScene(Scene scene)
	{
		if (Scenes.Contains(scene))
			scene.Pause = true;
		
	}
    
	public void ResumeScene(Scene scene)
	{
		if (Scenes.Contains(scene))
			scene.Pause = false;
	}
    
	public void HideScene(Scene scene)
	{
		scene.Visible = false;
	}
    
	public void ShowScene(Scene scene)
	{
		scene.Visible = true;
	}
}