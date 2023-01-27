using UnityEngine;

[AddComponentMenu("Indestructible/Vehicle/Wheeled Vehicle FX")]
public class WheeledVehicleFX : VehicleFX
{
	public struct WheelVisualFX
	{
		public Transform wheelTransform;

		public float wheelTargetPosition;

		public float wheelRadius;

		public float wheelRollAngle;

		public int axis;

		public float wheelOrientation;

		public float wheelRollScale;

		public Collider groundCollider;

		public int groundType;

		public DustEffect dustEffect;

		public float dustEmissionInterval;
	}

	public GameObject[] wheelVisualPrefabs;

	public float wheelsPositionLerpFactor = 0.5f;

	public float constFactor = 1f;

	public float rpmFactor = 1f;

	public float slipFactor = 1f;

	private Transform _wheelsParent;

	private GroundEffectsManager _groundEffectsManager;

	private VehiclePhysics _vehiclePhysics;

	private WheelVisualFX[] _wheelsVisualFX;

	private WheelPhysicsInfo[] _wheelsPhysicsInfo;

	private Collider _cachedGroundCollider;

	private int _cachedGroundType = -1;

	public WheelVisualFX[] wheelsVisualFX
	{
		get
		{
			return _wheelsVisualFX;
		}
	}

	protected override bool shouldUpdate
	{
		get
		{
			return base.isVisible;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_vehiclePhysics = GetComponent<VehiclePhysics>();
		base.vehicle.SubscribeToMountedEvent(VehiclePartsMounted);
		_wheelsPhysicsInfo = new WheelPhysicsInfo[0];
	}

	private void VehiclePartsMounted(Vehicle vehicle)
	{
		int num = wheelVisualPrefabs.Length;
		if (0 >= num)
		{
			return;
		}
		_wheelsVisualFX = new WheelVisualFX[num * 2];
		int num2 = 0;
		GameObject gameObject = wheelVisualPrefabs[num2];
		Transform transform = gameObject.transform;
		_wheelsParent = transform.parent;
		while (true)
		{
			GameObject gameObject2 = (GameObject)Object.Instantiate(gameObject, Vector3.zero, Quaternion.identity);
			Transform transform2 = gameObject2.transform;
			transform2.parent = _wheelsParent;
			Vector3 localPosition = transform.localPosition;
			localPosition.x = 0f - localPosition.x;
			transform2.localPosition = localPosition;
			transform2.localScale = transform.localScale;
			int num3 = num2 * 2;
			_wheelsVisualFX[num3].wheelTransform = transform;
			_wheelsVisualFX[num3].axis = num2;
			_wheelsVisualFX[num3].wheelOrientation = 0f;
			_wheelsVisualFX[num3].wheelRollScale = 6f;
			_wheelsVisualFX[num3].groundType = -1;
			num3++;
			_wheelsVisualFX[num3].wheelTransform = transform2;
			_wheelsVisualFX[num3].axis = num2;
			_wheelsVisualFX[num3].wheelOrientation = 180f;
			_wheelsVisualFX[num3].wheelRollScale = -6f;
			_wheelsVisualFX[num3].groundType = -1;
			if (++num2 == num)
			{
				break;
			}
			gameObject = wheelVisualPrefabs[num2];
			transform = gameObject.transform;
		}
		_groundEffectsManager = GroundEffectsManager.instance;
		_wheelsPhysicsInfo = _wheelsParent.gameObject.GetComponent<WheeledVehiclePhysics>().wheelsPhysicsInfo;
		int i = 0;
		for (int num4 = _wheelsPhysicsInfo.Length; i != num4; i++)
		{
			WheelCollider wheelCollider = _wheelsPhysicsInfo[i].collider;
			_wheelsVisualFX[i].wheelTargetPosition = wheelCollider.transform.localPosition.y - wheelCollider.suspensionDistance;
			_wheelsVisualFX[i].wheelRadius = wheelCollider.radius;
		}
	}

	protected override void Update()
	{
		base.Update();
		int i = 0;
		for (int num = _wheelsPhysicsInfo.Length; i != num; i++)
		{
			UpdateWheel(ref _wheelsPhysicsInfo[i], ref _wheelsVisualFX[i]);
		}
	}

	private void UpdateWheel(ref WheelPhysicsInfo wheelInfo, ref WheelVisualFX wheelFX)
	{
		Vector3 localPosition = wheelFX.wheelTransform.localPosition;
		float rpm = wheelInfo.collider.rpm;
		float deltaTime = Time.deltaTime;
		float num;
		if (!wheelInfo.isGrounded)
		{
			num = wheelFX.wheelTargetPosition - localPosition.y;
		}
		else
		{
			Vector3 point = wheelInfo.hit.point;
			Vector3 vector = _wheelsParent.InverseTransformPoint(point);
			num = wheelFX.wheelRadius - Vector3.Dot(localPosition - vector, new Vector3(0f, 1f, 0f));
			CheckGroundCollider(wheelInfo.hit.collider, ref wheelFX);
			DustEffect dustEffect = wheelFX.dustEffect;
			if (dustEffect != null)
			{
				float num2 = (Mathf.Abs(wheelInfo.hit.forwardSlip) + Mathf.Abs(wheelInfo.hit.sidewaysSlip)) * dustEffect.slipFactor * slipFactor + Mathf.Abs(rpm) * dustEffect.rpmFactor * rpmFactor;
				if (0.1f < num2)
				{
					num2 = dustEffect.constFactor * constFactor + num2;
					float num3 = wheelFX.dustEmissionInterval - num2 * deltaTime;
					if (num3 < 0f)
					{
						num3 = 1f / dustEffect.rndEmissionRate;
						point.y += 0.5f;
						dustEffect.EmitRandomSizeLifetimeRotation(1, point, dustEffect.worldVelocity + _vehiclePhysics.velocity * dustEffect.emitterVelocityScale, num2, 1f);
					}
					wheelFX.dustEmissionInterval = num3;
				}
			}
		}
		localPosition.y += num * wheelsPositionLerpFactor;
		wheelFX.wheelTransform.localPosition = localPosition;
		wheelFX.wheelRollAngle += deltaTime * rpm * wheelFX.wheelRollScale;
		float angle = wheelInfo.collider.steerAngle + wheelFX.wheelOrientation;
		wheelFX.wheelTransform.localRotation = Quaternion.AngleAxis(angle, Vector3.up) * Quaternion.AngleAxis(wheelFX.wheelRollAngle, Vector3.right);
	}

	private void CheckGroundCollider(Collider groundCollider, ref WheelVisualFX wheelFX)
	{
		if (groundCollider != wheelFX.groundCollider)
		{
			int num;
			if (groundCollider == _cachedGroundCollider)
			{
				num = _cachedGroundType;
			}
			else
			{
				SurfaceInfo component = groundCollider.GetComponent<SurfaceInfo>();
				num = ((!(component != null)) ? (-1) : component.groundType);
			}
			_cachedGroundCollider = wheelFX.groundCollider;
			wheelFX.groundCollider = groundCollider;
			if (num != wheelFX.groundType)
			{
				_cachedGroundType = wheelFX.groundType;
				wheelFX.groundType = num;
				wheelFX.dustEffect = ((num >= 0) ? _groundEffectsManager.GetDustEffect(num) : null);
			}
		}
	}
}
