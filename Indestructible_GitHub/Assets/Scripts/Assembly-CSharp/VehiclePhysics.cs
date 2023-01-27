using Glu;
using UnityEngine;

public class VehiclePhysics : Glu.MonoBehaviour, IVehicleActivationObserver
{
	private const float MaxSyncLatency = 0.5f;

	private const float InvMaxSyncLatency = 2f;

	private const float MaxLiveLatency = 13f;

	private const float RotationLerpFactor = 0.22500001f;

	private const float PositionCorrectionVelocityFactor = 2f;

	private const float VelocityLerpFactor = 0.35f;

	private const float MaxExtrapolationSqrVertSpeed = 169f;

	private const float MaxExtrapolationSqrSpeed = 2500f;

	private const float ExtrapolationFactor = 2.366864E-06f;

	private const float InvisiblePositionLerpFactor = 0.35f;

	public Transform centerOfMass;

	private float _sqrSpeed;

	private Vector3 _velocity;

	private Vector3 _angularVelocity;

	private Vector3 _position;

	private Quaternion _rotation;

	private Rigidbody _rigidbody;

	private Vector3 _serverPosition;

	private Quaternion _serverRotation;

	private Vector3 _serverVelocity;

	private Vector3 _serverAngularVelocity;

	private float _serverMessageLatency;

	private float _serverTimestamp;

	private PhotonView _photonView;

	private bool _isMine;

	private bool _isInvisibleAvatar;

	public Vector3 velocity
	{
		get
		{
			return _velocity;
		}
		set
		{
			_rigidbody.velocity = (_velocity = value);
			_sqrSpeed = _velocity.sqrMagnitude;
		}
	}

	public float sqrSpeed
	{
		get
		{
			return _sqrSpeed;
		}
	}

	public Vector3 angularVelocity
	{
		get
		{
			return _angularVelocity;
		}
		set
		{
			_rigidbody.angularVelocity = (_angularVelocity = value);
		}
	}

	public Vector3 position
	{
		get
		{
			return _position;
		}
		set
		{
			_rigidbody.position = (_position = value);
		}
	}

	public Quaternion rotation
	{
		get
		{
			return _rotation;
		}
		set
		{
			_rigidbody.rotation = (_rotation = value);
		}
	}

	public float serverMessageLatency
	{
		get
		{
			return _serverMessageLatency;
		}
	}

	public bool isMine
	{
		get
		{
			return _isMine;
		}
	}

	protected new Rigidbody rigidbody
	{
		get
		{
			return _rigidbody;
		}
	}

	protected bool isInvisibleAvatar
	{
		get
		{
			return _isInvisibleAvatar;
		}
	}

	protected virtual void Awake()
	{
		if (PhotonNetwork.room != null)
		{
			_photonView = GetComponent<PhotonView>();
		}
		_rigidbody = GetComponent<Rigidbody>();
		_serverPosition = (_position = _rigidbody.position);
		_serverRotation = (_rotation = _rigidbody.rotation);
		_serverTimestamp = Time.time;
	}

	protected virtual void Start()
	{
		GetComponent<Vehicle>().AddActivationObserver(this);
		GetComponent<VehicleFX>().SubscriveToAvatarVisibilityChange(AvatarBecameVisible, AvatarBecameInvisible);
	}

	public virtual void VehicleActivated(Vehicle vehicle)
	{
		float mass = _rigidbody.mass;
		Vector3 inertiaTensor = centerOfMass.localScale * mass;
		_rigidbody.centerOfMass = centerOfMass.localPosition;
		_rigidbody.inertiaTensor = inertiaTensor;
		_rigidbody.inertiaTensorRotation = centerOfMass.localRotation;
		_serverPosition = (_position = vehicle.transform.position);
		Vector3 vector = (velocity = Vector3.zero);
		angularVelocity = vector;
		CheckOwnership();
	}

	protected virtual void OnDestroy()
	{
		VehicleFX component = GetComponent<VehicleFX>();
		if (component != null)
		{
			component.UnsubscriveFromAvatarVisibilityChange(AvatarBecameVisible, AvatarBecameInvisible);
		}
	}

	public Vector3 ExtrapolatePosition(float dt)
	{
		return _position + _velocity * dt;
	}

	public Vector3 ExtrapolatePosition(float dt, Vector3 velocity)
	{
		return _position + velocity * dt;
	}

	public Quaternion ExtrapolateRotation(float dt)
	{
		return _rotation * Quaternion.AngleAxis(_angularVelocity.magnitude * dt * 57.29578f, _angularVelocity);
	}

	public Quaternion ExtrapolateRotation(float dt, Vector3 angularVelocity)
	{
		return _rotation * Quaternion.AngleAxis(angularVelocity.magnitude * dt * 57.29578f, angularVelocity);
	}

	protected void UpdatePhysicsValues()
	{
		_velocity = rigidbody.velocity;
		_sqrSpeed = _velocity.sqrMagnitude;
		_angularVelocity = _rigidbody.angularVelocity;
		_position = _rigidbody.position;
		_rotation = _rigidbody.rotation;
	}

	protected void LerpToServer()
	{
		float num = Time.time - _serverTimestamp;
		if (num < 0.5f)
		{
			float num2 = (0.5f - num) * 2f;
			angularVelocity = _serverAngularVelocity;
			rotation = Quaternion.Lerp(_rotation, _serverRotation, 0.22500001f * num2);
			float num3 = _serverVelocity.y * _serverVelocity.y;
			Vector3 vector = _serverPosition - _position;
			if (num3 < 169f)
			{
				float num4 = num3 + _serverVelocity.x * _serverVelocity.x + _serverVelocity.z * _serverVelocity.z;
				num *= (2500f - num4) * (169f - _serverVelocity.y) * 2.366864E-06f * num2;
				vector.x += _serverVelocity.x * num;
				vector.y += _serverVelocity.y * num;
				vector.z += _serverVelocity.z * num;
			}
			velocity = Vector3.Lerp(_velocity, _serverVelocity + vector * 2f, 0.35f);
		}
		else
		{
			CheckLatencyOverflow(num);
		}
	}

