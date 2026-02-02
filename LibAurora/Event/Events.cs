using LibAurora.Physics;
namespace LibAurora.Event;

public class CollisionShapeDirtEvent
{
	public CollisionShape? Shape;
}

public class GamepadRegisterEvent(int id)
{
	public int GamepadId => id;
}

public class GamepadUnregisterEvent(int id)
{
	public int GamepadId => id;
}