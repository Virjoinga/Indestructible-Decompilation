public class MoveStick : StickScript
{
	private static MoveStick _instance;

	public static MoveStick instance
	{
		get
		{
			return _instance;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_instance = this;
	}
}