	protected void LerpInvisibleToServer()
	{
		_position.x += (_serverPosition.x - _position.x) * 0.35f;
		_position.y += (_serverPosition.y - _position.y) * 0.35f;
		_position.z += (_serverPosition.z - _position.z) * 0.35f;
		_rigidbody.position = _position;
		float num = Time.time - _serverTimestamp;
		if (num < 0.5f)
		{
			velocity = _serverVelocity * (0.5f - num) * 2f;
			rotation = _serverRotation;
		}
		else
		{
			velocity = Vector3.zero;
			CheckLatencyOverflow(num);
		}
	}

	private void CheckLatencyOverflow(float latency)
	{
		if (13f < latency)
		{
			GetComponent<Destructible>().Die(DestructionReason.Disconnect);
		}
	}

	private void CheckOwnership()
	{
		_isMine = _photonView == null || _photonView.isMine;
	}

	protected virtual void OnMasterClientSwitched(PhotonPlayer player)
	{
		CheckOwnership();
	}

	public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			Vector3 vector = _velocity;
			if (4096f <= _sqrSpeed)
			{
				vector *= 63.9f / Mathf.Sqrt(_sqrSpeed);
			}
			stream.SendNext((long)(((ulong)(uint)((int)BitConverter32.ToUint(_position.x) & -2048) >> 11) | ((ulong)(uint)((int)BitConverter32.ToUint(_position.y) & -1024) << 11) | ((ulong)(uint)((int)BitConverter32.ToUint(_position.z) & -2048) << 32)));
			stream.SendNext((long)((FloatConvert.ToUint9(_rotation.x, -1f, 0.5f) & 0x1FF) | ((ulong)(FloatConvert.ToUint10(_rotation.y, -1f, 0.5f) & 0x3FF) << 9) | ((ulong)(FloatConvert.ToUint10(_rotation.z, -1f, 0.5f) & 0x3FF) << 19) | ((ulong)(FloatConvert.ToUint9(_rotation.w, -1f, 0.5f) & 0x1FF) << 29) | ((ulong)(FloatConvert.ToUint10(vector.x, -64f, 1f / 128f) & 0x3FF) << 38) | ((ulong)(FloatConvert.ToUint8(_angularVelocity.x, -5f, 0.1f) & 0xFF) << 48) | ((ulong)(FloatConvert.ToUint8(_angularVelocity.y, -5f, 0.1f) & 0xFF) << 56)));
			stream.SendNext((int)((FloatConvert.ToUint12(vector.y, -64f, 1f / 128f) & 0xFFF) | ((FloatConvert.ToUint12(vector.z, -64f, 1f / 128f) & 0xFFF) << 12) | ((FloatConvert.ToUint8(_angularVelocity.z, -5f, 0.1f) & 0xFF) << 24)));
			return;
		}
		_serverMessageLatency = (float)(PhotonNetwork.time - info.timestamp);
		_serverTimestamp = Time.time - _serverMessageLatency;
		ulong num = (ulong)(long)stream.ReceiveNext();
		_serverPosition.x = BitConverter32.ToFloat((uint)(int)(num << 11) & 0xFFFFF800u);
		_serverPosition.y = BitConverter32.ToFloat((uint)(int)(num >> 11) & 0xFFFFFC00u);
		_serverPosition.z = BitConverter32.ToFloat((uint)(int)(num >> 32) & 0xFFFFF800u);
		num = (ulong)(long)stream.ReceiveNext();
		_serverRotation.x = FloatConvert.FromUint9((uint)(num & 0x1FF), -1f, 2f);
		_serverRotation.y = FloatConvert.FromUint10((uint)(int)(num >> 9) & 0x3FFu, -1f, 2f);
		_serverRotation.z = FloatConvert.FromUint10((uint)(int)(num >> 19) & 0x3FFu, -1f, 2f);
		_serverRotation.w = FloatConvert.FromUint9((uint)(int)(num >> 29) & 0x1FFu, -1f, 2f);
		_serverVelocity.x = FloatConvert.FromUint10((uint)(int)(num >> 38) & 0x3FFu, -64f, 128f);
		_serverAngularVelocity.x = FloatConvert.FromUint8((uint)(int)(num >> 48) & 0xFFu, -5f, 10f);
		_serverAngularVelocity.y = FloatConvert.FromUint8((uint)(int)(num >> 56) & 0xFFu, -5f, 10f);
		uint num2 = (uint)(int)stream.ReceiveNext();
		_serverVelocity.y = FloatConvert.FromUint12(num2 & 0xFFFu, -64f, 128f);
		_serverVelocity.z = FloatConvert.FromUint12((num2 >> 12) & 0xFFFu, -64f, 128f);
		_serverAngularVelocity.z = FloatConvert.FromUint8((num2 >> 24) & 0xFFu, -5f, 10f);
	}

	protected virtual void AvatarBecameInvisible()
	{
		_rigidbody.useGravity = false;
		_isInvisibleAvatar = true;
	}

	protected virtual void AvatarBecameVisible()
	{
		position = _serverPosition;
		_rigidbody.useGravity = true;
		_isInvisibleAvatar = false;
	}
}
