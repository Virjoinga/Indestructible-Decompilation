using UnityEngine;

[AddComponentMenu("Indestructible/Camera/Following Camera with fixed direction")]
public class FollowingCameraFixedDirection : MonoBehaviour
{
	public Vector3 targetOffset = new Vector3(0f, 0f, -6f);

	public Vector3 offsetFromTarget = new Vector3(0f, 25f, -28f);

	public float horizontalLerpFactor = 0.15f;

	public float verticalLerpFactor = 0.02f;

	public Vector3 listenerOffset;

	private Transform _target;

	private Vector3 _position;

	private Vector3 _offset;

	private Vector3 _fromLerpFactors;

	private Vector3 _toLerpFactors;

	private Transform _transform;

	private Transform _audioListenerTransform;

	public Transform target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
			if (value != null)
			{
				_transform.localPosition = (_position = _target.localPosition + _offset);
				base.enabled = true;
			}
			else
			{
				base.enabled = false;
			}
		}
	}

	private void Start()
	{
		_offset = targetOffset + offsetFromTarget;
		_transform = base.transform;
		_transform.localRotation = Quaternion.LookRotation(-offsetFromTarget);
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
		float num = 1f - horizontalLerpFactor;
		float y = 1f - verticalLerpFactor;
		_fromLerpFactors = new Vector3(num, y, num);
		_toLerpFactors = new Vector3(horizontalLerpFactor, verticalLerpFactor, horizontalLerpFactor);
		_audioListenerTransform = MonoSingleton<AudioListenerController>.Instance.GetComponent<Transform>();
		_audioListenerTransform.localRotation = Quaternion.LookRotation(new Vector3(0f - offsetFromTarget.x, 0f, 0f - offsetFromTarget.z));
	}

	private void PlayerVehicleActivated(Vehicle vehicle)
	{
		target = vehicle.transform;
	}

	private void PlayerVehicleDeactivated(Vehicle vehicle)
	{
		base.enabled = false;
	}

	private void OnDestroy()
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
		Vector3 localPosition = _target.localPosition;
		_audioListenerTransform.localPosition = localPosition + listenerOffset;
		_position = Vector3.Scale(_position, _fromLerpFactors) + Vector3.Scale(localPosition + _offset, _toLerpFactors);
		_transform.localPosition = _position;
	}
}
