namespace LibAurora.Input;

public abstract class InputSource
{
	public abstract bool Pressed();
	public abstract bool Down();
	public abstract bool Released();
	public abstract bool Up();
	
	public abstract float Strength();
}