using UnityEngine;

public class PlayerDirectionIndicator : MonoBehaviour
{
	public StickScript StickScript;

	public Camera UICamera;

	private Transform _transform;

	private PackedSprite _sprite;

	private Camera _gameCamera;

	private Color _cacheColor;

	private Vector2 _cacheVector;

	private Vector3 _cachePosition;

	private Color _activeColor = new Color(1f, 1f, 1f, 0.5f);

	private Transform _playerTransform;

	private void Awake()
	{
		_transform = GetComponent<Transform>();
		GameObject gameObject = GameObject.Find("Main Camera");
		if (gameObject != null)
		{
			_gameCamera = gameObject.GetComponent<Camera>();
		}
		_cacheColor = Color.clear;
		_sprite = GetComponent<PackedSprite>();
		_sprite.SetColor(_cacheColor);
		_cacheVector = Vector2.zero;
		_cachePosition = Vector3.zero;
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

	public void OnDestroy()
	{
		VehiclesManager instance = VehiclesManager.instance;
		if (instance != null)
		{
			instance.playerVehicleActivatedEvent -= PlayerVehicleActivated;
			instance.playerVehicleDeactivatedEvent -= PlayerVehicleDeactivated;
		}
	}

	private void LateUpdate()
	{
		Vector2 vector = StickScript.GetVector();
		if (vector != Vector2.zero)
		{
			if (_cacheColor != _activeColor)
			{
				_sprite.SetColor(_activeColor);
				_cacheColor = _activeColor;
			}
			if (_cacheVector != vector || _cachePosition != _playerTransform.position)
			{
				Vector3 position = _gameCamera.WorldToViewportPoint(_playerTransform.position);
				position = UICamera.ViewportToWorldPoint(position);
				position.z = 0f;
				vector.Normalize();
				Vector3 vector2 = new Vector3(vector.x, vector.y, 0f);
				_transform.position = Vector3.zero;
				float num = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
				_transform.eulerAngles = new Vector3(0f, 0f, num - 90f);
				Vector3 position2 = position + vector2 * 31.2f;
				_transform.position = position2;
				_cachePosition = _playerTransform.position;
				_cacheVector = vector;
			}
		}
		else if (_cacheColor != Color.clear)
		{
			_sprite.SetColor(Color.clear);
			_cacheColor = Color.clear;
		}
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
}
