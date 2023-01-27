using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/Vehicle/Transmission 4WD")]
public class Transmission4WD : Glu.MonoBehaviour, ITransmission
{
	private const float RpmThreshold = 5f;

	private const float TorqueThreshold = 5f;

	private const float SteerFactorThreshold = 0.05f;

	public Differential frontDiff;

	public Differential rearDiff;

	public Differential centerDiff;

	public float steeringAssistance = 0.25f;

	public float steerDecreaseTorqueFactor = 0.7f;

	public float torqueThreshold = 100f;

	public float[] gears = new float[4] { -8f, 20f, 13f, 9f };

	public float shiftUpRpm = 8000f;

	public float shiftDownRpm = 6000f;

	public float shiftUpTime = 0.25f;

	public float shiftDownTime = 0.1f;

	private Engine _engine;

	private IAdvancedSteeringControl _steeringControl;

	private float _wheelsRpm;

	private float _engineTorque;

	private float _steerFactor;

	private float _gearRatio;

	private float _shiftEndTime;

	private int _gear;

	private WheelPhysicsInfo[] _wheelsPhysicsInfo;

	public int gear
	{
		get
		{
			return _gear;
		}
		set
		{
			if (value < _gear)
			{
				_gear = value;
				_engineTorque = -100f;
				if (value != 0 && shiftDownTime != 0f)
				{
					_gearRatio = 0f;
					_shiftEndTime = Time.time + shiftDownTime;
				}
				else
				{
					_gearRatio = gears[value];
				}
			}
			else if (_gear < value)
			{
				_engineTorque = -100f;
				if (_gear != 0 && shiftUpTime != 0f)
				{
					_gearRatio = 0f;
					_shiftEndTime = Time.time + shiftUpTime;
				}
				else
				{
					_gearRatio = gears[value];
				}
				_gear = value;
			}
		}
	}

	private void Awake()
	{
		_steeringControl = GetExistingComponentIface<IAdvancedSteeringControl>();
		_engine = GetComponent<Engine>();
	}

	private void Start()
	{
		_wheelsPhysicsInfo = GetComponent<WheeledVehiclePhysics>().wheelsPhysicsInfo;
		_gear = 1;
		_gearRatio = gears[_gear];
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
		float rpm = _wheelsPhysicsInfo[0].collider.rpm;
		float rpm2 = _wheelsPhysicsInfo[1].collider.rpm;
		float num = rpm + rpm2;
		int num2 = _wheelsPhysicsInfo.Length - 1;
		float rpm3 = _wheelsPhysicsInfo[num2].collider.rpm;
		float rpm4 = _wheelsPhysicsInfo[num2 - 1].collider.rpm;
		float num3 = rpm4 + rpm3;
		float num4 = (num + num3) * 0.25f;
		_engine.UpdateRpm(num4, _gearRatio);
		float torque = _engine.torque;
		float steerFactor = _steeringControl.steerFactor;
		if (_gearRatio == 0f)
		{
			CheckShifting();
		}
		else if (5f < Mathf.Abs(num4 - _wheelsRpm) || 5f < Mathf.Abs(torque - _engineTorque))
		{
			_wheelsRpm = num4;
			CheckGear(num4);
		}
		else if (Mathf.Abs(steerFactor - _steerFactor) < 0.05f)
		{
			return;
		}
		_engineTorque = torque;
		torque *= _gearRatio;
		_steerFactor = steerFactor;
		float num5 = Mathf.Abs(steerFactor);
		if (0 < _gear)
		{
			torque *= 1f - num5 * steerDecreaseTorqueFactor;
		}
		float num6 = torque * num5 * steeringAssistance;
		torque -= num6;
		float num7 = centerDiff.DistributeTorque(num, num3);
		float num8 = torque * num7;
		float num9 = torque - num8;
		float leftRightRatio = _steeringControl.leftRightRatio;
		float num10 = frontDiff.DistributeTorque(rpm2 * leftRightRatio, rpm);
		float num11 = num8 * num10;
		float num12 = num8 - num11;
		if (steerFactor < 0f)
		{
			num11 += num6;
		}
		else
		{
			num12 += num6;
		}
		_wheelsPhysicsInfo[0].collider.motorTorque = num12;
		_wheelsPhysicsInfo[1].collider.motorTorque = num11;
		float num13 = rearDiff.DistributeTorque(rpm3 * leftRightRatio, rpm4);
		float num14 = 1f / (float)((_wheelsPhysicsInfo.Length - 2) / 2);
		float num15 = num9 * num13;
		float motorTorque = (num9 - num15) * num14;
		num15 *= num14;
		for (int i = 2; i < _wheelsPhysicsInfo.Length; i += 2)
		{
			_wheelsPhysicsInfo[i].collider.motorTorque = motorTorque;
			_wheelsPhysicsInfo[i + 1].collider.motorTorque = num15;
		}
	}

	private void CheckShifting()
	{
		if (_shiftEndTime <= Time.time)
		{
			_gearRatio = gears[_gear];
		}
	}

	private void CheckGear(float wheelsRpm)
	{
		if (_gear <= 0)
		{
			return;
		}
		float time = Time.time;
		if (shiftUpRpm < _engine.rpm)
		{
			if (_gear < gears.Length - 1)
			{
				_gear++;
				if (shiftUpTime != 0f)
				{
					_gearRatio = 0f;
					_shiftEndTime = time + shiftUpTime;
				}
				else
				{
					_gearRatio = gears[_gear];
				}
			}
		}
		else
		{
			if (1 >= _gear)
			{
				return;
			}
			float num = gears[_gear - 1] * wheelsRpm;
			if (num < shiftDownRpm)
			{
				_gear--;
				if (shiftDownTime != 0f)
				{
					_gearRatio = 0f;
					_shiftEndTime = time + shiftDownTime;
				}
				else
				{
					_gearRatio = gears[_gear];
				}
			}
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
