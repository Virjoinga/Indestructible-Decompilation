using System;
using System.Collections;
using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/Destruction/Destructible")]
public class Destructible : Glu.MonoBehaviour
{
	public delegate void HPChangedDelegate(float deltaHP, Destructible destructible);

	public delegate void DamagedDelegate(float damage, Destructible destructible, INetworkWeapon weapon);

	public delegate void DestructedDelegate(Destructible destructed);

	public delegate void ForceDestructedDelegate(Destructible destructed, DestructionReason reason);

	public float crushSpeedFactor = 0.2f;

	public float baseMaxHP = 1000f;

	public float syncHPInterval = 0.5f;

	public float impactStability = 1000f;

	public float forwardFragilityFactor = 1f;

	public float sideFragilityFactor = 2f;

	public float backFragilityFactor = 3f;

	public GameObject explosionEffect;

	public GameObject animatedDestruction;

	public GameObject physicalDestruction;

	public float shakeCameraMagnitude = 2.5f;

	public float shakeCameraDuration = 1.25f;

	protected float _damageReducer;

	private float _maxHP;

	protected float _hp;

	private float _syncedHP;

	private YieldInstruction _syncHPIntervalInstruction;

	private bool _isActive;

	private Transform _transform;

	private int _id = -1;

	private Collider _mainDamageCollider;

	private PhotonView _photonView;

	private Vehicle _vehicle;

	private Rigidbody _rigidbody;

	private Renderer _renderer;

	private CachedObject.Cache _explosionEffectCache;

	private CachedObject.Cache _animatedDestructionCache;

	private CachedObject.Cache _physicalDestructionCache;

	private float _fragilityFactor;

	private bool _isMine;

	private bool _isHPSyncing;

	private static int _registeredDestructiblesCount;

	private static Destructible[] _registeredDestructibles = new Destructible[256];

	public float hp
	{
		get
		{
			return _hp;
		}
	}

	public float forwardFragility
	{
		get
		{
			return forwardFragilityFactor * GetFragilityFactor();
		}
	}

	public float sideFragility
	{
		get
		{
			return sideFragilityFactor * GetFragilityFactor();
		}
	}

	public float backFragility
	{
		get
		{
			return backFragilityFactor * GetFragilityFactor();
		}
	}

	public bool isActive
	{
		get
		{
			return _isActive;
		}
	}

	public bool isMine
	{
		get
		{
			return _isMine;
		}
	}

	public Vehicle vehicle
	{
		get
		{
			return _vehicle;
		}
	}

	public new Rigidbody rigidbody
	{
		get
		{
			return _rigidbody;
		}
	}

	public int id
	{
		get
		{
			return _id;
		}
	}

	public Collider mainDamageCollider
	{
		get
		{
			return _mainDamageCollider;
		}
	}

	public new Renderer renderer
	{
		get
		{
			return _renderer;
		}
		set
		{
			_renderer = value;
		}
	}

	public bool isVisible
	{
		get
		{
			return _renderer.isVisible;
		}
	}

	public event HPChangedDelegate healedEvent;

	public event HPChangedDelegate consumedEvent;

	public event DamagedDelegate damagedEvent;

	public event DestructedDelegate destructedEvent;

	public event ForceDestructedDelegate forceDestructedEvent;

	public event Action<Destructible> activatedEvent;

	public float GetBaseMaxHP()
	{
		return baseMaxHP;
	}

	public float GetMaxHP()
	{
		return _maxHP;
	}

	public void SetMaxHP(float newMaxHP)
	{
		_hp *= newMaxHP / _maxHP;
		_maxHP = newMaxHP;
	}

	public float GetBaseFragilityFactor()
	{
		return 1f;
	}

	public float GetFragilityFactor()
	{
		return _fragilityFactor;
	}

	public void SetFragilityFactor(float value)
	{
		_fragilityFactor = value;
	}

	public float GetBaseDamageReducer()
	{
		return 0f;
	}

	public float GetDamageReducer()
	{
		return _damageReducer;
	}

	public void SetDamageReducer(float newDamageReducer)
	{
		_damageReducer = newDamageReducer;
	}

