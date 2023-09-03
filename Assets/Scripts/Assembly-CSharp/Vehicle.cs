using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Indestructible/Vehicle/Vehicle")]
public class Vehicle : Weapon
{
	public class ComponentsMountInfo
	{
		public string ComponentPrefabName = string.Empty;

		public int MountMult = 1;

		public ComponentsMountInfo(string prefabName, int mult)
		{
			ComponentPrefabName = prefabName;
			MountMult = mult;
		}

		public ComponentsMountInfo(string prefabName)
		{
			ComponentPrefabName = prefabName;
			MountMult = 1;
		}
	}

	public delegate void CollisionDamageDelegate(float damage, Vehicle initiator);

	public float baseMaxEnergy = 100f;

	public float baseEnergyGainRate = 1f;

	public Transform BodyMountPoint;

	public Transform ArmorMountPoint;

	public Transform WeaponMountPoint;

	private Transform _transform;

	private VehiclePhysics _vehiclePhysics;

	private Destructible _destructible;

	private float _energy;

	private float _maxEnergy;

	private float _energyGainRate;

	private BuffSystem _buffSystem;

	private PhotonView _photonView;

	private MainWeapon _weapon;

	private List<IVehicleActivationObserver> _activationObservers = new List<IVehicleActivationObserver>();

	private List<IVehicleDeactivationObserver> _deactivationObservers = new List<IVehicleDeactivationObserver>();

	private bool _isPartsMounted;

	private bool _isMine;

	private bool _isStarted;

	public bool isActive
	{
		get
		{
			return _destructible.isActive;
		}
	}

	public Destructible destructible
	{
		get
		{
			return _destructible;
		}
	}

	public BuffSystem buffSystem
	{
		get
		{
			return _buffSystem;
		}
	}

	public PhotonView photonView
	{
		get
		{
			return _photonView;
		}
	}

	public MainWeapon weapon
	{
		get
		{
			return _weapon;
		}
	}

	public bool isMine
	{
		get
		{
			return _isMine;
		}
	}

	public new Transform transform
	{
		get
		{
			return _transform;
		}
	}

	public VehiclePhysics vehiclePhysics
	{
		get
		{
			return _vehiclePhysics;
		}
	}

	public float energy
	{
		get
		{
			return _energy;
		}
	}

	public event Action<Vehicle> damageLayersChangedEvent;

	public event Action<Vehicle> playerChangedEvent;

	public event CollisionDamageDelegate collisionDamageDelegate;

	private event Action<Vehicle> _partsMountedEvent;

	public override void SetPlayer(MatchPlayer value)
	{
		Debug.Log("Vehicle.SetPlayer:" + value);
		base.SetPlayer(value);
		if (this.playerChangedEvent != null)
		{
			this.playerChangedEvent(this);
		}
	}

	public override void SetDamageLayers(int value)
	{
		base.SetDamageLayers(value);
		if (this.damageLayersChangedEvent != null)
		{
			this.damageLayersChangedEvent(this);
		}
	}

	protected MainWeapon LocateWeapon()
	{
		_weapon = GetComponentInChildren<MainWeapon>();
		_weapon.Mounted(this);
		return _weapon;
	}

	protected override void Awake()
	{
		base.Awake();
		weaponDamageType = DamageType.Collision;
		_transform = base.transform;
		_destructible = GetComponent<Destructible>();
		_buffSystem = GetComponent<BuffSystem>();
		_vehiclePhysics = GetComponent<VehiclePhysics>();
		_maxEnergy = (_energy = baseMaxEnergy);
		_energyGainRate = baseEnergyGainRate;
		if (PhotonNetwork.room != null)
		{
			_photonView = GetComponent<PhotonView>();
		}
		CheckOwnership();
	}

	protected virtual void Start()
	{
		if (isMine)
		{
			PartsMounted();
		}
		_isStarted = true;
		Activated(true);
	}

