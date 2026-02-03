using Box2D.NetStandard.Collision;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.Fixtures;
namespace LibAurora.Physics;

public class BeginContactEvent
{
	public ContactData ContactData { get; private set; }
	
	internal void ChangeContactData(in Contact contact)
	{
		ContactData = CollisionEventsUntil.FromContact(contact);
	}
}

public class EndContactEvent
{
	public ContactData ContactData { get; private set; }
	internal void ChangeContactData(in Contact contact)
	{
		ContactData = CollisionEventsUntil.FromContact(contact);
	}
}

public static class CollisionEventsUntil
{
	public static ContactData FromContact(in Contact contact)
	{
		var data = new ContactData
		{
			FixtureA = FromFixture(contact.FixtureA),
			FixtureB = FromFixture(contact.FixtureB),
			IsEnabled = contact.IsEnabled(),
			IsTouching = contact.IsTouching(),
			Friction = contact.Friction,
			Restitution = contact.Restitution,
			TangentSpeed = contact.TangentSpeed,
		};
		return data;
	}
	public static FixtureData FromFixture(in Fixture fixture)
	{
		var data = new FixtureData
		{
			UserData = fixture.UserData,
			Density = fixture.Density,
			Friction = fixture.m_friction,
			Restitution = fixture.Restitution,
			IsSensor =  fixture.IsSensor(),
			Body = FromBody(fixture.Body),
			Shape = fixture.Shape,
		};
		return data;
	}
	public static BodyData FromBody(Body body)
	{
		var data = new BodyData
		{
			UserData = body.UserData,
			Type = body.Type(),
			Position = body.Position,
			Angle = body.GetAngle(),
			LinearVelocity = body.GetLinearVelocity(),
			AngularVelocity = body.GetAngularVelocity(),
			LinearDamping = body.GetLinearDamping(),
			AngularDamping = body.GetAngularDamping(),
			IsAwake =  body.IsAwake(),
			IsBullet =  body.IsBullet(),
		};
		return data;
	}
}