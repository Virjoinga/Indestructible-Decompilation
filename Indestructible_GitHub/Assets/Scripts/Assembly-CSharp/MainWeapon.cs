using System.Collections;
using UnityEngine;

public abstract class MainWeapon : Weapon, IPhotonObserver, IUpdatable, IMountable
{
	protected class ColliderInfo
	{
		public Collider collider;

		public GameObject gameObject;

		public Destructible destructible;

		public int surfaceType;
	}

	public LayerMask collisionLayerMask;

	public float baseShotEnergyConsumption = 1f;

	public bool autoAimTargetsDetection = true;

	public float baseFireInterval = 0.25f;

	public float baseRange = 50f;

	protected RaycastHit _hitInfo;

	private Transform _transform;

	private GunTurret _gunTurret;

	private Vehicle _vehicle;

	private int _collisionLayers;

	private int _shotsCount;

	private int _serverShotsCount;

	private Collider _cachedAimTarget;

	private Destructible _cachedAimDestructible;

	private Destructible _shotAimDestructible;

	private Vector3 _shotDirection;

	private Vector3 _shotAimTargetDirection;

	private ColliderInfo _cachedColliderInfo0;

	private ColliderInfo _cachedColliderInfo1;

	private PhotonView _photonView;

	private float _fireInterval;

	private YieldInstruction _fireIntervalInstruction;

	private float _range;

	private float _shotEnergyConsumption;

	private AudioHelper _audioHelper;

	private bool _shouldFire;

	private bool _isFiring;

	private bool _isMine;

	public Vector3 lastShotDirection
	{
		get
		{
			return _shotDirection;
		}
	}

	public Vector3 lastHitPoint
	{
		get
		{
			return _hitInfo.point;
		}
	}

	public Vector3 lastHitNormal
	{
		get
		{
			return _hitInfo.normal;
		}
	}

	public new Transform transform
	{
		get
		{
			return _transform;
		}
	}

	public GunTurret gunTurret
	{
		get
		{
			return _gunTurret;
		}
		set
		{
			_gunTurret = value;
		}
	}

	public Vehicle vehicle
	{
		get
		{
			return _vehicle;
		}
	}

	public int collisionLayers
	{
		get
		{
			return _collisionLayers;
		}
		set
		{
			_collisionLayers = value;
		}
	}

	protected YieldInstruction fireIntervalInstruction
	{
		get
		{
			return _fireIntervalInstruction;
		}
	}

	public int shotsCount
	{
		get
		{
			return _shotsCount;
		}
	}

	public PhotonView photonView
	{
		get
		{
			return _photonView;
		}
	}

	public bool isMine
	{
		get
		{
			return _isMine;
		}
	}

	public AudioHelper audioHelper
	{
		get
		{
			return _audioHelper;
		}
	}

	public virtual bool shouldFire
	{
		get
		{
			return _shouldFire;
		}
		set
		{
			if (value != _shouldFire)
			{
				_shouldFire = value;
				if (value && !_isFiring)
				{
					StartFireLoop();
				}
			}
		}
	}

