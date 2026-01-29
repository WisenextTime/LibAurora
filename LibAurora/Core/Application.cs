using LibAurora.Event;
using LibAurora.Framework;
using LibAurora.Graphics.Rendering;
using LibAurora.Input;
using LibAurora.Physics;
namespace LibAurora.Core;

public sealed class Application(IMainLoop mainLoop)
{
	private readonly RenderingServices _renderingServices = new();
	private readonly LogicService _logicService = new();
	private readonly EventServices _eventServices = new();
	private readonly InputService _inputService = new();
	private readonly CollisionService _collisionService = new();
	/// <summary>
	/// Init and run the application.
	/// </summary>
	public void Run()
	{
		mainLoop.Initialize();
		
		_renderingServices.Initialize();
		_logicService.Initialize();
		_collisionService.Initialize();
		
		_logicService.Register(_inputService);
		_logicService.Register(_collisionService);
		_logicService.Register(mainLoop);
		
		_renderingServices.Register(mainLoop);
		
		mainLoop.Ready();
		
		_logicService.Run();
		_renderingServices.Run();
	}
}