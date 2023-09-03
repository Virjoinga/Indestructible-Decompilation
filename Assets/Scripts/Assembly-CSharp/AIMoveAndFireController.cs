using System.Collections;
using Glu;
using UnityEngine;

public class AIMoveAndFireController : Glu.MonoBehaviour
{
	public float FireRadius = 20f;

	public float MinDistToTarget = 1f;

	public bool Enabled = true;

	public float PathUpdatePeriod = 1f;

	public float PathMoveUpdatePeriod = 0.2f;

	public float FireTargetUpdatePeriod = 0.5f;

	public float FireUpdatePeriod = 0.3f;

	public float StuckDiff = 0.3f;

	public float PathPointRadius = 3f;

	public float FinalPointRadius = 10f;

	public float SensorDistance = 7f;

	public float SensorsYOffset = 1f;

	public float SensorsWidth = 3f;

	public float OnStuckReverceTime = 3f;

	private GameObject _pawn;

	protected int _pathNextPoint;

	protected NavMeshPath _movementPath;

	protected Vector3 _prevCheckPosition = Vector3.zero;

	protected int _stuckCount;

	protected bool _isFinalPoint = true;

	protected bool _movementStarted;

	protected Vector3 _moveTarget = Vector3.zero;

	protected Transform _attackTarget;

	protected Transform _chaseTarget;

	protected Transform _transform;

	protected Vector3 _pathCornerPosition = Vector3.zero;

	protected ISteeringControl _carSteeringControl;

	protected IBrakes _carBrakes;

	protected MainWeapon _weapon;

	protected GunTurret _gunTurret;

	protected Engine _carEngine;

	protected VehiclePhysics _carPhysics;

	protected ITransmission _carTransmission;

	protected Vehicle _vehicle;

	protected Vector3 _homePoint = Vector3.zero;

	protected int _sensorsLayersMask = -1;

	protected int _frendlyLayersMask;

	public Vehicle AIVehicle
	{
		get
		{
			return _vehicle;
		}
	}

	public Vector3 HomePoint
	{
		get
		{
			return _homePoint;
		}
	}

	public bool IsMoveToTarget
	{
		get
		{
			return _movementStarted;
		}
	}

	public bool IsHaveAttackTarget
	{
		get
		{
			return _attackTarget != null;
		}
	}

	public bool IsFire
	{
		get
		{
			return _weapon != null && _weapon.shouldFire;
		}
	}

	public bool IsOnChase
	{
		get
		{
			return _chaseTarget != null;
		}
	}

