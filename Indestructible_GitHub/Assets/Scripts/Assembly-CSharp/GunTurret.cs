using System.Collections;
using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/Weapons/GunTurret")]
public class GunTurret : Glu.MonoBehaviour
{
	public delegate void AimTargetChangedDelegate(Collider target, GunTurret gunTurret);

	public float aimTargetsDetectionThreshold = 0.9f;

	public float baseFullAimTreshold = 0.9f;

	public bool shouldVerticalAiming = true;

	public bool isLocalTargetDirection;

	public bool shouldExtrapolateAiming;

	public float findAimTargetInterval = 0.25f;

	public float targetDirectionSmoothFactor = 7.5f;

	public float avatarDirectionSmoothFactor = 19f;

	public float aimLerpFactor = 0.6f;

	public Transform aimZoneCenter;

	public float aimZoneRadius;

	private Transform _transform;

	private Vector3 _targetVector = new Vector3(0f, 0f, 1f);

	private float _projectileSpeedFactor = 0.01f;

	private Vector3 _position;

	private Vector3 _forward = new Vector3(0f, 0f, 1f);

	private Collider _aimTarget;

	private Rigidbody _aimTargetRigidbody;

	private Vector3 _aimTargetDirection;

	private float _fullAimTreshold;

	private YieldInstruction _findAimTargetIntervalInstruction;

	private MainWeapon _weapon;

	private int _targetLayers;

	private Vector3 _aimSpherePosition;

	private bool _shouldExtrapolateAiming;

	private bool _shouldFullAiming;

	private bool _shouldAlwaysFullAiming;

	private bool _isDetectingAimTarget;

	private bool _shouldDetectAimTarget;

	private bool _hasFixedAimSpherePosition;

	public Vector3 position
	{
		get
		{
			return _position;
		}
	}

	public Vector3 forward
	{
		get
		{
			return _forward;
		}
	}

	public MainWeapon weapon
	{
		get
		{
			return _weapon;
		}
		set
		{
			if (!(_weapon != value))
			{
				return;
			}
			_weapon = value;
			_weapon.gunTurret = this;
			_targetLayers = value.damageLayers;
			if (aimZoneCenter != null)
			{
				_aimSpherePosition = aimZoneCenter.position;
				if (aimZoneRadius < 1f)
				{
					aimZoneRadius = value.GetBaseRange();
				}
				_hasFixedAimSpherePosition = true;
				return;
			}
			float num = aimTargetsDetectionThreshold;
			if (num < 0f)
			{
				num = 0f;
			}
			if (aimZoneRadius < 1f)
			{
				aimZoneRadius = value.GetBaseRange() * (1f - num * 0.45f);
			}
			_aimSpherePosition = new Vector3(0f, 0f, aimZoneRadius * num + 1f);
			_hasFixedAimSpherePosition = false;
		}
	}

	public int targetLayers
	{
		get
		{
			return _targetLayers;
		}
		set
		{
			_targetLayers = value;
		}
	}

	public float projectileSpeed
	{
		set
		{
			_projectileSpeedFactor = 1f / value;
		}
	}

	public Vector3 targetVector
	{
		get
		{
			return _targetVector;
		}
		set
		{
			_targetVector = value;
		}
	}

	public Collider aimTarget
	{
		get
		{
			return _aimTarget;
		}
		set
		{
			if (value != _aimTarget)
			{
				_aimTarget = value;
				if (shouldExtrapolateAiming && _aimTarget != null)
				{
					_aimTargetRigidbody = _aimTarget.attachedRigidbody;
					_shouldExtrapolateAiming = _aimTargetRigidbody != null;
				}
				else
				{
					_aimTargetRigidbody = null;
					_shouldExtrapolateAiming = false;
				}
				if (this.aimTargetChangedEvent != null)
				{
					this.aimTargetChangedEvent(value, this);
				}
			}
		}
	}

	public Vector3 aimTargetDirection
	{
		get
		{
			return _aimTargetDirection;
		}
	}

	public event AimTargetChangedDelegate aimTargetChangedEvent;

	public float GetBaseFullAimTreshold()
	{
		return baseFullAimTreshold;
	}

	public float GetFullAimTreshold()
	{
		return _fullAimTreshold;
	}

	public void SetFullAimTreshold(float value)
	{
		_fullAimTreshold = value;
		if (value < -0.97f)
		{
			_shouldAlwaysFullAiming = (_shouldFullAiming = true);
			return;
		}
		_shouldAlwaysFullAiming = false;
		_shouldFullAiming = value < 0.97f;
	}

	private void Awake()
	{
		_transform = base.transform;
		_findAimTargetIntervalInstruction = new WaitForSeconds(findAimTargetInterval);
		SetFullAimTreshold(baseFullAimTreshold);
		if (weapon == null)
		{
			weapon = GetComponentInChildren<MainWeapon>();
		}
	}

	private void OnEnable()
	{
		_isDetectingAimTarget = false;
		_shouldDetectAimTarget = false;
		UpdatePosition();
	}

	private void OnDisable()
	{
		ClearAimTarget();
	}

	private void Start()
	{
		VehiclesManager instance = VehiclesManager.instance;
		if (instance != null)
		{
			instance.vehicleDeactivatedEvent += VehicleDeactivated;
		}
	}

	public void SetTargetDirection(Vector2 value)
	{
		value.Normalize();
		_targetVector.x = value.x;
		_targetVector.z = value.y;
	}

	public void UpdatePosition()
	{
		_position = _transform.position;
	}

