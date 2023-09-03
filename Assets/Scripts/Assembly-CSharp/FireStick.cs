public class FireStick : StickScript
{
	private static FireStick _instance;

	public static FireStick instance
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
