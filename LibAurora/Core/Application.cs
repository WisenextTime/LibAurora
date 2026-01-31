using System;
using LibAurora.Event;
using LibAurora.Framework;
using LibAurora.Graphics;
using LibAurora.Graphics.Rendering;
using LibAurora.Input;
using LibAurora.Physics;
namespace LibAurora.Core;

public sealed class Application
{
	public static Application Instance
		=> _instance ?? throw new InvalidOperationException("Application instance is not initialized.");
	private static Application? _instance;
	private readonly RenderingServices _renderingServices = new();
	private readonly LogicService _logicService = new();
	private readonly IMainLoop _mainLoop;
	public Application(IMainLoop mainLoop)
	{
		if(_instance is not null)throw new InvalidOperationException("Application already created");
		_mainLoop = mainLoop;
		_instance = this;
	}
	/// <summary>
	/// Init and run the application.
	/// </summary>
	public void Run()
	{
		_mainLoop.Initialize();
		
		_renderingServices.Initialize();
		_logicService.Initialize();

		RegisterService<EventServices>();
		RegisterService<InputService>();
		RegisterService<CollisionService>();
		
		_renderingServices.Register(_mainLoop);
		_logicService.Register(_mainLoop);
		
		_mainLoop.Ready();
		
		_logicService.Run();
		_renderingServices.Run();
	}

	public T RegisterService<T>() where T : class, new()
	{
		var service = new T();
		if(service is IInitializable initializable)initializable.Initialize();
		if(service is IUpdatable updatable)_logicService.Register(updatable);
		if (service is IRenderable renderable) _renderingServices.Register(renderable);
		return service;
	}
}