	private void Awake()
	{
		_transform = base.transform;
		_vehicle = GetComponent<Vehicle>();
		_rigidbody = GetComponent<Rigidbody>();
		_syncHPIntervalInstruction = new WaitForSeconds(syncHPInterval);
		_maxHP = baseMaxHP;
		_fragilityFactor = GetBaseFragilityFactor();
		if (PhotonNetwork.room != null)
		{
			_photonView = GetComponent<PhotonView>();
		}
		Register();
		int num = (7 << LayerMask.NameToLayer("Player")) | (7 << LayerMask.NameToLayer("AI"));
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			if (!collider.isTrigger && ((1 << collider.gameObject.layer) & num) != 0)
			{
				_mainDamageCollider = collider;
				break;
			}
		}
	}

	private void Start()
	{
		if (_renderer == null)
		{
			renderer = GetComponentInChildren<Renderer>();
		}
		ObjectCacheManager instance = ObjectCacheManager.Instance;
		if (explosionEffect != null)
		{
			_explosionEffectCache = instance.GetCache(explosionEffect);
		}
		if (animatedDestruction != null)
		{
			_animatedDestructionCache = instance.GetCache(animatedDestruction);
		}
		if (physicalDestruction != null)
		{
			_physicalDestructionCache = instance.GetCache(physicalDestruction);
		}
	}

	private void OnEnable()
	{
		_isActive = true;
		_hp = _maxHP;
		_isHPSyncing = false;
		_isMine = !(_photonView != null) || _photonView.isMine;
		if (this.activatedEvent != null)
		{
			this.activatedEvent(this);
		}
	}

	public void Activate()
	{
		if (!base.gameObject.active)
		{
			base.gameObject.SetActiveRecursively(true);
			if (_photonView != null)
			{
				Vector3 position = _transform.position;
				ulong num = ((ulong)(uint)((int)BitConverter32.ToUint(position.x) & -256) >> 8) | ((ulong)(uint)((int)BitConverter32.ToUint(position.y) & -65536) << 8) | ((ulong)(uint)((int)BitConverter32.ToUint(position.z) & -256) << 32);
				Quaternion rotation = _transform.rotation;
				ulong num2 = (FloatConvert.ToUint16(rotation.x, -1f, 0.5f) & 0xFFFFu) | ((ulong)(FloatConvert.ToUint16(rotation.y, -1f, 0.5f) & 0xFFFF) << 16) | ((ulong)(FloatConvert.ToUint16(rotation.z, -1f, 0.5f) & 0xFFFF) << 32) | ((ulong)(FloatConvert.ToUint16(rotation.w, -1f, 0.5f) & 0xFFFF) << 48);
				_photonView.RPC("Activated", PhotonTargets.Others, (long)num, (long)num2);
			}
		}
	}

	public void Heal(float HealValue)
	{
		if (_isActive)
		{
			float num = _hp + HealValue;
			if (_maxHP < num)
			{
				num = _maxHP;
			}
			SetHP(num);
			if (this.healedEvent != null)
			{
				this.healedEvent(HealValue, this);
			}
		}
	}

	public void ConsumeHP(float consumeValue)
	{
		if (_isActive)
		{
			float hP = _hp - consumeValue;
			if (this.consumedEvent != null)
			{
				this.consumedEvent(0f - consumeValue, this);
			}
			SetHP(hP);
		}
	}

	public virtual DamageResult Damage(float damage, INetworkWeapon weapon)
	{
		if (_hp <= 0f)
		{
			return DamageResult.Ignored;
		}
		CallDamagedEvent(damage, this, weapon);
		float num = Mathf.Max(1f - _damageReducer, 0f);
		float num2 = _hp - damage * num;
		SetHP(num2);
		if (num2 <= 0f)
		{
			IDTGame instance = IDTGame.Instance;
			if (instance != null)
			{
				instance.Destructed(this, DestructionReason.Weapon, weapon);
			}
			return DamageResult.Destructed;
		}
		return DamageResult.Damaged;
	}

	public void Die(DestructionReason reason)
	{
		if (0f < _hp)
		{
			if (this.forceDestructedEvent != null)
			{
				this.forceDestructedEvent(this, reason);
			}
			SetHP(0f);
			IDTGame instance = IDTGame.Instance;
			if (instance != null)
			{
				instance.Destructed(this, reason, null);
			}
		}
	}

	protected void CallDamagedEvent(float damage, Destructible destructible, INetworkWeapon weapon)
	{
		if (this.damagedEvent != null)
		{
			this.damagedEvent(damage, this, weapon);
		}
	}

	[RPC]
	protected virtual void Activated(long posVal, long rotVal)
	{
		if (!base.gameObject.active)
		{
			Vector3 position = default(Vector3);
			position.x = BitConverter32.ToFloat((uint)(int)(posVal << 8) & 0xFFFFFF00u);
			position.y = BitConverter32.ToFloat((uint)(int)(posVal >> 8) & 0xFFFF0000u);
			position.z = BitConverter32.ToFloat((uint)(int)(posVal >> 32) & 0xFFFFFF00u);
			_transform.position = position;
			Quaternion rotation = default(Quaternion);
			rotation.x = FloatConvert.FromUint16((uint)(rotVal & 0xFFFF), -1f, 2f);
			rotation.y = FloatConvert.FromUint16((uint)(int)(rotVal >> 16) & 0xFFFFu, -1f, 2f);
			rotation.z = FloatConvert.FromUint16((uint)(int)(rotVal >> 32) & 0xFFFFu, -1f, 2f);
			rotation.w = FloatConvert.FromUint16((uint)(int)(rotVal >> 48) & 0xFFFFu, -1f, 2f);
			_transform.rotation = rotation;
			base.gameObject.SetActiveRecursively(true);
		}
	}

	protected void SetHP(float hp)
	{
		UpdateHP(hp);
		if (_isMine && _photonView != null)
		{
			if (!_isActive)
			{
				SyncHP();
			}
			else if (!_isHPSyncing)
			{
				StartCoroutine(SyncHPLoop());
			}
		}
	}

	[RPC]
	protected virtual void InitHP(float hp)
	{
		if ((_hp = hp) <= 0f)
		{
			Deactivate();
		}
	}

	[RPC]
	protected virtual void UpdateHP(float hp)
	{
		if ((_hp = hp) <= 0f)
		{
			Destruct();
		}
	}

	private IEnumerator SyncHPLoop()
	{
		do
		{
			SyncHP();
			_isHPSyncing = true;
			yield return _syncHPIntervalInstruction;
			_isHPSyncing = false;
		}
		while (_isMine && !Mathf.Approximately(_syncedHP, _hp));
	}

	private void SyncHP()
	{
		_photonView.RPC("UpdateHP", PhotonTargets.Others, _syncedHP = _hp);
	}

	private void Destruct()
	{
		ShowDestruction();
		if (this.destructedEvent != null)
		{
			this.destructedEvent(this);
		}
		Deactivate();
	}

	private void ShowDestruction()
	{
		Vector3 position = base.transform.position;
		position.y -= 0.5f;
		Quaternion rotation = base.transform.rotation;
		if (_explosionEffectCache != null)
		{
			HitEffect.Activate(_explosionEffectCache.RetainObject(), position, isVisible);
		}
		if (_animatedDestructionCache != null)
		{
			_animatedDestructionCache.Activate(position, rotation);
		}
		if (_physicalDestructionCache != null)
		{
			CachedObject cachedObject = _physicalDestructionCache.RetainObject();
			TmpPhysicalExplosion tmpPhysicalExplosion = null;
			if (_rigidbody != null)
			{
				tmpPhysicalExplosion = cachedObject as TmpPhysicalExplosion;
			}
			if (tmpPhysicalExplosion != null)
			{
				tmpPhysicalExplosion.Activate(position, rotation, _rigidbody.velocity);
			}
			else
			{
				cachedObject.Activate(position, rotation);
			}
		}
		CamShaker.ShakeIfExist(shakeCameraMagnitude, shakeCameraDuration, position);
	}

	private void Deactivate()
	{
		_isActive = false;
		base.gameObject.SetActiveRecursively(false);
	}

	private void OnMasterClientSwitched(PhotonPlayer player)
	{
		if (_photonView != null)
		{
			_isMine = _photonView.isMine;
		}
	}

	public static Destructible Find(int id)
	{
		return _registeredDestructibles[id];
	}

	private void Register()
	{
		if (PhotonNetwork.isMasterClient)
		{
			_id = _registeredDestructiblesCount++;
			AddRegistered(_id);
			if (_photonView != null)
			{
				_photonView.RPC("Registered", PhotonTargets.Others, _id);
			}
		}
	}

	[RPC]
	protected virtual void Registered(int id)
	{
		_id = id;
		_registeredDestructiblesCount++;
		AddRegistered(id);
	}

	private void AddRegistered(int id)
	{
		if (_registeredDestructibles.Length <= id)
		{
			int num = _registeredDestructibles.Length * 2;
			if (num <= id)
			{
				num = id + 1;
			}
			Destructible[] array = new Destructible[num];
			Array.Copy(_registeredDestructibles, array, _registeredDestructibles.Length);
			_registeredDestructibles = array;
		}
		_registeredDestructibles[id] = this;
	}
}