	public void SetDirection()
	{
		UpdatePosition();
		CalcDirection(out _forward);
		_transform.forward = _forward;
	}

	public void LerpDirection(float dt)
	{
		UpdatePosition();
		Vector3 dir;
		CalcDirection(out dir);
		float num = targetDirectionSmoothFactor * dt;
		_forward.x += (dir.x - _forward.x) * num;
		_forward.y = dir.y;
		_forward.z += (dir.z - _forward.z) * num;
		_transform.forward = _forward;
	}

	private void CalcDirection(out Vector3 dir)
	{
		if (isLocalTargetDirection)
		{
			dir = _transform.parent.TransformDirection(_targetVector);
		}
		else
		{
			dir = _targetVector;
		}
		if (shouldVerticalAiming && _aimTarget != null)
		{
			UpdateAimTargetDirection();
			if (_shouldFullAiming && (_shouldAlwaysFullAiming || _fullAimTreshold <= Vector3.Dot(_aimTargetDirection, dir)))
			{
				dir = _aimTargetDirection;
			}
			else
			{
				dir.y = _aimTargetDirection.y;
			}
		}
		else
		{
			dir.y = 0f;
		}
	}

	public void SetAvatarDirection()
	{
		CalcAvatarDirection(out _forward);
		_transform.forward = _forward;
	}

	public void LerpAvatarDirection(float dt)
	{
		Vector3 direction;
		CalcAvatarDirection(out direction);
		float num = avatarDirectionSmoothFactor * dt;
		_forward.x += (direction.x - _forward.x) * num;
		_forward.y += (direction.y - _forward.y) * num;
		_forward.z += (direction.z - _forward.z) * num;
		_transform.forward = _forward;
	}

	private void CalcAvatarDirection(out Vector3 direction)
	{
		direction = _targetVector;
		if (_aimTarget != null)
		{
			UpdatePosition();
			UpdateAimTargetDirection();
			direction += _aimTargetDirection;
		}
	}

	public void UpdateAimTargetDirection()
	{
		Vector3 vector = _aimTarget.bounds.center - _position;
		if (_shouldExtrapolateAiming)
		{
			vector += _aimTargetRigidbody.velocity * (vector.magnitude * _projectileSpeedFactor);
		}
		vector.Normalize();
		_aimTargetDirection = vector;
	}

	public void DetectAimTarget()
	{
		Collider[] array = Physics.OverlapSphere(GetAimSpherePosition(), aimZoneRadius, _targetLayers);
		int num = array.Length;
		if (num != 0)
		{
			bool flag = false;
			Collider currTarget = null;
			float distanceToCurrTarget = 0f;
			int num2 = 0;
			do
			{
				Collider collider = array[num2];
				if (collider != _weapon.mainOwnerCollider && TestTarget(collider, currTarget, ref distanceToCurrTarget))
				{
					currTarget = collider;
					flag = true;
				}
			}
			while (++num2 != num);
			if (flag)
			{
				aimTarget = currTarget;
				return;
			}
		}
		if (_aimTarget != null)
		{
			ClearAimTarget();
		}
	}

	private bool TestTarget(Collider target, Collider currTarget, ref float distanceToCurrTarget)
	{
		Vector3 center = target.bounds.center;
		Vector2 vector = new Vector2(center.x - _position.x, center.z - _position.z);
		float sqrMagnitude = vector.sqrMagnitude;
		float num = Mathf.Sqrt(sqrMagnitude);
		float num2 = vector.x * _forward.x + vector.y * _forward.z;
		if (num2 < aimTargetsDetectionThreshold * num)
		{
			return false;
		}
		if (currTarget != null)
		{
			if (distanceToCurrTarget <= num)
			{
				return false;
			}
		}
		else if (aimZoneRadius * 2f < num)
		{
			return false;
		}
		distanceToCurrTarget = num;
		return true;
	}

	private Vector3 GetAimSpherePosition()
	{
		if (_hasFixedAimSpherePosition)
		{
			return _aimSpherePosition;
		}
		Vector3 result = _position;
		if (isLocalTargetDirection)
		{
			Vector3 vector = _transform.parent.TransformDirection(_targetVector);
			result.x += vector.x * _aimSpherePosition.z;
			result.z += vector.z * _aimSpherePosition.z;
		}
		else
		{
			result.x += _targetVector.x * _aimSpherePosition.z;
			result.z += _targetVector.z * _aimSpherePosition.z;
		}
		return result;
	}

	public void StartDetectAimTarget()
	{
		_shouldDetectAimTarget = true;
		if (!_isDetectingAimTarget)
		{
			StartCoroutine(DetectAimTargetLoop());
		}
	}

	public void StopDetectAimTarget()
	{
		_shouldDetectAimTarget = false;
	}

	private IEnumerator DetectAimTargetLoop()
	{
		do
		{
			DetectAimTarget();
			_isDetectingAimTarget = true;
			yield return _findAimTargetIntervalInstruction;
			_isDetectingAimTarget = false;
		}
		while (_shouldDetectAimTarget);
	}

	private void ClearAimTarget()
	{
		aimTarget = null;
	}

	private void VehicleDeactivated(Vehicle vehicle)
	{
		if (aimTarget != null && aimTarget.transform.root.gameObject == vehicle.gameObject)
		{
			ClearAimTarget();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere((!(aimZoneCenter != null)) ? GetAimSpherePosition() : aimZoneCenter.position, aimZoneRadius);
	}
}