	private void OnEnable()
	{
		if (_isStarted)
		{
			Activated(false);
		}
		_energy = GetMaxEnergy();
	}

	private void OnDisable()
	{
		int i = 0;
		for (int count = _deactivationObservers.Count; i != count; i++)
		{
			IVehicleDeactivationObserver vehicleDeactivationObserver = _deactivationObservers[i];
			if (vehicleDeactivationObserver != null && vehicleDeactivationObserver as UnityEngine.Object != null)
			{
				vehicleDeactivationObserver.VehicleDeactivated(this);
			}
		}
		VehiclesManager instance = VehiclesManager.instance;
		if (instance != null)
		{
			instance.VehicleDeactivated(this);
		}
	}

	public void AddActivationObserver(IVehicleActivationObserver observer)
	{
		_activationObservers.Add(observer);
		if (base.enabled)
		{
			observer.VehicleActivated(this);
		}
	}

	public void RemoveActivationObserver(IVehicleActivationObserver observer)
	{
		_activationObservers.Remove(observer);
	}

	public void AddDeactivationObserver(IVehicleDeactivationObserver observer)
	{
		_deactivationObservers.Add(observer);
		if (!base.enabled)
		{
			observer.VehicleDeactivated(this);
		}
	}

	public void RemoveDeactivationObserver(IVehicleDeactivationObserver observer)
	{
		_deactivationObservers.Remove(observer);
	}

	private void Activated(bool isFirstTime)
	{
		CheckOwnership();
		int i = 0;
		for (int count = _activationObservers.Count; i != count; i++)
		{
			_activationObservers[i].VehicleActivated(this);
		}
		VehiclesManager.instance.VehicleActivated(this, isFirstTime);
	}

