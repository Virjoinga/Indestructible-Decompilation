using UnityEngine;

public class GroundEffectsManager : MonoBehaviour
{
	public GameObject[] dustEffectPrefabs;

	private DustEffect[] _dustEffects;

	private static GroundEffectsManager _instance;

	public static GroundEffectsManager instance
	{
		get
		{
			return _instance;
		}
	}

	public DustEffect GetDustEffect(int groundType)
	{
		return _dustEffects[groundType];
	}

	private void Awake()
	{
		_instance = this;
		int num = dustEffectPrefabs.Length;
		_dustEffects = new DustEffect[num];
		for (int i = 0; i != num; i++)
		{
			GameObject gameObject = dustEffectPrefabs[i];
			if (gameObject != null)
			{
				_dustEffects[i] = ((GameObject)Object.Instantiate(gameObject)).GetComponent<DustEffect>();
			}
		}
	}
}
