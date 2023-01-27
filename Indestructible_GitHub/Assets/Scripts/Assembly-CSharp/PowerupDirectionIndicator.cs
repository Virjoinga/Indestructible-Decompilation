using UnityEngine;

public class PowerupDirectionIndicator : GameplayObjectIndicator
{
	private Bounds _uiCameraBounds;

	public Transform PowerupTransform;

	private Transform _playerTransform;

	private PackedSprite _sprite;

	private Vector3 _cachePlayerPosition;

	private Vector3 _cachePowerupPosition;

	protected override void Start()
	{
		base.Start();
		_sprite = GetComponent<PackedSprite>();
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

	protected override void OnDestroy()
	{
		VehiclesManager instance = VehiclesManager.instance;
		if (instance != null)
		{
			instance.playerVehicleActivatedEvent -= PlayerVehicleActivated;
			instance.playerVehicleDeactivatedEvent -= PlayerVehicleDeactivated;
		}
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

	private void PlayerVehicleActivated(Vehicle vehicle)
	{
		_playerTransform = vehicle.transform;
		base.enabled = true;
	}

	private void PlayerVehicleDeactivated(Vehicle vehicle)
	{
		base.enabled = false;
	}

	private bool IsChanged()
	{
		return _playerTransform.position != _cachePlayerPosition || PowerupTransform.position != _cachePowerupPosition;
	}

	protected override void Update()
	{
		base.Update();
		if (_initialized && IsChanged())
		{
			_cachePowerupPosition = PowerupTransform.position;
			_cachePlayerPosition = _playerTransform.position;
			Vector3 vector = MainToUICamera(_cachePowerupPosition);
			vector.z = 0f;
			float num = _uiCameraBounds.SqrDistance(vector);
			Color color = Color.clear;
			if (num > 0f)
			{
				num = Mathf.Sqrt(num);
				Vector3 vector2 = MainToUICamera(_cachePlayerPosition);
				vector2.z = 0f;
				Vector3 vector3 = vector - vector2;
				vector3.Normalize();
				float num2 = Mathf.SmoothStep(0.4f, 0.8f, 1f - num / 250f);
				_transform.localScale = new Vector3(num2, num2, 1f);
				float num3 = Mathf.Atan2(vector3.y, vector3.x) * 57.29578f;
				_transform.rotation = Quaternion.Euler(0f, 0f, num3 - 90f);
				Vector3 position = vector2 + vector3 * 50f;
				_transform.position = position;
				float a = Mathf.SmoothStep(0f, 0.5f, num / 50f);
				color = new Color(1f, 1f, 1f, a);
			}
			if (_sprite.color != color)
			{
				_sprite.SetColor(color);
			}
		}
	}
}