	public Collider LocateMainOwnerCollider()
	{
		Collider collider = base.mainOwnerCollider;
		if (collider == null)
		{
			GameObject gameObject = base.gameObject;
			if (_weapon != null)
			{
				SetMainOwnerCollider(collider = _weapon.LocateMainOwnerCollider(gameObject));
			}
			else
			{
				Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>();
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
		}
		return collider;
	}

	public void SubscribeToMountedEvent(Action<Vehicle> mountedDelegate)
	{
		if (_isPartsMounted)
		{
			mountedDelegate(this);
		}
		else
		{
			this._partsMountedEvent = (Action<Vehicle>)Delegate.Combine(this._partsMountedEvent, mountedDelegate);
		}
	}

	public Transform Mount(GameObject newObject, Transform mountPoint)
	{
		GameObject gameObject = mountPoint.gameObject;
		MonoBehaviour[] components;
		int componentIfacesInChildren = GetComponentIfacesInChildren<IMountable>(gameObject, out components);
		for (int i = 0; i != componentIfacesInChildren; i++)
		{
			(components[i] as IMountable).WillUnmount(this);
		}
		Transform transform = newObject.transform;
		Vector3 localPosition = transform.localPosition + mountPoint.localPosition;
		transform.parent = mountPoint.parent;
		transform.localPosition = localPosition;
		transform.localRotation = mountPoint.localRotation;
		newObject.layer = gameObject.layer;
		mountPoint.parent = null;
		UnityEngine.Object.Destroy(gameObject);
		componentIfacesInChildren = GetComponentIfacesInChildren<IMountable>(newObject, out components);
		for (int j = 0; j != componentIfacesInChildren; j++)
		{
			(components[j] as IMountable).Mounted(this);
		}
		return transform;
	}

	public Transform MountPrefab(GameObject prefab, Transform mountPoint)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab) as GameObject;
		gameObject.name = prefab.name;
		return Mount(gameObject, mountPoint);
	}

	public Transform MountPrefab(string name, Transform mountPoint)
	{
		return MountPrefab(Resources.Load(name) as GameObject, mountPoint);
	}

	public void MountParts(string bodyName, string armorName, string weaponName)
	{
		if (isMine)
		{
			if (!string.IsNullOrEmpty(bodyName))
			{
				MountBody(bodyName);
			}
			if (!string.IsNullOrEmpty(armorName))
			{
				MountArmor(armorName);
			}
			if (!string.IsNullOrEmpty(weaponName))
			{
				MountWeapon(weaponName);
			}
			else
			{
				LocateWeapon();
			}
			if (photonView != null)
			{
				photonView.RPC("RemoteMountParts", PhotonTargets.OthersBuffered, bodyName, armorName, weaponName);
			}
		}
	}

	[RPC]
	protected virtual void RemoteMountParts(string bodyName, string armorName, string weaponName)
	{
		if (!string.IsNullOrEmpty(bodyName))
		{
			MountBody(bodyName);
		}
		if (!string.IsNullOrEmpty(armorName))
		{
			MountArmor(armorName);
		}
		if (!string.IsNullOrEmpty(weaponName))
		{
			MountWeapon(weaponName);
		}
		else
		{
			LocateWeapon();
		}
		PartsMounted();
	}

	private void MountBody(string name)
	{
		BodyMountPoint = MountPrefab(name, BodyMountPoint);
	}

	private void MountArmor(string name)
	{
		ArmorMountPoint = MountPrefab(name, ArmorMountPoint);
	}

	private void MountWeapon(string name)
	{
		WeaponMountPoint = MountPrefab(name, WeaponMountPoint);
		_weapon = WeaponMountPoint.GetComponentInChildren<MainWeapon>();
	}

	protected virtual void PartsMounted()
	{
		LocateMainOwnerCollider();
		_isPartsMounted = true;
		if (this._partsMountedEvent != null)
		{
			this._partsMountedEvent(this);
			this._partsMountedEvent = null;
		}
		SkelController component = GetComponent<SkelController>();
		if (component != null)
		{
			_destructible.renderer = component.CombineAndSkinChildren(BodyMountPoint.GetComponentInChildren<Renderer>().sharedMaterial, "Static");
		}
	}

	public void MountComponents(LinkedList<ComponentsMountInfo> components)
	{
		if (!isMine)
		{
			return;
		}
		string text = string.Empty;
		if (components != null && components.Count > 0)
		{
			foreach (ComponentsMountInfo component in components)
			{
				if (!string.IsNullOrEmpty(component.ComponentPrefabName))
				{
					MountComponent(component.ComponentPrefabName, component.MountMult);
					string text2 = string.Empty;
					if (component.MountMult > 1)
					{
						text2 = ":" + component.MountMult;
					}
					text = text + component.ComponentPrefabName + text2 + ";";
				}
			}
		}
		if ((bool)photonView && text.Length > 0)
		{
			photonView.RPC("RemoteMountComponents", PhotonTargets.OthersBuffered, text);
		}
	}

	protected void MountComponent(string componentAssetPath, int mult)
	{
		UnityEngine.Object @object = Resources.Load(componentAssetPath);
		if (@object != null)
		{
			Buff buff = (UnityEngine.Object.Instantiate(@object) as BuffConf).CreateBuff();
			if (buff != null)
			{
				buff.duration = 0f;
				buff.isVisible = false;
				buffSystem.AddInstancedBuffSuspended(buff, this, true);
				buff.IncreaseStack(mult);
			}
		}
	}

	[RPC]
	protected virtual void RemoteMountComponents(string components)
	{
		if (components.Length <= 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		while (num < components.Length)
		{
			num2 = components.IndexOf(';', num);
			if (num2 < 0)
			{
				break;
			}
			int num3 = components.IndexOf(':', num);
			string componentAssetPath = components.Substring(num, (num3 <= 0 || num3 >= num2) ? (num2 - num) : (num3 - num));
			num = num2 + 1;
			int result = 1;
			if (num3 > 0 && num3 < num2)
			{
				string s = components.Substring(num3 + 1, num2 - num3 - 1);
				if (!int.TryParse(s, out result))
				{
					result = 1;
				}
			}
			MountComponent(componentAssetPath, result);
		}
	}

	public float GetBaseMaxEnergy()
	{
		return baseMaxEnergy;
	}

	public float GetMaxEnergy()
	{
		return _maxEnergy;
	}

	public void SetMaxEnergy(float value)
	{
		_maxEnergy = value;
	}

	public float GetBaseEnergyGainRate()
	{
		return baseEnergyGainRate;
	}

	public float GetEnergyGainRate()
	{
		return _energyGainRate;
	}

	public void SetEnergyGainRate(float value)
	{
		_energyGainRate = value;
	}

	public bool HasEnergy(float value)
	{
		return value <= _energy;
	}

	public bool TryConsumeEnergy(float value)
	{
		float num = _energy - value;
		if (num < 0f)
		{
			return false;
		}
		_energy = num;
		return true;
	}

	public bool TryConsumeEnergy(float value, float minReserve)
	{
		if (_energy < minReserve)
		{
			return false;
		}
		_energy -= value;
		return true;
	}

	public void ConsumeEnergy(float value)
	{
		_energy -= value;
		if (_energy < 0f)
		{
			_energy = 0f;
		}
	}

	public void OnEnergyPickedUp()
	{
		AddEnergy(baseMaxEnergy);
	}

	public void AddEnergy(float value)
	{
		float num = _energy + value;
		if (_maxEnergy < num)
		{
			num = _maxEnergy;
		}
		_energy = num;
	}

	private void Update()
	{
		AddEnergy(_energyGainRate * Time.deltaTime);
	}

	private void CheckOwnership()
	{
		_isMine = _photonView == null || _photonView.isMine;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.contacts.Length == 0)
		{
			return;
		}
		Collider collider = collision.collider;
		Destructible destructible;
		if (!CheckFoeDamageAbilty(collider, out destructible))
		{
			return;
		}
		Vector3 relativeVelocity = collision.relativeVelocity;
		Vector3 vector = collision.contacts[0].normal;
		float num = Vector3.Dot(relativeVelocity, vector);
		ContactPoint[] contacts = collision.contacts;
		int i = 1;
		for (int num2 = contacts.Length; i < num2; i++)
		{
			Vector3 normal = collision.contacts[i].normal;
			float num3 = Vector3.Dot(relativeVelocity, normal);
			if (num < num3)
			{
				num = num3;
				vector = normal;
			}
		}
		if (!(0f < num))
		{
			return;
		}
		vector = destructible.transform.InverseTransformDirection(vector);
		float num4 = (Mathf.Abs(vector.x) + Mathf.Abs(vector.y)) * destructible.sideFragility;
		num4 = ((!(vector.z < 0f)) ? (num4 + vector.z * destructible.forwardFragility) : (num4 - vector.z * destructible.backFragility));
		num4 *= num * GetDamage();
		if (this.collisionDamageDelegate != null)
		{
			this.collisionDamageDelegate(num4, this);
		}
		if (Damage(destructible, num4) == DamageResult.Destructed)
		{
			float crushSpeedFactor = destructible.crushSpeedFactor;
			if (0f < crushSpeedFactor)
			{
				_vehiclePhysics.velocity *= crushSpeedFactor;
				_vehiclePhysics.angularVelocity *= crushSpeedFactor;
				float fixedDeltaTime = Time.fixedDeltaTime;
				_vehiclePhysics.position = _vehiclePhysics.ExtrapolatePosition(fixedDeltaTime);
				_vehiclePhysics.rotation = _vehiclePhysics.ExtrapolateRotation(fixedDeltaTime);
			}
		}
	}

	private void OnMasterClientSwitched(PhotonPlayer player)
	{
		CheckOwnership();
	}

	public override string ToString()
	{
		return string.Format("[Vehicle: name={0} isActive={1},  photonView={2}]", base.gameObject.name, isActive, photonView);
	}
}
