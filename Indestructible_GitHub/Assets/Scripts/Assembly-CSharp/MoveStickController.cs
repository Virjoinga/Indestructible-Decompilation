using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/Vehicle/Move Stick Controller")]
public class MoveStickController : Glu.MonoBehaviour
{
	private const float StickThreshold = 0.1f;

	public float timeToActivateCorrectionMode = 0.25f;

	public float startManeuversMinStickDeviation = 0.1f;

	public float speedDropTestMinSpeed = 1f;

	public float startManeuversSpeedDropFactor = 0.8f;

	public float startManeuversMaxSpeed = 1.25f;

	public float startManeuversTorqueFactor = 0.6f;

	public float startManeuversStickDirDeviationOnImpact = 1f;

	public float startManeuversStickDirDeviationOnPush = 0.8f;

	public float maneuversTime = 0.8f;

	public float maneuversBrakeAfterTime = 0.25f;

	public float maneuversBackSpeed;

	public float maneuversFrontUpSpeed = -4f;

	public float maneuversRotateSpeed = 10f;

	private StickScript _moveStick;

	private Transform _vehicleTransform;

	private Rigidbody _vehicleRigidbody;

	private ISteeringControl _vehicleSteeringControl;

	private Engine _vehicleEngine;

	private ITransmission _vehicleTransmission;

	private IBrakes _vehicleBrakes;

	private int _checkModeCounter;

	private int _checkModeCountToActivate;

	private Vector3 _forward;

	private float _forwardSpeed;

	private float _maneuversRemainingTime;

	private float _maneuverRotateSpeed;

	private float _maneuversBackSpeed;

	private bool _isManeuversModeActive;

	private bool _isDriving;

	protected virtual void Start()
	{
		PlayerVehicle component = GetComponent<PlayerVehicle>();
		if (component != null && component.isMine)
		{
			Init(component);
		}
		else
		{
			Object.Destroy(this);
		}
	}

	protected virtual void Init(Vehicle vehicle)
	{
		_moveStick = MoveStick.instance;
		_vehicleTransform = base.transform;
		_vehicleRigidbody = GetComponent<Rigidbody>();
		_vehicleEngine = GetComponent<Engine>();
		_vehicleSteeringControl = GetExistingComponentIface<ISteeringControl>();
		_vehicleTransmission = GetExistingComponentIface<ITransmission>();
		_vehicleBrakes = GetExistingComponentIface<IBrakes>();
		_checkModeCountToActivate = Mathf.CeilToInt(timeToActivateCorrectionMode / Time.fixedDeltaTime);
	}

	protected virtual void OnEnable()
	{
		if (_isManeuversModeActive)
		{
			StopManeuvers();
			return;
		}
		_forwardSpeed = 0f;
		_checkModeCounter = 0;
	}

	protected virtual void FixedUpdate()
	{
		Vector2 vector = _moveStick.GetVector();
		if (CheckManeuversMode(vector))
		{
			return;
		}
		if (vector.sqrMagnitude < 0.05f)
		{
			_vehicleEngine.throttle = 0f;
			if (_isDriving)
			{
				_isDriving = false;
				_vehicleBrakes.brakeFactor = 1f;
				_vehicleSteeringControl.targetSteerAngle = 0f;
			}
		}
		else
		{
			_vehicleSteeringControl.direction = vector;
			if (!_isDriving)
			{
				_isDriving = true;
				_vehicleEngine.throttle = 1f;
				_vehicleBrakes.brakeFactor = 0f;
			}
		}
	}

	private bool CheckManeuversMode(Vector3 stickVector)
	{
		if (_isManeuversModeActive)
		{
			UpdateManeuver();
			return true;
		}
		if (startManeuversMinStickDeviation < stickVector.sqrMagnitude)
		{
			Vector3 velocity = _vehicleRigidbody.velocity;
			Vector3 forward = _forward;
			_forward = _vehicleTransform.forward;
			if (speedDropTestMinSpeed < _forwardSpeed && _forwardSpeed * startManeuversSpeedDropFactor < _forwardSpeed - Vector3.Dot(velocity, forward))
			{
				stickVector.Normalize();
				Vector2 rhs = new Vector2(_forward.x, _forward.z);
				rhs.Normalize();
				if (Vector2.Dot(stickVector, rhs) <= startManeuversStickDirDeviationOnImpact)
				{
					StartManeuvers();
					return true;
				}
				_forwardSpeed = 0f;
				return false;
			}
			_forwardSpeed = Vector3.Dot(velocity, _forward);
			if (-0.25f <= _forwardSpeed && _forwardSpeed < startManeuversMaxSpeed && _vehicleEngine.GetMaxTorque() * startManeuversTorqueFactor < _vehicleEngine.torque)
			{
				_checkModeCounter++;
				stickVector.Normalize();
				Vector2 rhs2 = new Vector2(_forward.x, _forward.z);
				rhs2.Normalize();
				if (Vector2.Dot(stickVector, rhs2) <= startManeuversStickDirDeviationOnPush && _checkModeCountToActivate < _checkModeCounter)
				{
					StartManeuvers();
					return true;
				}
			}
			else
			{
				_checkModeCounter = 0;
			}
		}
		else
		{
			_forwardSpeed = 0f;
		}
		return false;
	}

	private void StartManeuvers()
	{
		_isManeuversModeActive = true;
		_isDriving = true;
		_vehicleBrakes.brakeFactor = 0f;
		_vehicleTransmission.gear = 0;
		_maneuversRemainingTime = maneuversTime;
		_maneuversBackSpeed = maneuversBackSpeed;
		Vector3 right = _vehicleTransform.right;
		Vector2 vector = _moveStick.GetVector();
		_maneuverRotateSpeed = Mathf.Sign(Vector2.Dot(new Vector2(right.x, right.z), vector));
		_vehicleSteeringControl.targetSteerAngle = -90f * _maneuverRotateSpeed;
		_maneuverRotateSpeed *= maneuversRotateSpeed;
		UpdateManeuver();
	}

	private void UpdateManeuver()
	{
		float deltaTime = Time.deltaTime;
		if (maneuversBrakeAfterTime < _maneuversRemainingTime)
		{
			if (_maneuversBackSpeed < 0f)
			{
				_vehicleRigidbody.AddRelativeForce(new Vector3(0f, 0f, _maneuversBackSpeed * deltaTime), ForceMode.VelocityChange);
				_maneuversBackSpeed *= 0.5f;
			}
			_vehicleRigidbody.AddRelativeTorque(new Vector3(maneuversFrontUpSpeed * deltaTime, _maneuverRotateSpeed * deltaTime, 0f), ForceMode.VelocityChange);
			if ((_maneuversRemainingTime -= deltaTime) < maneuversBrakeAfterTime)
			{
				_vehicleBrakes.brakeFactor = 1f;
			}
		}
		else if ((_maneuversRemainingTime -= deltaTime) < 0f)
		{
			StopManeuvers();
		}
	}

	private void StopManeuvers()
	{
		_vehicleTransmission.gear = 1;
		_forwardSpeed = 0f;
		_checkModeCounter = 0;
		_isManeuversModeActive = false;
		_isDriving = false;
	}
}
