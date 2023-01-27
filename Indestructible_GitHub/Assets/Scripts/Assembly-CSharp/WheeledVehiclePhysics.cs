using UnityEngine;

public class WheeledVehiclePhysics : VehiclePhysics
{
	private const float RolloverMinNormalUpFactor = 0.4f;

	private const float RolloverMinAngularSqrVelocity = 0.17f;

	private const float StabilizeOffsetTreshold = 0.02f;

	public WheelCollider[] wheelColliderPrefabs;

	public float stabilizationFactor = 35f;

	public float maxStabilizationOffset = 1.25f;

	public float rolloverFactor = 4f;

	private WheelPhysicsInfo[] _wheelsPhysicsInfo;

	private Vector3 _groundNormal;

	private int _groundedWheelsCount;

	private Transform _transform;

	private float _currentStabilizeOffset;

	private Vector3 _COMLocalPosition;

	public WheelPhysicsInfo[] wheelsPhysicsInfo
	{
		get
		{
			return _wheelsPhysicsInfo;
		}
	}

	public int groundedWheelsCount
	{
		get
		{
			return _groundedWheelsCount;
		}
	}

	public Vector3 groundNormal
	{
		get
		{
			return _groundNormal;
		}
	}

	protected new Transform transform
	{
		get
		{
			return _transform;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_transform = GetComponent<Transform>();
		int num = wheelColliderPrefabs.Length;
		_wheelsPhysicsInfo = new WheelPhysicsInfo[num * 2];
		for (int i = 0; i != num; i++)
		{
			WheelCollider wheelCollider = wheelColliderPrefabs[i];
			Transform transform = wheelCollider.transform;
			GameObject gameObject = new GameObject("wheelCollider");
			gameObject.layer = wheelCollider.gameObject.layer;
			Transform transform2 = gameObject.transform;
			transform2.parent = _transform;
			Vector3 localPosition = transform.localPosition;
			localPosition.x = 0f - localPosition.x;
			transform2.localPosition = localPosition;
			Vector3 localEulerAngles = transform.localEulerAngles;
			localEulerAngles.y = 0f - localEulerAngles.y;
			transform2.localEulerAngles = localEulerAngles;
			transform2.localScale = Vector3.one;
			WheelCollider wheelCollider2 = gameObject.AddComponent<WheelCollider>();
			wheelCollider2.radius = wheelCollider.radius;
			wheelCollider2.suspensionDistance = wheelCollider.suspensionDistance;
			wheelCollider2.suspensionSpring = wheelCollider.suspensionSpring;
			wheelCollider2.mass = wheelCollider.mass;
			wheelCollider2.forwardFriction = wheelCollider.forwardFriction;
			wheelCollider2.sidewaysFriction = wheelCollider.sidewaysFriction;
			_wheelsPhysicsInfo[i * 2].collider = wheelCollider;
			_wheelsPhysicsInfo[i * 2 + 1].collider = wheelCollider2;
		}
		_COMLocalPosition = centerOfMass.localPosition;
		rolloverFactor = 0f - rolloverFactor;
	}

	private void FixedUpdate()
	{
		if (!base.isInvisibleAvatar)
		{
			UpdatePhysicsValues();
			Vector3 vector = Vector3.zero;
			int num = 0;
			int i = 0;
			for (int num2 = _wheelsPhysicsInfo.Length; i != num2; i++)
			{
				if (_wheelsPhysicsInfo[i].collider.GetGroundHit(out _wheelsPhysicsInfo[i].hit))
				{
					_wheelsPhysicsInfo[i].isGrounded = true;
					vector += _wheelsPhysicsInfo[i].hit.normal;
					num++;
					for (i++; i != num2; i++)
					{
						if (_wheelsPhysicsInfo[i].collider.GetGroundHit(out _wheelsPhysicsInfo[i].hit))
						{
							_wheelsPhysicsInfo[i].isGrounded = true;
							vector += _wheelsPhysicsInfo[i].hit.normal;
							num++;
						}
						else
						{
							_wheelsPhysicsInfo[i].isGrounded = false;
						}
					}
					vector = (_groundNormal = vector / num);
					break;
				}
				_wheelsPhysicsInfo[i].isGrounded = false;
			}
			_groundedWheelsCount = num;
			Vector3 lhs = default(Vector3);
			lhs.y = 0f;
			float num3;
			float num4;
			if (num != 0)
			{
				lhs = base.rotation * Vector3.up;
				num3 = stabilizationFactor * (1f - Vector3.Dot(lhs, vector));
				num3 = num3 * num3 * num3;
				if (maxStabilizationOffset < num3)
				{
					num3 = maxStabilizationOffset;
				}
				num4 = num3 - _currentStabilizeOffset;
			}
			else
			{
				num3 = 0f;
				num4 = _currentStabilizeOffset;
			}
			_currentStabilizeOffset = num3;
			if (0.02f < num4 * num4)
			{
				base.rigidbody.centerOfMass = new Vector3(_COMLocalPosition.x, _COMLocalPosition.y - num3, _COMLocalPosition.z);
			}
			if (base.isMine)
			{
				if (num != 0 && !(lhs.y < 0.4f))
				{
					return;
				}
				Vector3 vector2 = base.angularVelocity;
				if (vector2.x * vector2.x + vector2.z * vector2.z < 0.17f)
				{
					Vector3 vector3 = base.rotation * Vector3.forward;
					Vector3 vector4 = transform.InverseTransformDirection(Vector3.up);
					if (vector4.y < -0.25f)
					{
						vector2 += vector3 * (Mathf.Sign(vector4.x) * (0.5f - vector4.y * 0.5f) * rolloverFactor);
					}
					else
					{
						vector4.x = vector4.x * vector4.x * vector4.x;
						vector4.z = vector4.z * vector4.z * vector4.z;
						vector2 += vector3 * (vector4.x * rolloverFactor);
						vector2 -= base.rotation * Vector3.right * (vector4.z * rolloverFactor);
					}
					base.angularVelocity = vector2;
				}
			}
			else
			{
				LerpToServer();
			}
		}
		else
		{
			LerpInvisibleToServer();
		}
	}

	protected override void AvatarBecameInvisible()
	{
		WheelPhysicsInfo[] array = wheelsPhysicsInfo;
		int i = 0;
		for (int num = array.Length; i != num; i++)
		{
			array[i].collider.enabled = false;
		}
		base.AvatarBecameInvisible();
	}

	protected override void AvatarBecameVisible()
	{
		if (base.isInvisibleAvatar)
		{
			WheelPhysicsInfo[] array = wheelsPhysicsInfo;
			int i = 0;
			for (int num = array.Length; i != num; i++)
			{
				array[i].collider.enabled = true;
			}
			base.AvatarBecameVisible();
		}
	}
}
