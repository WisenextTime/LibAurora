using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
namespace LibAurora.Physics;

public struct ContactData
{
	public FixtureData FixtureA { get; init; }
	public FixtureData FixtureB { get; init; }
    
	public bool IsEnabled { get; init; }
	public bool IsTouching { get; init; }
    
	public float Friction { get; init; }
	public float Restitution { get; init; }
    
	public float TangentSpeed { get; init; }
}

public struct FixtureData
{
	public object UserData { get; init; }
	public float Density { get; init; }
	public float Friction { get; init; }
	public float Restitution { get; init; }
	public bool IsSensor { get; init; }
    
	public BodyData Body { get; init; }
    
	public Shape Shape { get; init; }
}

public struct BodyData
{
	public object UserData { get; init; }
	public BodyType Type { get; init; }
	public Vector2 Position { get; init; }
	public float Angle { get; init; }
	public Vector2 LinearVelocity { get; init; }
	public float AngularVelocity { get; init; }
	public float LinearDamping { get; init; }
	public float AngularDamping { get; init; }
	public bool IsAwake { get; init; }
	public bool IsBullet { get; init; }
}