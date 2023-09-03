using System.Collections.Generic;
using UnityEngine;

public class PlayerBuffsIndicators : MonoBehaviour
{
	public static PlayerBuffsIndicators Instance;

	public GameObject BuffIndicatorPrefab;

	private BuffSystem _buffSystem;

	private Transform _transform;

	private List<PlayerBuffIndicator> _indicators = new List<PlayerBuffIndicator>();

	private float _buffWidth = 24f;

	private void BuffStarted(Buff buff)
	{
		if (buff.isVisible)
		{
			AddIndicator(buff);
		}
	}

	private void BuffEnded(Buff buff)
	{
		foreach (PlayerBuffIndicator indicator in _indicators)
		{
			if (indicator.RepresentedBuff == buff)
			{
				indicator.Deactivate();
				_indicators.Remove(indicator);
				UpdatePositions();
				break;
			}
		}
	}

	private PlayerBuffIndicator Activate()
	{
		CachedObject.Cache cache = ObjectCacheManager.Instance.GetCache(BuffIndicatorPrefab);
		PlayerBuffIndicator playerBuffIndicator = cache.Activate() as PlayerBuffIndicator;
		playerBuffIndicator.transform.parent = _transform;
		_indicators.Add(playerBuffIndicator);
		UpdatePositions();
		return playerBuffIndicator;
	}

	public void Deactivate(PlayerBuffIndicator indicator)
	{
		_indicators.Remove(indicator);
		indicator.Deactivate();
		UpdatePositions();
	}

	public PlayerBuffIndicator AddIndicator(string icon, float duration)
	{
		PlayerBuffIndicator playerBuffIndicator = Activate();
		playerBuffIndicator.SetData(this, icon, duration);
		return playerBuffIndicator;
	}

	public PlayerBuffIndicator AddIndicator(Buff buff)
	{
		PlayerBuffIndicator playerBuffIndicator = Activate();
		playerBuffIndicator.SetData(this, buff);
		return playerBuffIndicator;
	}

	private void UpdatePositions()
	{
		int num = 0;
		foreach (PlayerBuffIndicator indicator in _indicators)
		{
			float num2 = _buffWidth * (float)num;
			if (num == 1 || num == 6 || num == 11)
			{
				num2 += 0.1f;
			}
			num++;
			indicator.transform.localPosition = new Vector3(num2, 0f, 0f);
		}
	}

	private void PlayerVehicleActivated(Vehicle vehicle)
	{
		_buffSystem = vehicle.buffSystem;
		_buffSystem.BuffStartedEvent += BuffStarted;
		_buffSystem.BuffEndedEvent += BuffEnded;
	}

	private void PlayerVehicleDeactivated(Vehicle vehicle)
	{
		_buffSystem.BuffStartedEvent -= BuffStarted;
		_buffSystem.BuffEndedEvent -= BuffEnded;
		foreach (PlayerBuffIndicator indicator in _indicators)
		{
			indicator.Deactivate();
		}
		_indicators.Clear();
	}

	private void Awake()
	{
		_transform = GetComponent<Transform>();
		VehiclesManager instance = VehiclesManager.instance;
		instance.playerVehicleActivatedEvent += PlayerVehicleActivated;
		instance.playerVehicleDeactivatedEvent += PlayerVehicleDeactivated;
		Instance = this;
	}

	private void OnDestroy()
	{
		VehiclesManager instance = VehiclesManager.instance;
		if (instance != null)
		{
			instance.playerVehicleActivatedEvent -= PlayerVehicleActivated;
			instance.playerVehicleDeactivatedEvent -= PlayerVehicleDeactivated;
		}
		Instance = null;
	}
}
