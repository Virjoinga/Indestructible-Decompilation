using UnityEngine;

public class CRGameSetup : MonoBehaviour
{
	public Transform[] SpawnPoints;

	public CRLoadingArea[] TeamLoadingAreas;

	public ChargeItem ChargeItemInstance;

	private static CRGameSetup _instance;

	public static CRGameSetup Instance
	{
		get
		{
			return _instance;
		}
	}

	private void Start()
	{
		_instance = this;
	}

	private void OnDestroy()
	{
		_instance = null;
	}
}
