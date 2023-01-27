using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/Vehicle/Steering Control")]
public class SteeringControl : Glu.MonoBehaviour, ISteeringControl, IAdvancedSteeringControl
{
	public int steeringWheelsCount = 2;

	public float maxSteerAngle = 45f;

	public float steerSpeed = 230f;

	public float antiSkidFactor = 0.025f;

	public float neutralAngleThreshold = 12.5f;

	private float _steerAngle;

	private float _targetSteerAngle;

	private float _directionAngle = 360f;

	private float _base;

	private float _half_track;

	private float _steerFactor;

	private float _leftRightRatio = 1f;

	private WheelPhysicsInfo[] _wheelsPhysicsInfo;

	private Transform _transform;

	private ITransmission _transmission;

	private WheeledVehiclePhysics _vehiclePhysics;

	public float steerAngle
	{
		get
		{
			return _steerAngle;
		}
	}

	public float targetSteerAngle
	{
		get
		{
			return _targetSteerAngle;
		}
		set
		{
			_targetSteerAngle = Mathf.Clamp(value, 0f - maxSteerAngle, maxSteerAngle);
			_directionAngle = 360f;
		}
	}

	public Vector2 direction
	{
		set
		{
			_directionAngle = Mathf.Atan2(value.x, value.y) * 57.29578f;
		}
	}

	public float directionAngle
	{
		get
		{
			return _directionAngle;
		}
		set
		{
			_directionAngle = ((!(-180f <= value)) ? 360f : value);
		}
	}

	public float steerFactor
	{
		get
		{
			return _steerFactor;
		}
	}

	public float leftRightRatio
	{
		get
		{
			return _leftRightRatio;
		}
	}

	private void Start()
	{
		_vehiclePhysics = GetComponent<WheeledVehiclePhysics>();
		_wheelsPhysicsInfo = _vehiclePhysics.wheelsPhysicsInfo;
		_base = 57.29578f * Vector3.Distance(_wheelsPhysicsInfo[1].collider.transform.localPosition, _wheelsPhysicsInfo[_wheelsPhysicsInfo.Length - 1].collider.transform.localPosition);
		_half_track = Mathf.Abs(_wheelsPhysicsInfo[0].collider.transform.localPosition.x);
		_transform = base.transform;
		_transmission = GetExistingComponentIface<ITransmission>();
		GetComponent<VehicleFX>().SubscriveToAvatarVisibilityChange(AvatarBecameVisible, AvatarBecameInvisible);
	}

	private void OnDestroy()
	{
		VehicleFX component = GetComponent<VehicleFX>();
		if (component != null)
		{
			component.UnsubscriveFromAvatarVisibilityChange(AvatarBecameVisible, AvatarBecameInvisible);
		}
	}

	private void Update()
	{
		if (_directionAngle <= 180f)
		{
			Vector3 forward = _transform.forward;
			float num = Mathf.Atan2(forward.x, forward.z);
			if (_transmission.gear != 0)
			{
				float y = _vehiclePhysics.angularVelocity.y;
				num += y * y * y * antiSkidFactor;
			}
			_targetSteerAngle = Mathf.Clamp(Util.TrueAngle(_directionAngle - num * 57.29578f), 0f - maxSteerAngle, maxSteerAngle);
		}
		float num2 = steerSpeed * Time.deltaTime;
		SetSteerAngle(_steerAngle + Mathf.Clamp(_targetSteerAngle - _steerAngle, 0f - num2, num2));
	}

	private void SetSteerAngle(float value)
	{
		_steerAngle = value;
		if (Mathf.Abs(value) < neutralAngleThreshold)
		{
			_steerFactor = 0f;
			_leftRightRatio = 1f;
			for (int i = 0; i != steeringWheelsCount; i++)
			{
				_wheelsPhysicsInfo[i].collider.steerAngle = 0f;
			}
			return;
		}
		_steerFactor = value / maxSteerAngle;
		float num = _base / value;
		float num2 = num + _half_track;
		float num3 = num - _half_track;
		_leftRightRatio = num2 / num3;
		float num4 = _base / num2;
		float num5 = _base / num3;
		int num6;
		for (num6 = 0; num6 != steeringWheelsCount; num6++)
		{
			_wheelsPhysicsInfo[num6].collider.steerAngle = num4;
			_wheelsPhysicsInfo[++num6].collider.steerAngle = num5;
		}
	}

	private void AvatarBecameInvisible()
	{
		base.enabled = false;
	}

	private void AvatarBecameVisible()
	{
		base.enabled = true;
	}
}