	public bool shouldAvatarFire
	{
		get
		{
			return _shotsCount < _serverShotsCount;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_collisionLayers = collisionLayerMask.value;
		_cachedColliderInfo0 = new ColliderInfo();
		_cachedColliderInfo1 = new ColliderInfo();
		_transform = base.transform;
		_shotEnergyConsumption = baseShotEnergyConsumption;
		if (_fireIntervalInstruction == null)
		{
			_fireInterval = baseFireInterval;
			_fireIntervalInstruction = new WaitForSeconds(baseFireInterval);
		}
		if (_range == 0f)
		{
			_range = baseRange;
		}
		_audioHelper = new AudioHelper(GetComponent<AudioSource>(), false, false);
	}

	protected virtual void Start()
	{
		GameObject gameObject = _transform.root.gameObject;
		LocateMainOwnerCollider(gameObject);
		if (_gunTurret == null)
		{
			autoAimTargetsDetection = false;
		}
		if (PhotonNetwork.room == null)
		{
			return;
		}
		_photonView = _transform.root.GetComponentInChildren<PhotonView>();
		if (_photonView != null)
		{
			GameObject gameObject2 = _photonView.gameObject;
			if (gameObject2 != base.gameObject)
			{
				MainWeaponRPCDispatcher mainWeaponRPCDispatcher = gameObject2.GetComponent<MainWeaponRPCDispatcher>();
				if (mainWeaponRPCDispatcher == null)
				{
					mainWeaponRPCDispatcher = AddRPCDispatcher(gameObject2);
				}
				if (mainWeaponRPCDispatcher.weapon == null)
				{
					mainWeaponRPCDispatcher.weapon = this;
				}
			}
			CheckOwnership();
		}
		PhotonObserversGroup componentInChildren = gameObject.GetComponentInChildren<PhotonObserversGroup>();
		if (componentInChildren != null && componentInChildren.GetObserver(1) == null)
		{
			componentInChildren.SetObserver(1, this);
		}
	}

	protected virtual void OnEnable()
	{
		_shouldFire = false;
		if (_shotsCount < _serverShotsCount)
		{
			_shotsCount = _serverShotsCount;
		}
		_isFiring = false;
		CheckOwnership();
	}

	protected virtual void OnDisable()
	{
		_shouldFire = false;
		if (_shotsCount < _serverShotsCount)
		{
			_shotsCount = _serverShotsCount;
		}
	}

	protected virtual void OnDestroy()
	{
		_audioHelper.Dispose();
	}

	public float GetBaseFireInterval()
	{
		return baseFireInterval;
	}

	public float GetFireInterval()
	{
		return _fireInterval;
	}

	public virtual void SetFireInterval(float newFireInterval)
	{
		_fireInterval = newFireInterval;
		_fireIntervalInstruction = new WaitForSeconds(newFireInterval);
		if (isMine && (bool)photonView)
		{
			photonView.RPC("SetFireInterval", PhotonTargets.Others, newFireInterval);
		}
	}

	public float GetBaseRange()
	{
		return baseRange;
	}

	public float GetRange()
	{
		return _range;
	}

	public virtual void SetRange(float value)
	{
		_range = value;
		if (isMine && (bool)photonView)
		{
			photonView.RPC("SetRange", PhotonTargets.Others, value);
		}
	}

	public float GetBaseShotEnergyConsumption()
	{
		return baseShotEnergyConsumption;
	}

	public float GetShotEnergyConsumption()
	{
		return _shotEnergyConsumption;
	}

	public void SetShotEnergyConsumption(float newShotEnergyConsumption)
	{
		_shotEnergyConsumption = newShotEnergyConsumption;
	}

	protected bool TryConsumeShotEnergy()
	{
		return !(_vehicle != null) || _vehicle.TryConsumeEnergy(GetShotEnergyConsumption());
	}

	protected void ConsumeShotEnergy()
	{
		if (_vehicle != null)
		{
			_vehicle.ConsumeEnergy(GetShotEnergyConsumption());
		}
	}

	public override void SetDamageLayers(int value)
	{
		base.SetDamageLayers(value);
		if (_gunTurret != null)
		{
			_gunTurret.targetLayers = value;
		}
	}

	protected virtual void VehiclePlayerChanged(Vehicle vehicle)
	{
		SetPlayer(vehicle.player);
	}

	protected virtual void VehicleDamageLayersChanged(Vehicle vehicle)
	{
		SetDamageLayers(vehicle.damageLayers);
	}

	public Collider LocateMainOwnerCollider(GameObject rootObject)
	{
		Collider collider = base.mainOwnerCollider;
		if (collider == null)
		{
			Collider[] componentsInChildren = rootObject.GetComponentsInChildren<Collider>();
			int i = 0;
			for (int num = componentsInChildren.Length; i != num; i++)
			{
				collider = componentsInChildren[i];
				if (!collider.isTrigger)
				{
					SetMainOwnerCollider(collider);
					break;
				}
			}
		}
		return collider;
	}

	public override GameObject GetGameObject(Collider collider)
	{
		return GetColliderInfo(collider).gameObject;
	}

	public override Destructible GetDestructible(Collider collider)
	{
		return GetColliderInfo(collider).destructible;
	}

	public override Destructible GetDestructible(Collider collider, GameObject gameObject)
	{
		return GetColliderInfo(collider).destructible;
	}

	protected ColliderInfo GetColliderInfo(Collider collider)
	{
		if (collider != _cachedColliderInfo0.collider)
		{
			ColliderInfo cachedColliderInfo = _cachedColliderInfo1;
			_cachedColliderInfo1 = _cachedColliderInfo0;
			_cachedColliderInfo0 = cachedColliderInfo;
			if (collider != cachedColliderInfo.collider)
			{
				cachedColliderInfo.collider = collider;
				SurfaceInfo component = collider.GetComponent<SurfaceInfo>();
				cachedColliderInfo.surfaceType = ((component != null) ? component.surfaceType : 0);
				GameObject gameObject = collider.transform.root.gameObject;
				if (gameObject != cachedColliderInfo.gameObject)
				{
					cachedColliderInfo.gameObject = gameObject;
					cachedColliderInfo.destructible = gameObject.GetComponent<Destructible>();
				}
			}
		}
		return _cachedColliderInfo0;
	}

	private void StartFireLoop()
	{
		_isFiring = true;
		if (autoAimTargetsDetection)
		{
			_gunTurret.StartDetectAimTarget();
		}
		StartCoroutine(FireLoop());
	}

	protected abstract IEnumerator FireLoop();

	protected abstract IEnumerator AvatarFireLoop();

	protected void FireLoopEnd()
	{
		if (autoAimTargetsDetection)
		{
			_gunTurret.StopDetectAimTarget();
		}
		_isFiring = false;
	}

	protected void RegShot()
	{
		_shotsCount++;
		_shotDirection = _gunTurret.forward;
		Collider aimTarget = _gunTurret.aimTarget;
		if (aimTarget == null)
		{
			_shotAimDestructible = null;
			return;
		}
		_shotAimTargetDirection = _gunTurret.aimTargetDirection;
		if (aimTarget != _cachedAimTarget)
		{
			_cachedAimTarget = aimTarget;
			_cachedAimDestructible = GetColliderInfo(aimTarget).destructible;
		}
		_shotAimDestructible = _cachedAimDestructible;
	}

	public virtual bool DoUpdate()
	{
		return false;
	}

	public virtual void Mounted(Vehicle vehicle)
	{
		_vehicle = vehicle;
		SetPlayer(vehicle.player);
		vehicle.playerChangedEvent += VehiclePlayerChanged;
		SetDamageLayers(vehicle.damageLayers);
		vehicle.damageLayersChangedEvent += VehicleDamageLayersChanged;
	}

	public virtual void WillUnmount(Vehicle vehicle)
	{
		MainWeaponRPCDispatcher component = vehicle.GetComponent<MainWeaponRPCDispatcher>();
		if (component != null && component.weapon == this)
		{
			component.weapon = null;
		}
		PhotonObserversGroup componentInChildren = vehicle.GetComponentInChildren<PhotonObserversGroup>();
		if (componentInChildren != null && componentInChildren.GetObserver(1) as MainWeapon == this)
		{
			componentInChildren.SetObserver(1, null);
		}
		vehicle.playerChangedEvent -= VehiclePlayerChanged;
		vehicle.damageLayersChangedEvent -= VehicleDamageLayersChanged;
		_vehicle = null;
	}

	protected virtual MainWeaponRPCDispatcher AddRPCDispatcher(GameObject photonViewObject)
	{
		return photonViewObject.AddComponent<MainWeaponRPCDispatcher>();
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
			uint num;
			Vector3 vector;
			if (_shotAimDestructible != null)
			{
				num = (uint)_shotAimDestructible.id;
				vector = _shotDirection - _shotAimTargetDirection;
			}
			else
			{
				num = 4095u;
				vector = _shotDirection;
			}
			stream.SendNext((long)(FloatConvert.ToUint16(vector.x, -2f, 0.25f) | ((ulong)FloatConvert.ToUint10(vector.y, -2f, 0.25f) << 16) | ((ulong)FloatConvert.ToUint16(vector.z, -2f, 0.25f) << 26) | ((ulong)(num & 0xFFF) << 42)) | ((long)(_shotsCount & 0x3FF) << 54));
		}
		else
		{
			if (!base.enabled || !base.gameObject.active)
			{
				return;
			}
			ulong num2 = (ulong)(long)stream.ReceiveNext();
			if (_serverShotsCount < _shotsCount)
			{
				_shotsCount = _serverShotsCount;
			}
			_serverShotsCount = (int)(num2 >> 54);
			if (_serverShotsCount < _shotsCount)
			{
				_serverShotsCount += 1024;
			}
			bool flag;
			if (flag = _gunTurret != null)
			{
				Vector3 targetVector = default(Vector3);
				targetVector.x = FloatConvert.FromUint16((uint)(num2 & 0xFFFF), -2f, 4f);
				targetVector.y = FloatConvert.FromUint10((uint)((num2 >> 16) & 0x3FF), -2f, 4f);
				targetVector.z = FloatConvert.FromUint16((uint)((num2 >> 26) & 0xFFFF), -2f, 4f);
				_gunTurret.targetVector = targetVector;
				int num3 = (int)((num2 >> 42) & 0xFFF);
				_gunTurret.aimTarget = ((num3 == 4095) ? null : Destructible.Find(num3).mainDamageCollider);
			}
			if (_shotsCount < _serverShotsCount && !_isFiring)
			{
				_isFiring = true;
				if (flag)
				{
					StartCoroutine(AvatarFireLoop());
				}
			}
		}
	}

	private void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		if (isMine && (bool)photonView)
		{
			photonView.RPC("SetFireInterval", player, _fireInterval);
		}
	}
}
