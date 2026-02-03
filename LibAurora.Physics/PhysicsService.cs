using System.Numerics;
using Box2D.NetStandard.Collision;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using LibAurora.Event;
using LibAurora.Framework;
using LibAurora.Utils;
namespace LibAurora.Physics;

public class PhysicsService : ContactListener, IUpdatable, IInitializable
{
	/// <summary>
	/// Get the PhysicsService singleton
	/// </summary>
	/// <exception cref="InvalidOperationException">PhysicsService is not initialized.</exception>
	public static PhysicsService Instance
		=> _instance ?? throw new InvalidOperationException("Physics service is not initialized.");
	public PhysicsService()
	{
		if (_instance != null) throw new InvalidOperationException("Physics service already been created");
		_instance = this;
	}
	public World World => _world??throw new InvalidOperationException("Physics service is not initialized.");
	public Vector2 Gravity { get; set; } = new Vector2(0, 9.8f);
	public float PixelsPerMeter = 128f;
	public int VelocityIterations = 8;
	public int PositionIterations = 3;
	public Action<Contact, Manifold>? PreSolveCallback { get; set; }
	public Action<Contact, ContactImpulse>? PostSolveCallback { get; set; }

	private static PhysicsService? _instance;
	private World? _world;
	private float _ppm;
	private int _vi;
	private int _pi;

	private ObjectPool<BeginContactEvent> _beginEventPool = new(10);
	private ObjectPool<EndContactEvent> _endEventPool = new(10);

	private List<Body> _queuedDestroyedBodies = [];

	public void Update(double delta)
	{
		World.Step((float)delta, _vi, _pi);
		World.ClearForces();
		if (_queuedDestroyedBodies.Count <= 0) return;
		foreach (var body in _queuedDestroyedBodies)
			World.DestroyBody(body);
		_queuedDestroyedBodies.Clear();
	}
	public void Initialize()
	{
		_world = new World(Gravity);
		_ppm = PixelsPerMeter;
		_vi = VelocityIterations;
		_pi = PositionIterations;

		EventServices.Instance.RegisterEvent<BeginContactEvent>();
		EventServices.Instance.RegisterEvent<EndContactEvent>();

		_world.SetAllowSleeping(true);
		_world.SetContactListener(this);
	}
	
	public Body CreateBody(BodyDef def) => World.CreateBody(def);
    
	public Body CreateDynamicBody(Vector2 position, float rotation = 0)
	{
		var bodyDef = new BodyDef
		{
			type = BodyType.Dynamic,
			position = position,
			angle = rotation,
		};
		return World.CreateBody(bodyDef);
	}
    
	public Body CreateStaticBody(Vector2 position, float rotation = 0)
	{
		var bodyDef = new BodyDef
		{
			type = BodyType.Static,
			position = position,
			angle = rotation
		};
		return World.CreateBody(bodyDef);
	}
    
	public void DestroyBody(Body? body)
	{
		if (body == null) return;
		_queuedDestroyedBodies.Add(body);
	}
    
	public void DestroyAllBodies()
	{
		var body = World.GetBodyList();
		while (body != null)
		{
			var next = body.GetNext();
			_queuedDestroyedBodies.Add(body);
			body = next;
		}
	}

	public Vector2 PixelsToMeters(Vector2 pixels) => pixels / _ppm;
	public Vector2 MetersToPixels(Vector2 meters) => meters * _ppm;
	public float PixelsToMeters(float pixels) => pixels / _ppm;
	public float MetersToPixels(float meters) => meters * _ppm;

	public override void BeginContact(in Contact contact)
	{
		var eventSource = _beginEventPool.Get();
		eventSource.ChangeContactData(contact);
		EventServices.Instance.Publish(eventSource);
		_beginEventPool.Return(eventSource);
	}
	public override void EndContact(in Contact contact)
	{
		var eventSource = _endEventPool.Get();
		eventSource.ChangeContactData(contact);
		EventServices.Instance.Publish(eventSource);
		_endEventPool.Return(eventSource);
	}
	public override void PreSolve(in Contact contact, in Manifold oldManifold) => PreSolveCallback?.Invoke(contact, oldManifold);
	public override void PostSolve(in Contact contact, in ContactImpulse impulse) => PostSolveCallback?.Invoke(contact, impulse);
}