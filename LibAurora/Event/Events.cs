namespace LibAurora.Event;

public class GamepadRegisterEvent(int id)
{
	public int GamepadId => id;
}

public class GamepadUnregisterEvent(int id)
{
	public int GamepadId => id;
}