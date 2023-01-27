using System.Collections;
using UnityEngine;

public class EnemyDirectionIndicator : GameplayObjectIndicator
{
	private Bounds _uiCameraBounds;

	private Transform _enemyTransform;

	private Transform _playerTransform;

	private PackedSprite _sprite;

	private Vector3 _cachePlayerPosition;

	private Vector3 _cacheEnemyPosition;

	public Transform EnemyTransform
	{
		get
		{
			return _enemyTransform;
		}
		set
		{
			if (_enemyTransform != value)
			{
				_enemyTransform = value;
				UpdateColor();
			}
			else
			{
				_enemyTransform = value;
			}
		}
	}

	private void UpdateColor()
	{
		int layer = _enemyTransform.gameObject.layer;
		int teamId = MonoSingleton<Player>.Instance.GetTeamId(layer);
		Color white = Color.white;
		white = ((teamId != -1) ? MonoSingleton<Player>.Instance.GetTeamColor(teamId) : MonoSingleton<Player>.Instance.GetTeamColor(1));
		white.a = 0f;
		_sprite.SetColor(white);
	}

	private void VehicleActivated(Vehicle vehicle)
	{
		if ((bool)_enemyTransform && vehicle.transform == _enemyTransform)
		{
			base.gameObject.SetActiveRecursively(true);
		}
	}

	private void VehicleDeactivated(Vehicle vehicle)
	{
		if (_enemyTransform != null && vehicle.transform == _enemyTransform)
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	private void ActivateDeathEvents(bool activate)
	{
		if (VehiclesManager.instance != null)
		{
			if (activate)
			{
				VehiclesManager.instance.vehicleActivatedEvent += VehicleActivated;
				VehiclesManager.instance.vehicleDeactivatedEvent += VehicleDeactivated;
			}
			else
			{
				VehiclesManager.instance.vehicleActivatedEvent -= VehicleActivated;
				VehiclesManager.instance.vehicleDeactivatedEvent -= VehicleDeactivated;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_sprite = GetComponent<PackedSprite>();
		ActivateDeathEvents(true);
		VehiclesManager instance = VehiclesManager.instance;
		instance.playerVehicleActivatedEvent += PlayerVehicleActivated;
		instance.playerVehicleDeactivatedEvent += PlayerVehicleDeactivated;
		if (instance.playerVehicle != null && instance.playerVehicle.isActive)
		{
			PlayerVehicleActivated(instance.playerVehicle);
		}
		else
		{
			base.enabled = false;
		}
	}

	protected override void Start()
	{
		base.Start();
		StartCoroutine(DelayedUpdateColor());
	}

	private IEnumerator DelayedUpdateColor()
	{
		yield return null;
		UpdateColor();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ActivateDeathEvents(false);
	}

	protected override bool Initialize()
	{
		base.Initialize();
		if (_initialized)
		{
			Vector3 vector = _uiCamera.ScreenToWorldPoint(Vector3.zero);
			Vector3 scale = new Vector3(-2f, -2f, 0f);
			vector.Scale(scale);
			Vector3 size = vector - new Vector3(50f, 50f, 0f);
			_uiCameraBounds = new Bounds(Vector3.zero, size);
		}
		return _initialized;
	}

	private bool IsChanged()
	{
		return _playerTransform.position != _cachePlayerPosition || _enemyTransform.position != _cacheEnemyPosition;
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (_initialized && IsChanged())
		{
			_cacheEnemyPosition = _enemyTransform.position;
			_cachePlayerPosition = _playerTransform.position;
			Vector3 vector = MainToUICamera(_cacheEnemyPosition);
			vector.z = 0f;
			float num = _uiCameraBounds.SqrDistance(vector);
			Color color = _sprite.color;
			if (num > 0f)
			{
				num = Mathf.Sqrt(num);
				Vector3 vector2 = MainToUICamera(_cachePlayerPosition);
				vector2.z = 0f;
				Vector3 vector3 = vector - vector2;
				vector3.Normalize();
				float num2 = Mathf.SmoothStep(0.4f, 0.8f, 1f - num / 150f);
				_transform.localScale = new Vector3(num2, num2, 1f);
				float num3 = Mathf.Atan2(vector3.y, vector3.x) * 57.29578f;
				_transform.rotation = Quaternion.Euler(0f, 0f, num3 - 90f);
				Vector3 position = vector2 + vector3 * 50f;
				_transform.position = position;
				float a = Mathf.SmoothStep(0f, 0.5f, num / 50f);
				color.a = a;
			}
			else
			{
				color.a = 0f;
			}
			if (_sprite.color != color)
			{
				_sprite.SetColor(color);
			}
		}
	}

	private void PlayerVehicleActivated(Vehicle vehicle)
	{
		_playerTransform = vehicle.transform;
		base.enabled = true;
	}

	private void PlayerVehicleDeactivated(Vehicle vehicle)
	{
	}
}
