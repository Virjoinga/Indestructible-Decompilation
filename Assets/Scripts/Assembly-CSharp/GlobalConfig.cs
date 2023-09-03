using Glu;

public class GlobalConfig : MonoBehaviour
{
	public float BurningDebuffTickValue = 10f;

	public float BurningDebuffPeriod = 5f;

	public float StasisDebuffValue = 3f;

	public float StasisDebuffPeriod = 5f;

	private static GlobalConfig _instance;

	public static GlobalConfig Instance
	{
		get
		{
			return _instance;
		}
	}

	private void Awake()
	{
		_instance = this;
	}
}