	private void Start()
	{
		_sensorsLayersMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("PlayerTeam0")) | (1 << LayerMask.NameToLayer("PlayerTeam1")) | (1 << LayerMask.NameToLayer("AI")) | (1 << LayerMask.NameToLayer("AITeam0")) | (1 << LayerMask.NameToLayer("AITeam1"));
		_frendlyLayersMask = 1 << base.gameObject.layer;
	}

	private void OnEnable()
	{
		ActivateAI();
	}

	private void OnDisable()
	{
		_chaseTarget = null;
		StopMovementCoroutine();
	}

	private void ActivateAI()
	{
		if (PhotonNetwork.isMasterClient)
		{
			_transform = base.transform;
			_pawn = base.gameObject;
			_vehicle = GetComponent<Vehicle>();
			_homePoint = base.transform.position;
			_carSteeringControl = Glu.MonoBehaviour.GetExistingComponentIface<ISteeringControl>(_pawn);
			_gunTurret = _pawn.GetComponentInChildren<GunTurret>();
			_weapon = _gunTurret.GetComponentInChildren<MainWeapon>();
			_gunTurret.weapon = _weapon;
			_carBrakes = Glu.MonoBehaviour.GetComponentIface<IBrakes>(_pawn);
			_carEngine = _pawn.GetComponentInChildren<Engine>();
			_carPhysics = GetComponent<VehiclePhysics>();
			_carTransmission = GetComponentIface<ITransmission>();
			StartCoroutine(UpdateMovePath());
			StartCoroutine(UpdateFire());
		}
	}

	public void SetMoveTarget(Vector3 target)
	{
		_chaseTarget = null;
		_moveTarget = target;
		CalkPathToPoint(_moveTarget);
	}

	public void SetChaseTarget(Transform target)
	{
		_chaseTarget = target;
		CalkPathToPoint(_chaseTarget.position);
	}

	public void SetFireTarget(Transform target)
	{
		_attackTarget = target;
		if (_weapon != null)
		{
			_weapon.shouldFire = false;
		}
	}

	private IEnumerator UpdateMovePath()
	{
		yield return null;
		while (true)
		{
			if (_movementStarted || _chaseTarget != null)
			{
				if (_chaseTarget != null && !_chaseTarget.gameObject.active)
				{
					_chaseTarget = null;
					StopMovement();
				}
				else if (_chaseTarget != null)
				{
					if (CalkPathToPoint(_chaseTarget.position))
					{
						StartMovementCoroutine();
					}
				}
				else if (CalkPathToPoint(_moveTarget) && !IsFinalPointReached())
				{
					StartMovementCoroutine();
				}
				else
				{
					StopMovement();
				}
			}
			else
			{
				StopMovement();
			}
			yield return new WaitForSeconds(PathUpdatePeriod);
		}
	}

	private bool CalkPathToPoint(Vector3 destination)
	{
		_movementPath = null;
		_pathNextPoint = 0;
		_isFinalPoint = false;
		float maxDistance = 30f;
		int num = -1;
		NavMeshHit hit = default(NavMeshHit);
		if (NavMesh.SamplePosition(base.gameObject.transform.position, out hit, maxDistance, num))
		{
			NavMeshHit hit2 = default(NavMeshHit);
			if (NavMesh.SamplePosition(destination, out hit2, maxDistance, num))
			{
				_movementPath = new NavMeshPath();
				if (NavMesh.CalculatePath(hit.position, hit2.position, num, _movementPath))
				{
					if (_movementPath.corners.Length > 1)
					{
						_pathNextPoint = 1;
						_pathCornerPosition = _movementPath.corners[_pathNextPoint];
					}
					else
					{
						_pathNextPoint = 0;
						_pathCornerPosition = _movementPath.corners[_pathNextPoint];
					}
					for (int i = 0; i < _movementPath.corners.Length - 1; i++)
					{
					}
					CheckFinalPoint();
					return true;
				}
				_movementPath = null;
			}
		}
		return false;
	}

	private IEnumerator UpdateMovement()
	{
		yield return null;
		while (true)
		{
			float startTime = Time.time;
			if (_carTransmission != null && _carTransmission.gear == 0)
			{
				_carTransmission.gear = 1;
			}
			yield return StartCoroutine("UpdateMoveToPoint");
			SetNextPathPoint();
			float dt = Time.time - startTime;
			if (dt >= PathMoveUpdatePeriod)
			{
				yield return null;
			}
			else
			{
				yield return new WaitForSeconds(PathMoveUpdatePeriod - dt);
			}
		}
	}

	private IEnumerator UpdateFire()
	{
		yield return null;
		while (true)
		{
			if ((bool)_attackTarget && _attackTarget.gameObject.active)
			{
				Vector3 start = base.gameObject.transform.position;
				start.y += SensorsYOffset;
				Vector3 FireDirection = _attackTarget.transform.position + new Vector3(0f, -0.5f, 0f) - start;
				float distance = FireDirection.magnitude;
				if (distance > 0f && distance <= FireRadius)
				{
					FireDirection.Normalize();
					if (_gunTurret != null)
					{
						_gunTurret.SetTargetDirection(new Vector2(FireDirection.x, FireDirection.z));
					}
					_weapon.shouldFire = true;
				}
				else
				{
					_weapon.shouldFire = false;
				}
			}
			else
			{
				_weapon.shouldFire = false;
			}
			yield return new WaitForSeconds(FireUpdatePeriod);
		}
	}

	private void SetNextPathPoint()
	{
		if (_movementPath != null && _pathNextPoint < _movementPath.corners.Length)
		{
			_pathCornerPosition = _movementPath.corners[_pathNextPoint];
			_pathNextPoint++;
		}
		CheckFinalPoint();
	}

	private IEnumerator UpdateMoveToPoint()
	{
		if (IsWrongDirection())
		{
			yield return StartCoroutine("BreakTillStop");
		}
		yield return StartCoroutine("MoveToPoint");
	}

	private bool CheckSensor(Vector3 start, Vector3 dir, float dist, int layerMask, Color debugColor, Color debugColorHit, float debugTime)
	{
		RaycastHit[] array = Physics.RaycastAll(start, dir, dist, layerMask);
		if (array == null || array.Length == 0 || (array.Length == 1 && array[0].transform == base.transform))
		{
			return false;
		}
		return true;
	}

	private bool CheckSensor(Vector3 start, Vector3 dir, float dist, Color debugColor, Color debugColorHit, float debugTime)
	{
		return CheckSensor(start, dir, dist, _sensorsLayersMask, debugColor, debugColorHit, debugTime);
	}

	private bool IsWayBlocked(float dist)
	{
		Vector3 position = base.transform.position;
		position.y += SensorsYOffset;
		return CheckSensor(position, base.transform.forward, dist, Color.green, Color.red, 0.3f);
	}

	private bool UpdateDirectionBySideSensors(ref Vector2 dir, float dist)
	{
		bool result = false;
		Vector3 position = base.transform.position;
		position.y += SensorsYOffset;
		float num = SensorsWidth / (2f * dist);
		Vector3 normalized = (base.transform.forward - num * base.transform.right).normalized;
		Vector3 normalized2 = (base.transform.forward + num * base.transform.right).normalized;
		bool flag = CheckSensor(position, normalized, dist, Color.yellow, Color.magenta, 0.3f);
		bool flag2 = CheckSensor(position, normalized2, dist, Color.yellow, Color.magenta, 0.3f);
		Vector3 vector = dir;
		Vector3 forward = base.transform.forward;
		Vector2 lhs = new Vector2(forward.x, forward.z);
		bool flag3 = Vector2.Dot(lhs, dir) > 0f;
		if (flag && flag2)
		{
			if (flag3)
			{
				dir = -dir;
			}
			result = true;
		}
		else if (flag)
		{
			dir = Quaternion.Euler(0f, 0f, (!flag3) ? 60 : (-60)) * vector;
			result = true;
		}
		else if (flag2)
		{
			dir = Quaternion.Euler(0f, 0f, (!flag3) ? (-60) : 60) * vector;
			result = true;
		}
		return result;
	}

	private bool CheckStuck()
	{
		if (_carPhysics == null)
		{
			return false;
		}
		float num = 0.25f;
		if (_carPhysics.sqrSpeed <= num && (_prevCheckPosition - base.gameObject.transform.position).sqrMagnitude < StuckDiff * StuckDiff)
		{
			_stuckCount++;
			if (_stuckCount > 5)
			{
				_stuckCount = 0;
				return true;
			}
		}
		else
		{
			_prevCheckPosition = base.gameObject.transform.position;
			_stuckCount = 0;
		}
		return false;
	}

	private float GetDirectionDiff()
	{
		float num = Vector3.Dot(base.gameObject.transform.forward, (_pathCornerPosition - base.gameObject.transform.position).normalized);
		return (0f - (num - 1f)) / 2f;
	}

	private bool IsWrongDirection()
	{
		float num = Vector3.Dot(base.gameObject.transform.forward, _pathCornerPosition - base.gameObject.transform.position);
		return num < 0f;
	}

	private IEnumerator BreakTillStop()
	{
		float epsilon2 = 0.25f;
		if (_carPhysics.sqrSpeed > epsilon2)
		{
			PressBreak();
			yield return new WaitForSeconds(0.5f);
		}
		_carBrakes.brakeFactor = 0f;
	}

	private IEnumerator MoveReverceDirection(float time)
	{
		Vector3 carDir = base.gameObject.transform.forward;
		_carSteeringControl.direction = new Vector2(carDir.x, carDir.z);
		_carEngine.throttle = 1f;
		_carBrakes.brakeFactor = 0f;
		_carTransmission.gear = 0;
		yield return new WaitForSeconds(time);
		_carTransmission.gear = 1;
	}

	private IEnumerator MoveReverceUntillWayBlocked()
	{
		yield return StartCoroutine("BreakTillStop");
		while (IsWayBlocked(SensorDistance))
		{
			yield return StartCoroutine("MoveReverceDirection", OnStuckReverceTime / 3f);
		}
	}

	private IEnumerator MoveToPoint()
	{
		float epsilon2 = ((!_isFinalPoint) ? (PathPointRadius * PathPointRadius) : (FinalPointRadius * FinalPointRadius));
		while (true)
		{
			float velMagnitude = base.rigidbody.velocity.magnitude;
			if (IsWayBlocked(Mathf.Max(velMagnitude, SensorDistance)))
			{
				yield return StartCoroutine("MoveReverceUntillWayBlocked");
			}
			if (CheckStuck())
			{
				yield return StartCoroutine("MoveReverceDirection", OnStuckReverceTime);
			}
			Vector3 diff3d = _pathCornerPosition - base.gameObject.transform.position;
			Vector2 diff2d = new Vector2(diff3d.x, diff3d.z);
			if (!(diff2d.sqrMagnitude > epsilon2))
			{
				break;
			}
			diff2d.Normalize();
			bool steerObstacle = UpdateDirectionBySideSensors(ref diff2d, Mathf.Max(velMagnitude, SensorDistance * 1.3f));
			_carSteeringControl.direction = diff2d;
			_carEngine.throttle = ((!steerObstacle) ? 1f : 0.3f);
			_carBrakes.brakeFactor = 0f;
			if (_carTransmission.gear == 0)
			{
				_carTransmission.gear = 1;
			}
			yield return new WaitForSeconds(0.3f);
		}
		if (_isFinalPoint)
		{
			StopMovement();
		}
		else
		{
			PressBreak();
		}
	}

	private void PressBreak()
	{
		PressBreak(1f);
	}

	private void PressBreak(float power)
	{
		if (_carEngine != null)
		{
			_carEngine.throttle = 0f;
		}
		if (_carBrakes != null)
		{
			_carBrakes.brakeFactor = power;
		}
	}

	private bool CheckFinalPoint()
	{
		_isFinalPoint = _movementPath == null || _pathNextPoint >= _movementPath.corners.Length - 1;
		return _isFinalPoint;
	}

	private bool IsFinalPointReached()
	{
		if (!_isFinalPoint)
		{
			return false;
		}
		float num = FinalPointRadius * FinalPointRadius;
		Vector3 vector = _pathCornerPosition - base.gameObject.transform.position;
		if (new Vector2(vector.x, vector.z).sqrMagnitude <= num)
		{
			return true;
		}
		return false;
	}

	public void StopMovement()
	{
		if (PhotonNetwork.isMasterClient)
		{
			PressBreak(1f);
			StopMovementCoroutine();
		}
	}

	public void StartMovement()
	{
		if (PhotonNetwork.isMasterClient)
		{
			PressBreak(0f);
			StartMovementCoroutine();
		}
	}

	private void StartMovementCoroutine()
	{
		if (!_movementStarted)
		{
			StartCoroutine("UpdateMovement");
		}
		_movementStarted = true;
	}

	private void StopMovementCoroutine()
	{
		if (_movementStarted)
		{
			_movementStarted = false;
			StopCoroutine("UpdateMovement");
			StopCoroutine("UpdateMoveToPoint");
			StopCoroutine("BreakTillStop");
			StopCoroutine("MoveReverceDirection");
			StopCoroutine("MoveToPoint");
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawLine(_pathCornerPosition, base.gameObject.transform.position);
		if (_chaseTarget != null)
		{
			Gizmos.DrawLine(_chaseTarget.position, base.gameObject.transform.position);
		}
	}
}
