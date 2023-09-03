using System.Collections;
using UnityEngine;

public class DozerActiveAbility : CooldownAbility, INetworkWeapon
{
	public float Radius = 3f;

	public Vector3 PositionOffset;

	public float Damage = 100f;

	public float HitKickRelative = 50f;

	public float HitKickConstant;

	public GameObject ActivateFX;

	public float FXDuration = 1f;

	private Vehicle _playerVeh;

	private GameObject _bladeObj;

	private int _damageLayers;

	private MatchPlayer _player;

	private GameObject _activationFXGO;

	private YieldInstruction _stopFxDelayYI;

	public MatchPlayer player
	{
		get
		{
			return _player;
		}
	}

	public DamageType damageType
	{
		get
		{
			return DamageType.SecondaryCollision;
		}
	}

	protected override void Start()
	{
		base.Start();
		Init();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if ((bool)_bladeObj)
		{
			_bladeObj.animation.Play();
			_bladeObj.animation.Rewind();
			_bladeObj.animation["hit"].normalizedTime = 1f;
			_bladeObj.animation.Sample();
			_bladeObj.animation.Stop();
		}
	}

	protected void Init()
	{
		if (base.vehicle != null)
		{
			_damageLayers = base.vehicle.damageLayers;
			base.vehicle.damageLayersChangedEvent += DamageLayersChanged;
		}
		_playerVeh = base.vehicle;
		if ((bool)_playerVeh)
		{
			_player = _playerVeh.player;
		}
		PlayerVehicle playerVehicle = base.vehicle as PlayerVehicle;
		if ((bool)playerVehicle)
		{
			playerVehicle.playerChangedEvent += PlayerChanged;
		}
		if ((bool)ActivateFX)
		{
			_activationFXGO = (GameObject)Object.Instantiate(ActivateFX);
			_stopFxDelayYI = new WaitForSeconds(FXDuration);
		}
	}

	[RPC]
	protected override void AbilityActivated()
	{
		base.AbilityActivated();
		Hit();
	}

	private IEnumerator StopFX()
	{
		yield return _stopFxDelayYI;
		_activationFXGO.SetActiveRecursively(false);
	}

	private void Hit()
	{
		if (!_bladeObj && (bool)base.vehicle.ArmorMountPoint)
		{
			_bladeObj = base.vehicle.ArmorMountPoint.gameObject;
			if ((bool)_activationFXGO)
			{
				_activationFXGO.transform.parent = base.transform;
				_activationFXGO.transform.localPosition = Vector3.zero;
				_activationFXGO.transform.localRotation = Quaternion.identity;
			}
		}
		if ((bool)_bladeObj)
		{
			_bladeObj.animation["hit"].enabled = true;
			_bladeObj.animation["hit"].normalizedTime = 0f;
			_bladeObj.animation.Play("hit");
		}
		if ((bool)_activationFXGO)
		{
			_activationFXGO.SetActiveRecursively(true);
			StartCoroutine(StopFX());
		}
		Vector3 position = _rootObject.transform.TransformPoint(PositionOffset);
		Collider[] array = Physics.OverlapSphere(position, Radius, _damageLayers);
		Vector3 forward = base.transform.forward;
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (base.vehicle.mainOwnerCollider != collider)
			{
				Destructible component = collider.transform.root.GetComponent<Destructible>();
				if ((bool)component && component.isMine)
				{
					component.Damage(Damage, this);
					ApplyKick(forward, component);
				}
			}
		}
	}

	private void ApplyKick(Vector3 direction, Destructible hitDestructible)
	{
		Rigidbody component = hitDestructible.GetComponent<Rigidbody>();
		if (!(component == null))
		{
			Vector3 position = component.position;
			component.AddForce((HitKickRelative * component.mass + HitKickConstant) * direction, ForceMode.Impulse);
		}
	}

	private void PlayerChanged(Vehicle vehicle)
	{
		_player = _playerVeh.player;
	}

	private void DamageLayersChanged(Vehicle vehicle)
	{
		if ((bool)vehicle)
		{
			_damageLayers = vehicle.damageLayers;
		}
	}
}
