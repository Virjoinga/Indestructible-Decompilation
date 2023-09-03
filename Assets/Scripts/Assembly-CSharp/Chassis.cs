using UnityEngine;

[AddComponentMenu("Indestructible/Vehicle/Chassis")]
public class Chassis : WheeledVehiclePhysics, IPhotonObserver, IBrakes
{
	public float maximalBrakeTorque = 5000f;

	private float _brakeFactor;

	private Engine _engine;

	private float _torqueRange;

	private float _invTorqueRange;

	private ITransmission _transmission;

	private ISteeringControl _steeringControl;

	public float maxBrakeTorque
	{
		get
		{
			return maximalBrakeTorque;
		}
	}

	public float brakeFactor
	{
		get
		{
			return _brakeFactor;
		}
		set
		{
			if (_brakeFactor != value)
			{
				_brakeFactor = value;
				float brakeTorque = maximalBrakeTorque * value * 0.25f;
				WheelPhysicsInfo[] array = base.wheelsPhysicsInfo;
				array[0].collider.brakeTorque = brakeTorque;
				array[1].collider.brakeTorque = brakeTorque;
				array[2].collider.brakeTorque = brakeTorque;
				array[3].collider.brakeTorque = brakeTorque;
			}
		}
	}

	public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		base.OnPhotonSerializeView(stream, info);
		if (stream.isWriting)
		{
			stream.SendNext(_transmission.gear | (int)(FloatConvert.ToUint11(_steeringControl.targetSteerAngle, -90f, 1f / 180f) << 3) | (int)(FloatConvert.ToUint14(_engine.torque, 0f, _invTorqueRange) << 14) | (int)(FloatConvert.NormalizedToUint4(brakeFactor) << 28));
			return;
		}
		uint num = (uint)(int)stream.ReceiveNext();
		_transmission.gear = (int)(num & 7);
		_steeringControl.targetSteerAngle = FloatConvert.FromUint11((num >> 3) & 0x7FFu, -90f, 180f);
		_engine.torque = FloatConvert.FromUint14((num >> 14) & 0x3FFFu, 0f, _torqueRange);
		brakeFactor = FloatConvert.NormalizedFromUint4((num >> 28) & 0xFu);
	}

	protected override void Awake()
	{
		base.Awake();
		_engine = GetComponent<Engine>();
		_torqueRange = _engine.GetBaseMaxTorque() * 4f;
		_invTorqueRange = 1f / _torqueRange;
		_transmission = GetExistingComponentIface<ITransmission>();
		_steeringControl = GetExistingComponentIface<ISteeringControl>();
	}

	public override void VehicleActivated(Vehicle vehicle)
	{
		base.VehicleActivated(vehicle);
		if (base.isMine)
		{
			brakeFactor = 1f;
		}
	}
}
