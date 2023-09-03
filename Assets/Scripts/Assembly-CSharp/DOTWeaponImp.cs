using System;

[Serializable]
public class DOTWeaponImp
{
	public delegate void TargetOnDOTDamageDelegate(Destructible victim, Vehicle victimVehicle, float damage);

	public float baseDamage = 1f;

	public float baseDuration = 5f;

	public float baseHeat = 3f;

	private float _duration;

	private float _damage;

	private float _heat;

	private Destructible _cachedDestructible;

	private BurningBuff _cachedBuff;

	private Weapon _weapon;

	public event TargetOnDOTDamageDelegate onTargetDOTDamagedEvent;

	public void Init(Weapon weapon)
	{
		_weapon = weapon;
		Reset();
	}

	public float GetBaseDamage()
	{
		return baseDamage;
	}

	public float GetDamage()
	{
		return _damage;
	}

	public void SetDamage(float value)
	{
		_damage = value;
	}

	public float GetBaseDuration()
	{
		return baseDuration;
	}

	public float GetDuration()
	{
		return _duration;
	}

	public void SetDuration(float value)
	{
		_duration = value;
	}

	public float GetBaseHeat()
	{
		return baseHeat;
	}

	public float GetHeat()
	{
		return _heat;
	}

	public void SetHeat(float value)
	{
		_heat = value;
	}

	public virtual void Reset()
	{
		_cachedDestructible = null;
		_cachedBuff = null;
		SetDamage(GetBaseDamage());
		SetDuration(GetBaseDuration());
		SetHeat(GetBaseHeat());
	}

	public void Heat(Destructible destructible)
	{
		Heat(destructible, GetHeat());
	}

	public virtual void Heat(Destructible destructible, float heat)
	{
		if (_cachedDestructible != destructible || _cachedBuff.IsEnded())
		{
			_cachedDestructible = null;
			_cachedBuff = null;
			Vehicle vehicle = destructible.vehicle;
			if (vehicle == null)
			{
				return;
			}
			BuffSystem buffSystem = vehicle.buffSystem;
			if (buffSystem == null)
			{
				return;
			}
			_cachedBuff = vehicle.buffSystem.AddBuff<BurningBuff>(_weapon);
			if (_cachedBuff == null)
			{
				return;
			}
			_cachedDestructible = destructible;
			_cachedBuff.onBurningEvent += OnTargetBurning;
		}
		_cachedBuff.duration = GetDuration();
		_cachedBuff.effectScale = GetDamage();
		_cachedBuff.AddHeat(heat);
	}

	private void OnTargetBurning(INetworkWeapon _weaponOwner, Vehicle vehicle, Destructible target, float damage)
	{
		if (this.onTargetDOTDamagedEvent != null)
		{
			this.onTargetDOTDamagedEvent(target, vehicle, damage);
		}
	}
}
