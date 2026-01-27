using LibAurora.Framework;
using LibAurora.Graphics.Rendering;
using LibAurora.Input;
namespace LibAurora.Core;

public sealed class Application(IMainLoop mainLoop)
{
	private readonly RenderingServices _renderingServices = new(mainLoop);
	private readonly LogicService _logicService = new(mainLoop);
	private readonly InputService _inputService = new();
	/// <summary>
	/// Init and run the application.
	/// </summary>
	public void Run()
	{
		_logicService.Initialize();
		_renderingServices.Initialize();
		mainLoop.Initialize();
		
		_logicService.Run();
		_renderingServices.Run();
	}
}