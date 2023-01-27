using System.Collections;
using UnityEngine;

public class EnemyInfoIndicatorController : MonoBehaviour
{
	public GameObject IndicatorPrefab;

	public bool AIControlled;

	public Vector3 Offset;

	private CachedObject.Cache _cache;

	private GameObject _indicator;

	private Vehicle _vehicle;

	public bool indicatorEnabled
	{
		get
		{
			return _indicator != null && _indicator.gameObject.active;
		}
		set
		{
			if (_indicator != null)
			{
				_indicator.gameObject.SetActiveRecursively(value);
			}
		}
	}

	private void Awake()
	{
		_vehicle = GetComponent<Vehicle>();
		if (_vehicle != null)
		{
			_vehicle.playerChangedEvent += PlayerChanged;
			if (_vehicle.player != null)
			{
				PlayerChanged(_vehicle);
			}
		}
	}

	private void OnEnable()
	{
		Refresh();
	}

	private void OnDisable()
	{
		if (_indicator != null)
		{
			_indicator.gameObject.SetActiveRecursively(false);
		}
	}

	private void OnDestroy()
	{
		Vehicle component = GetComponent<Vehicle>();
		if (component != null)
		{
			component.playerChangedEvent -= PlayerChanged;
		}
	}

	public void PlayerChanged(Vehicle vehicle)
	{
		Refresh();
	}

	public void Refresh()
	{
		StartCoroutine(DelayedRefresh());
	}

	private IEnumerator DelayedRefresh()
	{
		yield return null;
		PhotonView view = GetComponent<PhotonView>();
		bool connected = PhotonNetwork.room != null;
		if (AIControlled)
		{
			base.enabled = true;
		}
		else if (view != null && connected)
		{
			base.enabled = true;
		}
		if (base.enabled)
		{
			if (!_indicator)
			{
				_indicator = (GameObject)Object.Instantiate(IndicatorPrefab);
			}
			_indicator.gameObject.SetActiveRecursively(true);
			EnemyInfoIndicator indicator = _indicator.GetComponent<EnemyInfoIndicator>();
			bool isMultiplayer = IDTGame.Instance is MultiplayerGame;
			bool isLocalPlayer = VehiclesManager.instance.playerVehicle == _vehicle;
			indicator.Startup(base.gameObject, Offset, isMultiplayer, AIControlled || !isLocalPlayer);
		}
		else
		{
			Object.Destroy(this);
		}
	}
}
