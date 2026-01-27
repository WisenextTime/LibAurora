namespace LibAurora.Input;

public class TriggerableSource : InputSource
{
	private bool _state;
	private bool _preState;
	public override bool Pressed()
		=> _state && !_preState;
	public override bool Down()
		=> _state;
	public override bool Released()
		=> !_state && _preState;
	public override bool Up()
		=> !_state;
	public override float Strength()
		=> _state ? 1 : 0;

	public void UpdateState(bool newState)
	{
		_preState = _state;
		_state = newState;
	}
}